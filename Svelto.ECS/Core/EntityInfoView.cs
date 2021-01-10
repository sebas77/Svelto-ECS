namespace Svelto.ECS
{
    struct EntityInfoComponent: IEntityComponent
    {
        public IComponentBuilder[] componentsToBuild;
    }
}