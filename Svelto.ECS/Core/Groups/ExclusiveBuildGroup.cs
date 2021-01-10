namespace Svelto.ECS
{
    public readonly struct ExclusiveBuildGroup
    {
        internal ExclusiveBuildGroup(ExclusiveGroupStruct group)
        {
            this.group = group;
        }

        public static implicit operator ExclusiveBuildGroup(ExclusiveGroupStruct group)
        {
            return new ExclusiveBuildGroup(group);
        }

        public static implicit operator ExclusiveBuildGroup(ExclusiveGroup group)
        {
            return new ExclusiveBuildGroup(group);
        }
        
        public static implicit operator uint(ExclusiveBuildGroup groupStruct)
        {
            return groupStruct.group;
        }

        internal ExclusiveGroupStruct @group { get; }
    }
}