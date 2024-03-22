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

using System.Runtime.InteropServices;
using matts.OrmLib.Statements;
using Nito.Collections;

namespace matts.OrmLib.Query;

public sealed class Neo4JQueryBuilder : IQueryBuilder<Neo4j.Driver.Query>
{
    private readonly Deque<IStatement> _deque;
    private readonly Dictionary<StatementType, int> _statementCount;
    private readonly StatementFactory _statementFactory;

    private Neo4JQueryBuilder(StatementFactory? withStatementFactory = null)
    {
        withStatementFactory ??= new StatementFactory();
        _statementFactory = withStatementFactory;
        _deque = new Deque<IStatement>();

        const int zero = 0;
        _statementCount = new Dictionary<StatementType, int>()
        {
            [StatementType.Match] = zero,
            [StatementType.Where] = zero,
            [StatementType.FinalStatement] = zero,
            [StatementType.PostFinalStatement] = zero,
        };
    }

    public static Neo4JQueryBuilder Builder() => new();
    internal static Neo4JQueryBuilder Builder(StatementFactory withStatementFactory) => new(withStatementFactory);

    public Neo4JQueryBuilder Match(object left)
    {
        return this;
    }

    public Neo4JQueryBuilder Match(object left, [Optional] object? right)
    {
        return this;
    }

    public Neo4JQueryBuilder Returns()
    {
        return this;
    }

    public Neo4JQueryBuilder OrderBy()
    {
        return this;
    }

    public Neo4j.Driver.Query Build()
    {
        throw new NotImplementedException();
    }
}
