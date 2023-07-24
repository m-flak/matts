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

public class UserService : IUserService
{
    private readonly IConfiguration _configuration;

    private bool _useDummyData;

    public UserService(IConfiguration configuration)
    {
        _configuration = configuration;

        ConfigureService();
    }

    public async Task<bool> AuthenticateUser(User user)
    {
        if (_useDummyData)
        {
            return true;
        }

        // TODO: Implement
        return false;
    }

    public async Task<string> GetUserApplicantId(User user)
    {
        if (_useDummyData)
        {
            return System.Guid.NewGuid().ToString();
        }

        // TODO: Implement
        return System.Guid.NewGuid().ToString();
    }

    private void ConfigureService()
    {
        var useDummyData = _configuration.GetValue<bool>("DummyData:UserService", false);
        _useDummyData = useDummyData;
    }
}
