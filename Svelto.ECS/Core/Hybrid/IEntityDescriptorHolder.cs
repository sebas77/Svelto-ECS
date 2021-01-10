namespace Svelto.ECS
{
    public interface IEntityDescriptorHolder
    {
        IEntityDescriptor GetDescriptor();

        string groupName { get; }
        ushort id { get; }
    }
}