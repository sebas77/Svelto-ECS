using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;

namespace Svelto.ECS
{
    public static class EntityCollectionExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1>
            (in this EntityCollection<T1> ec, out NB<T1> buffer, out int count) where T1 : unmanaged, IEntityComponent
        {
            buffer = ec._nativedBuffer;
            count  = (int) ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2>
            (in this EntityCollection<T1, T2> ec, out NB<T1> buffer1, out NB<T2> buffer2, out int count)
            where T1 : unmanaged, IEntityComponent where T2 : unmanaged, IEntityComponent
        {
            buffer1 = ec.buffer1._nativedBuffer;
            buffer2 = ec.buffer2._nativedBuffer;
            count   = (int) ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3>
        (in this EntityCollection<T1, T2, T3> ec, out NB<T1> buffer1, out NB<T2> buffer2, out NB<T3> buffer3
       , out int count) where T1 : unmanaged, IEntityComponent
                        where T2 : unmanaged, IEntityComponent
                        where T3 : unmanaged, IEntityComponent
        {
            buffer1 = ec.buffer1._nativedBuffer;
            buffer2 = ec.buffer2._nativedBuffer;
            buffer3 = ec.buffer3._nativedBuffer;
            count   = (int) ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3, T4>
        (in this EntityCollection<T1, T2, T3, T4> ec, out NB<T1> buffer1, out NB<T2> buffer2, out NB<T3> buffer3
       , out NB<T4> buffer4, out int count) where T1 : unmanaged, IEntityComponent
                                            where T2 : unmanaged, IEntityComponent
                                            where T3 : unmanaged, IEntityComponent
                                            where T4 : unmanaged, IEntityComponent
        {
            buffer1 = ec.buffer1._nativedBuffer;
            buffer2 = ec.buffer2._nativedBuffer;
            buffer3 = ec.buffer3._nativedBuffer;
            buffer4 = ec.buffer4._nativedBuffer;
            count   = (int) ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BT<NB<T1>> ToBuffer<T1>(in this EntityCollection<T1> ec) where T1 : unmanaged, IEntityComponent
        {
            return new BT<NB<T1>>(ec._nativedBuffer, ec.count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BT<NB<T1>, NB<T2>> ToBuffers<T1, T2>
            (in this EntityCollection<T1, T2> ec)
            where T2 : unmanaged, IEntityComponent where T1 : unmanaged, IEntityComponent
        {
            return new BT<NB<T1>, NB<T2>>(ec.buffer1._nativedBuffer, ec.buffer2._nativedBuffer, ec.count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BT<NB<T1>, NB<T2>, NB<T3>> ToBuffers<T1, T2, T3>
            (in this EntityCollection<T1, T2, T3> ec)
            where T2 : unmanaged, IEntityComponent
            where T1 : unmanaged, IEntityComponent
            where T3 : unmanaged, IEntityComponent
        {
            return new BT<NB<T1>, NB<T2>, NB<T3>>(ec.buffer1._nativedBuffer, ec.buffer2._nativedBuffer
                                                , ec.buffer3._nativedBuffer, ec.count);
        }
    }

    public static class EntityCollectionExtensionB
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1>
            (in this EntityCollection<T1> ec, out MB<T1> buffer, out int count) where T1 : struct, IEntityViewComponent
        {
            buffer = ec._managedBuffer;
            count  = (int) ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BT<MB<T1>> ToBuffer<T1>(in this EntityCollection<T1> ec) where T1 : struct, IEntityViewComponent
        {
            return new BT<MB<T1>>(ec._managedBuffer, ec.count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2>
            (in this EntityCollection<T1, T2> ec, out MB<T1> buffer1, out MB<T2> buffer2, out int count)
            where T1 : struct, IEntityViewComponent where T2 : struct, IEntityViewComponent
        {
            buffer1 = ec.buffer1._managedBuffer;
            buffer2 = ec.buffer2._managedBuffer;
            count   = (int) ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (MB<T1> buffer1, MB<T2> buffer2, uint count) ToBuffers<T1, T2>
            (in this EntityCollection<T1, T2> ec)
            where T2 : struct, IEntityViewComponent where T1 : struct, IEntityViewComponent
        {
            return (ec.buffer1._managedBuffer, ec.buffer2._managedBuffer, ec.count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3>
        (in this EntityCollection<T1, T2, T3> ec, out MB<T1> buffer1, out MB<T2> buffer2, out MB<T3> buffer3
       , out int count) where T1 : struct, IEntityViewComponent
                        where T2 : struct, IEntityViewComponent
                        where T3 : struct, IEntityViewComponent
        {
            buffer1 = ec.buffer1._managedBuffer;
            buffer2 = ec.buffer2._managedBuffer;
            buffer3 = ec.buffer3._managedBuffer;
            count   = (int) ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (MB<T1> buffer1, MB<T2> buffer2, MB<T3> buffer3, uint count) ToBuffers<T1, T2, T3>
            (in this EntityCollection<T1, T2, T3> ec)
            where T2 : struct, IEntityViewComponent
            where T1 : struct, IEntityViewComponent
            where T3 : struct, IEntityViewComponent
        {
            return (ec.buffer1._managedBuffer, ec.buffer2._managedBuffer, ec.buffer3._managedBuffer, ec.count);
        }
    }

    public static class EntityCollectionExtensionC
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (NB<T1> buffer1, MB<T2> buffer2, uint count) ToBuffers<T1, T2>
            (in this EntityCollection<T1, T2> ec)
            where T1 : unmanaged, IEntityComponent where T2 : struct, IEntityViewComponent
        {
            return (ec.buffer1._nativedBuffer, ec.buffer2._managedBuffer, ec.count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (NB<T1> buffer1, MB<T2> buffer2, MB<T3> buffer3, uint count) ToBuffers<T1, T2, T3>
            (in this EntityCollection<T1, T2, T3> ec)
            where T1 : unmanaged, IEntityComponent
            where T2 : struct, IEntityViewComponent
            where T3 : struct, IEntityViewComponent
        {
            return (ec.buffer1._nativedBuffer, ec.buffer2._managedBuffer, ec.buffer3._managedBuffer, ec.count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2>
            (in this EntityCollection<T1, T2> ec, out NB<T1> buffer1, out MB<T2> buffer2, out int count)
            where T1 : unmanaged, IEntityComponent where T2 : struct, IEntityViewComponent
        {
            buffer1 = ec.buffer1._nativedBuffer;
            buffer2 = ec.buffer2._managedBuffer;
            count   = (int) ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3>
            (in this EntityCollection<T1, T2, T3> ec,  out NB<T1> buffer1, out MB<T2> buffer2, out MB<T3> buffer3, out int count)
            where T1 : unmanaged, IEntityComponent
            where T2 : struct, IEntityViewComponent
            where T3 : struct, IEntityViewComponent
        {
            buffer1 = ec.buffer1._nativedBuffer;
            buffer2 = ec.buffer2._managedBuffer;
            buffer3 = ec.buffer3._managedBuffer;
            count   = (int) ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3, T4>
        (in this EntityCollection<T1, T2, T3, T4> ec, out NB<T1> buffer1, out NB<T2> buffer2, out NB<T3> buffer3
       , out MB<T4> buffer4, out int count) where T1 : unmanaged, IEntityComponent
                                            where T2 : unmanaged, IEntityComponent
                                            where T3 : unmanaged, IEntityComponent
                                            where T4 : struct, IEntityViewComponent
        {
            buffer1 = ec.buffer1._nativedBuffer;
            buffer2 = ec.buffer2._nativedBuffer;
            buffer3 = ec.buffer3._nativedBuffer;
            buffer4 = ec.buffer4._managedBuffer;
            count   = (int) ec.count;
        }
    }

    public static class EntityCollectionExtensionD
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3>
        (in this EntityCollection<T1, T2, T3> ec, out NB<T1> buffer1, out NB<T2> buffer2, out MB<T3> buffer3
       , out int count) where T1 : unmanaged, IEntityComponent
                        where T2 : unmanaged, IEntityComponent
                        where T3 : struct, IEntityViewComponent
        {
            buffer1 = ec.buffer1._nativedBuffer;
            buffer2 = ec.buffer2._nativedBuffer;
            buffer3 = ec.buffer3._managedBuffer;
            count   = (int) ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (NB<T1> buffer1, NB<T2> buffer2, MB<T3> buffer3, uint count) ToBuffers<T1, T2, T3>
            (in this EntityCollection<T1, T2, T3> ec)
            where T1 : unmanaged, IEntityComponent
            where T2 : unmanaged, IEntityComponent
            where T3 : struct, IEntityViewComponent
        {
            return (ec.buffer1._nativedBuffer, ec.buffer2._nativedBuffer, ec.buffer3._managedBuffer, ec.count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BT<NB<T1>, NB<T2>, NB<T3>, NB<T4>> ToBuffers<T1, T2, T3, T4>
            (in this EntityCollection<T1, T2, T3, T4> ec)
            where T2 : unmanaged, IEntityComponent
            where T1 : unmanaged, IEntityComponent
            where T3 : unmanaged, IEntityComponent
            where T4 : unmanaged, IEntityComponent
        {
            return new BT<NB<T1>, NB<T2>, NB<T3>, NB<T4>>(ec.buffer1._nativedBuffer, ec.buffer2._nativedBuffer
                                                        , ec.buffer3._nativedBuffer, ec.buffer4._nativedBuffer, ec.count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3, T4>
        (in this EntityCollection<T1, T2, T3, T4> ec, out NB<T1> buffer1, out NB<T2> buffer2, out MB<T3> buffer3
       , out MB<T4> buffer4, out int count) where T1 : unmanaged, IEntityComponent
                                            where T2 : unmanaged, IEntityComponent
                                            where T3 : struct, IEntityViewComponent
                                            where T4 : struct, IEntityViewComponent
        {
            buffer1 = ec.buffer1._nativedBuffer;
            buffer2 = ec.buffer2._nativedBuffer;
            buffer3 = ec.buffer3._managedBuffer;
            buffer4 = ec.buffer4._managedBuffer;
            count   = (int) ec.count;
        }
    }
}