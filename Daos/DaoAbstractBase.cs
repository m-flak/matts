﻿/* matts
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
using System.Diagnostics.CodeAnalysis;
using Neo4j.Driver;
using matts.Interfaces;
using matts.Utils;

namespace matts.Daos;

public abstract class DaoAbstractBase<T> : IDataAccessObject<T> where T : class
{
    public virtual Task<T> CreateNew(T createWhat)
        => this.CreateNewImpl(createWhat);
    public virtual Task<bool> CreateRelationshipBetween(DbRelationship relationship, T source, object other, Type typeOther)
        => this.CreateRelationshipBetweenImpl(relationship, source, other, typeof(T), typeOther);
    public virtual Task<bool> UpdateRelationshipBetween(DbRelationship relationship, T source, object other, Type typeOther)
        => this.UpdateRelationshipBetweenImpl(relationship, source, other, typeof(T), typeOther);
    public virtual Task<bool> DeleteRelationshipBetween(DbRelationship relationship, T? source, object? other, Type typeOther)
        => this.DeleteRelationshipBetweenImpl(relationship, source, other, typeof(T), typeOther);
    public virtual Task<List<T>> GetAll()
        => this.GetAllImpl(typeof(T), null);
    public virtual Task<List<T>> GetAllAndFilterByProperties(IReadOnlyDictionary<string, object> filterProperties)
        => this.GetAllAndFilterByPropertiesImpl(typeof(T), filterProperties, null);
    
    public virtual Task<List<T>> GetAllByRelationship(DbRelationship relationship, DbRelationship? optionalRelationship, string whomUuid)
    {
        if (optionalRelationship != null && !relationship.HasSameNodeTypes(optionalRelationship))
        {
            throw new NotSupportedException("Differing Node Types for relationship & optionalRelationship not supported at this time.");
        }

        return this.GetAllByRelationshipImpl(
            relationship.LeftType, 
            relationship.RightType, 
            new GetAllByRelationshipConfig(
                GetAllByRelationshipConfig.WhereNodeSelector.RIGHT, 
                GetAllByRelationshipConfig.ReturnNodeSelector.LEFT
            ), 
            relationship, 
            optionalRelationship, 
            whomUuid,
            null
        );
    }

    public virtual Task<T> GetByUuid(string uuid)
        => this.GetByUuidImpl(typeof(T), uuid);
    public virtual Task<List<P>> GetPropertyFromRelated<P>(string relationship, Type relatedNodeType, string propertyName)
        => this.GetPropertyFromRelatedImpl<P>(relationship, typeof(T), relatedNodeType, propertyName, null);
    public virtual Task<bool> HasRelationshipBetween(DbRelationship relationship, T source, object other, Type typeOther)
        => this.HasRelationshipBetweenImpl(relationship, source, other, typeof(T), typeOther);

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

    protected async Task<List<T>> GetAllByRelationshipImpl(Type lhNode, Type rhNode, GetAllByRelationshipConfig config, DbRelationship relationship, DbRelationship? optionalRelationship, string whomUuid, string? orderBy)
    {
        GetNodeAttribute(lhNode, out var lhAttr);
        GetNodeAttribute(rhNode, out var rhAttr);

        // Obtain the field used as the Uuid via reflection.
        var infos = GetUuidInfos((null, lhNode), (null, rhNode));

        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteReadAsync(
                async tx =>
                {
                    static string addOptionalParams(DbRelationship? optRel) => (optRel != null) ? DaoUtils.AddReturnsForRelationshipParams(optRel.Name, optRel.Selector) : "";

                    string whereSelector = (config.WhereSelector == GetAllByRelationshipConfig.WhereNodeSelector.LEFT) ? lhAttr.Selector : rhAttr.Selector;
                    string whereUuidProperty = (config.WhereSelector == GetAllByRelationshipConfig.WhereNodeSelector.LEFT) ? infos[0].Name : infos[1].Name;
                    string returnSelector = (config.ReturnSelector == GetAllByRelationshipConfig.ReturnNodeSelector.LEFT) ? lhAttr.Selector : rhAttr.Selector;
                    // Get Order By Property to sort by
                    string orderByProperty = (orderBy != null) 
                        ? orderBy 
                        : (config.ReturnSelector == GetAllByRelationshipConfig.ReturnNodeSelector.LEFT) 
                            ? GetOrderByProperty(lhNode) 
                            : GetOrderByProperty(rhNode);

                    var cursor = await tx.RunAsync(
                        $"MATCH({lhAttr.Selector}: {lhAttr.Node}){relationship}({rhAttr.Selector}: {rhAttr.Node}) " +
                        $"WHERE {whereSelector}.{whereUuidProperty} = $uuid " +
                        $"{DaoUtils.CreateOptionalMatchClause(optionalRelationship?.Name, lhAttr.Selector, rhAttr.Selector)}" +
                        $"RETURN {returnSelector} {DaoUtils.AddReturnsForRelationshipParams(relationship.Name, relationship.Selector)} {addOptionalParams(optionalRelationship)} " +
                        $"ORDER BY {returnSelector}.{orderByProperty}",
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

    protected async Task<List<T>> GetAllAndFilterByPropertiesImpl(Type node, IReadOnlyDictionary<string, object> filterProperties, string? orderBy)
    {
        GetNodeAttribute(node, out var nodeAttr);

        // Get Order By Property to sort by
        string orderByProperty = (orderBy != null) ? orderBy : GetOrderByProperty(node);

        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteReadAsync(
                async tx =>
                {
                    var cursor = await tx.RunAsync(
                        $"MATCH ({nodeAttr.Selector}: {nodeAttr.Node}) " +
                        $"WHERE {DaoUtils.CreateWhereClauseFromDict(filterProperties, nodeAttr.Selector)} " +
                        $"RETURN {nodeAttr.Selector} " +
                        $"ORDER BY {nodeAttr.Selector}.{orderByProperty}"
                    );
                    var rows = await Wrappers.RunToListAsync(cursor, record => record.Values[nodeAttr.Selector].As<INode>());
                    return rows.Select(row => DaoUtils.MapSimpleRow<T>(row))
                        .ToList();
                });
        }
    }

    protected async Task<List<T>> GetAllImpl(Type node, string? orderBy)
    {
        GetNodeAttribute(node, out var nodeAttr);

        // Get Order By Property to sort by
        string orderByProperty = (orderBy != null) ? orderBy : GetOrderByProperty(node);

        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteReadAsync(
                async tx =>
                {
                    var cursor = await tx.RunAsync(
                        $"MATCH ({nodeAttr.Selector}: {nodeAttr.Node}) " +
                        $"RETURN {nodeAttr.Selector} " +
                        $"ORDER BY {nodeAttr.Selector}.{orderByProperty}"
                    );
                    var rows = await Wrappers.RunToListAsync(cursor, record => record.Values[nodeAttr.Selector].As<INode>());
                    return rows.Select(row => DaoUtils.MapSimpleRow<T>(row))
                        .ToList();
                });
        }
    }

    protected async Task<T> GetByUuidImpl(Type node, string uuid)
    {
        GetNodeAttribute(node, out var nodeAttr);

        // Obtain the field used as the Uuid via reflection.
        var infos = GetUuidInfos((null, node));
        string uuidFieldName = infos[0].Name;

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

    protected async Task<List<P>> GetPropertyFromRelatedImpl<P>(string relationship, Type thisNodeType, Type relatedNodeType, string propertyName, string? orderBy)
    {
        GetNodeAttribute(thisNodeType, out var lhAttr);
        GetNodeAttribute(relatedNodeType, out var rhAttr);

        // Get Order By Property to sort by
        string orderByProperty = (orderBy != null) ? orderBy : GetOrderByProperty(thisNodeType);

        var queryRelationship = new DbRelationship(relationship, DbRelationship.Cardinality.BIDIRECTIONAL);

        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteReadAsync(
                async tx =>
                {
                    var cursor = await tx.RunAsync(
                        $"MATCH({lhAttr.Selector}: {lhAttr.Node}){queryRelationship}({rhAttr.Selector}: {rhAttr.Node}) " +
                        $"RETURN {rhAttr.Selector}.{propertyName} AS {propertyName} " +
                        $"ORDER BY {lhAttr.Selector}.{orderByProperty}"
                    );

                    var rows = await Wrappers.RunToListAsync(cursor, record => record.Values[propertyName].As<P>());
                    return rows.ToList();
                });
        }
    }

    protected async Task<T> CreateNewImpl(T createWhat)
    {
        Type createWhatType = typeof(T);
        GetNodeAttribute(createWhatType, out var nodeAttr);

        // Obtain the field used as the Uuid via reflection.
        var infos = GetUuidInfos((createWhat, typeof(T)));
        string uuid = infos[0].Value;

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
        GetNodeAttribute(typeSrc, out var nodeAttrSrc);
        GetNodeAttribute(typeDest, out var nodeAttrDest);

        // Obtain the field used as the Uuid via reflection.
        var infos = GetUuidInfos((src, typeSrc), (dest, typeDest));
        string uuidSrc = infos[0].Value;
        string uuidSrcName = infos[0].Name;
        string uuidDest = infos[1].Value;
        string uuidDestName = infos[1].Name;

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
        GetNodeAttribute(typeSrc, out var nodeAttrSrc);
        GetNodeAttribute(typeDest, out var nodeAttrDest);

        // Obtain the field used as the Uuid via reflection.
        var infos = GetUuidInfos((src, typeSrc), (dest, typeDest));
        string uuidSrc = infos[0].Value;
        string uuidSrcName = infos[0].Name;
        string uuidDest = infos[1].Value;
        string uuidDestName = infos[1].Name;

        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteReadAsync(
                async tx =>
                {
                    var cursor = await tx.RunAsync(
                        $"MATCH ({nodeAttrSrc.Selector}:{nodeAttrSrc.Node}){relationship.ToString(false, true)}({nodeAttrDest.Selector}:{nodeAttrDest.Node}) " +
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

    protected async Task<bool> UpdateRelationshipBetweenImpl(DbRelationship relationship, object src, object dest, Type typeSrc, Type typeDest)
    {
        GetNodeAttribute(typeSrc, out var nodeAttrSrc);
        GetNodeAttribute(typeDest, out var nodeAttrDest);

        // Obtain the field used as the Uuid via reflection.
        var infos = GetUuidInfos((src, typeSrc), (dest, typeDest));
        string uuidSrc = infos[0].Value;
        string uuidSrcName = infos[0].Name;
        string uuidDest = infos[1].Value;
        string uuidDestName = infos[1].Name;

        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteWriteAsync(
               async tx =>
               {
                   var queryParams = new Dictionary<string, object>(relationship.Parameters);
                   queryParams["uuid1"] = uuidSrc;
                   queryParams["uuid2"] = uuidDest;

                   var cursor = await tx.RunAsync(
                       $"MATCH ({nodeAttrSrc.Selector}:{nodeAttrSrc.Node}){relationship.ToString(false, true)}({nodeAttrDest.Selector}:{nodeAttrDest.Node}) " +
                       $"WHERE {nodeAttrSrc.Selector}.{uuidSrcName} = $uuid1 AND {nodeAttrDest.Selector}.{uuidDestName} = $uuid2 " +
                       DaoUtils.CreateSetStatements(relationship.Parameters, relationship.Selector),
                       queryParams
                   );

                   var result = await cursor.ConsumeAsync();
                   return result.Counters.PropertiesSet > 0;
               });
        }
    }

    protected async Task<bool> DeleteRelationshipBetweenImpl(DbRelationship relationship, object? src, object? dest, Type typeSrc, Type typeDest)
    {
        if (src == null && dest == null)
        {
            throw new ArgumentNullException("The source and destination objects cannot both be null.");
        }

        GetNodeAttribute(typeSrc, out var nodeAttrSrc);
        GetNodeAttribute(typeDest, out var nodeAttrDest);

        // Obtain the field used as the Uuid via reflection.
        var infos = GetUuidInfos((src, typeSrc), (dest, typeDest));
        string uuidSrc = infos[0].Value;    // will be empty str if src is null
        string uuidSrcName = infos[0].Name;
        string uuidDest = infos[1].Value;   // will be empty str if dest is null
        string uuidDestName = infos[1].Name;

        using (var session = _driver.AsyncSession())
        {
            return await session.ExecuteWriteAsync(
                async tx =>
                {
                    IResultCursor? cursor;
                    
                    // dest and src were provided
                    if (uuidDest.Length > 0 && uuidSrc.Length > 0)
                    {
                        cursor = await tx.RunAsync(
                            $"MATCH ({nodeAttrSrc.Selector}:{nodeAttrSrc.Node}){relationship.ToString(false, true)}({nodeAttrDest.Selector}:{nodeAttrDest.Node}) " +
                            $"WHERE {nodeAttrSrc.Selector}.{uuidSrcName} = $uuid1 AND {nodeAttrDest.Selector}.{uuidDestName} = $uuid2 " +
                            $"DELETE {relationship.Selector}",
                            new
                            {
                                uuid1 = uuidSrc,
                                uuid2 = uuidDest
                            }
                        );
                    }
                    else if (uuidSrc.Length > 0)
                    {
                        cursor = await tx.RunAsync(
                            $"MATCH ({nodeAttrSrc.Selector}:{nodeAttrSrc.Node}){relationship.ToString(false, true)}() " +
                            $"WHERE {nodeAttrSrc.Selector}.{uuidSrcName} = $uuid1 " +
                            $"DELETE {relationship.Selector}",
                            new
                            {
                                uuid1 = uuidSrc
                            }
                        );
                    }
                    else
                    {
                        cursor = await tx.RunAsync(
                            $"MATCH (){relationship.ToString(false, true)}({nodeAttrDest.Selector}:{nodeAttrDest.Node}) " +
                            $"WHERE {nodeAttrDest.Selector}.{uuidDestName} = $uuid2 " +
                            $"DELETE {relationship.Selector}",
                            new
                            {
                                uuid2 = uuidDest
                            }
                        );
                    }

                    var result = await cursor.ConsumeAsync();
                    return result.Counters.RelationshipsDeleted == 1;
                });
        }
    }

    private readonly struct UuidInfo
    {
        public required string Name { get; init; }
        public required string Value { get; init; }

        [SetsRequiredMembers]
        public UuidInfo(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    // Pass (someObject, typeof(SomeType)) when you want to both the field name and its value.
    // Pass (null, typeof(SomeType)) when you only care about the field name itself.
    private static UuidInfo[] GetUuidInfos(params (object?, Type)[] nodeSpecs)
    {
        UuidInfo[] infos = new UuidInfo[nodeSpecs.Length];
        int i = 0;
        try 
        {
            foreach ((object?, Type) spec in nodeSpecs)
            {
                var uuidInfo = spec.Item2.GetProperties().Where(p => p.GetCustomAttribute(typeof(DbNodeUuidAttribute)) != null).Single();

                // value is optional if the spec.Item1 is null
                string? value = "";
                if (spec.Item1 != null)
                {
                    value = (string?) uuidInfo.GetValue(spec.Item1);
                    if (value == null) 
                    {
                        throw new MissingMemberException($"Unable to get the value of the property with DbNodeUuidAttribute for type {spec.Item2.Name}!");
                    }
                }

                // name is required
                string? name = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(uuidInfo.Name);
                if (name == null) 
                {
                    throw new MissingMemberException($"Unable to get the name of the property with DbNodeUuidAttribute for type {spec.Item2.Name}!");
                }

                infos[i++] = new UuidInfo(name, value);
            }
        }
        catch (InvalidOperationException ioe)
        {
            throw new MissingMemberException("Unable to find the property with DbNodeUuidAttribute attached to the object/type.", ioe);
        }
        catch (MissingMemberException)
        {
            throw;
        }

        return infos;
    }

    private static string GetOrderByProperty(Type node)
    {
        PropertyInfo? info = null;
        try
        {
            info = node.GetProperties().Where(p => p.GetCustomAttribute(typeof(DbNodeOrderByAttribute)) != null).Single();
        }
        catch (InvalidOperationException ioe)
        {
            throw new MissingMemberException($"Unable to find the Order By property within {node.Name}!", ioe);
        }

        return System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(info.Name);
    }

    private static void GetNodeAttribute(Type node, out DbNodeAttribute attr)
    {
        var _attr = Attribute.GetCustomAttribute(node, typeof(DbNodeAttribute)) as DbNodeAttribute;
        if (_attr == null)
        {
            throw new MissingMemberException($"Unable to find the DbNodeAttribute that should be attached to {node.Name}!");
        }
        attr = _attr;
    }
}
