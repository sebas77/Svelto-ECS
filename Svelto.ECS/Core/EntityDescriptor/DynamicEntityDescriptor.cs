using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    /// <summary>
    /// DynamicEntityDescriptor can be used to add entity components to an existing EntityDescriptor that act as flags,
    /// at building time.
    /// This method allocates, so it shouldn't be abused
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public struct DynamicEntityDescriptor<TType> : IDynamicEntityDescriptor where TType : IEntityDescriptor, new()
    {
        internal DynamicEntityDescriptor(bool isExtendible) : this()
        {
            var defaultEntities = EntityDescriptorTemplate<TType>.descriptor.componentsToBuild;
            var length = defaultEntities.Length;

            ComponentsToBuild = new IComponentBuilder[length + 1];

            Array.Copy(defaultEntities, 0, ComponentsToBuild, 0, length);

            //assign it after otherwise the previous copy will overwrite the value in case the item
            //is already present
            ComponentsToBuild[length] = new ComponentBuilder<EntityInfoComponent>
            (
                new EntityInfoComponent
                {
                    componentsToBuild = ComponentsToBuild
                }
            );
        }

        public DynamicEntityDescriptor(IComponentBuilder[] extraEntityBuilders) : this()
        {
            var extraEntitiesLength = extraEntityBuilders.Length;

            ComponentsToBuild = Construct(extraEntitiesLength, extraEntityBuilders,
                EntityDescriptorTemplate<TType>.descriptor.componentsToBuild);
        }

        public DynamicEntityDescriptor(FasterList<IComponentBuilder> extraEntityBuilders) : this()
        {
            var extraEntities = extraEntityBuilders.ToArrayFast(out _);
            var extraEntitiesLength = extraEntityBuilders.count;

            ComponentsToBuild = Construct((int) extraEntitiesLength, extraEntities,
                EntityDescriptorTemplate<TType>.descriptor.componentsToBuild);
        }

        public void ExtendWith<T>() where T : IEntityDescriptor, new()
        {
            var newEntitiesToBuild = EntityDescriptorTemplate<T>.descriptor.componentsToBuild;

            ComponentsToBuild = Construct(newEntitiesToBuild.Length, newEntitiesToBuild, ComponentsToBuild);
        }
        
        public void ExtendWith(IComponentBuilder[] extraEntities)
        {
            ComponentsToBuild = Construct(extraEntities.Length, extraEntities, ComponentsToBuild);
        }

        static IComponentBuilder[] Construct(int extraEntitiesLength, IComponentBuilder[] extraEntities,
            IComponentBuilder[] startingEntities)
        {
            IComponentBuilder[] localEntitiesToBuild;

            if (extraEntitiesLength == 0)
            {
                localEntitiesToBuild = startingEntities;
                return localEntitiesToBuild;
            }

            var defaultEntities = startingEntities;
            
            var index = SetupEntityInfoComponent(defaultEntities, out localEntitiesToBuild, extraEntitiesLength);

            Array.Copy(extraEntities, 0, localEntitiesToBuild, defaultEntities.Length, extraEntitiesLength);

            //assign it after otherwise the previous copy will overwrite the value in case the item
            //is already present
            localEntitiesToBuild[index] = new ComponentBuilder<EntityInfoComponent>
            (
                new EntityInfoComponent
                {
                    componentsToBuild = localEntitiesToBuild
                }
            );

            return localEntitiesToBuild;
        }

        static int SetupEntityInfoComponent(IComponentBuilder[] defaultEntities, out IComponentBuilder[] componentsToBuild,
            int extraLenght)
        {
            int length = defaultEntities.Length;
            int index = -1;

            for (var i = 0; i < length; i++)
            {
                //the special entity already exists
                if (defaultEntities[i].GetEntityComponentType() == ComponentBuilderUtilities.ENTITY_INFO_COMPONENT)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                index = length + extraLenght;
                componentsToBuild = new IComponentBuilder[index + 1];
            }
            else
                componentsToBuild = new IComponentBuilder[length + extraLenght];

            Array.Copy(defaultEntities, 0, componentsToBuild, 0, length);

            return index;
        }

        public IComponentBuilder[] componentsToBuild => ComponentsToBuild;

        IComponentBuilder[] ComponentsToBuild;
    }
}