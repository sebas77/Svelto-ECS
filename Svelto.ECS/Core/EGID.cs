using System;
using System.Runtime.InteropServices;

#pragma warning disable 660,661

namespace Svelto.ECS
{
    [Serialization.DoNotSerialize]
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct EGID:IEquatable<EGID>,IComparable<EGID>
    {
        [FieldOffset(0)] public readonly uint                 entityID;
        [FieldOffset(4)] public readonly ExclusiveGroupStruct groupID;
        [FieldOffset(0)]        readonly ulong                _GID;

        public static readonly EGID Empty = new EGID();
        
        public static bool operator ==(EGID obj1, EGID obj2)
        {
            return obj1._GID == obj2._GID;
        }

        public static bool operator !=(EGID obj1, EGID obj2)
        {
            return obj1._GID != obj2._GID;
        }

        public EGID(uint entityID, ExclusiveGroupStruct groupID) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, groupID);
        }
        
        public EGID(uint entityID, ExclusiveBuildGroup groupID) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, groupID.group);
        }

        static ulong MAKE_GLOBAL_ID(uint entityId, uint groupId)
        {
            return (ulong)groupId << 32 | ((ulong)entityId & 0xFFFFFFFF);
        }

        public static explicit operator uint(EGID id)
        {
            return id.entityID;
        }

        //in the way it's used, ulong must be always the same for each id/group
        public static explicit operator ulong(EGID id) { return id._GID; }

        public bool Equals(EGID other)
        {
            return _GID == other._GID;
        }

        public bool Equals(EGID x, EGID y)
        {
            return x == y;
        }

        public override int GetHashCode()
        {
            return _GID.GetHashCode();
        }

        public int GetHashCode(EGID egid)
        {
            return egid.GetHashCode();
        }

        public int CompareTo(EGID other)
        {
            return _GID.CompareTo(other._GID);
        }
        
        internal EGID(uint entityID, uint groupID) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, groupID);
        }

        public override string ToString()
        {
            var value = groupID.ToName();
            return "id ".FastConcat(entityID).FastConcat(" group ").FastConcat(value);
        }
    }
}
