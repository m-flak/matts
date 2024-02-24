/* matts
 * "Matthew's ATS" - Portfolio Project
 * Copyright (C) 2023-2024  Matthew E. Kehrer <matthew@kehrer.dev>
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
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace matts.AzFunctions.Utils;

[DefaultStatusCode(200)]
public sealed class OptionsOkResult : StatusCodeResult
{
    private const int DefaultStatusCode = 200;
    private const string AllowHeader = "Allow";
    private const string AccessControlHeader = "Access-Control-Allow-Origin";

    private readonly Dictionary<string, string?[]> _resultHeaders;

    public string RequestedHttpMethod { get; init; } = "GET";
    public IReadOnlyDictionary<string, string?[]> ResultHeaders { get; init; }

    public OptionsOkResult()
        : base(DefaultStatusCode)
    {
        _resultHeaders = new Dictionary<string, string?[]>();
        ResultHeaders = _resultHeaders;
    }

    public OptionsOkResult(string requestedHttpMethod)
        : this()
    {
        RequestedHttpMethod = requestedHttpMethod;

        _resultHeaders.Add(AllowHeader, new[] { RequestedHttpMethod, "OPTIONS" });
        _resultHeaders.Add(AccessControlHeader, new[] { "*" });
    }

    public override void ExecuteResult(ActionContext context)
    {
        context.HttpContext.Response.Headers.Allow
            = _resultHeaders[AllowHeader];
        context.HttpContext.Response.Headers.AccessControlAllowOrigin
            = _resultHeaders[AccessControlHeader];

        context.HttpContext.Response.StatusCode = DefaultStatusCode;
    }
}
