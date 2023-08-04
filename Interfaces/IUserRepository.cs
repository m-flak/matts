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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using matts.Models;

namespace matts.Interfaces;

public interface IUserRepository
{
    public Task<User> GetUserByName(string userName);
    public Task<string> GetApplicantIdForUserByUserName(string userName);
    public Task<string> GetEmployerIdForUserByUserName(string userName);
    public Task<bool> CreateNewApplicantUser(UserRegistration user);
    public Task<bool> CreateNewEmployerUser(UserRegistration user);
}
