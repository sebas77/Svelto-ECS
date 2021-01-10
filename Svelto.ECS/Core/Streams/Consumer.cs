using System;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public struct Consumer<T> : IDisposable where T : unmanaged, IEntityComponent
    {
        internal Consumer(string name, uint capacity) : this()
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                _name = name;
#endif
                _ringBuffer = new RingBuffer<ValueTuple<T, EGID>>((int) capacity,
#if DEBUG && !PROFILE_SVELTO
                    _name
#else
                string.Empty
#endif
                );
                mustBeDisposed          = MemoryUtilities.Alloc(sizeof(bool), Allocator.Persistent);
                *(bool*) mustBeDisposed = false;
            }
        }

        internal Consumer(ExclusiveGroupStruct group, string name, uint capacity) : this(name, capacity)
        {
            this.group = group;
            hasGroup   = true;
        }

        internal void Enqueue(in T entity, in EGID egid)
        {
            _ringBuffer.Enqueue((entity, egid));
        }

        public bool TryDequeue(out T entity)
        {
            var tryDequeue = _ringBuffer.TryDequeue(out var values);

            entity = values.Item1;

            return tryDequeue;
        }

        public bool TryDequeue(out T entity, out EGID id)
        {
            var tryDequeue = _ringBuffer.TryDequeue(out var values);

            entity = values.Item1;
            id     = values.Item2;

            return tryDequeue;
        }

        public void Flush()
        {
            _ringBuffer.Reset();
        }

        public void Dispose()
        {
            unsafe
            {
                *(bool*) mustBeDisposed = true;
            }
        }

        public uint Count()
        {
            return (uint) _ringBuffer.Count;
        }

        public void Free()
        {
            MemoryUtilities.Free(mustBeDisposed, Allocator.Persistent);
        }

        readonly RingBuffer<ValueTuple<T, EGID>> _ringBuffer;

        internal readonly ExclusiveGroupStruct group;
        internal readonly bool                 hasGroup;
        internal          IntPtr               mustBeDisposed;

#if DEBUG && !PROFILE_SVELTO
        readonly string _name;
#endif
    }
}