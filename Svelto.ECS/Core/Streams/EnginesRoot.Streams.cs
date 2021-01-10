namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        internal Consumer<T> GenerateConsumer<T>(string name, uint capacity) where T : unmanaged, IEntityComponent
        {
            return _entityStreams.GenerateConsumer<T>(name, capacity);
        }

        internal Consumer<T> GenerateConsumer<T>(ExclusiveGroupStruct group, string name, uint capacity)
            where T : unmanaged, IEntityComponent
        {
            return _entityStreams.GenerateConsumer<T>(@group, name, capacity);
        }

        internal readonly EntitiesStreams _entityStreams;
    }
}