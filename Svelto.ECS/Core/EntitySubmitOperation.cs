using System;

namespace Svelto.ECS
{
#pragma warning disable 660,661
    struct EntitySubmitOperation
#pragma warning restore 660,661
        : IEquatable<EntitySubmitOperation>
    {
        public readonly EntitySubmitOperationType type;
        public readonly IComponentBuilder[]       builders;
        public readonly EGID                      fromID;
        public readonly EGID                      toID;
#if DEBUG && !PROFILE_SVELTO
        public System.Diagnostics.StackFrame trace;
#endif

        public EntitySubmitOperation(EntitySubmitOperationType operation, EGID from, EGID to,
                                     IComponentBuilder[]          builders         = null)
        {
            type          = operation;
            this.builders = builders;
            fromID        = from;
            toID          = to;
#if DEBUG && !PROFILE_SVELTO
            trace = default;
#endif
        }

        public EntitySubmitOperation
        (EntitySubmitOperationType operation, ExclusiveGroupStruct @group
       , IComponentBuilder[] descriptorComponentsToBuild):this()
        {
            type          = operation;
            this.builders = descriptorComponentsToBuild;
            fromID = new EGID(0, group);
#if DEBUG && !PROFILE_SVELTO
            trace = default;
#endif            
        }

        public static bool operator ==(EntitySubmitOperation obj1, EntitySubmitOperation obj2)
        {
            return obj1.Equals(obj2);
        }
        
        public static bool operator !=(EntitySubmitOperation obj1, EntitySubmitOperation obj2)
        {
            return obj1.Equals(obj2) == false;
        }

        public bool Equals(EntitySubmitOperation other)
        {
            return type == other.type && fromID == other.fromID && toID == other.toID;
        }
    }

    enum EntitySubmitOperationType
    {
        Swap,
        Remove,
        RemoveGroup,
        SwapGroup
    }
}