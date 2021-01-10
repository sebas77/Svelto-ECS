namespace Svelto.ECS
{
    public interface IQueryingEntitiesEngine : IEngine
    {
        EntitiesDB entitiesDB { set; }

        void Ready();
    }
}