using System;
using System.Collections.Generic;

#pragma warning disable 660,661

namespace Svelto.ECS
{
    /// <summary>
    /// Exclusive Groups guarantee that the GroupID is unique.
    ///
    /// The best way to use it is like:
    ///
    /// public static class MyExclusiveGroups //(can be as many as you want)
    /// {
    ///     public static ExclusiveGroup MyExclusiveGroup1 = new ExclusiveGroup();
    ///
    ///     public static ExclusiveGroup[] GroupOfGroups = { MyExclusiveGroup1, ...}; //for each on this!
    /// }
    /// </summary>
    
    ///To debug it use in your debug window: Svelto.ECS.Debugger.EGID.GetGroupNameFromId(groupID)
    public class ExclusiveGroup
    {
        public const uint MaxNumberOfExclusiveGroups = 2 << 20; 
        
        public ExclusiveGroup()
        {
            _group = ExclusiveGroupStruct.Generate();
        }

        public ExclusiveGroup(string recognizeAs)
        {
            _group = ExclusiveGroupStruct.Generate();

            _knownGroups.Add(recognizeAs, _group);
        }

        public ExclusiveGroup(ushort range)
        {
            _group = new ExclusiveGroupStruct(range);
#if DEBUG
            _range = range;
#endif
        }

        public static implicit operator ExclusiveGroupStruct(ExclusiveGroup group)
        {
            return group._group;
        }
        
        public static explicit operator uint(ExclusiveGroup group)
        {
            return group._group;
        }

        public static ExclusiveGroupStruct operator+(ExclusiveGroup a, uint b)
        {
#if DEBUG
            if (a._range == 0)
                throw new ECSException($"Adding values to a not ranged ExclusiveGroup: {(uint)a}");
            if (b >= a._range)
                throw new ECSException($"Using out of range group: {(uint)a} + {b}");
#endif
            return a._group + b;
        }
        
        //todo document the use case for this method
        public static ExclusiveGroupStruct Search(string holderGroupName)
        {
            if (_knownGroups.ContainsKey(holderGroupName) == false)
                throw new Exception("Named Group Not Found ".FastConcat(holderGroupName));

            return _knownGroups[holderGroupName];
        }

        public override string ToString()
        {
            return _group.ToString();
        }

        static readonly Dictionary<string, ExclusiveGroupStruct> _knownGroups = new Dictionary<string,
            ExclusiveGroupStruct>();

#if DEBUG
        readonly ushort _range;
#endif
        readonly ExclusiveGroupStruct _group;
    }
}

#if future
        public static void ConstructStaticGroups()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Assemblies or types aren't guaranteed to be returned in the same order,
            // and I couldn't find proof that `GetTypes()` returns them in fixed order either,
            // even for builds made with the exact same source code.
            // So will sort reflection results by name before constructing groups.
            var groupFields = new List<KeyValuePair<string, FieldInfo>>();

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = GetTypesSafe(assembly);

                foreach (Type type in types)
                {
                    if (type == null || !type.IsClass)
                    {
                        continue;
                    }

                    // Groups defined as static members in static classes
                    if (type.IsSealed && type.IsAbstract)
                    {
                        FieldInfo[] fields = type.GetFields();
                        foreach(var field in fields)
                        {
                            if (field.IsStatic && typeof(ExclusiveGroup).IsAssignableFrom(field.FieldType))
                            {
                                groupFields.Add(new KeyValuePair<string, FieldInfo>($"{type.FullName}.{field.Name}", field));
                            }
                        }
                    }
                    // Groups defined as classes
                    else if (type.BaseType != null
                             && type.BaseType.IsGenericType
                             && type.BaseType.GetGenericTypeDefinition() == typeof(ExclusiveGroup<>))
                    {
                        FieldInfo field = type.GetField("Group",
                            BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);

                        groupFields.Add(new KeyValuePair<string, FieldInfo>(type.FullName, field));
                    }
                }
            }

            groupFields.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));

            for (int i = 0; i < groupFields.Count; ++i)
            {
                groupFields[i].Value.GetValue(null);
#if DEBUG
                var group = (ExclusiveGroup) groupFields[i].Value.GetValue(null);
                groupNames[(uint) group] = groupFields[i].Key;
#endif
            }
        }

        static Type[] GetTypesSafe(Assembly assembly)
        {
            try
            {
                Type[] types = assembly.GetTypes();

                return types;
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types;
            }
        }

#if DEBUG
        static string[] groupNames = new string[ExclusiveGroup.MaxNumberOfExclusiveGroups];
#endif
#endif