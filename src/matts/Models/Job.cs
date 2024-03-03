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
using FluentValidation;

namespace matts.Models;

public class Job
{
    public long Id { get; set; }

    public string? Uuid { get; set; }

    public string? Name  { get; set; }

    public string? Status { get; set; }

    public string? Description { get; set; }

    public List<Applicant>? Applicants { get; set; }

    public long ApplicantCount { get; set; }

    public override string ToString()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.Serialize(this, options);
    }
}

#pragma warning disable CS8602
public class JobValidator : AbstractValidator<Job>
{
    public JobValidator()
    {
        RuleFor(x => x.Name).NotNull();
        RuleFor(x => x.Name.Length).GreaterThan(0).When(x => x.Name != null);
        RuleFor(x => x.Description).NotNull();
        RuleFor(x => x.Description.Length).GreaterThan(0).When(x => x.Description != null);
    }
}
#pragma warning restore CS8602
