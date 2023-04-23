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
using Neo4j.Driver;
using matts.Interfaces;
using matts.Models.Db;

namespace matts.Daos;

public class JobDao : IDataAccessObject<JobDb>
{
    private readonly IDriver _driver;

    public JobDao(IDriver driver)
    {
        _driver = driver;
    }

    public async Task<List<JobDb>> GetAll()
    {
        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteReadAsync(
                async tx =>
                {
                    var cursor = await tx.RunAsync(
                        "MATCH (j:Job) " +
                        "RETURN j"
                    );
                    var nodes = await cursor.ToListAsync(record => record.Values["j"].As<INode>());
                    return nodes.Select(node =>
                        {
                            // Why tf can't I use Mapster for this?
                            // It doesn't work :(
                            return new JobDb
                            {
                                Uuid = node.Properties["uuid"].As<string>(),
                                Name = node.Properties["name"].As<string>(),
                                Status = node.Properties["status"].As<string>()
                            };
                        })
                        .ToList();
                });
        }
    }

    public async Task<List<JobDb>> GetAllByRelationship(string relationship, string whomUuid)
    {
        throw new NotImplementedException();
    }
}
