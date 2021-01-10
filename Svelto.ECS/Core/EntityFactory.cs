using System;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    static class EntityFactory
    {
        public static FasterDictionary<RefWrapperType, ITypeSafeDictionary> BuildGroupedEntities
        (EGID egid, EnginesRoot.DoubleBufferedEntitiesToAdd groupEntitiesToAdd
       , IComponentBuilder[] componentsToBuild, IEnumerable<object> implementors, Type implementorType)
        {
            var group = FetchEntityGroup(egid.groupID, groupEntitiesToAdd);

            BuildEntitiesAndAddToGroup(egid, group, componentsToBuild, implementors, implementorType);

            return group;
        }

        static FasterDictionary<RefWrapperType, ITypeSafeDictionary> FetchEntityGroup(ExclusiveGroupStruct groupID,
            EnginesRoot.DoubleBufferedEntitiesToAdd groupEntityComponentsByType)
        {
            if (groupEntityComponentsByType.current.TryGetValue(groupID, out var group) == false)
            {
                group = new FasterDictionary<RefWrapperType, ITypeSafeDictionary>();
                
                groupEntityComponentsByType.current.Add(groupID, group);
            }

            if (groupEntityComponentsByType.currentEntitiesCreatedPerGroup.TryGetValue(groupID, out var value) == false)
                groupEntityComponentsByType.currentEntitiesCreatedPerGroup[groupID] = 0;
            else
                groupEntityComponentsByType.currentEntitiesCreatedPerGroup[groupID] = value+1;
            
            return group;
        }

        static void BuildEntitiesAndAddToGroup
        (EGID entityID, FasterDictionary<RefWrapperType, ITypeSafeDictionary> @group
       , IComponentBuilder[] componentBuilders, IEnumerable<object> implementors, Type implementorType)
        {
            var count = componentBuilders.Length;

#if DEBUG && !PROFILE_SVELTO
            HashSet<Type> types = new HashSet<Type>();

            for (var index = 0; index < count; ++index)
            {
                var entityComponentType = componentBuilders[index].GetEntityComponentType();
                if (types.Contains(entityComponentType))
                {
                    throw new ECSException($"EntityBuilders must be unique inside an EntityDescriptor. Descriptor Type {implementorType} Component Type: {entityComponentType}");
                }

                types.Add(entityComponentType);
            }
#endif
            for (var index = 0; index < count; ++index)
            {
                var entityComponentBuilder = componentBuilders[index];
                var entityComponentType    = entityComponentBuilder.GetEntityComponentType();

                BuildEntity(entityID, @group, entityComponentType, entityComponentBuilder, implementors);
            }
        }

        static void BuildEntity(EGID entityID, FasterDictionary<RefWrapperType, ITypeSafeDictionary> group,
                                Type entityComponentType, IComponentBuilder componentBuilder, IEnumerable<object> implementors)
        {
            var entityComponentsPoolWillBeCreated =
                group.TryGetValue(new RefWrapperType(entityComponentType), out var safeDictionary) == false;

            //passing the undefined entityComponentsByType inside the entityComponentBuilder will allow it to be created with the
            //correct type and casted back to the undefined list. that's how the list will be eventually of the target
            //type.
            componentBuilder.BuildEntityAndAddToList(ref safeDictionary, entityID, implementors);

            if (entityComponentsPoolWillBeCreated)
                group.Add(new RefWrapperType(entityComponentType), safeDictionary);
        }
    }
}