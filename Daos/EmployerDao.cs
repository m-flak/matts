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
using Neo4j.Driver;
using matts.Interfaces;
using matts.Models.Db;
using Mapster;
using System.Text;
using matts.Models;
using matts.Utils;
using matts.Constants;

namespace matts.Daos;

public class EmployerDao : DaoAbstractBase<EmployerDb>
{
    public EmployerDao(IDriver driver) : base(driver)
    {
    }

    public override async Task<EmployerDb> CreateNew(EmployerDb createWhat)
    {
        EmployerDb createWhatCopy = new EmployerDb(createWhat);
        createWhatCopy.Uuid = System.Guid.NewGuid().ToString();
        return await this.CreateNewImpl(createWhatCopy);
    }

    public override async Task<bool> CreateRelationshipBetween(DbRelationship relationship, EmployerDb source, object other, Type typeOther)
    {
        return await this.CreateRelationshipBetweenImpl(relationship, source, other, typeof(EmployerDb), typeOther);
    }

    public override async Task<bool> UpdateRelationshipBetween(DbRelationship relationship, EmployerDb source, object other, Type typeOther)
    {
        return await this.UpdateRelationshipBetweenImpl(relationship, source, other, typeof(EmployerDb), typeOther);
    }

    public override async Task<bool> DeleteRelationshipBetween(DbRelationship relationship, EmployerDb? source, object? other, Type typeOther)
    {
        return await this.DeleteRelationshipBetweenImpl(relationship, source, other, typeof(EmployerDb), typeOther);
    }

    public override async Task<List<EmployerDb>> GetAll()
    {
        return await this.GetAllImpl(typeof(EmployerDb), null);
    }

    public override async Task<List<EmployerDb>> GetAllAndFilterByProperties(IReadOnlyDictionary<string, object> filterProperties)
    {
        return await this.GetAllAndFilterByPropertiesImpl(typeof(EmployerDb), filterProperties, null);
    }

    public override async Task<List<EmployerDb>> GetAllByRelationship(string relationship, string? optionalRelationship, string whomUuid)
    {
        return await this.GetAllByRelationshipImpl(
            typeof(EmployerDb),
            typeof(ApplicantDb),
            new GetAllByRelationshipConfig(
                GetAllByRelationshipConfig.WhereNodeSelector.RIGHT,
                GetAllByRelationshipConfig.ReturnNodeSelector.LEFT
            ),
            new DbRelationship(relationship, "r"),
            (optionalRelationship != null) ? new DbRelationship(optionalRelationship, "r2") : null,
            whomUuid,
            null
        );
    }

    public override async Task<EmployerDb> GetByUuid(string uuid)
    {
        return await this.GetByUuidImpl(typeof(EmployerDb), uuid);
    }

    public override async Task<List<P>> GetPropertyFromRelated<P>(string relationship, Type relatedNodeType, string propertyName)
    {
        return await this.GetPropertyFromRelatedImpl<P>(relationship, typeof(EmployerDb), relatedNodeType, propertyName, null);
    }

    public override async Task<bool> HasRelationshipBetween(DbRelationship relationship, EmployerDb source, object other, Type typeOther)
    {
        return await this.HasRelationshipBetweenImpl(relationship, source, other, typeof(EmployerDb), typeOther);
    }
}
