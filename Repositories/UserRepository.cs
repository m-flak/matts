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
using matts.Constants;
using matts.Daos;
using matts.Interfaces;
using matts.Models;
using matts.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace matts.Repositories;

public class UserRepository : IUserRepository
{ 
    private readonly IDataAccessObject<User> _daoUser;
    private readonly IDataAccessObject<ApplicantDb> _daoApp;
    private readonly IDataAccessObject<EmployerDb> _daoEmp;
    private readonly IMapper _mapper;

    public UserRepository(IDataAccessObject<User> daoUser, IDataAccessObject<ApplicantDb> daoApp, IDataAccessObject<EmployerDb> daoEmp, IMapper mapper)
    {
        _daoUser = daoUser;
        _daoApp = daoApp;
        _daoEmp = daoEmp;
        _mapper = mapper;
    }

    public async Task<string> GetApplicantIdForUserByUserName(string userName)
    {
        UserDao dao = (UserDao) _daoUser;
        return await dao.GetApplicantIdForUserName(userName);
    }

    public async Task<string> GetEmployerIdForUserByUserName(string userName)
    {
        UserDao dao = (UserDao) _daoUser;
        return await dao.GetEmployerIdForUserName(userName);
    }

    public async Task<User> GetUserByName(string userName)
    {
        return await _daoUser.GetByUuid(userName);
    }

    public async Task<bool> CreateNewApplicantUser(UserRegistration user)
    {
        User userDb = _mapper.Map<User>(user);
        userDb.Password = BCrypt.Net.BCrypt.HashPassword(userDb.Password);
        ApplicantDb applicant = new ApplicantDb()
        {
            Name = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };

        var createdUser = await _daoUser.CreateNew(userDb);
        var createdApplicant = await _daoApp.CreateNew(applicant);

        if (createdUser == null || createdApplicant == null)
        {
            return false;
        }

        //UserDao dao = (UserDao) _daoUser;
        //return await dao.MakeUserForApplicant(createdUser, createdApplicant);
        return await _daoUser.CreateRelationshipBetween(RelationshipConstants.IS_USER_FOR, createdUser, createdApplicant, typeof(ApplicantDb));
    }

    public async Task<bool> CreateNewEmployerUser(UserRegistration user)
    {
        User userDb = _mapper.Map<User>(user);
        userDb.Password = BCrypt.Net.BCrypt.HashPassword(userDb.Password);
        EmployerDb employer = new EmployerDb()
        {
            Name = user.FullName,
            CompanyName = user.CompanyName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };

        var createdUser = await _daoUser.CreateNew(userDb);
        var createdEmployer = await _daoEmp.CreateNew(employer);

        if (createdUser == null || createdEmployer == null)
        {
            return false;
        }

        //UserDao dao = (UserDao) _daoUser;
        //return await dao.MakeUserForEmployer(createdUser, createdEmployer);
        return await _daoUser.CreateRelationshipBetween(RelationshipConstants.IS_USER_FOR, createdUser, createdEmployer, typeof(EmployerDb));
    }
}
