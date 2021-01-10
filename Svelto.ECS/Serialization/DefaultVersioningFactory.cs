using System.Collections.Generic;
using Svelto.Common;

namespace Svelto.ECS.Serialization
{
    public class DefaultVersioningFactory<T> : IDeserializationFactory where T : IEntityDescriptor, new()
    {
        readonly IEnumerable<object> _implementors;

        public DefaultVersioningFactory() {}

        public DefaultVersioningFactory(IEnumerable<object> implementors)
        {
            _implementors = implementors;
        }

        public EntityInitializer BuildDeserializedEntity
        (EGID egid, ISerializationData serializationData, ISerializableEntityDescriptor entityDescriptor
       , int serializationType, IEntitySerialization entitySerialization, IEntityFactory factory
       , bool enginesRootIsDeserializationOnly)
        {
            var entityDescriptorEntitiesToSerialize = enginesRootIsDeserializationOnly ? entityDescriptor.entitiesToSerialize : entityDescriptor.componentsToBuild;

            var initializer = factory.BuildEntity(egid, entityDescriptorEntitiesToSerialize, TypeCache<T>.type, _implementors);

            entitySerialization.DeserializeEntityComponents(serializationData, entityDescriptor, ref initializer
                                                          , serializationType);

            return initializer;
        }
    }
}