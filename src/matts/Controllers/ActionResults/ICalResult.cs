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
using Microsoft.AspNetCore.Mvc;
using Ical.Net.Serialization;
using Ical.Net;

namespace matts.Controllers.ActionResults;

public class ICalResult : ActionResult
{
    private readonly Calendar _calendar;

    public ICalResult(Calendar calendar)
        : base()
    {
        _calendar = calendar;
    }

    public override Task ExecuteResultAsync(ActionContext context)
    {
        var serializer = new CalendarSerializer();
        var response = context.HttpContext.Response;
        response.ContentType = "text/calendar";
        response.StatusCode = StatusCodes.Status200OK;
        
        return response
            .WriteAsync(serializer.SerializeToString(_calendar))
            .ContinueWith((_) => response.CompleteAsync(), scheduler: TaskScheduler.Current);
    }
}
