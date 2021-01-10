#if DEBUG && !PROFILE_SVELTO
#define ENABLE_DEBUG_FUNC
#endif

using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EntitiesDB
    {
        internal EntitiesDB(EnginesRoot enginesRoot)
        {
            _enginesRoot = enginesRoot;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FindEGID(EntityReference entityReference, out EGID egid)
        {
            return _referenceLocatorMap.TryGetEGID(entityReference, out egid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityReference GetEntityReference(EGID egid)
        {
            return _referenceLocatorMap.GetEntityReference(egid);
        }

        EntityCollection<T> InternalQueryEntities<T>(FasterDictionary<RefWrapperType, ITypeSafeDictionary> entitiesInGroupPerType)
            where T : struct, IEntityComponent
        {
            uint       count = 0;
            IBuffer<T> buffer;
            if (SafeQueryEntityDictionary<T>(out var typeSafeDictionary, entitiesInGroupPerType) == false)
                buffer = RetrieveEmptyEntityComponentArray<T>();
            else
            {
                var safeDictionary = (typeSafeDictionary as ITypeSafeDictionary<T>);
                buffer = safeDictionary.GetValues(out count);
            }

            return new EntityCollection<T>(buffer, count);
        }

        /// <summary>
        /// The QueryEntities<T> follows the rule that entities could always be iterated regardless if they
        /// are 0, 1 or N. In case of 0 it returns an empty array. This allows to use the same for iteration
        /// regardless the number of entities built.
        /// </summary>
        /// <param name="groupStructId"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public EntityCollection<T> QueryEntities<T>(ExclusiveGroupStruct groupStructId)
            where T : struct, IEntityComponent
        {
            if (groupEntityComponentsDB.TryGetValue(groupStructId, out var entitiesInGroupPerType) == false)
            {
                var buffer = RetrieveEmptyEntityComponentArray<T>();
                return new EntityCollection<T>(buffer, 0);
            }

            return InternalQueryEntities<T>(entitiesInGroupPerType);
        }

        public EntityCollection<T1, T2> QueryEntities<T1, T2>(ExclusiveGroupStruct groupStruct)
            where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent
        {
            if (groupEntityComponentsDB.TryGetValue(groupStruct, out var entitiesInGroupPerType) == false)
            {
                return new EntityCollection<T1, T2>(new EntityCollection<T1>(RetrieveEmptyEntityComponentArray<T1>(), 0),
                    new EntityCollection<T2>(RetrieveEmptyEntityComponentArray<T2>(), 0));
            }

            var T1entities = InternalQueryEntities<T1>(entitiesInGroupPerType);
            var T2entities = InternalQueryEntities<T2>(entitiesInGroupPerType);
#if DEBUG && !PROFILE_SVELTO
            if (T1entities.count != T2entities.count)
                throw new ECSException("Entity components count do not match in group. Entity 1: ' count: "
                   .FastConcat(T1entities.count).FastConcat(" ", typeof(T1).ToString())
                   .FastConcat("'. Entity 2: ' count: ".FastConcat(T2entities.count)
                       .FastConcat(" ", typeof(T2).ToString())
                       .FastConcat("' group: ", groupStruct.ToName())));
#endif

            return new EntityCollection<T1, T2>(T1entities, T2entities);
        }

        public EntityCollection<T1, T2, T3> QueryEntities<T1, T2, T3>(ExclusiveGroupStruct groupStruct)
            where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent where T3 : struct, IEntityComponent
        {
            if (groupEntityComponentsDB.TryGetValue(groupStruct, out var entitiesInGroupPerType) == false)
            {
                return new EntityCollection<T1, T2, T3>(
                    new EntityCollection<T1>(RetrieveEmptyEntityComponentArray<T1>(), 0),
                    new EntityCollection<T2>(RetrieveEmptyEntityComponentArray<T2>(), 0),
                    new EntityCollection<T3>(RetrieveEmptyEntityComponentArray<T3>(), 0));
            }

            var T1entities = InternalQueryEntities<T1>(entitiesInGroupPerType);
            var T2entities = InternalQueryEntities<T2>(entitiesInGroupPerType);
            var T3entities = InternalQueryEntities<T3>(entitiesInGroupPerType);
#if DEBUG && !PROFILE_SVELTO
            if (T1entities.count != T2entities.count || T2entities.count != T3entities.count)
                throw new ECSException("Entity components count do not match in group. Entity 1: "
                   .FastConcat(typeof(T1).ToString()).FastConcat(" count: ")
                   .FastConcat(T1entities.count).FastConcat(
                        " Entity 2: "
                           .FastConcat(typeof(T2).ToString()).FastConcat(" count: ")
                           .FastConcat(T2entities.count)
                           .FastConcat(" Entity 3: ".FastConcat(typeof(T3).ToString()))
                           .FastConcat(" count: ").FastConcat(T3entities.count)));
#endif

            return new EntityCollection<T1, T2, T3>(T1entities, T2entities, T3entities);
        }

        public EntityCollection<T1, T2, T3, T4> QueryEntities<T1, T2, T3, T4>(ExclusiveGroupStruct groupStruct)
            where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent where T3 : struct, IEntityComponent where T4 : struct, IEntityComponent
        {
            if (groupEntityComponentsDB.TryGetValue(groupStruct, out var entitiesInGroupPerType) == false)
            {
                return new EntityCollection<T1, T2, T3, T4>(
                    new EntityCollection<T1>(RetrieveEmptyEntityComponentArray<T1>(), 0),
                    new EntityCollection<T2>(RetrieveEmptyEntityComponentArray<T2>(), 0),
                    new EntityCollection<T3>(RetrieveEmptyEntityComponentArray<T3>(), 0),
                    new EntityCollection<T4>(RetrieveEmptyEntityComponentArray<T4>(), 0));
            }

            var T1entities = InternalQueryEntities<T1>(entitiesInGroupPerType);
            var T2entities = InternalQueryEntities<T2>(entitiesInGroupPerType);
            var T3entities = InternalQueryEntities<T3>(entitiesInGroupPerType);
            var T4entities = InternalQueryEntities<T4>(entitiesInGroupPerType);
#if DEBUG && !PROFILE_SVELTO
            if (T1entities.count != T2entities.count || T2entities.count != T3entities.count)
                throw new ECSException("Entity components count do not match in group. Entity 1: "
                   .FastConcat(typeof(T1).ToString()).FastConcat(" count: ")
                   .FastConcat(T1entities.count).FastConcat(
                        " Entity 2: "
                           .FastConcat(typeof(T2).ToString()).FastConcat(" count: ")
                           .FastConcat(T2entities.count)
                           .FastConcat(" Entity 3: ".FastConcat(typeof(T3).ToString()))
                           .FastConcat(" count: ").FastConcat(T3entities.count)));
#endif

            return new EntityCollection<T1, T2, T3, T4>(T1entities, T2entities, T3entities, T4entities);
        }

        public GroupsEnumerable<T> QueryEntities<T>
            (in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) where T : struct, IEntityComponent
        {
            return new GroupsEnumerable<T>(this, groups);
        }

        public GroupsEnumerable<T1, T2> QueryEntities<T1, T2>(in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
            where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent
        {
            return new GroupsEnumerable<T1, T2>(this, groups);
        }

        public GroupsEnumerable<T1, T2, T3> QueryEntities<T1, T2, T3>(in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
            where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent where T3 : struct, IEntityComponent
        {
            return new GroupsEnumerable<T1, T2, T3>(this, groups);
        }

        public GroupsEnumerable<T1, T2, T3, T4> QueryEntities<T1, T2, T3, T4>
            (in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
            where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent
            where T3 : struct, IEntityComponent where T4 : struct, IEntityComponent
        {
            return new GroupsEnumerable<T1, T2, T3, T4>(this, groups);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EGIDMapper<T> QueryMappedEntities<T>(ExclusiveGroupStruct groupStructId)
            where T : struct, IEntityComponent
        {
            if (SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false)
                throw new EntityGroupNotFoundException(typeof(T) , groupStructId.ToName());

            return (typeSafeDictionary as ITypeSafeDictionary<T>).ToEGIDMapper(groupStructId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryQueryMappedEntities<T>
            (ExclusiveGroupStruct groupStructId, out EGIDMapper<T> mapper) where T : struct, IEntityComponent
        {
            mapper = default;
            if (SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false
             || typeSafeDictionary.count == 0)
                return false;

            mapper = (typeSafeDictionary as ITypeSafeDictionary<T>).ToEGIDMapper(groupStructId);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists<T>(EGID entityGID) where T : struct, IEntityComponent
        {
            if (SafeQueryEntityDictionary<T>(entityGID.groupID, out var casted) == false)
                return false;

            return casted != null && casted.ContainsKey(entityGID.entityID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists<T>(uint id, ExclusiveGroupStruct group) where T : struct, IEntityComponent
        {
            if (SafeQueryEntityDictionary<T>(group, out var casted) == false)
                return false;

            return casted != null && casted.ContainsKey(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExistsAndIsNotEmpty(ExclusiveGroupStruct gid)
        {
            if (groupEntityComponentsDB.TryGetValue(
                gid, out FasterDictionary<RefWrapperType, ITypeSafeDictionary> group) == true)
            {
                return group.count > 0;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAny<T>(ExclusiveGroupStruct groupStruct) where T : struct, IEntityComponent
        {
            return Count<T>(groupStruct) > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count<T>(ExclusiveGroupStruct groupStruct) where T : struct, IEntityComponent
        {
            if (SafeQueryEntityDictionary<T>(groupStruct, out var typeSafeDictionary) == false)
                return 0;

            return (int) typeSafeDictionary.count;
        }

        public bool FoundInGroups<T1>() where T1 : IEntityComponent
        {
            return groupsPerEntity.ContainsKey(TypeRefWrapper<T1>.wrapper);
        }

        public bool IsDisposing => _enginesRoot._isDisposing;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool SafeQueryEntityDictionary<T>(out ITypeSafeDictionary typeSafeDictionary,
            FasterDictionary<RefWrapperType, ITypeSafeDictionary> entitiesInGroupPerType)
            where T : IEntityComponent
        {
            if (entitiesInGroupPerType.TryGetValue(new RefWrapperType(TypeCache<T>.type), out var safeDictionary) == false)
            {
                typeSafeDictionary = default;
                return false;
            }

            //return the indexes entities if they exist
            typeSafeDictionary = safeDictionary;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool SafeQueryEntityDictionary<T>(ExclusiveGroupStruct group, out ITypeSafeDictionary typeSafeDictionary)
            where T : IEntityComponent
        {
            if (UnsafeQueryEntityDictionary(group, TypeCache<T>.type, out var safeDictionary) == false)
            {
                typeSafeDictionary = default;
                return false;
            }

            //return the indexes entities if they exist
            typeSafeDictionary = safeDictionary;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool UnsafeQueryEntityDictionary(ExclusiveGroupStruct group, Type type, out ITypeSafeDictionary typeSafeDictionary)
        {
            //search for the group
            if (groupEntityComponentsDB.TryGetValue(group, out var entitiesInGroupPerType) == false)
            {
                typeSafeDictionary = null;
                return false;
            }

            //search for the indexed entities in the group
            return entitiesInGroupPerType.TryGetValue(new RefWrapperType(type), out typeSafeDictionary);
        }

        internal bool FindIndex(uint entityID, ExclusiveGroupStruct @group, Type type, out uint index)
        {
            EGID entityGID = new EGID(entityID, @group);

            index = default;

            if (UnsafeQueryEntityDictionary(@group, type, out var safeDictionary) == false)
                return false;

            if (safeDictionary.TryFindIndex(entityGID.entityID, out index) == false)
                return false;

            return true;
        }

        internal uint GetIndex(uint entityID, ExclusiveGroupStruct @group, Type type)
        {
            EGID entityGID = new EGID(entityID, @group);

            if (UnsafeQueryEntityDictionary(@group, type, out var safeDictionary) == false)
            {
                throw new EntityNotFoundException(entityGID, type);
            }

            if (safeDictionary.TryFindIndex(entityGID.entityID, out var index) == false)
                throw new EntityNotFoundException(entityGID, type);

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static IBuffer<T> RetrieveEmptyEntityComponentArray<T>() where T : struct, IEntityComponent
        {
            return EmptyList<T>.emptyArray;
        }

        static class EmptyList<T> where T : struct, IEntityComponent
        {
            internal static readonly IBuffer<T> emptyArray;

            static EmptyList()
            {
                if (ComponentBuilder<T>.IS_ENTITY_VIEW_COMPONENT)
                {
                    MB<T> b = default;

                    emptyArray = b;
                }
                else
                {
                    NB<T> b = default;

                    emptyArray = b;
                }
            }
        }

        static readonly FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary> _emptyDictionary =
            new FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>();

        readonly EnginesRoot _enginesRoot;

        IEntityReferenceLocatorMap _referenceLocatorMap => _enginesRoot;

        EntitiesStreams _entityStream => _enginesRoot._entityStreams;

        //grouped set of entity components, this is the standard way to handle entity components are grouped per
        //group, then indexable per type, then indexable per EGID. however the TypeSafeDictionary can return an array of
        //values directly, that can be iterated over, so that is possible to iterate over all the entity components of
        //a specific type inside a specific group.

        FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, ITypeSafeDictionary>>
                    groupEntityComponentsDB => _enginesRoot._groupEntityComponentsDB;

        //for each entity view type, return the groups (dictionary of entities indexed by entity id) where they are
        //found indexed by group id. TypeSafeDictionary are never created, they instead point to the ones hold
        //by _groupEntityComponentsDB
        //                        <EntityComponentType                            <groupID  <entityID, EntityComponent>>>
        FasterDictionary<RefWrapperType, FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>> groupsPerEntity =>
            _enginesRoot._groupsPerEntity;
    }
}