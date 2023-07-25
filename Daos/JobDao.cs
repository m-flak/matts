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
using Mapster;
using System.Text;
using matts.Models;
using matts.Utils;

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
                        "MATCH (a:Applicant)-[r:HAS_APPLIED_TO]->(jj:Job) " +
                        "WHERE j.uuid = jj.uuid " +
                        "RETURN j, COUNT(a) as cA"
                    );
                    var rows = await cursor.ToListAsync(record => new Tuple<INode, long>(record.Values["j"].As<INode>(), record.Values["cA"].As<long>()));
                    return rows.Select(row =>
                        {
                            (var job, long applicantCount) = row;

                            TypeAdapterConfig<IReadOnlyDictionary<string, object>, JobDb>.NewConfig()
                                .NameMatchingStrategy(NameMatchingStrategy.FromCamelCase)
                                .Compile();

                            JobDb result = TypeAdapter.Adapt<IReadOnlyDictionary<string, object>, JobDb>(job.Properties);
                            result.ApplicantCount = applicantCount;
                            return result;
                        })
                        .ToList();
                });
        }
    }

    public async Task<List<JobDb>> GetAllByRelationship(string relationship, string? optionalRelationship, string whomUuid)
    {
        throw new NotImplementedException();
    }

    public async Task<List<JobDb>> GetAllAndFilterByProperties(IReadOnlyDictionary<string, object> filterProperties)
    {
        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteReadAsync(
                async tx =>
                {
                    var cursor = await tx.RunAsync(
                        "MATCH (j:Job) " +
                        $"WHERE {DaoUtils.CreateWhereClauseFromDict(filterProperties, "j")} " +
                        "RETURN j"
                    );
                    var rows = await cursor.ToListAsync(record => record.Values["j"].As<INode>());
                    return rows.Select(row => DaoUtils.MapSimpleRow<JobDb>(row))
                        .ToList();
                });
        }
    }


    public async Task<JobDb> GetByUuid(string uuid)
    {
        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteReadAsync(
                async tx =>
                {
                    var cursor = await tx.RunAsync(
                        "MATCH (j:Job) " +
                        "WHERE j.uuid = $juuid " +
                        "RETURN j",
                        new
                        {
                            juuid = uuid
                        }
                    );

                    var row = await cursor.SingleAsync(record => record.Values["j"].As<INode>());

                    return DaoUtils.MapSimpleRow<JobDb>(row);
                });
        }
    }

    public async Task<JobDb> CreateNew(JobDb createWhat)
    {
        throw new NotImplementedException();
    }
}
