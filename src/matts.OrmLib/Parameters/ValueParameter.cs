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

namespace matts.OrmLib.Parameters;
public class ValueParameter : IParameter
{
    public IParameter? PreviousParameter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IParameter? NextParameter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string Selector { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public FieldNameValue Value { get; set; }

    public int CompareTo(IParameter? other)
    {
        throw new NotImplementedException();
    }

    public bool Equals(IParameter? other)
    {
        throw new NotImplementedException();
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IParameter> WalkNext()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IParameter> WalkPrevious()
    {
        throw new NotImplementedException();
    }
}
