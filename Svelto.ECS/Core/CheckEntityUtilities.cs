#if !DEBUG || PROFILE_SVELTO
#define DONT_USE
#endif
using System;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    /// <summary>
    ///     Note: this check doesn't catch the case when an add and remove is done on the same entity before the nextI am
    ///     submission. Two operations on the same entity are not allowed between submissions.
    /// </summary>
    public partial class EnginesRoot
    {
#if DONT_USE        
        [Conditional("CHECK_ALL")]
#endif
        void CheckRemoveEntityID(EGID egid, Type entityDescriptorType, string caller = "")
        {
            if (_multipleOperationOnSameEGIDChecker.ContainsKey(egid) == true)
                throw new ECSException(
                    "Executing multiple structural changes in one submission on the same entity is not supported "
                       .FastConcat(" caller: ", caller, " ").FastConcat(egid.entityID).FastConcat(" groupid: ")
                       .FastConcat(egid.groupID.ToName()).FastConcat(" type: ")
                       .FastConcat(entityDescriptorType != null ? entityDescriptorType.Name : "not available")
                       .FastConcat(" operation was: ")
                       .FastConcat(_multipleOperationOnSameEGIDChecker[egid] == 1 ? "add" : "remove"));

            if (_idChecker.TryGetValue(egid.groupID, out var hash))
                if (hash.Contains(egid.entityID) == false)
                    throw new ECSException("Trying to remove an Entity never submitted in the database "
                                          .FastConcat(" caller: ", caller, " ").FastConcat(egid.entityID)
                                          .FastConcat(" groupid: ").FastConcat(egid.groupID.ToName())
                                          .FastConcat(" type: ")
                                          .FastConcat(entityDescriptorType != null
                                                          ? entityDescriptorType.Name
                                                          : "not available"));
                else
                    hash.Remove(egid.entityID);

            _multipleOperationOnSameEGIDChecker.Add(egid, 0);
        }
#if DONT_USE
        [Conditional("CHECK_ALL")]
#endif
        void CheckAddEntityID(EGID egid, Type entityDescriptorType, string caller = "")
        {
            if (_multipleOperationOnSameEGIDChecker.ContainsKey(egid) == true)
                throw new ECSException(
                    "Executing multiple structural changes in one submission on the same entity is not supported "
                       .FastConcat(" caller: ", caller, " ").FastConcat(egid.entityID).FastConcat(" groupid: ")
                       .FastConcat(egid.groupID.ToName()).FastConcat(" type: ")
                       .FastConcat(entityDescriptorType != null ? entityDescriptorType.Name : "not available")
                       .FastConcat(" operation was: ")
                       .FastConcat(_multipleOperationOnSameEGIDChecker[egid] == 1 ? "add" : "remove"));

            var hash = _idChecker.GetOrCreate(egid.groupID, () => new HashSet<uint>());
            if (hash.Contains(egid.entityID) == true)
                throw new ECSException("Trying to add an Entity already submitted to the database "
                                      .FastConcat(" caller: ", caller, " ").FastConcat(egid.entityID)
                                      .FastConcat(" groupid: ").FastConcat(egid.groupID.ToName()).FastConcat(" type: ")
                                      .FastConcat(entityDescriptorType != null
                                                      ? entityDescriptorType.Name
                                                      : "not available"));
            hash.Add(egid.entityID);
            _multipleOperationOnSameEGIDChecker.Add(egid, 1);
            
        }

#if DONT_USE
        [Conditional("CHECK_ALL")]
#endif
        void RemoveGroupID(ExclusiveBuildGroup groupID) { _idChecker.Remove(groupID); }

#if DONT_USE
        [Conditional("CHECK_ALL")]
#endif
        void ClearChecks() { _multipleOperationOnSameEGIDChecker.FastClear(); }

        readonly FasterDictionary<EGID, uint> _multipleOperationOnSameEGIDChecker = new FasterDictionary<EGID, uint>();
        readonly FasterDictionary<uint, HashSet<uint>> _idChecker = new FasterDictionary<uint, HashSet<uint>>();
    }
}