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
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace matts.AzFunctions.Utils;

internal sealed class HttpUtils
{
    internal static bool HandleCreateOptionsResponse(HttpRequest request, [MaybeNullWhen(false)] out OptionsOkResult response, string allowMethod = "POST")
    {
        if (string.Equals(request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            response = new OptionsOkResult(allowMethod);
            return true;
        }

        response = null;
        return false;
    }

    internal static bool IsContentType(HttpRequest request, string contentType)
    {
        bool hasHeader = request.Headers.TryGetValue("Content-Type", out var values);
        bool matches = false;

        if (hasHeader)
        {
            string value = values.FirstOrDefault(defaultValue: string.Empty) ?? string.Empty;
            matches = string.Equals(contentType, value, StringComparison.OrdinalIgnoreCase);
        }

        return hasHeader && matches;
    }

    internal static ObjectResult ErrorResultWithDetails(
                                    [Optional, DefaultParameterValue(HttpStatusCode.BadRequest)]
                                        HttpStatusCode status,
                                        string msg)
    {
        return new ObjectResult(
            new
            {
                Message = msg
            })
        {
            StatusCode = (int)status
        };
    }

    private HttpUtils() { }
}
