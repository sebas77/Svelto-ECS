#if UNITY_NATIVE
using Svelto.ECS.DataStructures;

namespace Svelto.ECS
{
    public readonly struct NativeEntitySwap
    {
        readonly AtomicNativeBags _swapQueue;
        readonly int             _indexSwap;

        internal NativeEntitySwap(AtomicNativeBags EGIDsToSwap, int indexSwap)
        {
            _swapQueue = EGIDsToSwap;
            _indexSwap = indexSwap;
        }

        public void SwapEntity(EGID from, EGID to, int threadIndex)
        {
            var simpleNativeBag = _swapQueue.GetBuffer(threadIndex);
            simpleNativeBag.Enqueue(_indexSwap);
            simpleNativeBag.Enqueue(new DoubleEGID(from, to));
            
        }

        public void SwapEntity(EGID from, ExclusiveBuildGroup to, int threadIndex)
        {
            var simpleNativeBag = _swapQueue.GetBuffer(threadIndex);
            simpleNativeBag.Enqueue(_indexSwap);
            simpleNativeBag.Enqueue(new DoubleEGID(from, new EGID(from.entityID, to)));
        }
    }
}
#endif