namespace Svelto.ECS
{
    public interface IEntityDescriptor
    {
        IComponentBuilder[] componentsToBuild { get; }
    }
}