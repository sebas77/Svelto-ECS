using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;

namespace Svelto.ECS.Experimental
{
    struct GroupsList
    {
        static GroupsList()
        {
            groups = new FasterList<ExclusiveGroupStruct>();
        }

        static readonly FasterList<ExclusiveGroupStruct> groups;
        
        public FasterList<ExclusiveGroupStruct> reference => groups;
    }

    public ref struct QueryGroups
    {
        static readonly ThreadLocal<GroupsList> groups = new ThreadLocal<GroupsList>();

        public QueryGroups(LocalFasterReadOnlyList<ExclusiveGroupStruct> findGroups)
        {
            var groupsValue = groups.Value;
            var group = groupsValue.reference;

            group.FastClear();
            for (int i = 0; i < findGroups.count; i++)
                group.Add(findGroups[i]);
        }

        public QueryGroups(ExclusiveGroupStruct findGroups)
        {
            var groupsValue = groups.Value;
            var group       = groupsValue.reference;

            group.FastClear();
            group.Add(findGroups);
        }

        public QueryGroups(uint preparecount)
        {
            var groupsValue = groups.Value;
            var group       = groupsValue.reference;

            group.FastClear();
            group.EnsureCapacity(preparecount);
        }

        public QueryResult Except(ExclusiveGroupStruct[] groupsToIgnore)
        {
            var group = groups.Value.reference;
            var groupsCount = group.count;

            for (int i = 0; i < groupsToIgnore.Length; i++)
            for (int j = 0; j < groupsCount; j++)
            {
                    if (groupsToIgnore[i] == group[j])
                    {
                        group.UnorderedRemoveAt(j);
                        j--;
                        groupsCount--;
                    }
            }

            return new QueryResult(group);
        }
        
        public QueryResult Except(FasterList<ExclusiveGroupStruct> groupsToIgnore)
        {
            var group = groups.Value.reference;
            var groupsCount = group.count;

            for (int i = 0; i < groupsToIgnore.count; i++)
            for (int j = 0; j < groupsCount; j++)
            {
                if (groupsToIgnore[i] == group[j])
                {
                    group.UnorderedRemoveAt(j);
                    j--;
                    groupsCount--;
                }
            }

            return new QueryResult(group);
        }
        
        public QueryResult Except(ExclusiveGroupStruct groupsToIgnore)
        {
            var group       = groups.Value.reference;
            var groupsCount = group.count;

            for (int j = 0; j < groupsCount; j++)
                if (groupsToIgnore == group[j])
                {
                    group.UnorderedRemoveAt(j);
                    j--;
                    groupsCount--;
                }

            return new QueryResult(group);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count<T>
            (EntitiesDB entitiesDB, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) where T : struct, IEntityComponent
        {
            int count = 0;

            var groupsCount = groups.count;
            for (int i = 0; i < groupsCount; ++i)
            {
                count += entitiesDB.Count<T>(groups[i]);
            }

            return count;
        }
        
        public QueryResult WithAny<T>(EntitiesDB entitiesDB)
            where T : struct, IEntityComponent
        {
            var group       = groups.Value.reference;
            var groupsCount = group.count;

            for (var i = 0; i < groupsCount; i++)
            {
                if (entitiesDB.Count<T>(group[i]) == 0)
                {
                    group.UnorderedRemoveAt(i);
                    i--;
                    groupsCount--;
                }
            }

            return new QueryResult(group);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ExclusiveGroupStruct group)
        {
            groups.Value.reference.Add(group);
        }
    }

    public readonly ref struct QueryResult
    {
        readonly FasterReadOnlyList<ExclusiveGroupStruct> _group;
        public QueryResult(FasterList<ExclusiveGroupStruct> @group) { _group = @group; }
        
        public FasterReadOnlyList<ExclusiveGroupStruct> result => _group;       
    }
}