﻿/* matts
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
namespace matts.Interfaces;

using matts.Models;

public interface IUserService
{
    public Task<bool> AuthenticateUser(User user);

    public Task<string> GetUserApplicantId(User user);

    public Task<Applicant> GetApplicantForUser(User user);

    public Task<string> GetUserEmployerId(User user);

    public Task<Employer> GetEmployerForUser(User user);

    public Task<bool> RegisterNewUser(UserRegistration user);
}
