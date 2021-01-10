using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.Utilities;

namespace Svelto.ECS
{
#if DEBUG && !PROFILE_SVELTO
    struct ECSTuple<T1, T2>
    {
        public readonly T1 instance;
        public          T2 numberOfImplementations;

        public ECSTuple(T1 implementor, T2 v)
        {
            instance = implementor;
            numberOfImplementations = v;
        }
    }
#endif

    static class EntityComponentUtility
    {
        const string DUPLICATE_IMPLEMENTOR_ERROR =
            "<color=teal>Svelto.ECS</color> the same component is implemented with more than one implementor. This is "
          + "considered an error and MUST be fixed. ";

        const string NULL_IMPLEMENTOR_ERROR =
            "<color=teal>Svelto.ECS</color> Null implementor, please be careful about the implementors passed to avoid "
          + "performance loss ";

        const string NOT_FOUND_EXCEPTION =
            "<color=teal>Svelto.ECS</color> Implementor not found for an EntityComponent. ";

        public static void FillEntityComponent<T>
        (this IComponentBuilder componentBuilder, ref T entityComponent
       , FasterList<KeyValuePair<Type, FastInvokeActionCast<T>>> entityComponentBlazingFastReflection
       , IEnumerable<object> implementors
#if DEBUG && !PROFILE_SVELTO
      ,Dictionary<Type, ECSTuple<object, int>> implementorsByType
#else
      ,  Dictionary<Type, object> implementorsByType
#endif
       , Dictionary<Type, Type[]> cachedTypeInterfaces)
        {
            //efficient way to collect the fields of every EntityComponentType
            var setters = FasterList<KeyValuePair<Type, FastInvokeActionCast<T>>>.NoVirt.ToArrayFast(
                    entityComponentBlazingFastReflection, out var count);
            
            //todo this should happen once per T, not once per Build<T>
            if (implementors != null)
            {
                foreach (var implementor in implementors)
                {
                    if (implementor != null)
                    {
                        var type = implementor.GetType();

                        if (cachedTypeInterfaces.TryGetValue(type, out var interfaces) == false)
                            interfaces = cachedTypeInterfaces[type] = type.GetInterfacesEx();

                        for (var iindex = 0; iindex < interfaces.Length; iindex++)
                        {
                            var componentType = interfaces[iindex];
#if DEBUG && !PROFILE_SVELTO
                            if (implementorsByType.TryGetValue(componentType, out var implementorData))
                            {
                                implementorData.numberOfImplementations++;
                                implementorsByType[componentType] = implementorData;
                            }
                            else
                                implementorsByType[componentType] = new ECSTuple<object, int>(implementor, 1);
#else
                        implementorsByType[componentType] = implementor;
#endif
                        }
                    }
#if DEBUG && !PROFILE_SVELTO
                    else
                    {
                        Console.Log(NULL_IMPLEMENTOR_ERROR.FastConcat(" entityComponent "
                                                                    , componentBuilder
                                                                     .GetEntityComponentType().ToString()));
                    }
#endif
                }
            }

            for (var i = 0; i < count; i++)
            {
                var fieldSetter = setters[i];
                var fieldType   = fieldSetter.Key;

#if DEBUG && !PROFILE_SVELTO
                ECSTuple<object, int> implementor;
#else
                object implementor;
#endif

                if (implementorsByType.TryGetValue(fieldType, out implementor) == false)
                {
                    var e = new ECSException(NOT_FOUND_EXCEPTION + " Component Type: " + fieldType.Name
                                           + " - EntityComponent: " + componentBuilder.GetEntityComponentType().Name);

                    throw e;
                }
#if DEBUG && !PROFILE_SVELTO
                if (implementor.numberOfImplementations > 1)
                    throw new ECSException(DUPLICATE_IMPLEMENTOR_ERROR.FastConcat(
                        "Component Type: ", fieldType.Name, " implementor: ", implementor.instance.ToString()) +
                                           " - EntityComponent: " + componentBuilder.GetEntityComponentType().Name);
#endif
#if DEBUG && !PROFILE_SVELTO
                fieldSetter.Value(ref entityComponent, implementor.instance);
#else
                fieldSetter.Value(ref entityComponent, implementor);
#endif
            }

            implementorsByType.Clear();
        }
    }
}