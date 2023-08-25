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
using matts.Utils;

namespace matts.Interfaces;

public interface IDataAccessObject<T> where T : class
{
    public Task<T> CreateNew(T createWhat);

    public Task<bool> CreateRelationshipBetween(DbRelationship relationship, T source, object other, Type typeOther);

    public Task<bool> UpdateRelationshipBetween(DbRelationship relationship, T source, object other, Type typeOther);

    public Task<bool> DeleteRelationshipBetween(DbRelationship relationship, T source, object other, Type typeOther);

    public Task<List<T>> GetAll();

    public Task<List<T>> GetAllByRelationship(string relationship, string? optionalRelationship, string whomUuid);

    public Task<List<T>> GetAllAndFilterByProperties(IReadOnlyDictionary<string, object> filterProperties);

    public Task<T> GetByUuid(string uuid);

    public Task<bool> HasRelationshipBetween(DbRelationship relationship, T source, object other, Type typeOther);
}
