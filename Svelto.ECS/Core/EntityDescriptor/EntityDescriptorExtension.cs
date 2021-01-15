namespace Svelto.ECS
{
    public static class EntityDescriptorExtension
    {
        public static bool IsUnmanaged(this IEntityDescriptor descriptor)
        {
            foreach (var component in descriptor.componentsToBuild)
                if (component.isUnmanaged == false)
                    return false;
            
            return true;
        }
    }
}