using System.Runtime.CompilerServices;

namespace Svelto.ECS
{
    public partial class EntitiesDB
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PublishEntityChange<T>(EGID egid) where T : unmanaged, IEntityComponent
        {
            _entityStream.PublishEntity(ref this.QueryEntity<T>(egid), egid);
        }
#if later
        public ThreadSafeNativeEntityStream<T> GenerateThreadSafePublisher<T>() where T: unmanaged, IEntityComponent
        {
            return _entityStream.GenerateThreadSafePublisher<T>(this);
        }
#endif
    }
}