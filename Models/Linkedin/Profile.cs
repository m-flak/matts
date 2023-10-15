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

public class Profile
{
    public FirstName? FirstName { get; set; }
    public string? LocalizedFirstName { get; set; }
    public Headline? Headline { get; set; }
    public string? LocalizedHeadline { get; set; }
    public string? VanityName { get; set; }
    public string? Id { get; set; }
    public LastName? LastName { get; set; }
    public string? LocalizedLastName { get; set; }
    public ProfilePicture? ProfilePicture { get; set; }
}

public class Headline : LocalizedData
{
}
public class FirstName : LocalizedData
{
}

public class LastName : LocalizedData
{
}

public class ProfilePicture
{
    public string? DisplayImage { get; set; }
}
