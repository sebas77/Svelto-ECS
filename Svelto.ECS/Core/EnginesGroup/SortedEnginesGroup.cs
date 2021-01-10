using Svelto.DataStructures;
using Svelto.Common;

namespace Svelto.ECS
{
    public abstract class SortedEnginesGroup<Interface, SequenceOrder> : IStepGroupEngine
        where SequenceOrder : struct, ISequenceOrder where Interface : IStepEngine
    {
        protected SortedEnginesGroup(FasterList<Interface> engines)
        {
            _name = "SortedEnginesGroup - "+this.GetType().Name;
            _instancedSequence = new Sequence<Interface, SequenceOrder>(engines);
        }

        public void Step()
        {
            var sequenceItems = _instancedSequence.items;
            using (var profiler = new PlatformProfiler(_name))
            {
                for (var index = 0; index < sequenceItems.count; index++)
                {
                    var engine = sequenceItems[index];
                    using (profiler.Sample(engine.name)) engine.Step();
                }
            }
        }

        public string name   => _name;
        
        readonly string _name;
        readonly Sequence<Interface, SequenceOrder> _instancedSequence;
    } 
    
    public abstract class SortedEnginesGroup<Interface, Parameter, SequenceOrder>: IStepGroupEngine<Parameter>
        where SequenceOrder : struct, ISequenceOrder where Interface : IStepEngine<Parameter>
    {
        protected SortedEnginesGroup(FasterList<Interface> engines)
        {
            _name = "SortedEnginesGroup - "+this.GetType().Name;
            _instancedSequence = new Sequence<Interface, SequenceOrder>(engines);
        }

        public void Step(in Parameter param)
        {
            var sequenceItems = _instancedSequence.items;
            using (var profiler = new PlatformProfiler(_name))
            {
                for (var index = 0; index < sequenceItems.count; index++)
                {
                    var engine = sequenceItems[index];
                    using (profiler.Sample(engine.name)) engine.Step(param);
                }
            }
        }

        public string name => _name;
        
        readonly string _name;
        readonly Sequence<Interface, SequenceOrder> _instancedSequence;
    }
}
