#if UNITY_JOBS
using System;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    public struct DisposeJob<T>:IJob where T:struct,IDisposable
    {
        public DisposeJob(in T disposable)
        {
            _entityCollection = disposable;
        }

        public void Execute()
        {
            try
            {
                _entityCollection.Dispose();
            }
            catch (Exception e)
            {
                Console.LogException(e, this.GetType().ToString().FastConcat(" "));
            }
        }
        
        readonly T _entityCollection;
    }
    
    public struct DisposeJob<T1, T2>:IJob 
        where T1:struct,IDisposable where T2:struct,IDisposable
    {
        public DisposeJob(in T1 disposable1, in T2 disposable2)
        {
            _entityCollection1 = disposable1;
            _entityCollection2 = disposable2;
        }

        public void Execute()
        {
            try
            {
                _entityCollection1.Dispose();
                _entityCollection2.Dispose();
            }
            catch (Exception e)
            {
                Console.LogException(e, this.GetType().ToString().FastConcat(" "));
            }
        }
        
        readonly T1 _entityCollection1;
        readonly T2 _entityCollection2;
    }
    
    public struct DisposeJob<T1, T2, T3>:IJob 
        where T1:struct,IDisposable where T2:struct,IDisposable where T3:struct,IDisposable
    {
        public DisposeJob(in T1 disposable1, in T2 disposable2, in T3 disposable3)
        {
            _entityCollection1 = disposable1;
            _entityCollection2 = disposable2;
            _entityCollection3 = disposable3;
        }

        public void Execute()
        {
            try
            {
                _entityCollection1.Dispose();
                _entityCollection2.Dispose();
                _entityCollection3.Dispose();
            }
            catch (Exception e)
            {
                Console.LogException(e, this.GetType().ToString().FastConcat(" "));
            }
        }
        
        readonly T1 _entityCollection1;
        readonly T2 _entityCollection2;
        readonly T3 _entityCollection3;
    }
}
#endif