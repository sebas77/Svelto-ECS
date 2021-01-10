using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    /// <summary>
    /// Note: does mono devirtualize sealed classes? If so it could be worth to use TypeSafeDictionary instead of
    /// the interface 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct EGIDMapper<T>: IEGIDMapper where T : struct, IEntityComponent
    {
        public uint                 length          => _map.count;
        public ExclusiveGroupStruct groupID         { get; }
        public Type                 entityType            => TypeCache<T>.type;

        internal EGIDMapper(ExclusiveGroupStruct groupStructId, ITypeSafeDictionary<T> dic) : this()
        {
            groupID = groupStructId;
            _map     = dic;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Entity(uint entityID)
        {
#if DEBUG && !PROFILE_SVELTO
            if (_map == null)
                throw new System.Exception("Not initialized EGIDMapper in this group ".FastConcat(typeof(T).ToString()));
                if (_map.TryFindIndex(entityID, out var findIndex) == false)
                    throw new System.Exception("Entity not found in this group ".FastConcat(typeof(T).ToString()));
#else
            _map.TryFindIndex(entityID, out var findIndex);
#endif
            return ref _map.GetDirectValueByRef(findIndex);
        }

        public bool TryGetEntity(uint entityID, out T value)
        {
            if (_map != null && _map.TryFindIndex(entityID, out var index))
            {
                value = _map.GetDirectValueByRef(index);
                return true;
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists(uint idEntityId)
        {
            return _map.count > 0 && _map.TryFindIndex(idEntityId, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(uint entityID)
        {
            return _map.GetIndex(entityID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FindIndex(uint valueKey, out uint index)
        {
            return _map.TryFindIndex(valueKey, out index);
        }

        internal readonly ITypeSafeDictionary<T> _map;
    }

    public interface IEGIDMapper
    {
        bool FindIndex(uint valueKey, out uint index);
        uint GetIndex(uint entityID);
        bool Exists(uint idEntityId);
        
        ExclusiveGroupStruct groupID { get; }
        Type          entityType    { get; }
    }
}