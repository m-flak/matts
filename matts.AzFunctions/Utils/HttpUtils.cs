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
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;

namespace matts.AzFunctions.Utils;

internal sealed class HttpUtils
{
    internal static bool HandleCreateOptionsResponse(HttpRequestData request, [MaybeNullWhen(false)] out HttpResponseData response)
    {
        if (string.Equals(request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            var optionsResponse = request.CreateResponse();
            optionsResponse.Headers.Add("Allow", new[] { "POST", "OPTIONS" });
            optionsResponse.Headers.Add("Access-Control-Allow-Origin", "*");
            optionsResponse.StatusCode = HttpStatusCode.OK;
            response = optionsResponse;
            return true;
        }

        response = null;
        return false;
    }

    internal static async Task<HttpResponseData> CreateMessageResponseAsync(HttpRequestData request, HttpStatusCode code, string message)
    {
        var responseBad = request.CreateResponse(code);
        await responseBad.WriteStringAsync(message);
        return responseBad;
    }

    internal static bool IsContentType(HttpRequestData request, string contentType)
    {
        bool hasHeader = request.Headers.TryGetValues("Content-Type", out var values);
        bool matches = false;

        if (hasHeader)
        {
            string value = values?.FirstOrDefault(defaultValue: string.Empty) ?? string.Empty;
            matches = string.Equals(contentType, value, StringComparison.OrdinalIgnoreCase);
        }

        return hasHeader && matches;
    }

    private HttpUtils() { }
}
