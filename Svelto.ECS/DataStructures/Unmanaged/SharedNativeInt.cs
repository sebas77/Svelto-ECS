using System;
using System.Threading;
using Svelto.Common;

namespace Svelto.ECS.DataStructures
{
    public struct SharedNativeInt: IDisposable
    {
#if UNITY_NATIVE 
        [global::Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        unsafe int* data;

        Allocator _allocator;

        public SharedNativeInt(Allocator allocator)
        {
            unsafe
            {
                _allocator = allocator;
                data       = (int*) MemoryUtilities.Alloc(sizeof(int), allocator);
            }
        }

        public static SharedNativeInt Create(int t, Allocator allocator)
        {
            unsafe
            {
                var current = new SharedNativeInt();
                current._allocator    = allocator;
                current.data  = (int*) MemoryUtilities.Alloc(sizeof(int), allocator);
                *current.data = t;

                return current;
            }
        }
        
        public static implicit operator int(SharedNativeInt t)
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO                
                if (t.data == null)               
                    throw new Exception("using disposed SharedInt");
#endif    
                return *t.data;
            }
        }

        public void Dispose()
        {
            unsafe
            {
                if (data != null)
                {
                    MemoryUtilities.Free((IntPtr) data, _allocator);
                    data = null;
                }
            }
        }

        public int Decrement()
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (data == null)
                    throw new Exception("null-access");
#endif            
                
                return Interlocked.Decrement(ref *data);
            }
        }
        
        public int Increment()
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (data == null)
                    throw new Exception("null-access");
#endif            
                
                return Interlocked.Increment(ref *data);
            }
        }
        
        public int Add(int val)
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (data == null)
                    throw new Exception("null-access");
#endif            
                
                return Interlocked.Add(ref *data, val);
            }
        }
        
        public void Set(int val)
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (data == null)
                    throw new Exception("null-access");
#endif            
                
                Volatile.Write(ref *data, val);
            }
        }
    }
}