using System;
using Svelto.Common;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Serialization
{
    public struct SerializersInfo<SerializationEnum> where SerializationEnum:Enum
    {
        public uint numberOfSerializationTypes => (uint) length;

        static readonly int length = Enum.GetNames(typeof(SerializationEnum)).Length;
    }

    public class SerializableComponentBuilder<T> : ComponentBuilder<T>, ISerializableComponentBuilder
        where T : unmanaged, IEntityComponent
    {
        public static readonly uint SIZE = (uint) MemoryUtilities.SizeOf<T>();
        
          public void Serialize
        (uint entityID, ITypeSafeDictionary dictionary, ISerializationData serializationData
       , int serializationType)
        {
            IComponentSerializer<T> componentSerializer = _serializers[serializationType];

            var safeDictionary = (ITypeSafeDictionary<T>) dictionary;
            if (safeDictionary.TryFindIndex(entityID, out uint index) == false)
            {
                throw new ECSException("Entity Serialization failed");
            }

            ref T val    = ref safeDictionary.GetDirectValueByRef(index);

            serializationData.dataPos = (uint) serializationData.data.count;

            serializationData.data.ExpandBy(componentSerializer.size);
            componentSerializer.SerializeSafe(val, serializationData);
        }

        public void Deserialize
        (uint entityID, ITypeSafeDictionary dictionary, ISerializationData serializationData
       , int serializationType)
        {
            IComponentSerializer<T> componentSerializer = _serializers[(int) serializationType];

            // Handle the case when an entity struct is gone
            var safeDictionary = (ITypeSafeDictionary<T>) dictionary;
            if (safeDictionary.TryFindIndex(entityID, out uint index) == false)
            {
                throw new ECSException("Entity Deserialization failed");
            }

            ref T val    = ref safeDictionary.GetDirectValueByRef(index);

            componentSerializer.DeserializeSafe(ref val, serializationData);
        }

        public void Deserialize
        (ISerializationData serializationData, in EntityInitializer initializer
       , int serializationType)
        {
            IComponentSerializer<T> componentSerializer = _serializers[(int) serializationType];

            componentSerializer.DeserializeSafe(ref initializer.GetOrCreate<T>(), serializationData);
        }

        public void Deserialize
            (ISerializationData serializationData, ref T entityComponent, int serializationType)
        {
            IComponentSerializer<T> componentSerializer = _serializers[(int) serializationType];
            componentSerializer.DeserializeSafe(ref entityComponent, serializationData);
        }

        private protected IComponentSerializer<T>[] _serializers;
    }
    
    public class SerializableComponentBuilder<SerializationType, T> :  SerializableComponentBuilder<T> 
        where T : unmanaged, IEntityComponent where SerializationType : Enum
    {
        static SerializableComponentBuilder() { }

        public SerializableComponentBuilder(params ValueTuple<int, IComponentSerializer<T>>[] serializers)
        {
            var length = new SerializersInfo<SerializationType>().numberOfSerializationTypes;
            
            _serializers = new IComponentSerializer<T>[(int)length];
            for (int i = 0; i < serializers.Length; i++)
            {
                ref (int, IComponentSerializer<T>) s = ref serializers[i];
                _serializers[(int) s.Item1] = s.Item2;
            }

            // Just in case the above are the same type
            if (serializers.Length > 0)
            {
                for (int i = 0; i < (int) length; i++)
                {
                    if (_serializers[i] == null)
                        _serializers[i] = new DontSerialize<T>();
                }
            }
            else
                for (int i = 0; i < (int) length; i++)
                {
                    _serializers[i] = new DefaultSerializer<T>();
                }
        }
    }
}