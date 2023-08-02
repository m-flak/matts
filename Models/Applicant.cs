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
using System.Text.Json;

namespace matts.Models;

public class Applicant 
{
    public readonly struct ProfileImage 
    {
        public ProfileImage(string mimeType, string imageData) 
        {
            MimeType = mimeType;
            ImageData = imageData;
        }

        public string MimeType { get; init; }

        public string ImageData { get; init; }
    }

    public long Id { get; set; }

    public string? Uuid { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set;}

    public ProfileImage? ApplicantPhoto { get; set; }

    public DateTime? InterviewDate { get; set; }

    public bool? Rejected { get; set; }

    public override string ToString()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.Serialize(this, options);
    }
}
