using System;

namespace Svelto.ECS.Internal
{
    class EntityGroupNotFoundException : Exception
    {
        public EntityGroupNotFoundException(Type type, string toName)
            : base($"entity group {toName} not used for component type ".FastConcat(type.ToString()))
        {
        }
    }
}