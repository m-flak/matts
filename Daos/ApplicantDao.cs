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
                    var addOptionalParams = (string? optRel) => (optRel != null) ? AddReturnsForRelationshipParams(optRel, "r2") : "";

                    var cursor = await tx.RunAsync(
                        "MATCH(a: Applicant) -[r: " + $"{relationship}" +"]->(j: Job) " +
                        $"{CreateOptionalMatchClause(optionalRelationship)}" +
                        "WHERE j.uuid = $uuid " +
                        $"RETURN a {AddReturnsForRelationshipParams(relationship)} {addOptionalParams(optionalRelationship)}",
                        new
                        {
                            uuid = whomUuid
                        }
                    );

                    var rows = await cursor.ToListAsync(record => record.Values);
                    return rows.Select(row =>
                        {
                            IReadOnlyDictionary<string, object> applicantData;
                            string[]? relationshipParams = GetRelationshipParams(relationship);
                            string[]? optRelationshipParams = GetRelationshipParams(optionalRelationship);

                            if (relationshipParams != null || optRelationshipParams != null)
                            {
                                var applicant = new Dictionary<string, object>(row["a"].As<INode>().Properties);

                                if (relationshipParams != null)
                                {
                                    foreach (string param in relationshipParams)
                                    {
                                        var paramValue = row[$"r.{param}"];
                                        applicant.Add(param, paramValue);
                                    }
                                }

                                if (optRelationshipParams != null)
                                {
                                    foreach (string param in optRelationshipParams)
                                    {
                                        var paramValue = row[$"r2.{param}"];
                                        applicant.Add(param, paramValue);
                                    }
                                }

                                applicantData = applicant;
                            }
                            else
                            {
                                applicantData = row["a"].As<INode>().Properties;
                            }
                            

                            TypeAdapterConfig<IReadOnlyDictionary<string, object>, ApplicantDb>.NewConfig()
                                            .NameMatchingStrategy(NameMatchingStrategy.FromCamelCase)
                                            .Compile();

                            return TypeAdapter.Adapt<IReadOnlyDictionary<string, object>, ApplicantDb>(applicantData);
                        })
                        .ToList();
                });
        }
    }

    public async Task<List<ApplicantDb>> GetAllAndFilterByProperties(IReadOnlyDictionary<string, string> filterProperties)
    {
        throw new NotImplementedException();
    }

    public async Task<ApplicantDb> GetByUuid(string uuid)
    {
        throw new NotImplementedException();
    }

    internal static string[]? GetRelationshipParams(string? relationship)
    {
        string[]? relParams = null;

        switch (relationship)
        {
            case RelationshipConstants.HAS_APPLIED_TO:
                {
                    relParams = new string[]
                    {
                        "rejected"
                    };
                }
                return relParams;
            case RelationshipConstants.IS_INTERVIEWING_FOR:
                {
                    relParams = new string[]
                    {
                        "interviewDate"
                    };
                }
                return relParams;
            default:
                return relParams;
        }
    }

    internal static string AddReturnsForRelationshipParams(string? relationship)
    {
        return AddReturnsForRelationshipParams(relationship, "r");
    }

    internal static string AddReturnsForRelationshipParams(string? relationship, string prefix)
    {
        string returns = "";

        switch (relationship)
        {
            case RelationshipConstants.HAS_APPLIED_TO:
            case RelationshipConstants.IS_INTERVIEWING_FOR:
                {
                    var builder = new StringBuilder();

                    foreach (string relParam in GetRelationshipParams(relationship) ?? Enumerable.Empty<string>())
                    {
                        builder.AppendFormat(", {0}.{1}", prefix, relParam);
                    }

                    builder.Append(" ");
                    returns = builder.ToString();
                }
                return returns;
            
            default:
                return returns;
        }
    }

    internal static string CreateOptionalMatchClause(string? optionalRelationship)
    {
        string clause = "";

        if (optionalRelationship != null)
        {
            clause = $"OPTIONAL MATCH (a)-[r2:{optionalRelationship}]->(j) ";
        }

        return clause;
    }
}
