namespace Svelto.ECS
{
    /// <summary>
    /// This EntityStream can be used in parallel jobs, but does NOT guarantee order.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ThreadSafeNativeEntityStream<T> : ITypeSafeStream
    {
        public ThreadSafeNativeEntityStream(EntitiesDB entitiesDB)
        {
        }

        public void Dispose()
        {
            
        }

        /// <summary>
        /// I am thinking to pass the component and do the queryEntity only as a validation
        /// </summary>
        /// <param name="entityComponent"></param>
        /// <param name="id"></param>
        public void PublishEntityChange(in T entityComponent, EGID id)
        {
#if DEBUG && !PROFILE_SVELTO
            
#endif            
            
        }
    }
}