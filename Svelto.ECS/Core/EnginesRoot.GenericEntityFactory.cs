﻿using System;
using System.Collections.Generic;
using Svelto.Common;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        class GenericEntityFactory : IEntityFactory
        {
            public GenericEntityFactory(EnginesRoot weakReference)
            {
                _enginesRoot = new Svelto.DataStructures.WeakReference<EnginesRoot>(weakReference);
            }

            public EntityInitializer BuildEntity<T>
                (uint entityID, ExclusiveBuildGroup groupStructId, IEnumerable<object> implementors = null)
                where T : IEntityDescriptor, new()
            {
                return _enginesRoot.Target.BuildEntity(new EGID(entityID, groupStructId)
                                                     , EntityDescriptorTemplate<T>.descriptor.componentsToBuild
                                                     , TypeCache<T>.type, implementors);
            }

            public EntityInitializer BuildEntity<T>(EGID egid, IEnumerable<object> implementors = null)
                where T : IEntityDescriptor, new()
            {
                return _enginesRoot.Target.BuildEntity(
                    egid, EntityDescriptorTemplate<T>.descriptor.componentsToBuild, TypeCache<T>.type, implementors);
            }

            public EntityInitializer BuildEntity<T>
                (EGID egid, T entityDescriptor, IEnumerable<object> implementors) where T : IEntityDescriptor
            {
                return _enginesRoot.Target.BuildEntity(egid, entityDescriptor.componentsToBuild, TypeCache<T>.type, implementors);
            }
#if UNITY_NATIVE
            public NativeEntityFactory ToNative<T>(string callerName) where T : IEntityDescriptor, new()
            {
                return _enginesRoot.Target.ProvideNativeEntityFactoryQueue<T>(callerName);
            }
#endif            
            public EntityInitializer BuildEntity<T>
                (uint entityID, ExclusiveBuildGroup groupStructId, T descriptorEntity, IEnumerable<object> implementors)
                where T : IEntityDescriptor
            {
                return _enginesRoot.Target.BuildEntity(new EGID(entityID, groupStructId)
                                                     , descriptorEntity.componentsToBuild, TypeCache<T>.type, implementors);
            }

            public void PreallocateEntitySpace<T>(ExclusiveGroupStruct groupStructId, uint size)
                where T : IEntityDescriptor, new()
            {
                _enginesRoot.Target.Preallocate<T>(groupStructId, size);
            }
            
            public EntityInitializer BuildEntity(EGID egid, IComponentBuilder[] componentsToBuild, Type type, IEnumerable<object> implementors = null)
            {
                return _enginesRoot.Target.BuildEntity(egid, componentsToBuild, type, implementors);
            }

            //enginesRoot is a weakreference because GenericEntityStreamConsumerFactory can be injected inside
            //engines of other enginesRoot
            readonly Svelto.DataStructures.WeakReference<EnginesRoot> _enginesRoot;
        }
    }
}