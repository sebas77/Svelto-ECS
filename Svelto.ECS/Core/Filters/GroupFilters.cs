using Svelto.DataStructures;

namespace Svelto.ECS
{
    public struct GroupFilters
    {
        internal GroupFilters(SharedSveltoDictionaryNative<int, FilterGroup> filters, ExclusiveGroupStruct group)
        {
            this.filters = filters;
            _group       = @group;
        }

        public ref FilterGroup GetFilter(int filterIndex)
        {
#if DEBUG && !PROFILE_SVELTO
            if (filters.isValid == false)
                throw new ECSException($"trying to fetch not existing filters {filterIndex} group {_group.ToName()}");
            if (filters.ContainsKey(filterIndex) == false)
                throw new ECSException($"trying to fetch not existing filters {filterIndex} group {_group.ToName()}");
#endif
            return ref filters.GetValueByRef(filterIndex);
        }

        public bool HasFilter(int filterIndex) { return filters.ContainsKey(filterIndex); }

        public void ClearFilter(int filterIndex)
        {
            if (filters.TryFindIndex(filterIndex, out var index))
                filters.GetValues(out _)[index].Clear();
        }

        public void ClearFilters()
        {
            foreach (var filter in filters)
                filter.Value.Clear();
        }

        public bool TryGetFilter(int filterIndex, out FilterGroup filter)
        {
            return filters.TryGetValue(filterIndex, out filter);
        }

        public SveltoDictionary<int, FilterGroup, NativeStrategy<SveltoDictionaryNode<int>>, NativeStrategy<FilterGroup>
          , NativeStrategy<int>>.SveltoDictionaryKeyValueEnumerator GetEnumerator()
        {
            return filters.GetEnumerator();
        }
        
        //Note the following methods are internal because I was pondering the idea to be able to return
        //the list of GroupFilters linked to a specific filter ID. However this would mean to be able to
        //maintain a revers map which at this moment seems too much and also would need the following
        //method to be for ever internal (at this point in time I am not sure it's a good idea)
        internal void DisposeFilter(int filterIndex)
        {
            if (filters.TryFindIndex(filterIndex, out var index))
            {
                ref var filterGroup = ref filters.GetValues(out _)[index];
                
                filterGroup.Dispose();

                filters.Remove(filterIndex);
            }
        }

        internal void DisposeFilters()
        {
            //must release the native buffers!
            foreach (var filter in filters)
                filter.Value.Dispose();

            filters.FastClear();
        }

        internal ref FilterGroup CreateOrGetFilter(int filterID)
        {
            if (filters.TryFindIndex(filterID, out var index) == false)
            {
                var orGetFilterForGroup = new FilterGroup(_group, filterID);

                filters[filterID] = orGetFilterForGroup;

                return ref filters.GetValueByRef(filterID);
            }

            return ref filters.GetValues(out _)[index];
        }

        internal void Dispose()
        {
            foreach (var filter in filters)
            {
                filter.Value.Dispose();
            }

            filters.Dispose();
        }

        readonly ExclusiveGroupStruct _group;

        //filterID, filter
        SharedSveltoDictionaryNative<int, FilterGroup> filters;
    }
}