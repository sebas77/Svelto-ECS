using Svelto.DataStructures;

namespace Svelto.ECS
{
    /// <summary>
    ///     This feature must be eventually tied to the new ExclusiveGroup that won't allow the use of custom EntitiesID
    ///     The filters could be updated when entities buffer changes during the submission, while now this process
    ///     is completely manual.
    ///     Making this working properly is not in my priorities right now, as I will need to add the new group type
    ///     AND optimize the submission process to be able to add more overhead
    /// </summary>
    public partial class EntitiesDB
    {
        public readonly struct Filters
        {
            public Filters
            (FasterDictionary<RefWrapperType, FasterDictionary<ExclusiveGroupStruct, GroupFilters>> filters)
            {
                _filters    = filters;
            }

            public ref FilterGroup CreateOrGetFilterForGroup<T>(int filterID, ExclusiveGroupStruct groupID)
                where T : struct, IEntityComponent
            {
                var refWrapper = TypeRefWrapper<T>.wrapper;
                
                return ref CreateOrGetFilterForGroup(filterID, groupID, refWrapper);
            }

            ref FilterGroup CreateOrGetFilterForGroup(int filterID, ExclusiveGroupStruct groupID, RefWrapperType refWrapper)
            {
                var fasterDictionary =
                    _filters.GetOrCreate(refWrapper, () => new FasterDictionary<ExclusiveGroupStruct, GroupFilters>());

                GroupFilters filters =
                    fasterDictionary.GetOrCreate(
                        groupID, () => new GroupFilters(new SharedSveltoDictionaryNative<int, FilterGroup>(0), groupID));

                return ref filters.CreateOrGetFilter(filterID);
            }

            public bool HasFiltersForGroup<T>(ExclusiveGroupStruct groupID) where T : struct, IEntityComponent
            {
                if (_filters.TryGetValue(TypeRefWrapper<T>.wrapper, out var fasterDictionary) == false)
                    return false;
                
                return fasterDictionary.ContainsKey(groupID);
            }

            public bool HasFilterForGroup<T>(int filterID, ExclusiveGroupStruct groupID)
                where T : struct, IEntityComponent
            {
                if (_filters.TryGetValue(TypeRefWrapper<T>.wrapper, out var fasterDictionary) == false)
                    return false;

                if (fasterDictionary.TryGetValue(groupID, out var result))
                    return result.HasFilter(filterID);

                return false;
            }
            
            public ref GroupFilters CreateOrGetFiltersForGroup<T>(ExclusiveGroupStruct groupID)
                where T : struct, IEntityComponent
            {
                var fasterDictionary =
                    _filters.GetOrCreate(TypeRefWrapper<T>.wrapper, () => new FasterDictionary<ExclusiveGroupStruct, GroupFilters>());

                return ref
                    fasterDictionary.GetOrCreate(
                        groupID, () => new GroupFilters(new SharedSveltoDictionaryNative<int, FilterGroup>(0), groupID));
            }

            public ref GroupFilters GetFiltersForGroup<T>(ExclusiveGroupStruct groupID)
                where T : struct, IEntityComponent
            {
#if DEBUG && !PROFILE_SVELTO
                if (_filters.ContainsKey(TypeRefWrapper<T>.wrapper) == false)
                    throw new ECSException(
                        $"trying to fetch not existing filters, type {typeof(T)}");
                if (_filters[TypeRefWrapper<T>.wrapper].ContainsKey(groupID) == false)
                    throw new ECSException(
                        $"trying to fetch not existing filters, type {typeof(T)} group {groupID.ToName()}");
#endif

                return ref _filters[TypeRefWrapper<T>.wrapper].GetValueByRef(groupID);
            }

            public ref FilterGroup GetFilterForGroup<T>(int filterId, ExclusiveGroupStruct groupID)
                where T : struct, IEntityComponent
            {
#if DEBUG && !PROFILE_SVELTO
                if (_filters.ContainsKey(TypeRefWrapper<T>.wrapper) == false)
                    throw new ECSException(
                        $"trying to fetch not existing filters, type {typeof(T)}");
                if (_filters[TypeRefWrapper<T>.wrapper].ContainsKey(groupID) == false)
                    throw new ECSException(
                        $"trying to fetch not existing filters, type {typeof(T)} group {groupID.ToName()}");
#endif
                return ref _filters[TypeRefWrapper<T>.wrapper][groupID].GetFilter(filterId);
            }
            
            public bool TryGetFilterForGroup<T>(int filterId, ExclusiveGroupStruct groupID, out FilterGroup groupFilter)
                where T : struct, IEntityComponent
            {
                groupFilter = default;

                if (_filters.TryGetValue(TypeRefWrapper<T>.wrapper, out var fasterDictionary) == false)
                    return false;

                if (fasterDictionary.TryGetValue(groupID, out var groupFilters) == false)
                    return false;

                if (groupFilters.TryGetFilter(filterId, out groupFilter) == false)
                    return false;

                return true;
            }

            public bool TryGetFiltersForGroup<T>(ExclusiveGroupStruct groupID, out GroupFilters groupFilters)
                where T : struct, IEntityComponent
            {
                groupFilters = default;

                if (_filters.TryGetValue(TypeRefWrapper<T>.wrapper, out var fasterDictionary) == false)
                    return false;

                return fasterDictionary.TryGetValue(groupID, out groupFilters);
            }

            public void ClearFilter<T>(int filterID, ExclusiveGroupStruct exclusiveGroupStruct)
            {
                if (_filters.TryGetValue(TypeRefWrapper<T>.wrapper, out var fasterDictionary) == true)
                {
                    DBC.ECS.Check.Require(fasterDictionary.ContainsKey(exclusiveGroupStruct), $"trying to clear filter not present in group {exclusiveGroupStruct}");
                    
                    fasterDictionary[exclusiveGroupStruct].ClearFilter(filterID);
                }
            }

            public void ClearFilters<T>(int filterID)
            {
                if (_filters.TryGetValue(TypeRefWrapper<T>.wrapper, out var fasterDictionary) == true)
                {
                    foreach (var filtersPerGroup in fasterDictionary)
                        filtersPerGroup.Value.ClearFilter(filterID);
                }
            }

            public void DisposeFilters<T>(ExclusiveGroupStruct exclusiveGroupStruct)
            {
                if (_filters.TryGetValue(TypeRefWrapper<T>.wrapper, out var fasterDictionary) == true)
                {
                    fasterDictionary[exclusiveGroupStruct].DisposeFilters();
                    fasterDictionary.Remove(exclusiveGroupStruct);
                }
            }

            public void DisposeFilters<T>()
            {
                if (_filters.TryGetValue(TypeRefWrapper<T>.wrapper, out var fasterDictionary) == true)
                {
                    foreach (var filtersPerGroup in fasterDictionary)
                        filtersPerGroup.Value.DisposeFilters();
                }

                _filters.Remove(TypeRefWrapper<T>.wrapper);
            }

            public void DisposeFilterForGroup<T>(int resetFilterID, ExclusiveGroupStruct @group)
            {
                if (_filters.TryGetValue(TypeRefWrapper<T>.wrapper, out var fasterDictionary) == true)
                {
                    fasterDictionary[group].DisposeFilter(resetFilterID);
                }
            }

            public bool TryRemoveEntityFromFilter<T>(int filtersID, EGID egid) where T : unmanaged, IEntityComponent
            {
                if (TryGetFilterForGroup<T>(filtersID, egid.groupID, out var filter))
                {
                    return filter.TryRemove(egid.entityID);
                }

                return false;
            }

            public void RemoveEntityFromFilter<T>(int filtersID, EGID egid) where T : unmanaged, IEntityComponent
            {
                ref var filter = ref GetFilterForGroup<T>(filtersID, egid.groupID);

                filter.Remove(egid.entityID);
            }

            public void AddEntityToFilter<N>(int filtersID, EGID egid, N mapper) where N:IEGIDMapper
            {
                ref var filter = ref CreateOrGetFilterForGroup(filtersID, egid.groupID, new RefWrapperType(mapper.entityType));

                filter.Add(egid.entityID, mapper);
            }

            readonly FasterDictionary<RefWrapperType, FasterDictionary<ExclusiveGroupStruct, GroupFilters>> _filters;
        }

        public Filters GetFilters()
        {
            return new Filters(_filters);
        }

        FasterDictionary<RefWrapperType, FasterDictionary<ExclusiveGroupStruct, GroupFilters>> _filters
            => _enginesRoot._groupFilters;
    }
}