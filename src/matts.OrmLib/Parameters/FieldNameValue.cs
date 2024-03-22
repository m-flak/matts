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

public readonly struct FieldNameValue : IEquatable<FieldNameValue>
{
    public string Name { get; init; }
    public object Value { get; init; }
    public Type ValueType { get; init; }

    public FieldNameValue(string name, object value)
    {
        Name = name;
        Value = value ?? throw new ArgumentNullException(nameof(value));
        ValueType = value.GetType();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not FieldNameValue fieldNameValue)
        {
            return false;
        }

        return Name.Equals(fieldNameValue.Name, StringComparison.Ordinal)
            && Value.Equals(fieldNameValue.Value)
            && ValueType.Equals(fieldNameValue.ValueType);
    }

    public override int GetHashCode()
    {
        var code = Name.GetHashCode(StringComparison.Ordinal) ^ Value.GetHashCode();
        code ^= BitConverter.ToInt16(BitConverter.GetBytes(ValueType.GetHashCode()), 0);
        return code;
    }

    public static bool operator ==(FieldNameValue left, FieldNameValue right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(FieldNameValue left, FieldNameValue right)
    {
        return !(left == right);
    }

    bool IEquatable<FieldNameValue>.Equals(FieldNameValue other)
    {
        return Equals(other);
    }
}
