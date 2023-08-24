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
using System.Reflection;
using Neo4j.Driver;
using matts.Interfaces;
using matts.Utils;

namespace matts.Daos;

public abstract class DaoAbstractBase<T> : IDataAccessObject<T> where T : class
{
    public abstract Task<T> CreateNew(T createWhat);
    public abstract Task<bool> CreateRelationshipBetween(DbRelationship relationship, T source, object other, Type typeOther);
    public abstract Task<List<T>> GetAll();
    public abstract Task<List<T>> GetAllAndFilterByProperties(IReadOnlyDictionary<string, object> filterProperties);
    public abstract Task<List<T>> GetAllByRelationship(string relationship, string? optionalRelationship, string whomUuid);
    public abstract Task<T> GetByUuid(string uuid);
    public abstract Task<bool> HasRelationshipBetween(DbRelationship relationship, T source, object other, Type typeOther);

    // ////////////////////////////////////////////////////////////////////////

    internal Neo4JWrappers Wrappers { get; set; }

    protected readonly IDriver _driver;

    protected DaoAbstractBase(IDriver driver)
    {
        _driver = driver;
        Wrappers = new Neo4JWrappers();
    }

    protected readonly struct GetAllByRelationshipConfig
    {
        public enum WhereNodeSelector
        {
            LEFT,
            RIGHT
        }
        public enum ReturnNodeSelector
        {
            LEFT,
            RIGHT
        }

        public GetAllByRelationshipConfig(WhereNodeSelector whereSelector, ReturnNodeSelector returnSelector)
        {
            WhereSelector = whereSelector;
            ReturnSelector = returnSelector;
        }

        public WhereNodeSelector WhereSelector { get; init; }
        public ReturnNodeSelector ReturnSelector  { get; init; }
    }

    protected async Task<List<T>> GetAllByRelationshipImpl(Type lhNode, Type rhNode, GetAllByRelationshipConfig config, DbRelationship relationship, DbRelationship? optionalRelationship, string whomUuid)
    {
        var lhAttr = Attribute.GetCustomAttribute(lhNode, typeof(DbNodeAttribute)) as DbNodeAttribute;
        var rhAttr = Attribute.GetCustomAttribute(rhNode, typeof(DbNodeAttribute)) as DbNodeAttribute;

        if (lhAttr == null || rhAttr == null)
        {
            throw new MissingMemberException("Unable to find the DbNodeAttribute that should be attached to `lhNode` or `rhNode`!");
        }

        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteReadAsync(
                async tx =>
                {
                    var addOptionalParams = (DbRelationship? optRel) => (optRel != null) ? DaoUtils.AddReturnsForRelationshipParams(optRel.Name, optRel.Selector) : "";

                    string whereSelector = (config.WhereSelector == GetAllByRelationshipConfig.WhereNodeSelector.LEFT) ? lhAttr.Selector : rhAttr.Selector;
                    string returnSelector = (config.ReturnSelector == GetAllByRelationshipConfig.ReturnNodeSelector.LEFT) ? lhAttr.Selector : rhAttr.Selector;

                    var cursor = await tx.RunAsync(
                        $"MATCH({lhAttr.Selector}: {lhAttr.Node}){relationship}({rhAttr.Selector}: {rhAttr.Node}) " +
                        $"WHERE {whereSelector}.uuid = $uuid " +
                        $"{DaoUtils.CreateOptionalMatchClause(optionalRelationship?.Name, lhAttr.Selector, rhAttr.Selector)}" +
                        $"RETURN {returnSelector} {DaoUtils.AddReturnsForRelationshipParams(relationship.Name, relationship.Selector)} {addOptionalParams(optionalRelationship)}",
                        new
                        {
                            uuid = whomUuid
                        }
                    );

                    var rows = await Wrappers.RunToListAsync(cursor, record => record.Values);
                    
                    return rows.Select(row =>
                        DaoUtils.MapRowWithRelationships<T>(
                            row,
                            returnSelector,
                            relationship.Name,
                            optionalRelationship?.Name,
                            relationship.Selector,
                            optionalRelationship?.Selector
                        ))
                        .ToList();
                });
        }
    }

    protected async Task<List<T>> GetAllAndFilterByPropertiesImpl(Type node, IReadOnlyDictionary<string, object> filterProperties)
    {
        var nodeAttr = Attribute.GetCustomAttribute(node, typeof(DbNodeAttribute)) as DbNodeAttribute;

        if (nodeAttr == null)
        {
            throw new MissingMemberException("Unable to find the DbNodeAttribute that should be attached to `node`!");
        }

        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteReadAsync(
                async tx =>
                {
                    var cursor = await tx.RunAsync(
                        $"MATCH ({nodeAttr.Selector}: {nodeAttr.Node}) " +
                        $"WHERE {DaoUtils.CreateWhereClauseFromDict(filterProperties, nodeAttr.Selector)} " +
                        $"RETURN {nodeAttr.Selector}"
                    );
                    var rows = await Wrappers.RunToListAsync(cursor, record => record.Values[nodeAttr.Selector].As<INode>());
                    return rows.Select(row => DaoUtils.MapSimpleRow<T>(row))
                        .ToList();
                });
        }
    }

    protected async Task<List<T>> GetAllImpl(Type node)
    {
        var nodeAttr = Attribute.GetCustomAttribute(node, typeof(DbNodeAttribute)) as DbNodeAttribute;

        if (nodeAttr == null)
        {
            throw new MissingMemberException("Unable to find the DbNodeAttribute that should be attached to `node`!");
        }

        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteReadAsync(
                async tx =>
                {
                    var cursor = await tx.RunAsync(
                        $"MATCH ({nodeAttr.Selector}: {nodeAttr.Node}) " +
                        $"RETURN {nodeAttr.Selector}"
                    );
                    var rows = await Wrappers.RunToListAsync(cursor, record => record.Values[nodeAttr.Selector].As<INode>());
                    return rows.Select(row => DaoUtils.MapSimpleRow<T>(row))
                        .ToList();
                });
        }
    }

    protected async Task<T> GetByUuidImpl(Type node, string uuid)
    {
        var nodeAttr = Attribute.GetCustomAttribute(node, typeof(DbNodeAttribute)) as DbNodeAttribute;

        if (nodeAttr == null)
        {
            throw new MissingMemberException("Unable to find the DbNodeAttribute that should be attached to `node`!");
        }

        PropertyInfo? uuidInfo = null;
        string? uuidFieldName = null;
        try 
        {
            uuidInfo = node.GetProperties().Where(p => p.GetCustomAttribute(typeof(DbNodeUuidAttribute)) != null).Single();
            uuidFieldName = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(uuidInfo.Name);
        }
        catch (InvalidOperationException ioe)
        {
            throw new MissingMemberException("Unable to find the property with DbNodeUuidAttribute within `node`!", ioe);
        }

        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteReadAsync(
                async tx =>
                {
                    var cursor = await tx.RunAsync(
                        $"MATCH ({nodeAttr.Selector}: {nodeAttr.Node}) " +
                        $"WHERE {nodeAttr.Selector}.{uuidFieldName} = $nuuid " +
                        $"RETURN {nodeAttr.Selector}",
                        new
                        {
                            nuuid = uuid
                        }
                    );

                    var row = await Wrappers.RunSingleAsync(cursor, record => record.Values[nodeAttr.Selector].As<INode>());

                    return DaoUtils.MapSimpleRow<T>(row);
                });
        }
    }

    protected async Task<T> CreateNewImpl(T createWhat)
    {
        Type createWhatType = typeof(T);
        var nodeAttr = Attribute.GetCustomAttribute(createWhatType, typeof(DbNodeAttribute)) as DbNodeAttribute;

        if (nodeAttr == null)
        {
            throw new MissingMemberException("Unable to find the DbNodeAttribute that should be attached to `createWhat`!");
        }

        string? uuid = null;
        try 
        {
            var uuidInfo = createWhatType.GetProperties().Where(p => p.GetCustomAttribute(typeof(DbNodeUuidAttribute)) != null).Single();
            uuid = (string?) uuidInfo.GetValue(createWhat);

            if (uuid == null)
            {
                throw new MissingMemberException("Unable to get the value of the property with DbNodeUuidAttribute within `createWhat`!");
            }
        }
        catch (InvalidOperationException ioe)
        {
            throw new MissingMemberException("Unable to find the property with DbNodeUuidAttribute within `createWhat`!", ioe);
        }

        bool created = false;
        using (var session = _driver.AsyncSession())
        {
            created = await session.ExecuteWriteAsync(
               async tx =>
               {
                   var parameters = DaoUtils.CreateRunAsyncParameters<T>(createWhat);
                   var cursor = await tx.RunAsync(
                       $"CREATE ({nodeAttr.Selector}: {nodeAttr.Node} {DaoUtils.CreateCreationParameterString(parameters)} )",
                       parameters
                   );

                   var result = await cursor.ConsumeAsync();
                   return result.Counters.NodesCreated == 1;
               });
        }

        if (!created)
        {
            throw new InvalidOperationException($"Unable to create the {nodeAttr.Node} in the database!");
        }

        return await GetByUuid(uuid);
    }

    protected async Task<bool> CreateRelationshipBetweenImpl(DbRelationship relationship, object src, object dest, Type typeSrc, Type typeDest)
    {
        var nodeAttrSrc = Attribute.GetCustomAttribute(typeSrc, typeof(DbNodeAttribute)) as DbNodeAttribute;
        var nodeAttrDest = Attribute.GetCustomAttribute(typeDest, typeof(DbNodeAttribute)) as DbNodeAttribute;

        if (nodeAttrSrc == null || nodeAttrDest == null)
        {
            throw new MissingMemberException("Unable to find the DbNodeAttribute that should be attached to `src` and/or `dest`!");
        }

        string? uuidSrc = null;
        string? uuidSrcName = null;
        string? uuidDest = null;
        string? uuidDestName = null;
        try 
        {
            var uuidSrcInfo = typeSrc.GetProperties().Where(p => p.GetCustomAttribute(typeof(DbNodeUuidAttribute)) != null).Single();
            uuidSrc = (string?) uuidSrcInfo.GetValue(src);
            uuidSrcName = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(uuidSrcInfo.Name);

            var uuidDestInfo = typeDest.GetProperties().Where(p => p.GetCustomAttribute(typeof(DbNodeUuidAttribute)) != null).Single();
            uuidDest = (string?) uuidDestInfo.GetValue(dest);
            uuidDestName = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(uuidDestInfo.Name);

            if (uuidSrc == null || uuidSrcName == null)
            {
                throw new MissingMemberException("Unable to get the value of the property with DbNodeUuidAttribute within `src`!");
            }
            if (uuidDest == null || uuidDestName == null)
            {
                throw new MissingMemberException("Unable to get the value of the property with DbNodeUuidAttribute within `dest`!");
            }
        }
        catch (InvalidOperationException ioe)
        {
            throw new MissingMemberException("Unable to find the property with DbNodeUuidAttribute within `src` and/or `dest`!", ioe);
        }

        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteWriteAsync(
               async tx =>
               {
                   var cursor = await tx.RunAsync(
                       $"MATCH ({nodeAttrSrc.Selector}:{nodeAttrSrc.Node}" + " { " + $"{uuidSrcName}: $uuid1" + " }) " +
                       $"MATCH ({nodeAttrDest.Selector}:{nodeAttrDest.Node}" + " { " + $"{uuidDestName}: $uuid2" + " }) "  +
                       $"CREATE ({nodeAttrSrc.Selector}){relationship}({nodeAttrDest.Selector})",
                       new
                       {
                           uuid1 = uuidSrc,
                           uuid2 = uuidDest
                       }
                   );

                   var result = await cursor.ConsumeAsync();
                   return result.Counters.RelationshipsCreated == 1;
               });
        }
    }

    protected async Task<bool> HasRelationshipBetweenImpl(DbRelationship relationship, object src, object dest, Type typeSrc, Type typeDest)
    {
        var nodeAttrSrc = Attribute.GetCustomAttribute(typeSrc, typeof(DbNodeAttribute)) as DbNodeAttribute;
        var nodeAttrDest = Attribute.GetCustomAttribute(typeDest, typeof(DbNodeAttribute)) as DbNodeAttribute;

        if (nodeAttrSrc == null || nodeAttrDest == null)
        {
            throw new MissingMemberException("Unable to find the DbNodeAttribute that should be attached to `src` and/or `dest`!");
        }

        string? uuidSrc = null;
        string? uuidSrcName = null;
        string? uuidDest = null;
        string? uuidDestName = null;
        try 
        {
            var uuidSrcInfo = typeSrc.GetProperties().Where(p => p.GetCustomAttribute(typeof(DbNodeUuidAttribute)) != null).Single();
            uuidSrc = (string?) uuidSrcInfo.GetValue(src);
            uuidSrcName = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(uuidSrcInfo.Name);

            var uuidDestInfo = typeDest.GetProperties().Where(p => p.GetCustomAttribute(typeof(DbNodeUuidAttribute)) != null).Single();
            uuidDest = (string?) uuidDestInfo.GetValue(dest);
            uuidDestName = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(uuidDestInfo.Name);

            if (uuidSrc == null || uuidSrcName == null)
            {
                throw new MissingMemberException("Unable to get the value of the property with DbNodeUuidAttribute within `src`!");
            }
            if (uuidDest == null || uuidDestName == null)
            {
                throw new MissingMemberException("Unable to get the value of the property with DbNodeUuidAttribute within `dest`!");
            }
        }
        catch (InvalidOperationException ioe)
        {
            throw new MissingMemberException("Unable to find the property with DbNodeUuidAttribute within `src` and/or `dest`!", ioe);
        }

        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteReadAsync(
                async tx =>
                {
                    var cursor = await tx.RunAsync(
                        $"MATCH ({nodeAttrSrc.Selector}:{nodeAttrSrc.Node}{relationship}({nodeAttrDest.Selector}:{nodeAttrDest.Node}) " +
                        $"WHERE {nodeAttrSrc.Selector}.{uuidSrcName} = $uuid1 AND {nodeAttrDest.Selector}.{uuidDestName} = $uuid2 " +
                        $"RETURN COUNT({relationship.Selector}) AS relCount",
                        new
                        {
                           uuid1 = uuidSrc,
                           uuid2 = uuidDest
                        }
                    );

                    try 
                    {
                        var count = await Wrappers.RunSingleAsync(cursor, record => record.Values["relCount"].As<long>());
                        return count >= 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return false;
                    }
                });
        }
    }
}
