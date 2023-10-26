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
namespace matts.Models.Linkedin;

public sealed class UserInfo
{
    public string? Sub {  get; set; }
    public string? Name { get; set;}
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public Uri? Picture { get; set; }
    public object? Locale { get; set; }
    public string? Email { get; set; }
    public bool? EmailVerified { get; set; }
}
