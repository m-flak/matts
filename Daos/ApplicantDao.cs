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
using Mapster;
using matts.Interfaces;
using matts.Models;
using matts.Models.Db;
using Neo4j.Driver;
using System;

namespace matts.Daos;

public class ApplicantDao : IDataAccessObject<ApplicantDb>
{
    private readonly IDriver _driver;

    public ApplicantDao(IDriver driver)
    {
        _driver = driver;
    }

    public async Task<List<ApplicantDb>> GetAll()
    {
        throw new NotImplementedException();
    }

    public async Task<List<ApplicantDb>> GetAllByRelationship(string relationship, string whomUuid)
    {
        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteReadAsync(
                async tx =>
                {
                    var cursor = await tx.RunAsync(
                        "MATCH(a: Applicant) -[r: $relation]->(j: Job) " +
                        "WHERE j.uuid = $uuid " +
                        "RETURN a ",
                        new
                        {
                            relation = relationship,
                            uuid = whomUuid
                        }
                    );

                    var rows = await cursor.ToListAsync(record => record.Values["a"].As<INode>());
                    return rows.Select(row =>
                        {
                            TypeAdapterConfig<IReadOnlyDictionary<string, object>, ApplicantDb>.NewConfig()
                                        .NameMatchingStrategy(NameMatchingStrategy.FromCamelCase)
                                        .Compile();

                            return TypeAdapter.Adapt<IReadOnlyDictionary<string, object>, ApplicantDb>(row.Properties);
                        })
                        .ToList();
                });
        }
    }
}
