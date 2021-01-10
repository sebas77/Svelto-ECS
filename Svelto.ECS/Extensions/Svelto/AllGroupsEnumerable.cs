using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    /// <summary>
    /// ToDo it would be interesting to have a version of this dedicated to unmanaged, IEntityComponent
    /// that can be burstifiable 
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    public readonly struct AllGroupsEnumerable<T1> where T1 : struct, IEntityComponent
    {
        public ref struct GroupCollection
        {
            internal EntityCollection<T1> collection;
            internal ExclusiveGroupStruct group;

            public void Deconstruct(out EntityCollection<T1> collection, out ExclusiveGroupStruct group)
            {
                collection = this.collection;
                group      = this.@group;
            }
        }

        public AllGroupsEnumerable(EntitiesDB db) { _db = db; }

        public ref struct GroupsIterator
        {
            public GroupsIterator(EntitiesDB db) : this()
            {
                _db = db.FindGroups_INTERNAL(TypeCache<T1>.type).GetEnumerator();
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (_db.MoveNext() == true)
                {
                    var group = _db.Current;
                    ITypeSafeDictionary<T1> typeSafeDictionary = @group.Value as ITypeSafeDictionary<T1>;

                    if (typeSafeDictionary.count == 0)
                        continue;

                    _array.collection = new EntityCollection<T1>(typeSafeDictionary.GetValues(out var count), count);
                    _array.@group     = new ExclusiveGroupStruct(group.Key);

                    return true;
                }

                return false;
            }

            public GroupCollection Current => _array;

            SveltoDictionary<ExclusiveGroupStruct, ITypeSafeDictionary,
                ManagedStrategy<SveltoDictionaryNode<ExclusiveGroupStruct>>, ManagedStrategy<ITypeSafeDictionary>,
                ManagedStrategy<int>>.SveltoDictionaryKeyValueEnumerator _db;

            GroupCollection _array;
        }

        public GroupsIterator GetEnumerator() { return new GroupsIterator(_db); }

        readonly EntitiesDB _db;
    }
}