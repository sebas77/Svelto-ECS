using System;
using System.Collections.Generic;

namespace Svelto.ECS
{
    /// <summary>
    /// Entities are always built in group. Where the group is not specificed, a special standard group is used
    /// ID can be reused within groups
    /// an EnginesRoot reference cannot be held by anything else than the Composition Root
    /// where it has been created. IEntityFactory and IEntityFunctions allow a weakreference
    /// of the EnginesRoot to be passed around.
    ///
    /// ExclusiveGroups must be used in your game like:
    /// static class GameExclusiveGroup
    ///{
    ///   public static readonly ExclusiveGroups PlayerEntitiesGroup = new ExclusiveGroups();
    ///}
    ///
    /// </summary>
    public interface IEntityFactory
    {
        ///  <summary>
        /// where performance is critical, you may wish to pre allocate the space needed
        /// to store the entities
        ///  </summary>
        ///  <typeparam name="T"></typeparam>
        ///  <param name="groupStructId"></param>
        ///  <param name="size"></param>
        void PreallocateEntitySpace<T>(ExclusiveGroupStruct groupStructId, uint size)
            where T : IEntityDescriptor, new();

        /// <summary>
        ///     The EntityDescriptor doesn't need to be ever instantiated. It just describes the Entity
        ///     itself in terms of EntityComponents to build. The Implementors are passed to fill the
        ///     references of the EntityComponents components. Please read the articles on my blog
        ///     to understand better the terminologies
        ///     Using this function is like building a normal entity, but the entity components
        ///     are grouped by groupID to be more efficiently processed inside engines and
        ///     improve cache locality. Either class entityComponents and struct entityComponents can be
        ///     grouped.
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="groupStructId"></param>
        /// <param name="ed"></param>
        /// <param name="implementors"></param>
        EntityInitializer BuildEntity<T>(uint entityID, ExclusiveBuildGroup groupStructId,
                                               IEnumerable<object> implementors = null)
            where T : IEntityDescriptor, new();

        EntityInitializer BuildEntity<T>(EGID egid, IEnumerable<object> implementors = null)
            where T : IEntityDescriptor, new();

        EntityInitializer BuildEntity<T>(uint entityID, ExclusiveBuildGroup groupStructId,
                                               T    descriptorEntity, IEnumerable<object>  implementors = null)
            where T : IEntityDescriptor;

        EntityInitializer BuildEntity<T>(EGID egid, T entityDescriptor, IEnumerable<object> implementors = null)
            where T : IEntityDescriptor;

        EntityInitializer BuildEntity
            (EGID egid, IComponentBuilder[] componentsToBuild, Type type, IEnumerable<object> implementors = null);

#if UNITY_NATIVE
        NativeEntityFactory ToNative<T>(string callerName) where T : IEntityDescriptor, new();
#endif        
    }
}