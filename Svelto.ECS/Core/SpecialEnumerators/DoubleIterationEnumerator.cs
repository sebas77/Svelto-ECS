using System;

namespace Svelto.ECS
{
    public readonly ref struct DoubleEntitiesEnumerator<T1> where T1 : struct, IEntityComponent
    {
        public DoubleEntitiesEnumerator(GroupsEnumerable<T1> groupsEnumerable) { _groupsEnumerable = groupsEnumerable; }

        public EntityGroupsIterator GetEnumerator() { return new EntityGroupsIterator(_groupsEnumerable); }

        readonly GroupsEnumerable<T1> _groupsEnumerable;

        public ref struct EntityGroupsIterator
        {
            public EntityGroupsIterator(GroupsEnumerable<T1> groupsEnumerable) : this()
            {
                _groupsEnumerableA = groupsEnumerable.GetEnumerator();
                _groupsEnumerableA.MoveNext();
                _groupsEnumerableB = _groupsEnumerableA;
                _indexA            = 0;
                _indexB            = 0;
            }

            public bool MoveNext()
            {
                //once GroupEnumerables complete, they reset. If they reset and moveNext doesn't happen, they are invalid
                while (_groupsEnumerableA.isValid)
                {
                    var (buffersA, _) = _groupsEnumerableA.Current;
                    var (buffersB, _) = _groupsEnumerableB.Current;

                    //current index A must iterate as long as is less than the current A group count
                    while (_indexA < buffersA.count)
                    {
                        //current index B must iterate as long as is less than the current B group count
                        if (++_indexB < buffersB.count)
                        {
                            return true;
                        }

                        //if B iteration is over, move to the next group
                        if (_groupsEnumerableB.MoveNext() == false)
                        {
                            //if there is no valid next groups, we reset B and we need to move to the next A element
                            _groupsEnumerableB = _groupsEnumerableA;
                            (buffersB, _)      = _groupsEnumerableB.Current;
                            ++_indexA; //next A element
                            _indexB = _indexA;
                        }
                        else
                            //otherwise the current A will be checked against the new B group. IndexB must be reset
                            //to work on the new group
                        {
                            _indexB = -1;
                        }
                    }

                    //the current group A iteration is done, so we move to the next A group
                    if (_groupsEnumerableA.MoveNext() == true)
                    {
                        //there is a new group, we reset the iteration
                        _indexA            = 0;
                        _indexB            = 0;
                        _groupsEnumerableB = _groupsEnumerableA;
                    }
                    else
                        return false;
                }

                return false;
            }

            public void Reset() { throw new Exception(); }

            public ValueRef Current
            {
                get
                {
                    var valueRef = new ValueRef(_groupsEnumerableA.Current, _indexA, _groupsEnumerableB.Current
                                              , _indexB);
                    return valueRef;
                }
            }

            public void Dispose() { }

            GroupsEnumerable<T1>.GroupsIterator _groupsEnumerableA;
            GroupsEnumerable<T1>.GroupsIterator _groupsEnumerableB;
            int                                 _indexA;
            int                                 _indexB;
        }

        public ref struct ValueRef
        {
            public readonly GroupsEnumerable<T1>.RefCurrent _current;
            public readonly int                             _indexA;
            public readonly GroupsEnumerable<T1>.RefCurrent _refCurrent;
            public readonly int                             _indexB;

            public ValueRef
            (GroupsEnumerable<T1>.RefCurrent current, int indexA, GroupsEnumerable<T1>.RefCurrent refCurrent
           , int indexB)
            {
                _current    = current;
                _indexA     = indexA;
                _refCurrent = refCurrent;
                _indexB     = indexB;
            }

            public void Deconstruct
                (out EntityCollection<T1> buffers, out int indexA, out EntityCollection<T1> refCurrent, out int indexB)
            {
                buffers    = _current._buffers;
                indexA     = _indexA;
                refCurrent = _refCurrent._buffers;
                indexB     = _indexB;
            }

            public void Deconstruct
            (out EntityCollection<T1> buffers, out int indexA, out ExclusiveGroupStruct groupA
           , out EntityCollection<T1> refCurrent, out int indexB, out ExclusiveGroupStruct groupB)
            {
                buffers    = _current._buffers;
                indexA     = _indexA;
                refCurrent = _refCurrent._buffers;
                indexB     = _indexB;
                groupA     = _current._group;
                groupB     = _refCurrent._group;
            }
        }
    }

    public readonly ref struct DoubleIterationEnumerator<T1, T2> where T1 : struct, IEntityComponent
                                                                where T2 : struct, IEntityComponent
    {
        public DoubleIterationEnumerator(GroupsEnumerable<T1, T2> groupsEnumerable)
        {
            _groupsEnumerable = groupsEnumerable;
        }

        public EntityGroupsIterator GetEnumerator() { return new EntityGroupsIterator(_groupsEnumerable); }

        readonly GroupsEnumerable<T1, T2> _groupsEnumerable;

        public ref struct EntityGroupsIterator
        {
            public EntityGroupsIterator(GroupsEnumerable<T1, T2> groupsEnumerable) : this()
            {
                _groupsEnumerableA = groupsEnumerable.GetEnumerator();
                _groupsEnumerableA.MoveNext();
                _groupsEnumerableB = _groupsEnumerableA;
                _indexA            = 0;
                _indexB            = 0;
            }

            public bool MoveNext()
            {
                 //once GroupEnumerables complete, they reset. If they reset and moveNext doesn't happen, they are invalid
                while (_groupsEnumerableA.isValid)
                {
                    var (buffersA, _) = _groupsEnumerableA.Current;
                    var (buffersB, _) = _groupsEnumerableB.Current;

                    //current index A must iterate as long as is less than the current A group count
                    while (_indexA < buffersA.count)
                    {
                        //current index B must iterate as long as is less than the current B group count
                        if (++_indexB < buffersB.count)
                        {
                            return true;
                        }

                        //if B iteration is over, move to the next group
                        if (_groupsEnumerableB.MoveNext() == false)
                        {
                            //if there is no valid next groups, we reset B and we need to move to the next A element
                            _groupsEnumerableB = _groupsEnumerableA;
                            (buffersB, _)      = _groupsEnumerableB.Current;
                            ++_indexA; //next A element
                            _indexB = _indexA;
                        }
                        else
                            //otherwise the current A will be checked against the new B group. IndexB must be reset
                            //to work on the new group
                        {
                            _indexB = -1;
                        }
                    }

                    //the current group A iteration is done, so we move to the next A group
                    if (_groupsEnumerableA.MoveNext() == true)
                    {
                        //there is a new group, we reset the iteration
                        _indexA            = 0;
                        _indexB            = 0;
                        _groupsEnumerableB = _groupsEnumerableA;
                    }
                    else
                        return false;
                }

                return false;
            }

            public void Reset() { throw new Exception(); }

            public ValueRef Current
            {
                get
                {
                    var valueRef = new ValueRef(_groupsEnumerableA.Current, _indexA, _groupsEnumerableB.Current
                                              , _indexB);
                    return valueRef;
                }
            }

            public void Dispose() { }

            GroupsEnumerable<T1, T2>.GroupsIterator _groupsEnumerableA;
            GroupsEnumerable<T1, T2>.GroupsIterator _groupsEnumerableB;
            int                                     _indexA;
            int                                     _indexB;
        }

        public ref struct ValueRef
        {
            public readonly GroupsEnumerable<T1, T2>.RefCurrent _current;
            public readonly int                                 _indexA;
            public readonly GroupsEnumerable<T1, T2>.RefCurrent _refCurrent;
            public readonly int                                 _indexB;

            public ValueRef
            (GroupsEnumerable<T1, T2>.RefCurrent current, int indexA, GroupsEnumerable<T1, T2>.RefCurrent refCurrent
           , int indexB)
            {
                _current    = current;
                _indexA     = indexA;
                _refCurrent = refCurrent;
                _indexB     = indexB;
            }

            public void Deconstruct(out EntityCollection<T1, T2> buffers, out int indexA, 
                                    out EntityCollection<T1, T2> refCurrent, out int indexB)
            {
                buffers    = _current._buffers;
                indexA     = _indexA;
                refCurrent = _refCurrent._buffers;
                indexB     = _indexB;
            }

            public void Deconstruct
            (out EntityCollection<T1, T2> buffers, out int indexA, out ExclusiveGroupStruct groupA
           , out EntityCollection<T1, T2> refCurrent, out int indexB, out ExclusiveGroupStruct groupB)
            {
                buffers    = _current._buffers;
                indexA     = _indexA;
                refCurrent = _refCurrent._buffers;
                indexB     = _indexB;
                groupA     = _current._group;
                groupB     = _refCurrent._group;
            }
        }
    }

    /// <summary>
    /// Special Enumerator to iterate a group of entities against themselves with complexity n*(n+1)/2 (skips already tested couples)
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    public readonly ref struct DoubleEntitiesEnumerator<T1, T2, T3> where T1 : struct, IEntityComponent
                                                                    where T2 : struct, IEntityComponent
                                                                    where T3 : struct, IEntityComponent
    {
        public DoubleEntitiesEnumerator(GroupsEnumerable<T1, T2, T3> groupsEnumerable)
        {
            _groupsEnumerable = groupsEnumerable;
        }

        public EntityGroupsIterator GetEnumerator() { return new EntityGroupsIterator(_groupsEnumerable); }

        readonly GroupsEnumerable<T1, T2, T3> _groupsEnumerable;

        public ref struct EntityGroupsIterator
        {
            public EntityGroupsIterator(GroupsEnumerable<T1, T2, T3> groupsEnumerable) : this()
            {
                _groupsEnumerableA = groupsEnumerable.GetEnumerator();
                _groupsEnumerableA.MoveNext();
                _groupsEnumerableB = _groupsEnumerableA;
                _indexA            = 0;
                _indexB            = 0;
            }

            public bool MoveNext()
            {
                 //once GroupEnumerables complete, they reset. If they reset and moveNext doesn't happen, they are invalid
                while (_groupsEnumerableA.isValid)
                {
                    var (buffersA, _) = _groupsEnumerableA.Current;
                    var (buffersB, _) = _groupsEnumerableB.Current;

                    //current index A must iterate as long as is less than the current A group count
                    while (_indexA < buffersA.count)
                    {
                        //current index B must iterate as long as is less than the current B group count
                        if (++_indexB < buffersB.count)
                        {
                            return true;
                        }

                        //if B iteration is over, move to the next group
                        if (_groupsEnumerableB.MoveNext() == false)
                        {
                            //if there is no valid next groups, we reset B and we need to move to the next A element
                            _groupsEnumerableB = _groupsEnumerableA;
                            (buffersB, _)      = _groupsEnumerableB.Current;
                            ++_indexA; //next A element
                            _indexB = _indexA;
                        }
                        else
                            //otherwise the current A will be checked against the new B group. IndexB must be reset
                            //to work on the new group
                        {
                            _indexB = -1;
                        }
                    }

                    //the current group A iteration is done, so we move to the next A group
                    if (_groupsEnumerableA.MoveNext() == true)
                    {
                        //there is a new group, we reset the iteration
                        _indexA            = 0;
                        _indexB            = 0;
                        _groupsEnumerableB = _groupsEnumerableA;
                    }
                    else
                        return false;
                }

                return false;
            }

            public void Reset() { throw new Exception(); }

            public ValueRef Current
            {
                get
                {
                    var valueRef = new ValueRef(_groupsEnumerableA.Current, _indexA, _groupsEnumerableB.Current
                                              , _indexB);
                    return valueRef;
                }
            }

            public void Dispose() { }

            GroupsEnumerable<T1, T2, T3>.GroupsIterator _groupsEnumerableA;
            GroupsEnumerable<T1, T2, T3>.GroupsIterator _groupsEnumerableB;
            int                                         _indexA;
            int                                         _indexB;
        }

        public ref struct ValueRef
        {
            public readonly GroupsEnumerable<T1, T2, T3>.RefCurrent _current;
            public readonly int                                     _indexA;
            public readonly GroupsEnumerable<T1, T2, T3>.RefCurrent _refCurrent;
            public readonly int                                     _indexB;

            public ValueRef
            (GroupsEnumerable<T1, T2, T3>.RefCurrent current, int indexA
           , GroupsEnumerable<T1, T2, T3>.RefCurrent refCurrent, int indexB)
            {
                _current    = current;
                _indexA     = indexA;
                _refCurrent = refCurrent;
                _indexB     = indexB;
            }

            public void Deconstruct
            (out EntityCollection<T1, T2, T3> buffers, out int indexA, out EntityCollection<T1, T2, T3> refCurrent
           , out int indexB)
            {
                buffers    = _current._buffers;
                indexA     = _indexA;
                refCurrent = _refCurrent._buffers;
                indexB     = _indexB;
            }

            public void Deconstruct
            (out EntityCollection<T1, T2, T3> buffers, out int indexA, out ExclusiveGroupStruct groupA
           , out EntityCollection<T1, T2, T3> refCurrent, out int indexB, out ExclusiveGroupStruct groupB)
            {
                buffers    = _current._buffers;
                indexA     = _indexA;
                refCurrent = _refCurrent._buffers;
                indexB     = _indexB;
                groupA     = _current._group;
                groupB     = _refCurrent._group;
            }
        }
    }

    /// <summary>
    /// Special Enumerator to iterate a group of entities against themselves with complexity n*(n+1)/2 (skips already tested couples)
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    public readonly ref struct DoubleEntitiesEnumerator<T1, T2, T3, T4> where T1 : struct, IEntityComponent
                                                                        where T2 : struct, IEntityComponent
                                                                        where T3 : struct, IEntityComponent
                                                                        where T4 : struct, IEntityComponent
    {
        public DoubleEntitiesEnumerator(GroupsEnumerable<T1, T2, T3, T4> groupsEnumerable)
        {
            _groupsEnumerable = groupsEnumerable;
        }

        public EntityGroupsIterator GetEnumerator() { return new EntityGroupsIterator(_groupsEnumerable); }

        readonly GroupsEnumerable<T1, T2, T3, T4> _groupsEnumerable;

        public ref struct EntityGroupsIterator
        {
            public EntityGroupsIterator(GroupsEnumerable<T1, T2, T3, T4> groupsEnumerable) : this()
            {
                _groupsEnumerableA = groupsEnumerable.GetEnumerator();
                _groupsEnumerableA.MoveNext();
                _groupsEnumerableB = _groupsEnumerableA;
                _indexA            = 0;
                _indexB            = 0;
            }

            public bool MoveNext()
            {
                 //once GroupEnumerables complete, they reset. If they reset and moveNext doesn't happen, they are invalid
                while (_groupsEnumerableA.isValid)
                {
                    var (buffersA, _) = _groupsEnumerableA.Current;
                    var (buffersB, _) = _groupsEnumerableB.Current;

                    //current index A must iterate as long as is less than the current A group count
                    while (_indexA < buffersA.count)
                    {
                        //current index B must iterate as long as is less than the current B group count
                        if (++_indexB < buffersB.count)
                        {
                            return true;
                        }

                        //if B iteration is over, move to the next group
                        if (_groupsEnumerableB.MoveNext() == false)
                        {
                            //if there is no valid next groups, we reset B and we need to move to the next A element
                            _groupsEnumerableB = _groupsEnumerableA;
                            (buffersB, _)      = _groupsEnumerableB.Current;
                            ++_indexA; //next A element
                            _indexB = _indexA;
                        }
                        else
                            //otherwise the current A will be checked against the new B group. IndexB must be reset
                            //to work on the new group
                        {
                            _indexB = -1;
                        }
                    }

                    //the current group A iteration is done, so we move to the next A group
                    if (_groupsEnumerableA.MoveNext() == true)
                    {
                        //there is a new group, we reset the iteration
                        _indexA            = 0;
                        _indexB            = 0;
                        _groupsEnumerableB = _groupsEnumerableA;
                    }
                    else
                        return false;
                }

                return false;
            }

            public void Reset() { throw new Exception(); }

            public ValueRef Current
            {
                get
                {
                    var valueRef = new ValueRef(_groupsEnumerableA.Current, _indexA, _groupsEnumerableB.Current
                                              , _indexB);
                    return valueRef;
                }
            }

            public void Dispose() { }

            GroupsEnumerable<T1, T2, T3, T4>.GroupsIterator _groupsEnumerableA;
            GroupsEnumerable<T1, T2, T3, T4>.GroupsIterator _groupsEnumerableB;
            int                                             _indexA;
            int                                             _indexB;
        }

        public ref struct ValueRef
        {
            public readonly GroupsEnumerable<T1, T2, T3, T4>.RefCurrent _current;
            public readonly int                                         _indexA;
            public readonly GroupsEnumerable<T1, T2, T3, T4>.RefCurrent _refCurrent;
            public readonly int                                         _indexB;

            public ValueRef
            (GroupsEnumerable<T1, T2, T3, T4>.RefCurrent current, int indexA
           , GroupsEnumerable<T1, T2, T3, T4>.RefCurrent refCurrent, int indexB)
            {
                _current    = current;
                _indexA     = indexA;
                _refCurrent = refCurrent;
                _indexB     = indexB;
            }

            public void Deconstruct
            (out EntityCollection<T1, T2, T3, T4> buffers, out int indexA, out EntityCollection<T1, T2, T3, T4> refCurrent
           , out int indexB)
            {
                buffers    = _current._buffers;
                indexA     = _indexA;
                refCurrent = _refCurrent._buffers;
                indexB     = _indexB;
            }

            public void Deconstruct
            (out EntityCollection<T1, T2, T3, T4> buffers, out int indexA, out ExclusiveGroupStruct groupA
           , out EntityCollection<T1, T2, T3, T4> refCurrent, out int indexB, out ExclusiveGroupStruct groupB)
            {
                buffers    = _current._buffers;
                indexA     = _indexA;
                refCurrent = _refCurrent._buffers;
                indexB     = _indexB;
                groupA     = _current._group;
                groupB     = _refCurrent._group;
            }
        }
    }
}