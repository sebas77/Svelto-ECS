using System.Collections;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        readonly FasterList<EntitySubmitOperation> _transientEntitiesOperations;

        IEnumerator SubmitEntityComponents(uint maxNumberOfOperations)
        {
            using (var profiler = new PlatformProfiler("Svelto.ECS - Entities Submission"))
            {
                int iterations = 0;
                do
                {
                    var submitEntityComponents = SingleSubmission(profiler, maxNumberOfOperations);
                    while (submitEntityComponents.MoveNext() == true)
                        yield return null;
                } while ((_groupedEntityToAdd.currentEntitiesCreatedPerGroup.count > 0 ||
                          _entitiesOperations.count > 0) && ++iterations < 5);

#if DEBUG && !PROFILE_SVELTO
                if (iterations == 5)
                    throw new ECSException("possible circular submission detected");
#endif
            }
        }

        /// <summary>
        /// Todo: it would be probably better to split even further the logic between submission and callbacks
        /// Something to do when I will optimize the callbacks
        /// </summary>
        /// <param name="profiler"></param>
        /// <param name="maxNumberOfOperations"></param>
        IEnumerator SingleSubmission(PlatformProfiler profiler, uint maxNumberOfOperations)
        {
#if UNITY_NATIVE          
            NativeOperationSubmission(profiler);
#endif
            ClearChecks();

            bool entitiesAreSubmitted = false;
            uint numberOfOperations = 0;
            
            if (_entitiesOperations.count > 0)
            {
                using (profiler.Sample("Remove and Swap operations"))
                {
                    _transientEntitiesOperations.FastClear();
                    _entitiesOperations.CopyValuesTo(_transientEntitiesOperations);
                    _entitiesOperations.FastClear();

                    EntitySubmitOperation[] entitiesOperations = _transientEntitiesOperations.ToArrayFast(out var count);
                    for (var i = 0; i < count; i++)
                    {
                        try
                        {
                            switch (entitiesOperations[i].type)
                            {
                                case EntitySubmitOperationType.Swap:
                                    MoveEntityFromAndToEngines(entitiesOperations[i].builders,
                                        entitiesOperations[i].fromID, entitiesOperations[i].toID);
                                    break;
                                case EntitySubmitOperationType.Remove:
                                    MoveEntityFromAndToEngines(entitiesOperations[i].builders,
                                        entitiesOperations[i].fromID, null);
                                    break;
                                case EntitySubmitOperationType.RemoveGroup:
                                    RemoveEntitiesFromGroup(
                                        entitiesOperations[i].fromID.groupID, profiler);
                                    break;
                                case EntitySubmitOperationType.SwapGroup:
                                    SwapEntitiesBetweenGroups(entitiesOperations[i].fromID.groupID,
                                        entitiesOperations[i].toID.groupID, profiler);
                                    break;
                            }
                        }
                        catch
                        {
                            var str = "Crash while executing Entity Operation "
                                .FastConcat(entitiesOperations[i].type.ToString());
                            
                            
                            Svelto.Console.LogError(str.FastConcat(" ")
#if DEBUG && !PROFILE_SVELTO
                                                      .FastConcat(entitiesOperations[i].trace.ToString())
#endif
                                                 );

                            throw;
                        }

                        ++numberOfOperations;

                        if ((uint)numberOfOperations >= (uint)maxNumberOfOperations)
                        {
                            yield return null;
                            
                            numberOfOperations = 0;
                            
                        }
                    }
                }

                entitiesAreSubmitted = true;
            }

            _groupedEntityToAdd.Swap();

            if (_groupedEntityToAdd.otherEntitiesCreatedPerGroup.count > 0)
            {
                using (profiler.Sample("Add operations"))
                {
                    try
                    {
                        using (profiler.Sample("Add entities to database"))
                        {
                            //each group is indexed by entity view type. for each type there is a dictionary indexed by entityID
                            foreach (var groupToSubmit in _groupedEntityToAdd.otherEntitiesCreatedPerGroup)
                            {
                                var groupID = groupToSubmit.Key;
                                var groupDB = GetOrCreateGroup(groupID, profiler);

                                //add the entityComponents in the group
                                foreach (var entityComponentsToSubmit in _groupedEntityToAdd.other[groupID])
                                {
                                    var type                     = entityComponentsToSubmit.Key;
                                    var targetTypeSafeDictionary = entityComponentsToSubmit.Value;
                                    var wrapper                  = new RefWrapperType(type);

                                    ITypeSafeDictionary dbDic = GetOrCreateTypeSafeDictionary(groupID, groupDB, wrapper, 
                                        targetTypeSafeDictionary);

                                    //Fill the DB with the entity components generate this frame.
                                    dbDic.AddEntitiesFromDictionary(targetTypeSafeDictionary, groupID);
                                }
                            }
                        }

                        //then submit everything in the engines, so that the DB is up to date with all the entity components
                        //created by the entity built
                        using (profiler.Sample("Add entities to engines"))
                        {
                            foreach (var groupToSubmit in _groupedEntityToAdd.otherEntitiesCreatedPerGroup)
                            {
                                var groupID = groupToSubmit.Key;
                                var groupDB = _groupEntityComponentsDB[groupID];
//entityComponentsToSubmit is the array of components found in the groupID per component type. 
//if there are N entities to submit, and M components type to add for each entity, this foreach will run NxM times. 
                                foreach (var entityComponentsToSubmit in _groupedEntityToAdd.other[groupID])
                                {
                                    var realDic = groupDB[new RefWrapperType(entityComponentsToSubmit.Key)];

                                    entityComponentsToSubmit.Value.ExecuteEnginesAddOrSwapCallbacks(_reactiveEnginesAddRemove, realDic,
                                        null, new ExclusiveGroupStruct(groupID), in profiler);
                                    
                                    numberOfOperations += entityComponentsToSubmit.Value.count;

                                    if (numberOfOperations >= maxNumberOfOperations)
                                    {
                                        yield return null;
                                        
                                        numberOfOperations = 0;
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        using (profiler.Sample("clear double buffering"))
                        {
                            //other can be cleared now, but let's avoid deleting the dictionary every time
                            _groupedEntityToAdd.ClearOther();
                        }
                    }
                }
                
                entitiesAreSubmitted = true;
            }

            if (entitiesAreSubmitted)
            {
                var enginesCount = _reactiveEnginesSubmission.count;
                for (int i = 0; i < enginesCount; i++)
                    _reactiveEnginesSubmission[i].EntitiesSubmitted();
            }
        }

        readonly DoubleBufferedEntitiesToAdd                        _groupedEntityToAdd;
        readonly FasterDictionary<ulong, EntitySubmitOperation> _entitiesOperations;
    }
}