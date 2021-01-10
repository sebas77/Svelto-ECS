#if !DEBUG || PROFILE_SVELTO
#define DISABLE_CHECKS
using System.Diagnostics;
#endif
using System;
using System.Reflection;
using Svelto.Common;

namespace Svelto.ECS
{
    static class ComponentBuilderUtilities
    {
        const string MSG = "Entity Components and Entity View Components fields cannot hold managed fields outside the Svelto rules.";

#if DISABLE_CHECKS
        [Conditional("_CHECKS_DISABLED")]
#endif
        public static void CheckFields(Type entityComponentType, bool needsReflection, bool isStringAllowed = false)
        {
            if (entityComponentType == ENTITY_INFO_COMPONENT || entityComponentType == EGIDType ||
                entityComponentType == EXCLUSIVEGROUPSTRUCTTYPE || entityComponentType == SERIALIZABLE_ENTITY_STRUCT)
            {
                return;
            }

            if (needsReflection == false)
            {
                if (entityComponentType.IsClass)
                {
                    throw new ECSException("EntityComponents must be structs.", entityComponentType);
                }

                FieldInfo[] fields = entityComponentType.GetFields(BindingFlags.Public | BindingFlags.Instance);

                for (var i = fields.Length - 1; i >= 0; --i)
                {
                    FieldInfo fieldInfo = fields[i];
                    Type fieldType = fieldInfo.FieldType;

                    SubCheckFields(fieldType, entityComponentType, isStringAllowed);
                }
            }
            else
            {
                FieldInfo[] fields = entityComponentType.GetFields(BindingFlags.Public | BindingFlags.Instance);

                if (fields.Length < 1)
                {
                    ProcessError("No valid fields found in Entity View Components", entityComponentType);
                }

                for (int i = fields.Length - 1; i >= 0; --i)
                {
                    FieldInfo fieldInfo = fields[i];

                    if (fieldInfo.FieldType.IsInterfaceEx() == true)
                    {
                        PropertyInfo[] properties = fieldInfo.FieldType.GetProperties(
                            BindingFlags.Public | BindingFlags.Instance
                                                | BindingFlags.DeclaredOnly);

                        for (int j = properties.Length - 1; j >= 0; --j)
                        {
                            if (properties[j].PropertyType.IsGenericType)
                            {
                                Type genericTypeDefinition = properties[j].PropertyType.GetGenericTypeDefinition();
                                if (genericTypeDefinition == DISPATCHONSETTYPE
                                 || genericTypeDefinition == DISPATCHONCHANGETYPE)
                                {
                                    continue;
                                }
                            }

                            Type propertyType = properties[j].PropertyType;

                            //for EntityComponentStructs, component fields that are structs that hold strings
                            //are allowed
                            SubCheckFields(propertyType, entityComponentType, isStringAllowed: true);
                        }
                    }
                    else
                    if (fieldInfo.FieldType.IsUnmanagedEx() == false)
                    {
                        ProcessError("Entity View Components must hold only public interfaces, strings or unmanaged type fields.",
                                     entityComponentType);

                    }
                }
            }
        }

        static bool IsString(Type type)
        {
            return type == STRINGTYPE || type == STRINGBUILDERTYPE;
        }

        /// <summary>
        /// This method checks the fields if it's an IEntityComponent, but checks all the properties if it's
        /// IEntityViewComponent
        /// </summary>
        /// <param name="fieldType"></param>
        /// <param name="entityComponentType"></param>
        /// <param name="isStringAllowed"></param>
        static void SubCheckFields(Type fieldType, Type entityComponentType, bool isStringAllowed = false)
        {
            //pass if it's Primitive or C# 8 unmanaged, or it's a string and string are allowed
            //this check must allow pointers are they are unmanaged types
            if ((isStringAllowed == true && IsString(fieldType) == true) || fieldType.IsValueTypeEx() == true)
            {
                //if it's a struct we have to check the fields recursively
                if (IsString(fieldType) == false)
                {
                    CheckFields(fieldType, false, isStringAllowed);
                }

                return;
            }
            
            ProcessError(MSG, entityComponentType, fieldType);
        }

        static void ProcessError(string message, Type entityComponentType, Type fieldType = null)
        {
            if (fieldType != null)
            {
                throw new ECSException(message, entityComponentType, fieldType);
            }

            throw new ECSException(message, entityComponentType);
        }

        static readonly Type DISPATCHONCHANGETYPE       = typeof(DispatchOnChange<>);
        static readonly Type DISPATCHONSETTYPE          = typeof(DispatchOnSet<>);
        static readonly Type EGIDType                   = typeof(EGID);
        static readonly Type EXCLUSIVEGROUPSTRUCTTYPE   = typeof(ExclusiveGroupStruct);
        static readonly Type SERIALIZABLE_ENTITY_STRUCT = typeof(SerializableEntityComponent);
        static readonly Type STRINGTYPE                 = typeof(string);
        static readonly Type STRINGBUILDERTYPE          = typeof(System.Text.StringBuilder);

        internal static readonly Type ENTITY_INFO_COMPONENT = typeof(EntityInfoComponent);
    }
}