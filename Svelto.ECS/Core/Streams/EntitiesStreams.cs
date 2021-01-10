using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    /// <summary>
    ///     I eventually realised that, with the ECS design, no form of communication other than polling entity components can
    ///     exist.
    ///     Using groups, you can have always an optimal set of entity components to poll. However EntityStreams
    ///     can be useful if:
    ///     - you need to react on seldom entity changes, usually due to user events
    ///     - you want engines to be able to track entity changes
    ///     - you want a thread-safe way to read entity states, which includes all the state changes and not the last
    ///     one only
    ///     - you want to communicate between EnginesRoots
    /// </summary>
    internal struct EntitiesStreams : IDisposable
    {
        internal Consumer<T> GenerateConsumer<T>(string name, uint capacity)
            where T : unmanaged, IEntityComponent
        {
            if (_streams.ContainsKey(TypeRefWrapper<T>.wrapper) == false)
                _streams[TypeRefWrapper<T>.wrapper] = new EntityStream<T>();

            return (_streams[TypeRefWrapper<T>.wrapper] as EntityStream<T>).GenerateConsumer(name, capacity);
        }

        public Consumer<T> GenerateConsumer<T>(ExclusiveGroupStruct group, string name, uint capacity)
            where T : unmanaged, IEntityComponent
        {
            if (_streams.ContainsKey(TypeRefWrapper<T>.wrapper) == false)
                _streams[TypeRefWrapper<T>.wrapper] = new EntityStream<T>();

            var typeSafeStream = (EntityStream<T>) _streams[TypeRefWrapper<T>.wrapper];
            return typeSafeStream.GenerateConsumer(group, name, capacity);
        }
#if later
        public ThreadSafeNativeEntityStream<T> GenerateThreadSafePublisher<T>(EntitiesDB entitiesDB) where T: unmanaged, IEntityComponent
        {
            var threadSafeNativeEntityStream = new ThreadSafeNativeEntityStream<T>(entitiesDB);
            
            _streams[TypeRefWrapper<T>.wrapper] = threadSafeNativeEntityStream;

            return threadSafeNativeEntityStream;
        }
#endif

        internal void PublishEntity<T>(ref T entity, EGID egid) where T : unmanaged, IEntityComponent
        {
            if (_streams.TryGetValue(TypeRefWrapper<T>.wrapper, out var typeSafeStream))
                (typeSafeStream as EntityStream<T>).PublishEntity(ref entity, egid);
            else
                Console.LogDebug("No Consumers are waiting for this entity to change ", typeof(T));
        }

        public void Dispose()
        {
            foreach (var stream in _streams)
                stream.Value.Dispose();
        }

        public static EntitiesStreams Create()
        {
            var stream = new EntitiesStreams();
            stream._streams = ManagedSveltoDictionary<RefWrapperType, ITypeSafeStream>.Create();

            return stream;
        }

        ManagedSveltoDictionary<RefWrapperType, ITypeSafeStream> _streams;
    }
}