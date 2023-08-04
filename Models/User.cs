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
using matts.Constants;

[DbNode("User", "u")]
public class User
{
    [DbNodeUuid]
    [DbNodeCreationField]
    public string? UserName { get; set; }

    [DbNodeCreationField]
    public string? Password { get; set; }

    [DbNodeCreationField]
    public string? Role { get; set; }
}

public class UserRegistration : User
{
    public string? FullName { get; set; }
    public string? CompanyName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set;}
}

#pragma warning disable CS8602
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

public class UserRegistrationValidator : AbstractValidator<UserRegistration>
{
    public UserRegistrationValidator()
    {
        RuleFor(x => x.Role).NotNull();
        RuleFor(x => x.Role.Length).GreaterThan(0).When(x => x.Role != null);
        RuleFor(x => x.CompanyName).NotNull().When(x => x?.Role == UserRoleConstants.USER_ROLE_EMPLOYER);
        RuleFor(x => x.CompanyName.Length).GreaterThan(0).When(x => x.CompanyName != null && x?.Role == UserRoleConstants.USER_ROLE_EMPLOYER);
        RuleFor(x => x.FullName).NotNull();
        RuleFor(x => x.FullName.Length).GreaterThan(0).When(x => x.FullName != null);
        RuleFor(x => x.Email).NotNull();
        RuleFor(x => x.Email.Length).GreaterThan(0).When(x => x.Email != null);
        RuleFor(x => x.Email).EmailAddress().When(x => x.Email != null);
        RuleFor(x => x.PhoneNumber).NotNull();
        RuleFor(x => x.PhoneNumber.Length).GreaterThan(0).When(x => x.PhoneNumber != null);
        RuleFor(x => x.UserName).NotNull();
        RuleFor(x => x.UserName.Length).GreaterThan(0).When(x => x.UserName != null);
        RuleFor(x => x.Password).NotNull();
        RuleFor(x => x.Password.Length).GreaterThan(0).When(x => x.Password != null);
    }
}
#pragma warning restore CS8602
