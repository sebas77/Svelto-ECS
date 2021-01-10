using System.Runtime.CompilerServices;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS
{
    public readonly struct FilteredIndices
    {
        public FilteredIndices(NativeDynamicArrayCast<uint> denseListOfIndicesToEntityComponentArray)
        {
            _denseListOfIndicesToEntityComponentArray = denseListOfIndicesToEntityComponentArray;
            _count                                    = _denseListOfIndicesToEntityComponentArray.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count() => _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Get(uint index) => _denseListOfIndicesToEntityComponentArray[index];

        public uint this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _denseListOfIndicesToEntityComponentArray[index];
        }

        public uint this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _denseListOfIndicesToEntityComponentArray[index];
        }

        readonly NativeDynamicArrayCast<uint> _denseListOfIndicesToEntityComponentArray;
        readonly int                          _count;
    }
}