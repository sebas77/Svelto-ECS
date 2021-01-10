#if UNITY_JOBS
using Svelto.DataStructures;
using Unity.Jobs;
using Svelto.Common;

namespace Svelto.ECS.Extensions.Unity
{
    /// <summary>
    /// Note sorted jobs run in serial
    /// </summary> 
    /// <typeparam name="Interface"></typeparam>
    /// <typeparam name="SequenceOrder"></typeparam>
    public abstract class SortedJobifiedEnginesGroup<Interface, SequenceOrder> 
        where SequenceOrder : struct, ISequenceOrder where Interface : class, IJobifiedEngine
    {
        protected SortedJobifiedEnginesGroup(FasterList<Interface> engines)
        {
            _name = "SortedJobifiedEnginesGroup - "+this.GetType().Name;
            _instancedSequence = new Sequence<Interface, SequenceOrder>(engines);
        }

        public JobHandle Execute(JobHandle inputHandles)
        {
            var sequenceItems = _instancedSequence.items;
            JobHandle combinedHandles = inputHandles;
            using (var profiler = new PlatformProfiler(_name))
            {
                for (var index = 0; index < sequenceItems.count; index++)
                {
                    var engine = sequenceItems[index];
                    using (profiler.Sample(engine.name)) combinedHandles = engine.Execute(combinedHandles);
                }
            }

            return combinedHandles; 
        }

        public string name => _name;
        
        readonly string _name;
        readonly Sequence<Interface, SequenceOrder> _instancedSequence;
    } 
    
    public abstract class SortedJobifiedEnginesGroup<Interface, Parameter, SequenceOrder>: IJobifiedGroupEngine<Parameter>
        where SequenceOrder : struct, ISequenceOrder where Interface : class, IJobifiedEngine<Parameter>
    {
        protected SortedJobifiedEnginesGroup(FasterList<Interface> engines)
        {
            _name = "SortedJobifiedEnginesGroup - "+this.GetType().Name;
            _instancedSequence = new Sequence<Interface, SequenceOrder>(engines);
        }

        public JobHandle Execute(JobHandle combinedHandles, ref Parameter param)
        {
            var sequenceItems = _instancedSequence.items;
            using (var profiler = new PlatformProfiler(_name))
            {
                for (var index = 0; index < sequenceItems.count; index++)
                {
                    var engine = sequenceItems[index];
                    using (profiler.Sample(engine.name)) combinedHandles = engine.Execute(combinedHandles, ref param);
                }
            }

            return combinedHandles;
        }

        public string name => _name;
        
        readonly string _name;
        readonly Sequence<Interface, SequenceOrder> _instancedSequence;
    }
}
#endif