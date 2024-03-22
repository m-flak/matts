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
using System.Runtime.CompilerServices;

namespace matts.OrmLib.Attributes;

#pragma warning disable CA1813,CA1051

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public class DbFieldAttribute : Attribute
{
    // When implemented in a derived class, gets a unique identifier for this Attribute.
    protected Guid instanceGUID;

    public string? PropertyName { get; }
    public bool Special {  get; }

    public DbFieldAttribute(bool special = false, [CallerMemberName] string? propertyName = null)
    {
        Special = special;
        PropertyName = propertyName;
        this.instanceGUID = Guid.NewGuid();
    }

    public DbFieldAttribute(string? propertyName)
    {
        PropertyName = propertyName;
    }

    // Override TypeId to provide a unique identifier for the instance.
    public override object TypeId
    {
        get { return (object)instanceGUID; }
    }
}

#pragma warning restore CA1813,CA1051
