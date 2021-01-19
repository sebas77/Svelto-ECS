using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    // The EntityLocatorMap provides a bidirectional map to help locate entities without using an EGID which might
    // change in runtime. The Entity Locator map uses a reusable unique identifier struct called EntityLocator to
    // find the last known EGID from last entity submission.
    public partial class EnginesRoot : IEntityReferenceLocatorMap
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CreateReferenceLocator(EGID egid)
        {
            // Check if we need to create a new EntityLocator or whether we can recycle an existing one.s
            EntityReference reference;
            if (_nextReferenceIndex == _entityLocatorMap.count)
            {
                _entityLocatorMap.Add(new EntityLocatorMapElement(egid));
                reference = new EntityReference(_nextReferenceIndex++);
            }
            else
            {
                ref var element = ref _entityLocatorMap[_nextReferenceIndex];
                reference = new EntityReference(_nextReferenceIndex, element.version);
                // The recycle entities form a linked list, using the egid.entityID to store the next element.
                _nextReferenceIndex = element.egid.entityID;
                element.egid = egid;
            }

            // Update reverse map from egid to locator.
            if (_egidToLocatorMap.TryGetValue(egid.groupID, out var groupMap) == false)
            {
                groupMap = new FasterDictionary<uint, EntityReference>();
                _egidToLocatorMap[egid.groupID] = groupMap;
            }
            groupMap[egid.entityID] = reference;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void UpdateReferenceLocator(EGID from, EGID to)
        {
            var locator = GetAndRemoveReferenceLocator(from);

#if DEBUG && !PROFILE_SVELTO
            if (locator.Equals(EntityReference.Invalid))
            {
                throw new ECSException("Unable to update locator from egid: "
                    .FastConcat(from.ToString(), "to egid: ")
                    .FastConcat(to.ToString(), ". Locator was not found")
                );
            }
#endif

            _entityLocatorMap[locator.uniqueID].egid = to;

            if (_egidToLocatorMap.TryGetValue(to.groupID, out var groupMap) == false)
            {
                groupMap = new FasterDictionary<uint, EntityReference>();
                _egidToLocatorMap[to.groupID] = groupMap;
            }
            groupMap[to.entityID] = locator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RemoveReferenceLocator(EGID egid)
        {
            var locator = GetAndRemoveReferenceLocator(egid);

#if DEBUG && !PROFILE_SVELTO
            if (locator.Equals(EntityReference.Invalid))
            {
                throw new ECSException("Unable to remove locator for egid: "
                    .FastConcat(egid.ToString(), ". Locator was not found")
                );
            }
#endif

            // Invalidate the entity locator element by bumping its version and setting the egid to point to a unexisting element.
            _entityLocatorMap[locator.uniqueID].egid = new EGID(_nextReferenceIndex, 0);
            _entityLocatorMap[locator.uniqueID].version++;

            // Mark the element as the last element used.
            _nextReferenceIndex = locator.uniqueID;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RemoveAllGroupReferenceLocators(uint groupId)
        {
            if (_egidToLocatorMap.TryGetValue(groupId, out var groupMap) == false)
            {
                return;
            }

            // We need to traverse all entities in the group and remove the locator using the egid.
            // RemoveLocator would modify the enumerator so this is why we traverse the dictionary from last to first.
            var keys = groupMap.unsafeKeys;
            for (var i = groupMap.count - 1; true; i--)
            {
                RemoveReferenceLocator(new EGID(keys[i].key, groupId));
                if (i == 0) break;
            }

            _egidToLocatorMap.Remove(groupId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void UpdateAllGroupReferenceLocators(uint fromGroupId, uint toGroupId)
        {
            if (_egidToLocatorMap.TryGetValue(fromGroupId, out var groupMap) == false)
            {
                return;
            }

            // We need to traverse all entities in the group and update the locator using the egid.
            // UpdateLocator would modify the enumerator so this is why we traverse the dictionary from last to first.
            var keys = groupMap.unsafeKeys;
            for (var i = groupMap.count - 1; true; i--)
            {
                UpdateReferenceLocator(new EGID(keys[i].key, fromGroupId), new EGID(keys[i].key, toGroupId));
                if (i == 0) break;
            }

            _egidToLocatorMap.Remove(fromGroupId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        EntityReference GetAndRemoveReferenceLocator(EGID egid)
        {
            if (_egidToLocatorMap.TryGetValue(egid.groupID, out var groupMap))
            {
                if (groupMap.TryGetValue(egid.entityID, out var locator))
                {
                    groupMap.Remove(egid.entityID);
                    return locator;
                }
            }

            return EntityReference.Invalid;
        }

        EntityReference IEntityReferenceLocatorMap.GetEntityReference(EGID egid)
        {
            if (_egidToLocatorMap.TryGetValue(egid.groupID, out var groupMap))
            {
                if (groupMap.TryGetValue(egid.entityID, out var locator))
                {
                    return locator;
                }
            }

            return EntityReference.Invalid;
        }

        bool IEntityReferenceLocatorMap.TryGetEGID(EntityReference reference, out EGID egid)
        {
            egid = new EGID();
            if (reference == EntityReference.Invalid) return false;
            // Make sure we are querying for the current version of the locator.
            // Otherwise the locator is pointing to a removed entity.
            if (_entityLocatorMap[reference.uniqueID].version == reference.version)
            {
                egid = _entityLocatorMap[reference.uniqueID].egid;
                return true;
            }

            return false;
        }

        uint _nextReferenceIndex;
        readonly FasterList<EntityLocatorMapElement> _entityLocatorMap;
        readonly FasterDictionary<uint, FasterDictionary<uint, EntityReference>> _egidToLocatorMap;
    }
}