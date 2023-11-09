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
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace matts.Middleware.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class LinkedinResultFilterAttribute : ResultFilterAttribute
{
    public string SkipWhenKey { get; }
    public string SkipWhenValue { get; }

    public LinkedinResultFilterAttribute(string skipWhenKey, string skipWhenValue)
    {
        SkipWhenKey = skipWhenKey;
        SkipWhenValue = skipWhenValue;
    }

#pragma warning disable VSTHRD100
    public override async void OnResultExecuting(ResultExecutingContext context)
    {
        try
        {
            if (context.Result is StatusCodeResult res)
            {
                // This should be set when you don't want the controller's result to be short-circuited
                // and rewritten by this filter.
                if (context.HttpContext.Items.TryGetValue(SkipWhenKey, out var item)
                    && item is string value
                    && string.Equals(SkipWhenValue, value))
                {
                    return;
                }

                HttpRequest request = context.HttpContext.Request;
                HttpResponse response = context.HttpContext.Response;

                string? errorName = request.Query.TryGetValue("error", out var eName)
                    ? eName.FirstOrDefault(defaultValue: default)
                    : default;

                string? errorDesc = request.Query.TryGetValue("error_description", out var eDesc)
                    ? eDesc.FirstOrDefault(defaultValue: default)
                    : default;

                response.StatusCode = res.StatusCode;
                response.ContentType = MediaTypeNames.Text.Html;
                await using var writer = new StreamWriter(response.Body, Encoding.UTF8);
                // Generate response accordingly
                if (res.StatusCode == StatusCodes.Status200OK)
                {
                    await writer.WriteAsync(GenerateSuccessPage());
                    await writer.FlushAsync();
                }
                else
                {
                    await writer.WriteAsync(GenerateErrorPage(errorName, errorDesc));
                    await writer.FlushAsync();
                }

                context.Cancel = true;
            }
        }
        catch (Exception)
        {
            //OH FFFUUU--
            return;
        }
    }
#pragma warning restore VSTHRD100

    private static string GenerateErrorPage(string? errorName, string? errorDesc)
    {
        const string undefined = "undefined";
        errorName ??= undefined; errorDesc ??= undefined;

        var body =
            @$"
            <h1>Error: {errorName}</h1><hr/><br/><p>{errorDesc}</p><br/><a id=""close"" href=""#"">CLICK HERE TO CLOSE.</a>
            <script>function e(){{{{window.close()}}}}document.querySelector(""#close"").addEventListener(""click"",e);</script>
            ";

        return GeneratePage("Authentication Failure!", body);
    }

    private static string GenerateSuccessPage()
    {
        const string body =
            @"
            <p>This page will close automatically...</p><br/><a id=""close"" href=""#"">Still see me? CLICK HERE.</a>
            <script>function e(){{window.close()}}document.querySelector(""#close"").addEventListener(""click"",e),e();</script>
            ";
        return GeneratePage("Authentication Success!", body);
    }

    private static string GeneratePage(string title, string body)
    {
        return @$"<!DOCTYPE html>
        <html lang=""en"">
            <head>
                <meta charset=""UTF-8"">
                <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>{title}</title>
            </head>
            <body>
            {body}
            </body>
        </html>";
    }
}
