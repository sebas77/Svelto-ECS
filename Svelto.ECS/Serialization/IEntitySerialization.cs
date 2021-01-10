namespace Svelto.ECS.Serialization
{
    public interface IEntitySerialization
    {
        /// <summary>
        /// Fill the serializationData of the entitiesToSerialize of this descriptor
        /// </summary>
        /// <param name="egid"></param>
        /// <param name="serializedData"></param>
        /// <param name="serializationType"></param>
        /// <returns>Size in bytes of the newly instantiated entity</returns>
        void SerializeEntity(EGID egid, ISerializationData serializationData, int serializationType);

        /// <summary>
        /// Deserialize a serializationData and copy directly onto the appropriate entities
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataPos"></param>
        /// <param name="serializationType"></param>
        void DeserializeEntity(ISerializationData serializationData, int serializationType);

        /// <summary>
        /// Deserialize a serializationData and copy directly onto the appropriate entities with explicit EGID
        /// </summary>
        /// <param name="egid"></param>
        /// <param name="data"></param>
        /// <param name="dataPos"></param>
        /// <param name="serializationType"></param>
        void DeserializeEntity(EGID egid, ISerializationData serializationData, int serializationType);
        
        /// <summary>
        /// Deserialize a serializationData and copy directly to an previously created EntityInitializer
        /// </summary>
        /// <param name="serializationData"></param>
        /// <param name="entityDescriptor"></param>
        /// <param name="initializer"></param>
        /// <param name="serializationType"></param>
        void DeserializeEntityComponents(ISerializationData serializationData,
            ISerializableEntityDescriptor entityDescriptor,
            ref EntityInitializer initializer, int serializationType);

        /// <summary>
        /// Contrary to the other Deserialize methods that assume that the entity exists, this method is used to deserialise
        /// a new Entity
        /// </summary>
        /// <param name="egid"></param>
        /// <param name="serializationData"></param>
        /// <param name="serializationType"></param>
        /// <returns></returns>
        EntityInitializer DeserializeNewEntity(EGID egid, ISerializationData serializationData,
                                                        int serializationType);

        /// <summary>
        /// Special Entity Swap method that works without knowing the EntityDescriptor to swap
        /// </summary>
        /// <param name="localEgid"></param>
        /// <param name="toEgid"></param>
        void DeserializeEntityToSwap(EGID localEgid, EGID toEgid);

        /// <summary>
        /// Special Entity delete method that works without knowing the EntityDescriptor to delete
        /// </summary>
        /// <param name="egid"></param>
        void DeserializeEntityToDelete(EGID egid);

        void RegisterSerializationFactory<T>(IDeserializationFactory deserializationFactory)
            where T : ISerializableEntityDescriptor, new();

        T DeserializeEntityComponent<T>(ISerializationData serializationData,
            ISerializableEntityDescriptor entityDescriptor, int serializationType) 
            where T : unmanaged, IEntityComponent;
    }
}