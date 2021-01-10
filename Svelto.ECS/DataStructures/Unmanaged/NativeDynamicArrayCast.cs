using System.Runtime.CompilerServices;

namespace Svelto.ECS.DataStructures
{
    public struct NativeDynamicArrayCast<T> where T : struct
    {
        public NativeDynamicArrayCast(NativeDynamicArray array) : this() { _array = array; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count() => _array.Count<T>();

        public int count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array.Count<T>();
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _array.Get<T>((uint) index);
        }

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _array.Get<T>(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T id) { _array.Add(id); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnorderedRemoveAt(uint index) { _array.UnorderedRemoveAt<T>(index); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(uint index) { _array.RemoveAt<T>(index); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() { _array.FastClear(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() { _array.Dispose(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddAt(uint lastIndex) { return ref _array.AddAt<T>(lastIndex); }

        public bool isValid => _array.isValid;

        NativeDynamicArray _array;
    }
}