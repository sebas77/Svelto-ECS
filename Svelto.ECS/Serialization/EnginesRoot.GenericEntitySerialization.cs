using System;
using Svelto.DataStructures;
using Svelto.ECS.Serialization;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        sealed class EntitySerialization : IEntitySerialization
        {
            public void SerializeEntity(EGID egid, ISerializationData serializationData, int serializationType)
            {
                var entitiesDb = _enginesRoot._entitiesDB;

                //needs to retrieve the meta data associated with the entity
                ref var serializableEntityComponent = ref entitiesDb.QueryEntity<SerializableEntityComponent>(egid);
                uint    descriptorHash              = serializableEntityComponent.descriptorHash;

                SerializationDescriptorMap serializationDescriptorMap = _enginesRoot.serializationDescriptorMap;
                var entityDescriptor = serializationDescriptorMap.GetDescriptorFromHash(descriptorHash);
                var entityComponentsToSerialise = entityDescriptor.entitiesToSerialize;

                var header =
                    new SerializableEntityHeader(descriptorHash, egid, (byte) entityComponentsToSerialise.Length);
                header.Copy(serializationData);

                for (int index = 0; index < entityComponentsToSerialise.Length; index++)
                {
                    var entityBuilder = entityComponentsToSerialise[index];

                    serializationData.BeginNextEntityComponent();
                    SerializeEntityComponent(egid, entityBuilder, serializationData, serializationType);
                }
            }

            public EntityInitializer DeserializeNewEntity
                (EGID egid, ISerializationData serializationData, int serializationType)
            {
                //todo: SerializableEntityHeader may be needed to be customizable
                var serializableEntityHeader = new SerializableEntityHeader(serializationData);

                uint descriptorHash = serializableEntityHeader.descriptorHash;
                SerializationDescriptorMap serializationDescriptorMap = _enginesRoot.serializationDescriptorMap;
                var entityDescriptor = serializationDescriptorMap.GetDescriptorFromHash(descriptorHash);
                IDeserializationFactory factory = serializationDescriptorMap.GetSerializationFactory(descriptorHash);

                return factory.BuildDeserializedEntity(egid, serializationData, entityDescriptor, serializationType
                                                     , this, this._enginesRoot.GenerateEntityFactory()
                                                     , _enginesRoot._isDeserializationOnly);
            }

            public void DeserializeEntity(ISerializationData serializationData, int serializationType)
            {
                var serializableEntityHeader = new SerializableEntityHeader(serializationData);

                EGID egid = serializableEntityHeader.egid;

                DeserializeEntityInternal(serializationData, egid, serializableEntityHeader, serializationType);
            }

            public void DeserializeEntity(EGID egid, ISerializationData serializationData, int serializationType)
            {
                var serializableEntityHeader = new SerializableEntityHeader(serializationData);

                DeserializeEntityInternal(serializationData, egid, serializableEntityHeader, serializationType);
            }

            public void DeserializeEntityComponents
            (ISerializationData serializationData, ISerializableEntityDescriptor entityDescriptor
           , ref EntityInitializer initializer, int serializationType)
            {
                foreach (var serializableEntityBuilder in entityDescriptor.entitiesToSerialize)
                {
                    serializationData.BeginNextEntityComponent();
                    serializableEntityBuilder.Deserialize(serializationData, initializer, serializationType);
                }
            }

            public T DeserializeEntityComponent<T>
            (ISerializationData serializationData, ISerializableEntityDescriptor entityDescriptor
           , int serializationType) where T : unmanaged, IEntityComponent
            {
                var readPos         = serializationData.dataPos;
                T   entityComponent = default;
                foreach (var serializableEntityBuilder in entityDescriptor.entitiesToSerialize)
                {
                    if (serializableEntityBuilder is SerializableComponentBuilder<T> entityBuilder)
                    {
                        entityBuilder.Deserialize(serializationData, ref entityComponent, serializationType);
                    }

                    break;
                }

                serializationData.dataPos = readPos;
                return entityComponent;
            }

            public void DeserializeEntityToSwap(EGID localEgid, EGID toEgid)
            {
                EntitiesDB entitiesDb = _enginesRoot._entitiesDB;
                ref var serializableEntityComponent =
                    ref entitiesDb.QueryEntity<SerializableEntityComponent>(localEgid);

                SerializationDescriptorMap serializationDescriptorMap = _enginesRoot.serializationDescriptorMap;
                uint descriptorHash = serializableEntityComponent.descriptorHash;
                var entityDescriptor = serializationDescriptorMap.GetDescriptorFromHash(descriptorHash);

                var entitySubmitOperation =
                    new EntitySubmitOperation(EntitySubmitOperationType.Swap, localEgid, toEgid
                                            , entityDescriptor.componentsToBuild);

                _enginesRoot.CheckRemoveEntityID(localEgid, entityDescriptor.realType);
                _enginesRoot.CheckAddEntityID(toEgid, entityDescriptor.realType);

                _enginesRoot.QueueEntitySubmitOperation(entitySubmitOperation);
            }

            public void DeserializeEntityToDelete(EGID egid)
            {
                EntitiesDB entitiesDB                  = _enginesRoot._entitiesDB;
                ref var    serializableEntityComponent = ref entitiesDB.QueryEntity<SerializableEntityComponent>(egid);
                uint       descriptorHash              = serializableEntityComponent.descriptorHash;

                SerializationDescriptorMap serializationDescriptorMap = _enginesRoot.serializationDescriptorMap;
                var entityDescriptor = serializationDescriptorMap.GetDescriptorFromHash(descriptorHash);

                _enginesRoot.CheckRemoveEntityID(egid, entityDescriptor.realType);

                var entitySubmitOperation =
                    new EntitySubmitOperation(EntitySubmitOperationType.Remove, egid, egid
                                            , entityDescriptor.componentsToBuild);

                _enginesRoot.QueueEntitySubmitOperation(entitySubmitOperation);
            }

            public void RegisterSerializationFactory<T>(IDeserializationFactory deserializationFactory)
                where T : ISerializableEntityDescriptor, new()
            {
                SerializationDescriptorMap serializationDescriptorMap = _enginesRoot.serializationDescriptorMap;
                serializationDescriptorMap.RegisterSerializationFactory<T>(deserializationFactory);
            }

            internal EntitySerialization(EnginesRoot enginesRoot) { _enginesRoot = enginesRoot; }

            void SerializeEntityComponent
            (EGID entityGID, ISerializableComponentBuilder componentBuilder, ISerializationData serializationData
           , int serializationType)
            {
                ExclusiveGroupStruct groupId    = entityGID.groupID;
                Type                 entityType = componentBuilder.GetEntityComponentType();
                if (!_enginesRoot._entitiesDB.UnsafeQueryEntityDictionary(groupId, entityType, out var safeDictionary))
                {
                    throw new Exception("Entity Serialization failed");
                }

                componentBuilder.Serialize(entityGID.entityID, safeDictionary, serializationData, serializationType);
            }

            void DeserializeEntityInternal
            (ISerializationData serializationData, EGID egid, SerializableEntityHeader serializableEntityHeader
           , int serializationType)
            {
                SerializationDescriptorMap descriptorMap = _enginesRoot.serializationDescriptorMap;
                var entityDescriptor = descriptorMap.GetDescriptorFromHash(serializableEntityHeader.descriptorHash);

                if (_enginesRoot._groupEntityComponentsDB.TryGetValue(egid.groupID, out var entitiesInGroupPerType)
                 == false)
                    throw new Exception("Entity Serialization failed");

                foreach (var serializableEntityBuilder in entityDescriptor.entitiesToSerialize)
                {
                    entitiesInGroupPerType.TryGetValue(
                        new RefWrapperType(serializableEntityBuilder.GetEntityComponentType()), out var safeDictionary);

                    serializationData.BeginNextEntityComponent();
                    serializableEntityBuilder.Deserialize(egid.entityID, safeDictionary, serializationData
                                                        , serializationType);
                }
            }

            readonly EnginesRoot _enginesRoot;
        }

        public IEntitySerialization GenerateEntitySerializer() { return new EntitySerialization(this); }

        readonly bool _isDeserializationOnly;
    }
}