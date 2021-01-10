using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS
{
    /// <summary>
    ///     In order to complete this feature, I need to be able to detect if the entity pointed by the filter
    ///     is still present.
    ///     This feature should work only with groups where entityID cannot be chosen by the user, so that
    ///     a real sparse set can be used like explained at: https://skypjack.github.io/2020-08-02-ecs-baf-part-9/
    ///     For a sparse set to work, the index in the sparse list must coincide with the ID of the entity
    ///     so that from the dense list (that holds unordered entity index), I can get to the sparse list index
    ///     sparse[0] = position in the dense list of the entity 0
    ///     dense[index] = entity ID but also index in the sparse list of the same entity ID
    /// </summary>
    public struct FilterGroup
    {
        internal FilterGroup(ExclusiveGroupStruct exclusiveGroupStruct, int ID)
        {
            _denseListOfIndicesToEntityComponentArray =
                new NativeDynamicArrayCast<uint>(NativeDynamicArray.Alloc<uint>(Allocator.Persistent));
            //from the index, find the entityID
            _reverseEIDs = new NativeDynamicArrayCast<uint>(NativeDynamicArray.Alloc<uint>(Allocator.Persistent));
            //from the entityID, find the index
            _indexOfEntityInDenseList                 = new SharedSveltoDictionaryNative<uint, uint>(0, Allocator.Persistent);
            _exclusiveGroupStruct = exclusiveGroupStruct;
            _ID = ID;
        }

        /// <summary>
        /// Todo: how to detect if the indices are still pointing to valid entities?
        /// </summary>
        public FilteredIndices filteredIndices => new FilteredIndices(_denseListOfIndicesToEntityComponentArray);

        public void Add<N>(uint entityID, N mapper)  where N:IEGIDMapper
        {
#if DEBUG && !PROFILE_SVELTO
            if (_denseListOfIndicesToEntityComponentArray.isValid == false)
                throw new ECSException($"using an invalid filter");
            if (_indexOfEntityInDenseList.ContainsKey(entityID) == true)
                throw new ECSException(
                    $"trying to add an existing entity {entityID} to filter {mapper.entityType} - {_ID} with group {mapper.groupID}");
            if (mapper.Exists(entityID) == false)
                throw new ECSException(
                    $"trying adding an entity {entityID} to filter {mapper.entityType} - {_ID} with group {mapper.groupID}, but entity is not found! ");
#endif
            //Get the index of the Entity in the component array
            var indexOfEntityInBufferComponent = mapper.GetIndex(entityID);

            //add the index in the list of filtered indices
            _denseListOfIndicesToEntityComponentArray.Add(indexOfEntityInBufferComponent);

            //inverse map: need to get the entityID from the index. This wouldn't be needed with a real sparseset
            var lastIndex = (uint) (_denseListOfIndicesToEntityComponentArray.Count() - 1);
            _reverseEIDs.AddAt(lastIndex) = entityID;

            //remember the entities indices. This is needed to remove entities from the filter
            _indexOfEntityInDenseList.Add(entityID, lastIndex);
        }

        public void Remove(uint entityID)
        {
#if DEBUG && !PROFILE_SVELTO
            if (_denseListOfIndicesToEntityComponentArray.isValid == false)
                throw new ECSException($"invalid Filter");
            if (_indexOfEntityInDenseList.ContainsKey(entityID) == false)
                throw new ECSException(
                    $"trying to remove a not existing entity {new EGID(entityID, _exclusiveGroupStruct)} from filter");
#endif
            InternalRemove(entityID);
        }

        public bool TryRemove(uint entityID)
        {
#if DEBUG && !PROFILE_SVELTO
            if (_denseListOfIndicesToEntityComponentArray.isValid == false)
                throw new ECSException($"invalid Filter");
#endif
            if (_indexOfEntityInDenseList.ContainsKey(entityID) == false)
                return false;

            InternalRemove(entityID);

            return true;
        }

        /// <summary>
        /// Filters were initially designed to be used for tagging operations within submissions of entities.
        /// They were designed as a fast tagging mechanism to be used within the submission frame. However I then
        /// extended it, but the extension includes a drawback:
        ///If filters are not in sync with the operations of remove and swap, filters may end up pointing to
        ///invalid indices. I need to put in place a way to be able to recognised an invalid filter.
        ///This is currently a disadvantage of the filters. The filters are not updated by the framework
        ///but they must be updated by the user.
        ///When to use this method: Add and Removed should be used to add and remove entities in the filters. This is
        /// valid as long as no structural changes happen in the group of entities involved.
        /// IF structural changes happen, the indices stored in the filters won't be valid anymore as they will possibly
        /// point to entities that were not the original ones. On structural changes
        /// (specifically entities swapped or removed)
        /// the filters must then be rebuilt. It would be too slow to add this in the standard flow of Svelto in
        /// the current state, so calling this method is a user responsibility. 
        /// </summary>
        public void RebuildIndicesOnStructuralChange<N>(N mapper) where N:IEGIDMapper
        {
#if DEBUG && !PROFILE_SVELTO
            if (_denseListOfIndicesToEntityComponentArray.isValid == false)
                throw new ECSException($"invalid Filter");
#endif
            _denseListOfIndicesToEntityComponentArray.Clear();
            _reverseEIDs.Clear();

            foreach (var value in _indexOfEntityInDenseList)
                if (mapper.FindIndex(value.Key, out var indexOfEntityInBufferComponent) == true)
                {
                    _denseListOfIndicesToEntityComponentArray.Add(indexOfEntityInBufferComponent);
                    var lastIndex = (uint) (_denseListOfIndicesToEntityComponentArray.Count() - 1);
                    _reverseEIDs.AddAt(lastIndex) = value.Key;
                }

            _indexOfEntityInDenseList.Clear();

            for (uint i = 0; i < _reverseEIDs.Count(); i++)
                _indexOfEntityInDenseList[_reverseEIDs[i]] = i;
        }

        public void Clear()
        {
#if DEBUG && !PROFILE_SVELTO
            if (_denseListOfIndicesToEntityComponentArray.isValid == false)
                throw new ECSException($"invalid Filter");
#endif
            _indexOfEntityInDenseList.FastClear();
            _reverseEIDs.Clear();
            _denseListOfIndicesToEntityComponentArray.Clear();
        }

        internal void Dispose()
        {
#if DEBUG && !PROFILE_SVELTO
            if (_denseListOfIndicesToEntityComponentArray.isValid == false)
                throw new ECSException($"invalid Filter");
#endif
            _denseListOfIndicesToEntityComponentArray.Dispose();
            _indexOfEntityInDenseList.Dispose();
            _reverseEIDs.Dispose();
        }

        void InternalRemove(uint entityID)
        {
            var count = (uint) _denseListOfIndicesToEntityComponentArray.Count();
            if (count > 0)
            {
                if (count > 1)
                {
                    //get the index in the filter array of the entity to delete
                    var indexInDenseListFromEGID = _indexOfEntityInDenseList[entityID];
                    //get the entityID of the last entity in the filter array
                    uint entityIDToMove = _reverseEIDs[count - 1];
                    
                    //the last index of the last entity is updated to the slot of the deleted entity
                    if (entityIDToMove != entityID)
                    {
                        _indexOfEntityInDenseList[entityIDToMove] = indexInDenseListFromEGID;
                        //the reverseEGID is updated accordingly
                        _reverseEIDs[indexInDenseListFromEGID] = entityIDToMove;
                    }
                    
                    //
                    _reverseEIDs.UnorderedRemoveAt(count - 1);

                    //finally remove the deleted entity from the filters array
                    _denseListOfIndicesToEntityComponentArray.UnorderedRemoveAt(indexInDenseListFromEGID);
                    
                    //remove the entity to delete from the tracked Entity
                    _indexOfEntityInDenseList.Remove(entityID);
                }
                else
                {
                    _indexOfEntityInDenseList.FastClear();
                    _reverseEIDs.Clear();
                    _denseListOfIndicesToEntityComponentArray.Clear();
                }
            }
        }

        NativeDynamicArrayCast<uint>            _denseListOfIndicesToEntityComponentArray;
        NativeDynamicArrayCast<uint>            _reverseEIDs; //forced to use this because it's not a real sparse set
        SharedSveltoDictionaryNative<uint, uint> _indexOfEntityInDenseList;

        readonly ExclusiveGroupStruct _exclusiveGroupStruct;
        readonly int                  _ID;
    }
}