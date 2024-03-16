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

internal class QueryTree
{
    public QueryTreeNode Root { get; init; }

    protected QueryTree(QueryTreeNode root)
    {
        Root = root;
    }

    public static QueryTree MakeQueryTree(ArraySegment<IStatement> sortedIn)
    {
        var stub = new QueryTreeNode();
        return new QueryTree(stub);
    }
}
