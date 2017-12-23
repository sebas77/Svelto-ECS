using System;
using System.Collections.Generic;

namespace Svelto.ECS
{
    public interface IEntityDescriptor
    {
        IEntityViewBuilder[] entityViewsToBuild { get; }
    }
    
    static class EntityDescriptorTemplate<TType> where TType : IEntityDescriptor, new()
    {
        public static readonly EntityDescriptorInfo Default = new EntityDescriptorInfo(new TType());
    }

    public class EntityDescriptorInfo
    {
        public readonly IEntityDescriptor descriptor;
        public readonly string name;

        public EntityDescriptorInfo(IEntityDescriptor entityDescriptor)
        {
            descriptor = entityDescriptor;
            name = descriptor.ToString();
        }
    }
}

namespace Svelto.ECS.Internal
{
    static class EntityFactory
    {
        internal static void BuildGroupedEntityViews(int entityID, int groupID,
            Dictionary<int, Dictionary<Type, ITypeSafeList>> groupEntityViewsByType,
                                                     EntityDescriptorInfo entityViewsToBuildDescriptor,
                                                     object[] implementors)
        {
            var entityViewsToBuild = entityViewsToBuildDescriptor.descriptor.entityViewsToBuild;
            int count = entityViewsToBuild.Length;

            RemoveEntityImplementor removeEntityImplementor = null;

            for (int index = 0; index < count; index++)
            {
                var entityViewBuilder = entityViewsToBuild[index];
                var entityViewType = entityViewBuilder.GetEntityViewType();

                Dictionary<Type, ITypeSafeList> groupedEntityViewsTyped;

                if (groupEntityViewsByType.TryGetValue(groupID, out groupedEntityViewsTyped) == false)
                {
                    groupedEntityViewsTyped = new Dictionary<Type, ITypeSafeList>();
                    groupEntityViewsByType.Add(groupID, groupedEntityViewsTyped);
                }

                var entityViewObjectToFill =
                    BuildEntityView(entityID, groupedEntityViewsTyped, entityViewType, entityViewBuilder);

                //the semantic of this code must still be improved
                //but only classes can be filled, so I am aware
                //it's a EntityViewWithID
                if (entityViewObjectToFill != null)
                {
                    if (removeEntityImplementor == null)
                        removeEntityImplementor = new RemoveEntityImplementor(entityViewsToBuildDescriptor.descriptor, groupID);

                    FillEntityView(entityViewObjectToFill as EntityView, implementors, removeEntityImplementor, 
                                   entityViewsToBuildDescriptor.name);
                }
            }
        }

        internal static void BuildEntityViews(int entityID, Dictionary<Type, ITypeSafeList> entityViewsByType,
                                              EntityDescriptorInfo entityViewsToBuildDescriptor,
                                              object[] implementors)
        {
            var entityViewsToBuild = entityViewsToBuildDescriptor.descriptor.entityViewsToBuild;
            int count = entityViewsToBuild.Length;
            
            RemoveEntityImplementor removeEntityImplementor = null;

            for (int index = 0; index < count; index++)
            {
                var entityViewBuilder = entityViewsToBuild[index];
                var entityViewType = entityViewBuilder.GetEntityViewType();

                var entityViewObjectToFill =
                    BuildEntityView(entityID, entityViewsByType, entityViewType, entityViewBuilder);

                //the semantic of this code must still be improved
                //but only classes can be filled, so I am aware
                //it's a EntityView
                if (entityViewObjectToFill != null)
                {
                    if (removeEntityImplementor == null)
                        removeEntityImplementor = new RemoveEntityImplementor(entityViewsToBuildDescriptor.descriptor);
                    
                    FillEntityView(entityViewObjectToFill as EntityView, implementors, removeEntityImplementor,
                                   entityViewsToBuildDescriptor.name);
                }
            }
        }

        static IEntityView BuildEntityView(int entityID, Dictionary<Type, ITypeSafeList> entityViewsByType,
                                                 Type entityViewType, IEntityViewBuilder entityViewBuilderId)
        {
            ITypeSafeList entityViewsList;

            var entityViewsPoolWillBeCreated =
                entityViewsByType.TryGetValue(entityViewType, out entityViewsList) == false;
            var entityViewObjectToFill = entityViewBuilderId.BuildEntityViewAndAddToList(ref entityViewsList, entityID);

            if (entityViewsPoolWillBeCreated)
                entityViewsByType.Add(entityViewType, entityViewsList);

            return entityViewObjectToFill as IEntityView;
        }

        static void FillEntityView(EntityView entityView, object[] implementors, RemoveEntityImplementor removeEntity,
                                   string entityDescriptorName)
        {
            for (int index = 0; index < implementors.Length; index++)
            {
                var implementor = implementors[index];

                if (implementor != null)
                {
                    var type = implementor.GetType();

                    Type[] interfaces;
                    if (_cachedTypes.TryGetValue(type, out interfaces) == false)
                        interfaces = _cachedTypes[type] = type.GetInterfaces(); 

                    for (int iindex = 0; iindex < interfaces.Length; iindex++)
                    {
                        var componentType = interfaces[iindex];
#if DEBUG && !PROFILER
                        Tuple<object, int> implementorHolder;
                        if (implementorsByType.TryGetValue(componentType, out implementorHolder) == true)
                            implementorHolder.item2++;
                        else
#endif

                            implementorsByType[componentType] = new Tuple<object, int>(implementor, 0);
                    }
                }
#if DEBUG && !PROFILER
                else
                    Utility.Console.LogError(NULL_IMPLEMENTOR_ERROR.FastConcat(entityView.ToString()));
#endif
            }

            int count;

            //Very efficent way to collect the fields of every EntityViewType
            KeyValuePair<Type, Action<EntityView, object>>[] setters =
                entityView.EntityViewBlazingFastReflection(out count);

            var removeEntityComponentType = typeof(IRemoveEntityComponent);

            for (int i = 0; i < count; i++)
            {
                var keyValuePair = setters[i];
                Type fieldType = keyValuePair.Key;
                
                if (fieldType != removeEntityComponentType)
                {
#if DEBUG && !PROFILER
                    Tuple<object, int> component;
#else
                    object component;
#endif

                    if (implementorsByType.TryGetValue(fieldType, out component) == false)
                    {
                        Exception e = new Exception(NOT_FOUND_EXCEPTION + " Component Type: " + fieldType.Name + " - EntityView: " +
                                                    entityView.GetType().Name + " - EntityDescriptor " + entityDescriptorName);

                        throw e;
                    }
#if DEBUG && !PROFILER
                    if (component.item2 > 1)
                        Utility.Console.LogError(DUPLICATE_IMPLEMENTOR_ERROR.FastConcat(
                                                     "Component Type: ", fieldType.Name, " implementor: ",
                                                     component.item1.ToString()) + " - EntityView: " +
                                                     entityView.GetType().Name + " - EntityDescriptor " + entityDescriptorName);
#endif
#if DEBUG && !PROFILER
                    keyValuePair.Value(entityView, component.item1);
#else
                    keyValuePair.Value(entityView, component);
#endif
                }
                else
                {
                    keyValuePair.Value(entityView, removeEntity);
                }
            }

            implementorsByType.Clear();
        }
#if DEBUG && !PROFILER
        struct Tuple<T1, T2>
        {
            public T1 item1;
            public T2 item2;

            public Tuple(T1 implementor, T2 v)
            {
                item1 = implementor;
                item2 = v;
            }
        }
#endif
        //this is used to avoid newing a dictionary every time, but it's used locally only
#if DEBUG && !PROFILER
        static readonly Dictionary<Type, Tuple<object, int>> implementorsByType = new Dictionary<Type, Tuple<object, int>>();
#else
        static readonly Dictionary<Type, object> implementorsByType = new Dictionary<Type, object>();
#endif
        static Dictionary<Type, Type[]> _cachedTypes = new Dictionary<Type, Type[]>();

        const string DUPLICATE_IMPLEMENTOR_ERROR =
            "<color=orange>Svelto.ECS</color> the same component is implemented with more than one implementor. This is considered an error and MUST be fixed.";

        const string NULL_IMPLEMENTOR_ERROR =
            "<color=orange>Svelto.ECS</color> Null implementor, please be careful about the implementors passed to avoid performance loss";

        const string NOT_FOUND_EXCEPTION = "<color=orange>Svelto.ECS</color> Implementor not found for an EntityView.";
    }
}

