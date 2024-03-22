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
using matts.OrmLib.Statements;

namespace matts.OrmLib.Query;

internal class QueryTreeNode : IComparable<QueryTreeNode>, IEquatable<QueryTreeNode>
{
    public QueryTreeNode? Left { get; set; }
    public QueryTreeNode? Right { get; set; }
    public IStatement? Value { get; set; }
    public long Order { get; set; }
    public bool IsLeaf
    {
        get
        {
            return Left is null && Right is null;
        }
    }

    public int CompareTo(QueryTreeNode? other)
    {
        if (other is QueryTreeNode quack)
        {
            return Order.CompareTo(quack.Order);
        }

        throw new ArgumentException(Properties.Resources.QUERY_TREE_NODE_EX1, nameof(other));
    }

    bool IEquatable<QueryTreeNode>.Equals(QueryTreeNode? other)
    {
        return Equals(other);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not QueryTreeNode quack)
        {
            return base.Equals(obj);
        }
        return this.Equals(quack);
    }

    public override int GetHashCode()
    {
        int dank = 0;
        dank += RuntimeHelpers.GetHashCode(Left);
        dank += RuntimeHelpers.GetHashCode(Right);
        dank ^= RuntimeHelpers.GetHashCode(Value);
        return dank;
    }

    public static bool operator > (QueryTreeNode? left, QueryTreeNode? right)
    {
        return left?.Order > right?.Order;
    }

    public static bool operator < (QueryTreeNode? left, QueryTreeNode? right)
    {
        return left?.Order < right?.Order;
    }

    public static bool operator >= (QueryTreeNode? left, QueryTreeNode? right)
    {
        return left?.Order >= right?.Order;
    }

    public static bool operator <= (QueryTreeNode? left, QueryTreeNode? right)
    {
        return left?.Order <= right?.Order;
    }

    public static bool operator == (QueryTreeNode? left, QueryTreeNode? right)
    {
        return RuntimeHelpers.Equals(left, right);
    }

    public static bool operator != (QueryTreeNode? left, QueryTreeNode? right)
    {
        return !RuntimeHelpers.Equals(left, right);
    }
}
