using Svelto.ECS.Internal;

namespace Svelto.ECS.Serialization
{
    public interface ISerializableComponentBuilder : IComponentBuilder
    {
        void Serialize(uint id, ITypeSafeDictionary dictionary, ISerializationData serializationData
                     , int serializationType);

        void Deserialize(uint id, ITypeSafeDictionary dictionary, ISerializationData serializationData
                       , int serializationType);

        void Deserialize(ISerializationData serializationData, in EntityInitializer initializer
                       , int serializationType);
    }
}