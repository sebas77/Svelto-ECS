using System;
using System.Collections.Generic;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public interface IComponentBuilder
    {
        void BuildEntityAndAddToList(ref ITypeSafeDictionary dictionary, EGID egid,
            IEnumerable<object> implementors);
        ITypeSafeDictionary Preallocate(ref ITypeSafeDictionary dictionary, uint size);

        Type GetEntityComponentType();
    }
}