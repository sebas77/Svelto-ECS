using System;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EntitiesDB
    {
        public LocalFasterReadOnlyList<ExclusiveGroupStruct> FindGroups<T1>() where T1 : IEntityComponent
        {
            FasterList<ExclusiveGroupStruct> result = groups.Value;
            result.FastClear();
            if (groupsPerEntity.TryGetValue(TypeRefWrapper<T1>.wrapper
                                           , out FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary> result1) == false)
                return result;
        
            var result1Count           = result1.count;
            var fasterDictionaryNodes1 = result1.unsafeKeys;
        
            for (int j = 0; j < result1Count; j++)
            {
                result.Add(new ExclusiveGroupStruct(fasterDictionaryNodes1[j].key));
            }
        
            return result;
        }
        
        public LocalFasterReadOnlyList<ExclusiveGroupStruct> FindGroups<T1, T2>() where T1 : IEntityComponent  where T2 : IEntityComponent
        {
            FasterList<ExclusiveGroupStruct> result = groups.Value;
            result.FastClear();
            if (groupsPerEntity.TryGetValue(TypeRefWrapper<T1>.wrapper
                                           , out FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary> result1) == false)
                return result;
            if (groupsPerEntity.TryGetValue(TypeRefWrapper<T2>.wrapper
                                           , out FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary> result2) == false)
                return result;
            
            var result1Count           = result1.count;
            var result2Count           = result2.count;
            var fasterDictionaryNodes1 = result1.unsafeKeys;
            var fasterDictionaryNodes2 = result2.unsafeKeys;

            for (int i = 0; i < result1Count; i++)
            {
                var groupID = fasterDictionaryNodes1[i].key;
                for (int j = 0; j < result2Count; j++)
                {
                    //if the same group is found used with both T1 and T2
                    if (groupID == fasterDictionaryNodes2[j].key)
                    {
                        result.Add(new ExclusiveGroupStruct(groupID));
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Remember that this operation os O(N*M*P) where N,M,P are the number of groups where each component
        /// is found.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <returns></returns>
        public LocalFasterReadOnlyList<ExclusiveGroupStruct> FindGroups<T1, T2, T3>()
            where T1 : IEntityComponent where T2 : IEntityComponent where T3 : IEntityComponent
        {
            FasterList<ExclusiveGroupStruct> result = groups.Value;
            result.FastClear();
            if (groupsPerEntity.TryGetValue(TypeRefWrapper<T1>.wrapper
              , out FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary> groupOfEntities1) == false)
                return result;
            if (groupsPerEntity.TryGetValue(TypeRefWrapper<T2>.wrapper
              , out FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary> groupOfEntities2) == false)
                return result;
            if (groupsPerEntity.TryGetValue(TypeRefWrapper<T3>.wrapper
              , out FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary> groupOfEntities3) == false)
                return result;
            
            var result1Count           = groupOfEntities1.count;
            var result2Count           = groupOfEntities2.count;
            var result3Count           = groupOfEntities3.count;
            var fasterDictionaryNodes1 = groupOfEntities1.unsafeKeys;
            var fasterDictionaryNodes2 = groupOfEntities2.unsafeKeys;
            var fasterDictionaryNodes3 = groupOfEntities3.unsafeKeys;
            
            //
            //TODO: I have to find once for ever a solution to be sure that the entities in the groups match
            //Currently this returns group where the entities are found, but the entities may not match in these
            //groups.
            //Checking the size of the entities is an early check, needed, but not sufficient, as entities components may
            //coincidentally match in number but not from which entities they are generated
            
            //foreach group where T1 is found
            for (int i = 0; i < result1Count; i++)
            {
                var groupT1 = fasterDictionaryNodes1[i].key;
                
                //foreach group where T2 is found
                for (int j = 0; j < result2Count; ++j)
                {
                    if (groupT1 == fasterDictionaryNodes2[j].key)
                    {
                        //foreach group where T3 is found
                        for (int k = 0; k < result3Count; ++k)
                        {
                            if (groupT1 == fasterDictionaryNodes3[k].key)
                            {
                                result.Add(new ExclusiveGroupStruct(groupT1));
                                break;
                            }
                        }
                        
                        break;
                    }
                }
            }

            return result;
        }

        internal FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary> FindGroups_INTERNAL(Type type) 
        {
            if (groupsPerEntity.ContainsKey(new RefWrapperType(type)) == false)
                return _emptyDictionary;

            return groupsPerEntity[new RefWrapperType(type)];
        }

        struct GroupsList
        {
            static GroupsList()
            {
                groups = new FasterList<ExclusiveGroupStruct>();
            }

            static readonly FasterList<ExclusiveGroupStruct> groups;

            public static implicit operator FasterList<ExclusiveGroupStruct>(in GroupsList list)
            {
                return list.reference;
            }

            FasterList<ExclusiveGroupStruct> reference => groups;
        }
        
        static readonly ThreadLocal<GroupsList> groups = new ThreadLocal<GroupsList>();
    }
}