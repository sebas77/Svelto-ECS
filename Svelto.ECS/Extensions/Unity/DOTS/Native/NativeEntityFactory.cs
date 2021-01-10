#if UNITY_NATIVE
using Svelto.ECS.DataStructures;

namespace Svelto.ECS
{
    public readonly struct NativeEntityFactory
    {
        readonly AtomicNativeBags _addOperationQueue;
        readonly int             _index;

        internal NativeEntityFactory(AtomicNativeBags addOperationQueue, int index)
        {
            _index             = index;
            _addOperationQueue = addOperationQueue;
        }

        public NativeEntityInitializer BuildEntity
            (uint eindex, ExclusiveBuildGroup exclusiveBuildGroup, int threadIndex)
        {
            NativeBag unsafeBuffer = _addOperationQueue.GetBuffer(threadIndex + 1);

            unsafeBuffer.Enqueue(_index);
            unsafeBuffer.Enqueue(new EGID(eindex, exclusiveBuildGroup));
            unsafeBuffer.ReserveEnqueue<uint>(out var index) = 0;

            return new NativeEntityInitializer(unsafeBuffer, index);
        }
        
        public NativeEntityInitializer BuildEntity(EGID egid, int threadIndex)
        {
            NativeBag unsafeBuffer = _addOperationQueue.GetBuffer(threadIndex + 1);

            unsafeBuffer.Enqueue(_index);
            unsafeBuffer.Enqueue(new EGID(egid.entityID, egid.groupID));
            unsafeBuffer.ReserveEnqueue<uint>(out var index) = 0;

            return new NativeEntityInitializer(unsafeBuffer, index);
        }
    }
}
#endif