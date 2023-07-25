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
using matts.Interfaces;
using matts.Models.Db;
using matts.Utils;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matts.Daos;

public class UserDao : IDataAccessObject<User>
{
    private readonly IDriver _driver;

    public UserDao(IDriver driver)
    {
        _driver = driver;
    }

    public async Task<User> CreateNew(User createWhat)
    {
        throw new NotImplementedException();
    }

    public async Task<List<User>> GetAll()
    {
        throw new NotImplementedException();
    }

    public async Task<List<User>> GetAllAndFilterByProperties(IReadOnlyDictionary<string, object> filterProperties)
    {
        throw new NotImplementedException();
    }

    public async Task<List<User>> GetAllByRelationship(string relationship, string? optionalRelationship, string whomUuid)
    {
        throw new NotImplementedException();
    }

    public async Task<User> GetByUuid(string uuid)
    {
        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteReadAsync(
                async tx =>
                {
                    var cursor = await tx.RunAsync(
                        "MATCH (u:User) " +
                        "WHERE u.userName = $username " +
                        "RETURN u",
                        new
                        {
                            username = uuid
                        }
                    );

                    var row = await cursor.SingleAsync(record => record.Values["u"].As<INode>());

                    return DaoUtils.MapSimpleRow<User>(row);
                });
        }
    }

    public async Task<string> GetApplicantIdForUserName(string userName)
    {
        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteReadAsync(
                async tx =>
                {
                    var cursor = await tx.RunAsync(
                        "MATCH (u:User)-[:IS_USER_FOR]->(a:Applicant) " +
                        "WHERE u.userName = $username " +
                        "RETURN a.uuid",
                        new
                        {
                            username = userName
                        }
                    );

                    var row = await cursor.SingleAsync(record => record.Values["a.uuid"].As<string>());

                    return row;
                });
        }
    }
}
