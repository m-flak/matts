/* matts
 * "Matthew's ATS" - Portfolio Project
 * Copyright (C) 2023  Matthew E. Kehrer <matthew@kehrer.dev>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
**/
using System.Net.Mime;

namespace matts.Middleware;

public class ContentSecurityPolicyMiddleware
{
    private const string CSP_HEADER = "Content-Security-Policy";

    private readonly RequestDelegate _next;
    private readonly CSP _cspPolicy;

    public ContentSecurityPolicyMiddleware(RequestDelegate next, CSP cspPolicy)
    {
        _next = next;
        _cspPolicy = cspPolicy;
    }

    public async Task Invoke(HttpContext context)
    {
        var requestPath = context.Request.Path.Value;
        if (
            context.Request.Method == HttpMethod.Get.Method
            && !(requestPath?.StartsWith("/ws", StringComparison.OrdinalIgnoreCase) ?? false)
           )
        {
            context.Response.OnStarting(state =>
            {
                var httpContext = (HttpContext)state;

                if (httpContext.Response.ContentType?.StartsWith(MediaTypeNames.Text.Html) == true)
                {
                    string nonce = CSP.CreateNonce();
                    var policy = _cspPolicy.Clone();
                    policy.AddScriptSrc($"'nonce-{nonce}'");

                    httpContext.Response.Cookies.Append("CSP-NONCE", nonce,
                        new CookieOptions { HttpOnly = false, IsEssential = true, Secure = true });

                    if (!httpContext.Response.Headers.ContainsKey(CSP_HEADER))
                    {
                        httpContext.Response.Headers.Add(CSP_HEADER, policy.ToString());
                    }

                    policy = null;
                }

                return Task.CompletedTask;
            }, context);
        }

        await _next(context);
    }
}
