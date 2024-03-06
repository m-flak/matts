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
using System.Reflection;
using Microsoft.Azure.Functions.Worker;

namespace matts.AzFunctions.Tests.Helpers;
public static class FuncData
{
    public static string GetHttpRoute(Type functionClass)
    {
        string? route = null;

        var runMethod = functionClass.GetMethod("Run");
        var runParams = runMethod?.GetParameters();
        if (runMethod is null
            || runParams is null
            || runParams.Length == 0)
        {
            return string.Empty;
        }

        var attribute = runParams[0]
            .GetCustomAttributes(true)
            .FirstOrDefault(
                a => typeof(HttpTriggerAttribute).IsInstanceOfType(a),
                null)
            as HttpTriggerAttribute;

        route = attribute?.Route;
        route ??= string.Empty;
        return route;
    }
}
