using System.Threading;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS
{
    public class GlobalTypeID
    {
        internal static uint NextID<T>() { return (uint) (Interlocked.Increment(ref value) - 1); }

        static GlobalTypeID() { value = 0; }

        static int value;
    }

    interface IFiller
    {
        void FillFromByteArray(EntityInitializer init, NativeBag buffer);
    }

    class Filler<T> : IFiller where T : struct, IEntityComponent
    {
        static Filler()
        {
            DBC.ECS.Check.Require(TypeCache<T>.IsUnmanaged == true, "invalid type used");
        }

        //it's an internal interface
        public void FillFromByteArray(EntityInitializer init, NativeBag buffer)
        {
            var component = buffer.Dequeue<T>();

            init.Init(component);
        }
    }

    static class EntityComponentID<T>
    {
#if UNITY_NATIVE
        internal static readonly Unity.Burst.SharedStatic<uint> ID =
            Unity.Burst.SharedStatic<uint>.GetOrCreate<GlobalTypeID, T>();
#else
        internal struct SharedStatic
        {
            public uint Data;
        }

        internal static SharedStatic ID;
#endif
    }

    static class EntityComponentIDMap
    {
        static readonly FasterList<IFiller> TYPE_IDS = new FasterList<IFiller>();

        internal static void Register<T>(IFiller entityBuilder) where T : struct, IEntityComponent
        {
            var location = EntityComponentID<T>.ID.Data = GlobalTypeID.NextID<T>();
            TYPE_IDS.AddAt(location, entityBuilder);
        }

        internal static IFiller GetTypeFromID(uint typeId) { return TYPE_IDS[typeId]; }
    }
}