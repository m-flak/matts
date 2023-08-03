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
using matts.Constants;
using matts.Interfaces;
using matts.Models;
using matts.Models.Db;
using matts.Utils;
using Neo4j.Driver;
using System;
using System.Text;

namespace matts.Daos;

public class ApplicantDao : DaoAbstractBase<ApplicantDb>
{
    public ApplicantDao(IDriver driver) : base(driver)
    {
    }

    public override async Task<List<ApplicantDb>> GetAll()
    {
        return await this.GetAllImpl(typeof(ApplicantDb));
    }

    public override async Task<List<ApplicantDb>> GetAllByRelationship(string relationship, string? optionalRelationship, string whomUuid)
    {
        return await this.GetAllByRelationshipImpl(
            typeof(ApplicantDb), 
            typeof(JobDb), 
            new GetAllByRelationshipConfig(
                GetAllByRelationshipConfig.WhereNodeSelctor.RIGHT, 
                GetAllByRelationshipConfig.ReturnNodeSelector.LEFT
            ), 
            relationship, 
            optionalRelationship, 
            whomUuid
        );
    }

    public override async Task<List<ApplicantDb>> GetAllAndFilterByProperties(IReadOnlyDictionary<string, object> filterProperties)
    {
        return await this.GetAllAndFilterByPropertiesImpl(typeof(ApplicantDb), filterProperties);
    }

    public override async Task<ApplicantDb> GetByUuid(string uuid)
    {
        return await this.GetByUuidImpl(typeof(ApplicantDb), uuid);
    }

    public override async Task<ApplicantDb> CreateNew(ApplicantDb createWhat)
    {
        bool created = false;
        string newUuid = System.Guid.NewGuid().ToString();
        using (var session = _driver.AsyncSession())
        {
            created = await session.ExecuteWriteAsync(
               async tx =>
               {
                   var cursor = await tx.RunAsync(
                       "CREATE (a: Applicant { uuid: $appid, name: $appname, email: $email, phoneNumber: $phoneNumber })",
                       new
                       {
                           appid = newUuid,
                           appname = createWhat.Name,
                           email = createWhat.Email,
                           phoneNumber = createWhat.PhoneNumber
                       }
                   );

                   var result = await cursor.ConsumeAsync();
                   return result.Counters.NodesCreated == 1;
               });
        }

        if (!created)
        {
            throw new InvalidOperationException("Unable to create the applicant in the database!");
        }

        return await GetByUuid(newUuid);
    }
}
