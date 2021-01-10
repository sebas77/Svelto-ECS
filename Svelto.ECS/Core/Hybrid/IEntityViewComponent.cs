namespace Svelto.ECS.Hybrid
{
    public interface IManagedComponent:IEntityComponent
    {}
    
    public interface IEntityViewComponent:IManagedComponent, INeedEGID
    {}
}

