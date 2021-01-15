#if UNITY_ECS
using Svelto.Common;
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    public abstract class SubmissionEngine : SystemBase, IJobifiedEngine
    {
        public JobHandle Execute(JobHandle inputDeps)
        {
            Dependency = JobHandle.CombineDependencies(Dependency, inputDeps);
            
            OnUpdate();
            
            return Dependency; 
        }

        public    EntityCommandBuffer ECB { get; internal set; }

        public string name => TypeToString.Name(this);
    }
}
#endif