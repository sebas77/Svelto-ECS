#if UNITY_ECS
using System.Collections;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers;
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    /// <summary>
    /// Group of UECS/Svelto SystemBase engines that creates UECS entities.
    /// Svelto entities are submitted
    /// Svelto Add and remove callback are called
    /// OnUpdate of the systems are called
    /// finally the UECS command buffer is flushed
    /// Note: I cannot use Unity ComponentSystemGroups nor I can rely on the SystemBase Dependency field to
    /// solve external dependencies. External dependencies are tracked, but only linked to the UECS components operations
    /// With Dependency I cannot guarantee that an external container is used before previous jobs working on it are completed
    /// </summary>
    public sealed class SveltoUECSEntitiesSubmissionGroup
    {
        public SveltoUECSEntitiesSubmissionGroup(SimpleEntitiesSubmissionScheduler submissionScheduler, World UECSWorld)
        {
            _submissionScheduler = submissionScheduler;
            _ECBSystem           = UECSWorld.CreateSystem<SubmissionEntitiesCommandBufferSystem>();
            _engines             = new FasterList<SubmissionEngine>();
        }

        public void SubmitEntities(JobHandle jobHandle)
        {
            if (_submissionScheduler.paused)
                return;
            
            jobHandle.Complete();

            //prepare the entity command buffer to be used by the registered engines
            var entityCommandBuffer = _ECBSystem.CreateCommandBuffer();

            foreach (var system in _engines)
            {
                system.ECB = entityCommandBuffer;
            }

            //Submit Svelto Entities, calls Add/Remove/MoveTo that can be used by the IUECSSubmissionEngines
            _submissionScheduler.SubmitEntities();

            //execute submission engines and complete jobs because of this I don't need to do _ECBSystem.AddJobHandleForProducer(Dependency);
            using (var profiler = new PlatformProfiler("SveltoUECSEntitiesSubmissionGroup"))
            {
                for (var index = 0; index < _engines.count; index++)
                {
                    ref var engine = ref _engines[index];
                    using (profiler.Sample(engine.name))
                    {
                        jobHandle = engine.Execute(jobHandle);
                    }
                }
            }

            //Sync Point as we must be sure that jobs that create/swap/remove entities are done
            jobHandle.Complete();

            //flush command buffer
            _ECBSystem.Update();
        }
        
        public void Add(SubmissionEngine engine)
        {
            _ECBSystem.World.AddSystem(engine);
            _engines.Add(engine);
        }

        readonly SimpleEntitiesSubmissionScheduler     _submissionScheduler;
        readonly SubmissionEntitiesCommandBufferSystem _ECBSystem;
        readonly FasterList<SubmissionEngine>          _engines;

        [DisableAutoCreation]
        class SubmissionEntitiesCommandBufferSystem : EntityCommandBufferSystem { }
    }
}
#endif