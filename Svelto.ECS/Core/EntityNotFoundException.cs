using System;

namespace Svelto.ECS
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(EGID entityEGID, Type entityType) : base(
            $"entity of type '{entityType}' with ID '{entityEGID.entityID}', group '{entityEGID.groupID.ToName()}' not found!")
        {
        }
    }
}