using Svelto.DataStructures;

namespace Svelto.ECS
{
    internal interface ITypeSafeStream
    {
        void Dispose();
    }

    public class EntityStream<T> : ITypeSafeStream where T : unmanaged, IEntityComponent
    {
        readonly ThreadSafeFasterList<Consumer<T>> _consumers;

        internal EntityStream()
        {
            _consumers = new ThreadSafeFasterList<Consumer<T>>();
        }

        ~EntityStream()
        {
            for (var i = 0; i < _consumers.count; i++)
                _consumers[i].Free();
        }

        public void Dispose()
        { }

        internal void PublishEntity(ref T entity, EGID egid)
        {
            for (var i = 0; i < _consumers.count; i++)
                unsafe
                {
                    if (*(bool*) _consumers[i].mustBeDisposed)
                    {
                        _consumers[i].Free();
                        _consumers.UnorderedRemoveAt(i);
                        --i;
                        continue;
                    }

                    if (_consumers[i].hasGroup)
                    {
                        if (egid.groupID == _consumers[i].@group) 
                        _consumers[i].Enqueue(entity, egid);
                    }
                    else
                    {
                        _consumers[i].Enqueue(entity, egid);
                    }
                }
        }

        internal Consumer<T> GenerateConsumer(string name, uint capacity)
        {
            var consumer = new Consumer<T>(name, capacity);

            _consumers.Add(consumer);

            return consumer;
        }

        internal Consumer<T> GenerateConsumer(ExclusiveGroupStruct group, string name, uint capacity)
        {
            var consumer = new Consumer<T>(group, name, capacity);

            _consumers.Add(consumer);

            return consumer;
        }
    }
}