using System;
using System.Collections;

namespace Svelto.ECS.Schedulers
{
    public sealed class SimpleEntitiesSubmissionScheduler : EntitiesSubmissionScheduler
    {
        public SimpleEntitiesSubmissionScheduler(uint maxNumberOfOperationsPerFrame = UInt32.MaxValue)
        {
            _maxNumberOfOperationsPerFrame = maxNumberOfOperationsPerFrame;
        }
        
        public IEnumerator SubmitEntitiesAsync()
        {
            if (paused == false)
            {
                var submitEntities = _onTick.Invoke(_maxNumberOfOperationsPerFrame);
                
                while (submitEntities.MoveNext())
                    yield return null;
            }
        }

        public void SubmitEntities()
        {
            var enumerator = SubmitEntitiesAsync();

            while (enumerator.MoveNext());
        }

        public override bool paused                        { get; set; }
        public override void Dispose() { }

        protected internal override EnginesRoot.EntitiesSubmitter onTick
        {
            set
            {
                DBC.ECS.Check.Require(_onTick.IsUnused, "a scheduler can be exclusively used by one enginesRoot only");
                
                _onTick = value;
            }
        }

        EnginesRoot.EntitiesSubmitter _onTick;
        readonly uint                 _maxNumberOfOperationsPerFrame;
    }
}