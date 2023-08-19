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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace matts.Utils;

public class DbRelationship
{
    public enum Cardinality
    {
        UNIDIRECTIONAL,
        BIDIRECTIONAL
    }

    public string Name { get; private set; }
    public string Selector { get; private set; }
    public Cardinality RelationshipCardinality { get; private set; }
    public IDictionary<string, object> Parameters { get; private set; }

    public DbRelationship(string name)
    {
        Name = name;
        Selector = "";
        RelationshipCardinality = Cardinality.UNIDIRECTIONAL;
        Parameters = new Dictionary<string, object>();
    }

    public DbRelationship(string name, string selector)
    {
        Name = name;
        Selector = selector;
        RelationshipCardinality = Cardinality.UNIDIRECTIONAL;
        Parameters = new Dictionary<string, object>();
    }

    public DbRelationship(string name, string selector, Cardinality cardinality)
    {
        Name = name;
        Selector = selector;
        RelationshipCardinality = cardinality;
        Parameters = new Dictionary<string, object>();
    }

    public override string ToString()
    {
        if (RelationshipCardinality == Cardinality.BIDIRECTIONAL)
        {
            return $"-[{Selector}:{Name} {CreateParametersString()}]-";
        }

        return $"-[{Selector}:{Name} {CreateParametersString()}]->";
    }

    public string ToString(bool omitCardinality)
    {
        if (!omitCardinality)
        {
            return ToString();
        }

        return $"[{Selector}:{Name} {CreateParametersString()}]";
    }

    private string CreateParametersString()
    {
        if (Parameters.Count < 1)
        {
            return "";
        }

        var keys = Parameters.Keys.ToList();
        var builder = new StringBuilder();

        builder.Append("{ ");
        for (int i = 0; i < keys.Count; ++i)
        {
            var value = Parameters[ keys[i] ];
            var type = value?.GetType();

            if (typeof(string).IsEquivalentTo(type))
            {
                Regex trailingSlashes = new(@"(?<=.+)/+");
                Regex quotes = new(@"'");
                string cleanValue = trailingSlashes.Replace( ((string?) value ?? ""), "");
                cleanValue = quotes.Replace(cleanValue, quote => quote.Value.Insert(0, "\\"));

                builder.Append($"{keys[i]}: '{cleanValue}'");
            }
            else if (typeof(bool).IsEquivalentTo(type))
            {
                builder.Append($"{keys[i]}: {value?.ToString()?.ToLower()}");
            }
            else if (typeof(DateTime).IsEquivalentTo(type))
            {
                var date = (DateTime?) value ?? DateTime.UtcNow;
                builder.Append($"{keys[i]}: '{date.ToUniversalTime().ToString("O")}'");
            }
            else
            {
                builder.Append($"{keys[i]}: {value}");
            }

            if (i + 1 != keys.Count)
            {
                builder.Append(", ");
            }
        }
        builder.Append(" }");

        return builder.ToString();
    }
}
