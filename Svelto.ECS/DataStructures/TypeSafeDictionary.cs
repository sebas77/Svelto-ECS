using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Internal
{
    sealed class TypeSafeDictionary<TValue> : ITypeSafeDictionary<TValue> where TValue : struct, IEntityComponent
    {
        static readonly Type   _type     = typeof(TValue);
        static readonly string _typeName = _type.Name;
        static readonly bool   _hasEgid  = typeof(INeedEGID).IsAssignableFrom(_type);

        internal static readonly bool IsUnmanaged =
            _type.IsUnmanagedEx() && (typeof(IEntityViewComponent).IsAssignableFrom(_type) == false);

        SveltoDictionary<uint, TValue, NativeStrategy<SveltoDictionaryNode<uint>>, ManagedStrategy<TValue>,
            ManagedStrategy<int>> implMgd;

        //used directly by native methods
        internal SveltoDictionary<uint, TValue, NativeStrategy<SveltoDictionaryNode<uint>>, NativeStrategy<TValue>,
            NativeStrategy<int>> implUnmgd;

        public TypeSafeDictionary(uint size)
        {
            if (IsUnmanaged)
                implUnmgd = new SveltoDictionary<uint, TValue, NativeStrategy<SveltoDictionaryNode<uint>>,
                    NativeStrategy<TValue>, NativeStrategy<int>>(size);
            else
            {
                implMgd = new SveltoDictionary<uint, TValue, NativeStrategy<SveltoDictionaryNode<uint>>,
                    ManagedStrategy<TValue>, ManagedStrategy<int>>(size);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint egidEntityId, in TValue entityComponent)
        {
            if (IsUnmanaged)
                implUnmgd.Add(egidEntityId, entityComponent);
            else
                implMgd.Add(egidEntityId, entityComponent);
        }

        /// <summary>
        ///     Add entities from external typeSafeDictionary
        /// </summary>
        /// <param name="entitiesToSubmit"></param>
        /// <param name="groupId"></param>
        /// <exception cref="TypeSafeDictionaryException"></exception>
        public void AddEntitiesFromDictionary(ITypeSafeDictionary entitiesToSubmit, uint groupId)
        {
            if (IsUnmanaged)
            {
                var typeSafeDictionary = (entitiesToSubmit as TypeSafeDictionary<TValue>).implUnmgd;

                foreach (var tuple in typeSafeDictionary)
                    try
                    {
                        if (_hasEgid)
                            SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(
                                ref tuple.Value, new EGID(tuple.Key, groupId));

                        implUnmgd.Add(tuple.Key, tuple.Value);
                    }
                    catch (Exception e)
                    {
                        Console.LogException(
                            e, "trying to add an EntityComponent with the same ID more than once Entity: ".FastConcat(typeof(TValue).ToString()).FastConcat(", group ").FastConcat(groupId).FastConcat(", id ").FastConcat(tuple.Key));

                        throw;
                    }
            }
            else
            {
                var typeSafeDictionary = (entitiesToSubmit as TypeSafeDictionary<TValue>).implMgd;

                foreach (var tuple in typeSafeDictionary)
                    try
                    {
                        if (_hasEgid)
                            SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(
                                ref tuple.Value, new EGID(tuple.Key, groupId));

                        implMgd.Add(tuple.Key, tuple.Value);
                    }
                    catch (Exception e)
                    {
                        Console.LogException(
                            e, "trying to add an EntityComponent with the same ID more than once Entity: ".FastConcat(typeof(TValue).ToString()).FastConcat(", group ").FastConcat(groupId).FastConcat(", id ").FastConcat(tuple.Key));

                        throw;
                    }
            }
        }

        public void ExecuteEnginesAddOrSwapCallbacks
        (FasterDictionary<RefWrapperType, FasterList<IReactEngine>> entityComponentEnginesDB
       , ITypeSafeDictionary realDic, ExclusiveGroupStruct? fromGroup, ExclusiveGroupStruct toGroup
       , in PlatformProfiler profiler)
        {
            if (IsUnmanaged)
            {
                var typeSafeDictionary = realDic as ITypeSafeDictionary<TValue>;

                //this can be optimized, should pass all the entities and not restart the process for each one
                foreach (var value in implUnmgd)
                    ExecuteEnginesAddOrSwapCallbacksOnSingleEntity(entityComponentEnginesDB
                                                                 , ref typeSafeDictionary[value.Key], fromGroup
                                                                 , in profiler, new EGID(value.Key, toGroup));
            }
            else
            {
                var typeSafeDictionary = realDic as ITypeSafeDictionary<TValue>;

                //this can be optimized, should pass all the entities and not restart the process for each one
                foreach (var value in implMgd)
                    ExecuteEnginesAddOrSwapCallbacksOnSingleEntity(entityComponentEnginesDB
                                                                 , ref typeSafeDictionary[value.Key], fromGroup
                                                                 , in profiler, new EGID(value.Key, toGroup));
            }
        }

        public void AddEntityToDictionary(EGID fromEntityGid, EGID toEntityID, ITypeSafeDictionary toGroup)
        {
            if (IsUnmanaged)
            {
                var valueIndex = implUnmgd.GetIndex(fromEntityGid.entityID);

                DBC.ECS.Check.Require(toGroup != null
                                    , "Invalid To Group"); //todo check this, if it's right merge GetIndex
                {
                    var     toGroupCasted = toGroup as ITypeSafeDictionary<TValue>;
                    ref var entity        = ref implUnmgd.GetDirectValueByRef(valueIndex);

                    if (_hasEgid)
                        SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref entity, toEntityID);

                    toGroupCasted.Add(toEntityID.entityID, entity);
                }
            }
            else
            {
                var valueIndex = implMgd.GetIndex(fromEntityGid.entityID);

                DBC.ECS.Check.Require(toGroup != null
                                    , "Invalid To Group"); //todo check this, if it's right merge GetIndex
                {
                    var     toGroupCasted = toGroup as ITypeSafeDictionary<TValue>;
                    ref var entity        = ref implMgd.GetDirectValueByRef(valueIndex);

                    if (_hasEgid)
                        SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref entity, toEntityID);

                    toGroupCasted.Add(toEntityID.entityID, entity);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (IsUnmanaged)
            {
                implUnmgd.Clear();
            }
            else
            {
                implMgd.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastClear()
        {
            if (IsUnmanaged)
            {
                implUnmgd.FastClear();
            }
            else
            {
                implMgd.FastClear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(uint egidEntityId)
        {
            if (IsUnmanaged)
            {
                return implUnmgd.ContainsKey(egidEntityId);
            }
            else
            {
                return implMgd.ContainsKey(egidEntityId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITypeSafeDictionary Create() { return TypeSafeDictionaryFactory<TValue>.Create(1); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(uint valueEntityId)
        {
            if (IsUnmanaged)
            {
                return this.implUnmgd.GetIndex(valueEntityId);
            }
            else
            {
                return this.implMgd.GetIndex(valueEntityId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrCreate(uint idEntityId)
        {
            if (IsUnmanaged)
            {
                return ref this.implUnmgd.GetOrCreate(idEntityId);
            }
            else
            {
                return ref this.implMgd.GetOrCreate(idEntityId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBuffer<TValue> GetValues(out uint count)
        {
            if (IsUnmanaged)
            {
                return this.implUnmgd.GetValues(out count);
            }
            else
            {
                return this.implMgd.GetValues(out count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetDirectValueByRef(uint key)
        {
            if (IsUnmanaged)
            {
                return ref this.implUnmgd.GetDirectValueByRef(key);
            }
            else
            {
                return ref this.implMgd.GetDirectValueByRef(key);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(uint key)
        {
            if (IsUnmanaged)
            {
                return this.implUnmgd.ContainsKey(key);
            }
            else
            {
                return this.implMgd.ContainsKey(key);
            }
        }

        public void ExecuteEnginesSwapOrRemoveCallbacks
        (EGID fromEntityGid, EGID? toEntityID, ITypeSafeDictionary toGroup
       , FasterDictionary<RefWrapperType, FasterList<IReactEngine>> engines, in PlatformProfiler profiler)
        {
            if (IsUnmanaged)
            {
                var valueIndex = this.implUnmgd.GetIndex(fromEntityGid.entityID);

                ref var entity = ref this.implUnmgd.GetDirectValueByRef(valueIndex);

                //move
                if (toGroup != null)
                {
                    var toGroupCasted = toGroup as ITypeSafeDictionary<TValue>;
                    var previousGroup = fromEntityGid.groupID;

                    if (_hasEgid)
                        SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref entity, toEntityID.Value);

                    var index = toGroupCasted.GetIndex(toEntityID.Value.entityID);

                    ExecuteEnginesAddOrSwapCallbacksOnSingleEntity(engines, ref toGroupCasted.GetDirectValueByRef(index)
                                                        , previousGroup, in profiler, toEntityID.Value);
                }
                //remove
                else
                {
                    ExecuteEnginesRemoveCallbackOnSingleEntity(engines, ref entity, in profiler, fromEntityGid);
                }
            }
            else
            {
                var valueIndex = this.implMgd.GetIndex(fromEntityGid.entityID);

                ref var entity = ref this.implMgd.GetDirectValueByRef(valueIndex);

                if (toGroup != null)
                {
                    var toGroupCasted = toGroup as ITypeSafeDictionary<TValue>;
                    var previousGroup = fromEntityGid.groupID;

                    if (_hasEgid)
                        SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref entity, toEntityID.Value);

                    var index = toGroupCasted.GetIndex(toEntityID.Value.entityID);

                    ExecuteEnginesAddOrSwapCallbacksOnSingleEntity(engines, ref toGroupCasted.GetDirectValueByRef(index)
                                                        , previousGroup, in profiler, toEntityID.Value);
                }
                else
                {
                    ExecuteEnginesRemoveCallbackOnSingleEntity(engines, ref entity, in profiler, fromEntityGid);
                }
            }
        }

        public void ExecuteEnginesRemoveCallbacks
        (FasterDictionary<RefWrapperType, FasterList<IReactEngine>> engines, in PlatformProfiler profiler
       , ExclusiveGroupStruct group)
        {
            if (IsUnmanaged)
            {
                foreach (var value in implUnmgd)
                    ExecuteEnginesRemoveCallbackOnSingleEntity(engines, ref implUnmgd.GetValueByRef(value.Key)
                                                             , in profiler, new EGID(value.Key, group));
            }
            else
            {
                foreach (var value in implMgd)
                    ExecuteEnginesRemoveCallbackOnSingleEntity(engines, ref implMgd.GetValueByRef(value.Key)
                                                             , in profiler, new EGID(value.Key, group));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveEntityFromDictionary(EGID fromEntityGid)
        {
            if (IsUnmanaged)
            {
                this.implUnmgd.Remove(fromEntityGid.entityID);
            }
            else
            {
                this.implMgd.Remove(fromEntityGid.entityID);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(uint size)
        {
            if (IsUnmanaged)
            {
                this.implUnmgd.SetCapacity(size);
            }
            else
            {
                this.implMgd.SetCapacity(size);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim()
        {
            if (IsUnmanaged)
            {
                this.implUnmgd.Trim();
            }
            else
            {
                this.implMgd.Trim();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindIndex(uint entityId, out uint index)
        {
            if (IsUnmanaged)
            {
                return implUnmgd.TryFindIndex(entityId, out index);
            }
            else
            {
                return implMgd.TryFindIndex(entityId, out index);
            }
        }

        public void KeysEvaluator(Action<uint> action)
        {
            if (IsUnmanaged)
            {
                foreach (var key in implUnmgd.keys)
                {
                    action(key);
                }
            }
            else
            {
                foreach (var key in implMgd.keys)
                {
                    action(key);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(uint entityId, out TValue item)
        {
            if (IsUnmanaged)
            {
                return this.implUnmgd.TryGetValue(entityId, out item);
            }
            else
            {
                return this.implMgd.TryGetValue(entityId, out item);
            }
        }

        public uint count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (IsUnmanaged)
                {
                    return (uint) this.implUnmgd.count;
                }
                else
                {
                    return (uint) this.implMgd.count;
                }
            }
        }

        public ref TValue this[uint idEntityId]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (IsUnmanaged)
                {
                    return ref this.implUnmgd.GetValueByRef(idEntityId);
                }
                else
                {
                    return ref this.implMgd.GetValueByRef(idEntityId);
                }
            }
        }

        static void ExecuteEnginesRemoveCallbackOnSingleEntity
        (FasterDictionary<RefWrapperType, FasterList<IReactEngine>> engines, ref TValue entity
       , in PlatformProfiler profiler, EGID egid)
        {
            if (!engines.TryGetValue(new RefWrapperType(_type), out var entityComponentsEngines))
                return;

            for (var i = 0; i < entityComponentsEngines.count; i++)
                try
                {
                    using (profiler.Sample(entityComponentsEngines[i], _typeName))
                    {
                        (entityComponentsEngines[i] as IReactOnAddAndRemove<TValue>).Remove(ref entity, egid);
                    }
                }
                catch
                {
                    Svelto.Console.LogError(
                        "Code crashed inside Remove callback ".FastConcat(typeof(TValue).ToString()));

                    throw;
                }
        }

        void ExecuteEnginesAddOrSwapCallbacksOnSingleEntity
        (FasterDictionary<RefWrapperType, FasterList<IReactEngine>> engines, ref TValue entity
       , ExclusiveGroupStruct? previousGroup, in PlatformProfiler profiler, EGID egid)
        {
            //get all the engines linked to TValue
            if (!engines.TryGetValue(new RefWrapperType(_type), out var entityComponentsEngines))
                return;

            if (previousGroup == null)
            {
                for (var i = 0; i < entityComponentsEngines.count; i++)
                    try
                    {
                        using (profiler.Sample(entityComponentsEngines[i], _typeName))
                        {
                            (entityComponentsEngines[i] as IReactOnAddAndRemove<TValue>).Add(ref entity, egid);
                        }
                    }
                    catch
                    {
                        Svelto.Console.LogError(
                            "Code crashed inside Add callback ".FastConcat(typeof(TValue).ToString()));

                        throw;
                    }
            }
            else
            {
                for (var i = 0; i < entityComponentsEngines.count; i++)
                    try
                    {
                        using (profiler.Sample(entityComponentsEngines[i], _typeName))
                        {
                            (entityComponentsEngines[i] as IReactOnSwap<TValue>).MovedTo(
                                ref entity, previousGroup.Value, egid);
                        }
                    }
                    catch (Exception)
                    {
                        Svelto.Console.LogError(
                            "Code crashed inside MoveTo callback ".FastConcat(typeof(TValue).ToString()));

                        throw;
                    }
            }
        }

        public void Dispose()
        {
            if (IsUnmanaged)
                implUnmgd.Dispose();
            else
                implMgd.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}