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
using MapsterMapper;
using matts.Daos;
using matts.Interfaces;
using matts.Models;
using matts.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matts.Repositories;

public class UserRepository : IUserRepository
{ 
    private readonly IDataAccessObject<User> _daoUser;

    public UserRepository(IDataAccessObject<User> daoUser)
    {
        _daoUser = daoUser;
    }

    public async Task<string> GetApplicantIdForUserByUserName(string userName)
    {
        UserDao dao = (UserDao) _daoUser;
        return await dao.GetApplicantIdForUserName(userName);
    }

    public async Task<User> GetUserByName(string userName)
    {
        return await _daoUser.GetByUuid(userName);
    }
}
