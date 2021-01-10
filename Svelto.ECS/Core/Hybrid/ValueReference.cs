using System.Runtime.InteropServices;

namespace Svelto.ECS.Hybrid
{
    public struct ValueReference<T> where T:class, IImplementor
    {
        public ValueReference(T obj) { _pointer = GCHandle.Alloc(obj, GCHandleType.Normal); }

        public static explicit operator T(ValueReference<T> t) => (T) t._pointer.Target;

        GCHandle _pointer;
    }
}