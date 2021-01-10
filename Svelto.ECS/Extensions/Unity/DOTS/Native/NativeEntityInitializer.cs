using Svelto.ECS.DataStructures;

namespace Svelto.ECS
{
    public readonly ref struct NativeEntityInitializer
    {
        readonly NativeBag        _unsafeBuffer;
        readonly UnsafeArrayIndex _index;

        public NativeEntityInitializer(in NativeBag unsafeBuffer, UnsafeArrayIndex index)
        {
            _unsafeBuffer = unsafeBuffer;
            _index        = index;
        }

        public void Init<T>(in T component) where T : unmanaged, IEntityComponent
        {
            uint id = EntityComponentID<T>.ID.Data;

            _unsafeBuffer.AccessReserved<uint>(_index)++;

            _unsafeBuffer.Enqueue(id);
            _unsafeBuffer.Enqueue(component);
        }
    }
}