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

using matts.OrmLib.Parameters;

namespace matts.OrmLib.Statements;
public class FilterStatement : IStatement
{
    public StatementType Type { get; set; } = StatementType.FilterStatement;
    public long OrderMultiplier { get; set; }

    public FilterStatement(StatementType type, long orderMultiplier, IParameter[]? parameters = null)
    { 
        Type = type;
        OrderMultiplier = orderMultiplier;

        // Link Parameters
        if (parameters?.Length > 1)
        {
            int len = parameters.Length;
            for (var i = 0; i < len; ++i)
            {
                parameters[i].PreviousParameter = (i > 0) ? parameters[i - 1] : null;
                parameters[i].NextParameter = (i + 1 < len) ? parameters[i + 1] : null;
            }
        }
    }

    public void AppendParameter(IParameter parameter)
    {
        throw new NotImplementedException();
    }

    public int CompareTo(IStatement? other)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IParameter> GetParameters()
    {
        throw new NotImplementedException();
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        throw new NotImplementedException();
    }
}
