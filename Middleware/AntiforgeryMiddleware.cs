﻿/* matts
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
using Microsoft.AspNetCore.Antiforgery;

namespace matts.Middleware;

public class AntiforgeryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAntiforgery _antiforgery;
    private readonly IWebHostEnvironment _environment;

    public AntiforgeryMiddleware(RequestDelegate next, IAntiforgery antiforgery, IWebHostEnvironment environment)
    {
        _next = next;
        _antiforgery = antiforgery;
        _environment = environment;
    }

    public async Task Invoke(HttpContext context)
    {
        var requestPath = context.Request.Path.Value;
        bool condition = string.Equals(requestPath, "/", StringComparison.OrdinalIgnoreCase)
            || string.Equals(requestPath, "/index.html", StringComparison.OrdinalIgnoreCase);

        // The SPA Proxy Middleware will keep the above from firing when running on local
        if (_environment.IsDevelopment()) 
        {
            condition |= string.Equals(requestPath, "/config/", StringComparison.OrdinalIgnoreCase);
        }

        if (condition)
        {
            var tokenSet = _antiforgery.GetAndStoreTokens(context);
            context.Response.Cookies.Append("XSRF-TOKEN", tokenSet.RequestToken!,
                new CookieOptions { HttpOnly = false });
        }

        await _next(context);
    }
}
