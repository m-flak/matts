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
namespace matts.Models;

using FluentValidation;
using matts.Utils;

public class ApplyToJob
{
    public string? JobUuid { get; set; }
    public string? ApplicantUuid { get; set; }
}

public class ApplyToJobValidator : AbstractValidator<ApplyToJob>
{
    public ApplyToJobValidator()
    {
        RuleFor(x => x.JobUuid).NotNull();
        RuleFor(x => x.JobUuid.Length).GreaterThan(0).When(x => x.JobUuid != null);
        RuleFor(x => x.ApplicantUuid).NotNull();
        RuleFor(x => x.ApplicantUuid.Length).GreaterThan(0).When(x => x.ApplicantUuid != null);
    }
}
