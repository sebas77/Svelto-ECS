using Svelto.ECS.Internal;

namespace Svelto.ECS.Internal
{
    public interface IReactEngine: IEngine
    {}
    
    public interface IReactOnAddAndRemove : IReactEngine
    {}
    
    public interface IReactOnDispose : IReactEngine
    {}

    public interface IReactOnSwap : IReactEngine
    {}
}

namespace Svelto.ECS
{
    public interface IEngine
    {}
    
    public interface IReactOnAddAndRemove<T> : IReactOnAddAndRemove where T : IEntityComponent
    {
        void Add(ref T entityComponent, EGID egid);
        void Remove(ref T entityComponent, EGID egid);
    }
    
    public interface IReactOnDispose<T> : IReactOnDispose where T : IEntityComponent
    {
        void Remove(ref T entityComponent, EGID egid);
    }
    
    public interface IReactOnSwap<T> : IReactOnSwap where T : IEntityComponent
    {
        void MovedTo(ref T entityComponent, ExclusiveGroupStruct previousGroup, EGID egid);
    }

    public interface IReactOnSubmission:IReactEngine
    {
        void EntitiesSubmitted();
    }
}