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

using matts.OrmLib.Statements;

namespace matts.OrmLib.Query;

internal class QueryTreeNode : IComparable<QueryTreeNode>, IEquatable<QueryTreeNode>
{
    public QueryTreeNode? Left { get; set; }
    public QueryTreeNode? Right { get; set; }
    public IStatement? Value { get; set; }
    public bool IsLeaf
    {
        get
        {
            return Left is null && Right is null;
        }
    }

    public int CompareTo(QueryTreeNode? other)
    {
        throw new NotImplementedException();
    }

    public bool Equals(QueryTreeNode? other)
    {
        throw new NotImplementedException();
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as QueryTreeNode);
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }

    public static bool operator > (QueryTreeNode? left, QueryTreeNode? right)
    {
        throw new NotImplementedException();
    }

    public static bool operator < (QueryTreeNode? left, QueryTreeNode? right)
    {
        throw new NotImplementedException();
    }

    public static bool operator >= (QueryTreeNode? left, QueryTreeNode? right)
    {
        throw new NotImplementedException();
    }

    public static bool operator <= (QueryTreeNode? left, QueryTreeNode? right)
    {
        throw new NotImplementedException();
    }

    public static bool operator == (QueryTreeNode? left, QueryTreeNode? right)
    {
        throw new NotImplementedException();
    }

    public static bool operator != (QueryTreeNode? left, QueryTreeNode? right)
    {
        throw new NotImplementedException();
    }
}
