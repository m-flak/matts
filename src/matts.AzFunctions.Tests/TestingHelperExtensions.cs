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
using Microsoft.Azure.Functions.Worker;
using Moq;
using UnitTestEx.Xunit.Internal;

namespace matts.AzFunctions.Tests;
internal static class TestingHelperExtensions
{
    public static string GetFunctionRoute(this IAzFunctions klass)
    {
        try
        {
            var attribute = klass
                .HostClass
                .GetMethod("Run")
                ?.GetParameters()[0]
                .GetCustomAttributes(true)
                .FirstOrDefault(
                    a => typeof(HttpTriggerAttribute).IsInstanceOfType(a),
                    null)
                as HttpTriggerAttribute;

            return attribute?.Route ?? string.Empty;
        }
        catch (IndexOutOfRangeException)
        {
            return string.Empty;
        }
    }

    public static void AddSingletonToTestHost<TMock, TStartup>(this Mock<TMock> mock, FunctionTester<TStartup> testHost)
        where TMock : class
        where TStartup : class, new()
    {
        testHost.MockSingleton(mock);
    }
}
