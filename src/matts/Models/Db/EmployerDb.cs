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
namespace matts.Models.Db;

using matts.Utils;

[DbNode("Employer", "e")]
public class EmployerDb
{
    public EmployerDb() {}
    public EmployerDb(EmployerDb other)
    {
        Uuid = other.Uuid;
        Name = other.Name;
        Email = other.Email;
        PhoneNumber = other.PhoneNumber;
        CompanyName = other.CompanyName;
    }

    [DbNodeUuid]
    [DbNodeCreationField]
    public string? Uuid { get; set; }

    [DbNodeOrderBy]
    [DbNodeCreationField]
    public string? Name { get; set; }

    [DbNodeCreationField]
    public string? Email { get; set; }

    [DbNodeCreationField]
    public string? PhoneNumber { get; set;}

    [DbNodeCreationField]
    public string? CompanyName { get; set; }
}