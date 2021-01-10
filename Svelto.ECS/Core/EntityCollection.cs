using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public readonly ref struct EntityCollection<T> where T : struct, IEntityComponent
    {
        static readonly bool IsUnmanaged = TypeSafeDictionary<T>.IsUnmanaged;

        public EntityCollection(IBuffer<T> buffer, uint count) : this()
        {
            DBC.ECS.Check.Require(count == 0 || buffer.isValid, "Buffer is found in impossible state");
            if (IsUnmanaged)
                _nativedBuffer = (NB<T>) buffer;
            else
                _managedBuffer = (MB<T>) buffer;

            _count = count;
        }

        public uint count => _count;

        internal readonly MB<T> _managedBuffer;
        internal readonly NB<T> _nativedBuffer;

        readonly uint _count;
    }

    public readonly ref struct EntityCollection<T1, T2>
        where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent
    {
        internal EntityCollection(in EntityCollection<T1> array1, in EntityCollection<T2> array2)
        {
            _array1 = array1;
            _array2 = array2;
        }

        public uint count => _array1.count;

        internal EntityCollection<T2> buffer2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array2;
        }

        internal EntityCollection<T1> buffer1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array1;
        }

        readonly EntityCollection<T1> _array1;
        readonly EntityCollection<T2> _array2;
    }

    public readonly ref struct EntityCollection<T1, T2, T3> where T3 : struct, IEntityComponent
                                                            where T2 : struct, IEntityComponent
                                                            where T1 : struct, IEntityComponent
    {
        internal EntityCollection
            (in EntityCollection<T1> array1, in EntityCollection<T2> array2, in EntityCollection<T3> array3)
        {
            _array1 = array1;
            _array2 = array2;
            _array3 = array3;
        }

        internal EntityCollection<T1> buffer1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array1;
        }

        internal EntityCollection<T2> buffer2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array2;
        }

        internal EntityCollection<T3> buffer3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array3;
        }

        internal uint count => buffer1.count;

        readonly EntityCollection<T1> _array1;
        readonly EntityCollection<T2> _array2;
        readonly EntityCollection<T3> _array3;
    }

    public readonly ref struct EntityCollection<T1, T2, T3, T4> where T1 : struct, IEntityComponent
                                                                where T2 : struct, IEntityComponent
                                                                where T3 : struct, IEntityComponent
                                                                where T4 : struct, IEntityComponent
    {
        internal EntityCollection
        (in EntityCollection<T1> array1, in EntityCollection<T2> array2, in EntityCollection<T3> array3
       , in EntityCollection<T4> array4)
        {
            _array1 = array1;
            _array2 = array2;
            _array3 = array3;
            _array4 = array4;
        }

        internal EntityCollection<T1> buffer1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array1;
        }

        internal EntityCollection<T2> buffer2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array2;
        }

        internal EntityCollection<T3> buffer3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array3;
        }

        internal EntityCollection<T4> buffer4
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array4;
        }

        internal uint count => _array1.count;

        readonly EntityCollection<T1> _array1;
        readonly EntityCollection<T2> _array2;
        readonly EntityCollection<T3> _array3;
        readonly EntityCollection<T4> _array4;
    }

    public readonly struct BT<BufferT1, BufferT2, BufferT3, BufferT4>
    {
        public readonly BufferT1 buffer1;
        public readonly BufferT2 buffer2;
        public readonly BufferT3 buffer3;
        public readonly BufferT4 buffer4;
        public readonly int      count;

        public BT(BufferT1 bufferT1, BufferT2 bufferT2, BufferT3 bufferT3, BufferT4 bufferT4, uint count) : this()
        {
            this.buffer1 = bufferT1;
            this.buffer2 = bufferT2;
            this.buffer3 = bufferT3;
            this.buffer4 = bufferT4;
            this.count   = (int) count;
        }
    }

    public readonly struct BT<BufferT1, BufferT2, BufferT3>
    {
        public readonly BufferT1 buffer1;
        public readonly BufferT2 buffer2;
        public readonly BufferT3 buffer3;
        public readonly int      count;

        public BT(BufferT1 bufferT1, BufferT2 bufferT2, BufferT3 bufferT3, uint count) : this()
        {
            this.buffer1 = bufferT1;
            this.buffer2 = bufferT2;
            this.buffer3 = bufferT3;
            this.count   = (int) count;
        }

        public void Deconstruct(out BufferT1 bufferT1, out BufferT2 bufferT2, out BufferT3 bufferT3, out int count)
        {
            bufferT1 = buffer1;
            bufferT2 = buffer2;
            bufferT3 = buffer3;
            count    = this.count;
        }
    }

    public readonly struct BT<BufferT1>
    {
        public readonly BufferT1 buffer;
        public readonly int      count;

        public BT(BufferT1 bufferT1, uint count) : this()
        {
            this.buffer = bufferT1;
            this.count  = (int) count;
        }

        public void Deconstruct(out BufferT1 bufferT1, out int count)
        {
            bufferT1 = buffer;
            count    = this.count;
        }

        public static implicit operator BufferT1(BT<BufferT1> t) => t.buffer;
    }

    public readonly struct BT<BufferT1, BufferT2>
    {
        public readonly BufferT1 buffer1;
        public readonly BufferT2 buffer2;
        public readonly int      count;

        public BT(BufferT1 bufferT1, BufferT2 bufferT2, uint count) : this()
        {
            this.buffer1 = bufferT1;
            this.buffer2 = bufferT2;
            this.count   = (int) count;
        }

        public void Deconstruct(out BufferT1 bufferT1, out BufferT2 bufferT2, out int count)
        {
            bufferT1 = buffer1;
            bufferT2 = buffer2;
            count    = this.count;
        }
    }
}