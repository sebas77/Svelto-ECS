#if UNITY_NATIVE
using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public struct NativeEGIDMultiMapper<T>:IDisposable where T : unmanaged, IEntityComponent
    {
        SveltoDictionary<ExclusiveGroupStruct, SveltoDictionary<uint, T, 
                NativeStrategy<SveltoDictionaryNode<uint>>,
                NativeStrategy<T>, 
                NativeStrategy<int>>, 
            NativeStrategy<SveltoDictionaryNode<ExclusiveGroupStruct>>, 
            NativeStrategy<SveltoDictionary<uint, T, 
                NativeStrategy<SveltoDictionaryNode<uint>>, 
                NativeStrategy<T>,
                NativeStrategy<int>>>,
            NativeStrategy<int>> _dic;

        public NativeEGIDMultiMapper
        (SveltoDictionary<ExclusiveGroupStruct, SveltoDictionary<uint, T, 
                NativeStrategy<SveltoDictionaryNode<uint>>,
                NativeStrategy<T>, 
                NativeStrategy<int>>, 
            NativeStrategy<SveltoDictionaryNode<ExclusiveGroupStruct>>, 
            NativeStrategy<SveltoDictionary<uint, T, 
                NativeStrategy<SveltoDictionaryNode<uint>>, 
                NativeStrategy<T>,
                NativeStrategy<int>>>,
            NativeStrategy<int>> dictionary)
        {
            _dic = dictionary;
        }

        public int count => (int) _dic.count;

        public void Dispose()
        {
            _dic.Dispose();
        }

        public ref T Entity(EGID entity)
        {
            ref var sveltoDictionary = ref _dic.GetValueByRef(entity.groupID);
            return ref sveltoDictionary.GetValueByRef(entity.entityID);
        }

        public bool Exists(EGID entity)
        {
            return _dic.TryFindIndex(entity.groupID, out var index)
                && _dic.GetDirectValueByRef(index).ContainsKey(entity.entityID);
        }
    }
}
#endif