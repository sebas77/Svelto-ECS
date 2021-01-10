#if UNITY_JOBS
using System;
using Svelto.ECS.Extensions.Unity;
using Unity.Jobs;

namespace Svelto.ECS
{
    public static class UnityJobExtensions2
    {
        public static JobHandle ScheduleDispose
            <T1>(this T1 disposable, JobHandle inputDeps) where T1 : struct, IDisposable
        {
            return new DisposeJob<T1>(disposable).Schedule(inputDeps);
        }
        
        public static JobHandle ScheduleDispose
            <T1, T2>(this T1 disposable1, T2 disposable2, JobHandle inputDeps) 
            where T1 : struct, IDisposable where T2 : struct, IDisposable
        {
            return new DisposeJob<T1, T2>(disposable1, disposable2).Schedule(inputDeps);
        }
        
        public static JobHandle ScheduleParallel
            <JOB>(this JOB job, int iterations, JobHandle inputDeps) where JOB: struct, IJobParallelFor
        {
            if (iterations <= 0)
                return inputDeps;
            
            var innerloopBatchCount = ProcessorCount.BatchSize((uint) iterations);
            return job.Schedule((int)iterations, innerloopBatchCount, inputDeps);
        }
        
        public static JobHandle ScheduleParallel
            <JOB>(this JOB job, uint iterations, JobHandle inputDeps) where JOB: struct, IJobParallelFor
        {
            if (iterations == 0)
                return inputDeps;
            
            var innerloopBatchCount = ProcessorCount.BatchSize(iterations);
            return job.Schedule((int)iterations, innerloopBatchCount, inputDeps);
        }
    }
}
#endif