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
using matts.Constants;

namespace matts.Daos;

public class JobDao : DaoAbstractBase<JobDb>
{
    public JobDao(IDriver driver) : base(driver)
    {
    }

    public override async Task<JobDb> CreateNew(JobDb createWhat)
    {
        JobDb createWhatCopy = new JobDb(createWhat);
        createWhatCopy.Uuid = System.Guid.NewGuid().ToString();
        return await this.CreateNewImpl(createWhatCopy);
    }

    public override async Task<bool> CreateRelationshipBetween(DbRelationship relationship, JobDb source, object other, Type typeOther)
    {
        return await this.CreateRelationshipBetweenImpl(relationship, source, other, typeof(JobDb), typeOther);
    }

    public override async Task<bool> UpdateRelationshipBetween(DbRelationship relationship, JobDb source, object other, Type typeOther)
    {
        return await this.UpdateRelationshipBetweenImpl(relationship, source, other, typeof(JobDb), typeOther);
    }

    public override async Task<List<JobDb>> GetAll()
    {
        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteReadAsync(
                async tx =>
                {
                    var cursor = await tx.RunAsync(
                        "MATCH (j:Job) " +
                        "OPTIONAL MATCH (a:Applicant)-[r:HAS_APPLIED_TO]->(jj:Job) " +
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

    public override async Task<List<JobDb>> GetAllAndFilterByProperties(IReadOnlyDictionary<string, object> filterProperties)
    {
        return await this.GetAllAndFilterByPropertiesImpl(typeof(JobDb), filterProperties);
    }

    public override async Task<List<JobDb>> GetAllByRelationship(string relationship, string? optionalRelationship, string whomUuid)
    {
        // This relationship is applicant --> job, so use different node settings
        if (relationship == RelationshipConstants.HAS_APPLIED_TO)
        {
            return await GetAllByRelationshipImpl(
                typeof(ApplicantDb), 
                typeof(JobDb),
                new GetAllByRelationshipConfig(
                    GetAllByRelationshipConfig.WhereNodeSelector.LEFT,
                    GetAllByRelationshipConfig.ReturnNodeSelector.RIGHT
                ),
                new DbRelationship(relationship, "r"),
                (optionalRelationship != null) ? new DbRelationship(optionalRelationship, "r2") : null,
                whomUuid
            );
        }

        throw new NotImplementedException();
    }

    public override async Task<JobDb> GetByUuid(string uuid)
    {
        return await this.GetByUuidImpl(typeof(JobDb), uuid);
    }

    public override async Task<bool> HasRelationshipBetween(DbRelationship relationship, JobDb source, object other, Type typeOther)
    {
        return await this.HasRelationshipBetweenImpl(relationship, source, other, typeof(JobDb), typeOther);
    }
}
