﻿/* matts
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

    public async Task<List<ApplicantDb>> GetAllByRelationship(string relationship, string? optionalRelationship, string whomUuid)
    {
        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteReadAsync(
                async tx =>
                {
                    var addOptionalParams = (string? optRel) => (optRel != null) ? DaoUtils.AddReturnsForRelationshipParams(optRel, "r2") : "";

                    var cursor = await tx.RunAsync(
                        "MATCH(a: Applicant) -[r: " + $"{relationship}" +"]->(j: Job) " +
                        $"{DaoUtils.CreateOptionalMatchClause(optionalRelationship, "a", "j")}" +
                        "WHERE j.uuid = $uuid " +
                        $"RETURN a {DaoUtils.AddReturnsForRelationshipParams(relationship, "r")} {addOptionalParams(optionalRelationship)}",
                        new
                        {
                            uuid = whomUuid
                        }
                    );

                    var rows = await cursor.ToListAsync(record => record.Values);
                    return rows.Select(row => DaoUtils.MapRowWithRelationships<ApplicantDb>(row, "a", relationship, optionalRelationship, "r", "r2"))
                        .ToList();
                });
        }
    }

    public async Task<List<ApplicantDb>> GetAllAndFilterByProperties(IReadOnlyDictionary<string, object> filterProperties)
    {
        throw new NotImplementedException();
    }

    public async Task<ApplicantDb> GetByUuid(string uuid)
    {
        throw new NotImplementedException();
    }

    public async Task<ApplicantDb> CreateNew(ApplicantDb createWhat)
    {
        throw new NotImplementedException();
    }
}
