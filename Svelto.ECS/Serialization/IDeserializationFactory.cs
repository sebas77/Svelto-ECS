namespace Svelto.ECS.Serialization
{
    public interface IDeserializationFactory
    {
        EntityInitializer BuildDeserializedEntity
        (EGID egid, ISerializationData serializationData, ISerializableEntityDescriptor entityDescriptor
       , int serializationType, IEntitySerialization entitySerialization, IEntityFactory factory
       , bool enginesRootIsDeserializationOnly);
    }
}
