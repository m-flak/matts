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

using FluentValidation;

public class User
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
}

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.UserName).NotNull();
        RuleFor(x => x.UserName.Length).GreaterThan(0).When(x => x.UserName != null);
        RuleFor(x => x.Password).NotNull();
        RuleFor(x => x.Password.Length).GreaterThan(0).When(x => x.Password != null);
        RuleFor(x => x.Role).NotNull();
        RuleFor(x => x.Role.Length).GreaterThan(0).When(x => x.Role != null);
    }
}