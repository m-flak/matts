/* matts
 * "Matthew's ATS OrmLib" - Portfolio Project
 * Copyright (C) 2023-2024  Matthew E. Kehrer <matthew@kehrer.dev>
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
using EnumsNET;
using matts.OrmLib.Attributes;
using matts.OrmLib.Parameters;

namespace matts.OrmLib.Statements;

internal class StatementFactory
{
    private readonly Dictionary<StatementType, int> _multiplierCache = new();

    internal IStatement CreateStatement(StatementType type, IParameter[]? parameters)
    {
        if (type == StatementType.FilterStatement
            || type == StatementType.FinalStatement
            || type == StatementType.PostFinalStatement)
        {
            // Generic value exception
            throw new ArgumentException(Properties.Resources.STATEMENT_FACTORY_EX1, nameof(type));
        }

        if (!_multiplierCache.TryGetValue(type, out var multi))
        {
            multi = type.GetMember()
                        ?.Attributes
                        .Get<TreeOrderMultiplierAttribute>()
                        ?.Value
                    ?? throw new ArgumentException(Properties.Resources.STATEMENT_FACTORY_EX2, nameof(type));

            _multiplierCache[type] = multi;
        }
        
        return type switch
        {
            StatementType.Match => new FilterStatement(type, multi, parameters),
            StatementType.Where => new FilterStatement(type, multi, parameters),
            StatementType.Return => new FinalStatement(type, multi, parameters),
            StatementType.Create => new FinalStatement(type, multi, parameters),
            StatementType.Set => new FinalStatement(type, multi, parameters),
            StatementType.Delete => new FinalStatement(type, multi, parameters),
            StatementType.OrderBy => new FinalStatement(type, multi, parameters),
            _ => throw new InvalidOperationException(Properties.Resources.STATEMENT_FACTORY_EX3) // Unsupported!?
        };
    }
}
