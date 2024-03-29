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
namespace matts.Utils;

using Mapster;
using matts.Constants;
using matts.Models.Db;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;

internal sealed class DaoUtils
{
    internal static T MapRowWithRelationships<T>(IReadOnlyDictionary<string, object> row, string tKey, string relationship, string? optionalRelationship, string relPrefix, string? optRelPrefix)
    {
        IReadOnlyDictionary<string, object> rowData;
        string[]? relationshipParams = GetRelationshipParams(relationship);
        string[]? optRelationshipParams = GetRelationshipParams(optionalRelationship);

        if (relationshipParams != null || optRelationshipParams != null)
        {
            var t = new Dictionary<string, object>(row[tKey].As<INode>().Properties);

            if (relationshipParams != null)
            {
                foreach (string param in relationshipParams)
                {
                    try
                    {
                        var paramValue = row[$"{relPrefix}.{param}"];
                        t.Add(param, paramValue);
                    }
                    catch (KeyNotFoundException)
                    {
                        continue;
                    }
                }
            }

            if (optRelationshipParams != null)
            {
                foreach (string param in optRelationshipParams)
                {
                    try
                    {
                        var paramValue = row[$"{optRelPrefix}.{param}"];
                        t.Add(param, paramValue);
                    }
                    catch (KeyNotFoundException)
                    {
                        continue;
                    }
                }
            }

            rowData = t;
        }
        else
        {
            rowData = row[tKey].As<INode>().Properties;
        }

        TypeAdapterConfig<IReadOnlyDictionary<string, object>, T>.NewConfig()
                        .NameMatchingStrategy(NameMatchingStrategy.FromCamelCase)
                        .Compile();

        return rowData.Adapt<IReadOnlyDictionary<string, object>, T>();
    }

    internal static T MapSimpleRow<T>(INode rowNode)
    {
        TypeAdapterConfig<IReadOnlyDictionary<string, object>, T>.NewConfig()
            .NameMatchingStrategy(NameMatchingStrategy.FromCamelCase)
            .Compile();

        T result = rowNode.Properties.Adapt<IReadOnlyDictionary<string, object>, T>();
        return result;
    }

    internal static string CreateWhereClauseFromDict(IReadOnlyDictionary<string, object> filterProperties, string prefix)
    {
        var keys = filterProperties.Keys.ToList();
        var builder = new StringBuilder();

        builder.Append("( ");
        for (int i = 0; i < keys.Count; ++i)
        {
            var value = filterProperties.GetValueOrDefault(keys[i]);
            var type = value?.GetType();

            if (typeof(string).IsEquivalentTo(type))
            {
                Regex trailingSlashes = new("(?<=.+)/+");
                Regex quotes = new("'");
                string cleanValue = trailingSlashes.Replace( ((string?) value ?? ""), "");
                cleanValue = quotes.Replace(cleanValue, quote => quote.Value.Insert(0, "\\"));

                builder.Append(prefix).Append('.').Append(keys[i]).Append(" = '").Append(cleanValue).Append('\'');
            }
            else if (typeof(bool).IsEquivalentTo(type))
            {
                builder.Append(prefix).Append('.').Append(keys[i]).Append(" = ").Append(value?.ToString()?.ToLower());
            }
            else
            {
                builder.Append(prefix).Append('.').Append(keys[i]).Append(" = ").Append(value);
            }

            if (i + 1 != keys.Count)
            {
                builder.Append(" AND ");
            }
        }
        builder.Append(" )");

        return builder.ToString();
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
            case RelationshipConstants.IS_USER_FOR:
            default:
                return relParams;
        }
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
            case RelationshipConstants.IS_USER_FOR:
            default:
                return returns;
        }
    }

    internal static string CreateOptionalMatchClause(string? optionalRelationship, string? lhSymbol, string? rhSymbol)
    {
        string clause = "";

        if (optionalRelationship != null && lhSymbol != null && rhSymbol != null)
        {
            clause = $"OPTIONAL MATCH ({lhSymbol})-[r2:{optionalRelationship}]->({rhSymbol}) ";
        }

        return clause;
    }

    internal static IDictionary<string, object> CreateRunAsyncParameters<T>(T fromNode)
    {
        Type typeNode = typeof(T);
        var parameters = new Dictionary<string, object>();

        var properties = typeNode.GetProperties().Where(p => p.GetCustomAttribute(typeof(DbNodeCreationFieldAttribute)) != null).ToArray();
        foreach (PropertyInfo prop in properties)
        {
            parameters.Add(
                System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(prop.Name),
                prop.GetValue(fromNode)!
            );
        }

        return parameters;
    }

    internal static string CreateCreationParameterString(IDictionary<string, object> parameters)
    {
        var keys = parameters.Keys.ToList();
        var builder = new StringBuilder();

        builder.Append("{ ");
        for (int i = 0; i < keys.Count; ++i)
        {
            builder.Append(keys[i]).Append(": $").Append(keys[i]);

            if (i + 1 != keys.Count)
            {
                builder.Append(", ");
            }
        }
        builder.Append(" }");

        return builder.ToString();
    }

    internal static string CreateSetStatements(IDictionary<string, object> parameters, string prefix)
    {
        var keys = parameters.Keys.ToList();
        var builder = new StringBuilder();

        for (int i = 0; i < keys.Count; ++i)
        {
            builder.Append("SET ").Append(prefix).Append('.').Append(keys[i]).Append(" = $").Append(keys[i]).Append(' ');
        }

        return builder.ToString();
    }

    // class only holds static methods
    private DaoUtils() { }
}
