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
namespace matts.Services;

using Microsoft.Extensions.Configuration;
using matts.Interfaces;
using matts.Models;
using matts.Constants;
using System.Threading.Tasks;
using BCrypt.Net;

public class UserService : IUserService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _repository;
    private readonly IApplicantRepository _appRepository;
    private readonly IEmployerRepository _empRepository;

    private bool _useDummyData;

    public UserService(
        IConfiguration configuration,
        ILogger<UserService> logger,
        IUserRepository repository,
        IApplicantRepository appRepository,
        IEmployerRepository empRepository
        )
    {
        _configuration = configuration;
        _logger = logger;
        _repository = repository;
        _appRepository = appRepository;
        _empRepository = empRepository;

        ConfigureService();
    }

    public async Task<bool> AuthenticateUser(User user)
    {
        if (_useDummyData)
        {
            return true;
        }

        // If user doesn't exist the Neo4j driver will throw
        try
        {
            var userDb = await _repository.GetUserByName(user.UserName!);
            return (BCrypt.Verify(user.Password, userDb.Password) && userDb.Role == user.Role);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public async Task<string> GetUserApplicantId(User user)
    {
        if (_useDummyData)
        {
            return System.Guid.NewGuid().ToString();
        }

        return await _repository.GetApplicantIdForUserByUserName(user.UserName!);
    }

    public async Task<Applicant> GetApplicantForUser(User user)
    {
        if (_useDummyData)
        {
            return new Applicant()
            {
                Uuid = System.Guid.NewGuid().ToString(),
                Name = "Dummy Datason",
                PhoneNumber = "1234567890",
                Email = "dummy.datason@johndoecorp.com"
            };
        }

        string uuid = await GetUserApplicantId(user);
        return await _appRepository.GetApplicantByUuid(uuid);
    }

    public async Task<string> GetUserEmployerId(User user)
    {
        if (_useDummyData)
        {
            return System.Guid.NewGuid().ToString();
        }

        return await _repository.GetEmployerIdForUserByUserName(user.UserName!);
    }

    public async Task<Employer> GetEmployerForUser(User user)
    {
        if (_useDummyData)
        {
            return new Employer()
            {
                Uuid = System.Guid.NewGuid().ToString(),
                CompanyName = "John Doe Corporation",
                Name = "Dummy Datason",
                PhoneNumber = "1234567890",
                Email = "dummy.datason@johndoecorp.com",
            };
        }

        string uuid = await GetUserEmployerId(user);
        return await _empRepository.GetEmployerByUuid(uuid);
    }

    public async Task<bool> RegisterNewUser(UserRegistration user)
    {
        if (_useDummyData)
        {
            return true;
        }

        return user.Role switch 
        {
            UserRoleConstants.USER_ROLE_APPLICANT => await _repository.CreateNewApplicantUser(user),
            UserRoleConstants.USER_ROLE_EMPLOYER => await _repository.CreateNewEmployerUser(user),
            _ => false
        };
    }

    private void ConfigureService()
    {
        var useDummyData = _configuration.GetValue<bool>("DummyData:UserService", false);
        _useDummyData = useDummyData;
    }
}
