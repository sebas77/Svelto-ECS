using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public abstract class UnsortedEnginesGroup<Interface> : IStepGroupEngine
        where Interface : IStepEngine
    {
        protected UnsortedEnginesGroup(FasterList<Interface> engines)
        {
            _name              = "UnsortedEnginesGroup - "+this.GetType().Name;
            _instancedSequence = engines;
        }

        public void Step()
        {
            var sequenceItems = _instancedSequence;
            using (var profiler = new PlatformProfiler(_name))
            {
                for (var index = 0; index < sequenceItems.count; index++)
                {
                    var engine = sequenceItems[index];
                    using (profiler.Sample(engine.name)) engine.Step();
                }
            }
        }

        public string name => _name;
        
        readonly string                _name;
        readonly FasterList<Interface> _instancedSequence;
    }
    
    public abstract class UnsortedEnginesGroup<Interface, Parameter> : IStepGroupEngine<Parameter>
        where Interface : IStepEngine<Parameter>
    {
        protected UnsortedEnginesGroup(FasterList<Interface> engines)
        {
            _name              = "UnsortedEnginesGroup - "+this.GetType().Name;
            _instancedSequence = engines;
        }

        public void Step(in Parameter param)
        {
            var sequenceItems = _instancedSequence;
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
        
        readonly string                _name;
        readonly FasterList<Interface> _instancedSequence;
    }
}