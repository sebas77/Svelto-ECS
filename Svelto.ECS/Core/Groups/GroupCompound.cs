using System;
using System.Threading;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    /// <summary>
    /// Very naive fail safe, but at least it's simple to understand and safe
    /// </summary>
    static class GroupCompoundInitializer
    {
        internal static readonly ThreadLocal<bool> isInitializing4 = new ThreadLocal<bool>();
        internal static readonly ThreadLocal<bool> isInitializing3 = new ThreadLocal<bool>();
         internal static readonly ThreadLocal<bool> isInitializing2 = new ThreadLocal<bool>();
     }
     
     public abstract class GroupCompound<G1, G2, G3, G4>
         where G1 : GroupTag<G1> where G2 : GroupTag<G2> where G3 : GroupTag<G3> where G4 : GroupTag<G4>
     {
         static readonly FasterList<ExclusiveGroupStruct> _Groups;
 
         public static FasterReadOnlyList<ExclusiveGroupStruct> Groups => new FasterReadOnlyList<ExclusiveGroupStruct>(_Groups);
         public static ExclusiveBuildGroup BuildGroup => new ExclusiveBuildGroup(_Groups[0]);
 
         static GroupCompound()
         {
             if (GroupCompoundInitializer.isInitializing4.Value == false)
             {
                 _Groups = new FasterList<ExclusiveGroupStruct>(1);
 
                 var Group = new ExclusiveGroup();
                 _Groups.Add(Group);
 
                 GroupCompound<G1, G2, G3>.Add(Group);
                 GroupCompound<G1, G2, G4>.Add(Group);
                 GroupCompound<G1, G3, G4>.Add(Group);
                 GroupCompound<G2, G3, G4>.Add(Group);
 
                 GroupCompound<G1, G2>.Add(Group); //<G1/G2> and <G2/G1> must share the same array
                 GroupCompound<G1, G3>.Add(Group);
                 GroupCompound<G1, G4>.Add(Group);
                 GroupCompound<G2, G3>.Add(Group);
                 GroupCompound<G2, G4>.Add(Group);
                 GroupCompound<G3, G4>.Add(Group);
 
                 //This is done here to be sure that the group is added once per group tag
                 //(if done inside the previous group compound it would be added multiple times)
                 GroupTag<G1>.Add(Group);
                 GroupTag<G2>.Add(Group);
                 GroupTag<G3>.Add(Group);
                 GroupTag<G4>.Add(Group);
 
 #if DEBUG
                 GroupMap.idToName[(uint) Group] = $"Compound: {typeof(G1).Name}-{typeof(G2).Name}-{typeof(G3).Name}-{typeof(G4).Name} ID {(uint)Group}";
 #endif
                 GroupCompoundInitializer.isInitializing4.Value = true;
                 //all the combinations must share the same group
                 GroupCompound<G1, G2, G4, G3>._Groups         = _Groups;
                 GroupCompound<G1, G3, G2, G4>._Groups         = _Groups;
                 GroupCompound<G1, G3, G4, G2>._Groups         = _Groups;
                 GroupCompound<G1, G4, G2, G3>._Groups         = _Groups;
                 GroupCompound<G2, G1, G3, G4>._Groups         = _Groups;
                 GroupCompound<G2, G3, G4, G1>._Groups         = _Groups;
                 GroupCompound<G3, G1, G2, G4>._Groups         = _Groups;
                 GroupCompound<G4, G1, G2, G3>._Groups         = _Groups;
                 GroupCompound<G1, G4, G3, G2>._Groups         = _Groups;
                 GroupCompound<G2, G1, G4, G3>._Groups         = _Groups;
                 GroupCompound<G2, G4, G3, G1>._Groups         = _Groups;
                 GroupCompound<G3, G1, G4, G2>._Groups         = _Groups;
                 GroupCompound<G4, G1, G3, G2>._Groups         = _Groups;
                 GroupCompound<G2, G3, G1, G4>._Groups         = _Groups;
                 GroupCompound<G3, G4, G1, G2>._Groups         = _Groups;
                 GroupCompound<G2, G4, G1, G3>._Groups         = _Groups;
                 GroupCompound<G3, G2, G1, G4>._Groups         = _Groups;
                 GroupCompound<G3, G2, G4, G1>._Groups         = _Groups;
                 GroupCompound<G3, G4, G2, G1>._Groups         = _Groups;
                 GroupCompound<G4, G2, G1, G3>._Groups         = _Groups;
                 GroupCompound<G4, G2, G3, G1>._Groups         = _Groups;
                 GroupCompound<G4, G3, G1, G2>._Groups         = _Groups;
                 GroupCompound<G4, G3, G2, G1>._Groups         = _Groups;
                 GroupCompoundInitializer.isInitializing4.Value = false;
             }
         }
 
         public static void Add(ExclusiveGroupStruct @group)
         {
             for (int i = 0; i < _Groups.count; ++i)
                 if (_Groups[i] == group)
                     throw new Exception("temporary must be transformed in unit test");
 
             _Groups.Add(group);
         }
     }
 
     public abstract class GroupCompound<G1, G2, G3>
         where G1 : GroupTag<G1> where G2 : GroupTag<G2> where G3 : GroupTag<G3>
     {
         static readonly FasterList<ExclusiveGroupStruct> _Groups;
 
         public static FasterReadOnlyList<ExclusiveGroupStruct> Groups =>
             new FasterReadOnlyList<ExclusiveGroupStruct>(_Groups);
 
         public static ExclusiveBuildGroup BuildGroup => new ExclusiveBuildGroup(_Groups[0]);
 
         public static void Add(ExclusiveGroupStruct group)
         {
             for (var i = 0; i < _Groups.count; ++i)
                 if (_Groups[i] == group)
                     throw new Exception("temporary must be transformed in unit test");
 
             _Groups.Add(group);
         }
 
         static GroupCompound()
         {
             if (GroupCompoundInitializer.isInitializing3.Value == false)
             {
                 _Groups = new FasterList<ExclusiveGroupStruct>(1);
 
                 var Group = new ExclusiveGroup();
                 _Groups.Add(Group);
 
                 GroupCompound<G1, G2>.Add(Group); //<G1/G2> and <G2/G1> must share the same array
                 GroupCompound<G1, G3>.Add(Group);
                 GroupCompound<G2, G3>.Add(Group);
 
                 //This is done here to be sure that the group is added once per group tag
                 //(if done inside the previous group compound it would be added multiple times)
                 GroupTag<G1>.Add(Group);
                 GroupTag<G2>.Add(Group);
                 GroupTag<G3>.Add(Group);
 
 #if DEBUG
                 GroupMap.idToName[(uint) Group] = $"Compound: {typeof(G1).Name}-{typeof(G2).Name}-{typeof(G3).Name} ID {(uint)Group}";
 #endif
                 //all the combinations must share the same group
                 GroupCompoundInitializer.isInitializing3.Value = true;
                 GroupCompound<G3, G1, G2>._Groups             = _Groups;
                 GroupCompound<G2, G3, G1>._Groups             = _Groups;
                 GroupCompound<G3, G2, G1>._Groups             = _Groups;
                 GroupCompound<G1, G3, G2>._Groups             = _Groups;
                 GroupCompound<G2, G1, G3>._Groups             = _Groups;
                 GroupCompoundInitializer.isInitializing3.Value = false;
             }
         }
     }
 
     public abstract class GroupCompound<G1, G2> where G1 : GroupTag<G1> where G2 : GroupTag<G2>
     {
         static readonly FasterList<ExclusiveGroupStruct> _Groups;
 
         public static FasterReadOnlyList<ExclusiveGroupStruct> Groups =>
             new FasterReadOnlyList<ExclusiveGroupStruct>(_Groups);
 
         public static ExclusiveBuildGroup BuildGroup => new ExclusiveBuildGroup(_Groups[0]);
 
         public static void Add(ExclusiveGroupStruct group)
         {
             for (var i = 0; i < _Groups.count; ++i)
                 if (_Groups[i] == group)
                     throw new Exception("temporary must be transformed in unit test");
 
             _Groups.Add(group);
         }
 
         static GroupCompound()
         {
             if (GroupCompoundInitializer.isInitializing2.Value == false)
             {
                 var Group = new ExclusiveGroup();
 
                 _Groups = new FasterList<ExclusiveGroupStruct>(1);
                 _Groups.Add(Group);
 
                 //every abstract group preemptively adds this group, it may or may not be empty in future
                 GroupTag<G1>.Add(Group);
                 GroupTag<G2>.Add(Group);
 
 #if DEBUG
                 GroupMap.idToName[(uint) Group] = $"Compound: {typeof(G1).Name}-{typeof(G2).Name} ID {(uint)Group}";
 #endif
                 GroupCompoundInitializer.isInitializing2.Value = true;
                 GroupCompound<G2, G1>._Groups                 = _Groups;
                 GroupCompoundInitializer.isInitializing2.Value = false;
             }
         }
     }
 
     /// <summary>
     ///A Group Tag holds initially just a group, itself. However the number of groups can grow with the number of
     ///combinations of GroupTags including this one. This because a GroupTag is an adjective and different entities
     ///can use the same adjective together with other ones. However since I need to be able to iterate over all the
     ///groups with the same adjective, a group tag needs to hold all the groups sharing it.
     /// </summary>
     /// <typeparam name="T"></typeparam>
     public abstract class GroupTag<T> where T : GroupTag<T>
     {
         static readonly FasterList<ExclusiveGroupStruct> _Groups = new FasterList<ExclusiveGroupStruct>(1);
 
         public static FasterReadOnlyList<ExclusiveGroupStruct> Groups =>
             new FasterReadOnlyList<ExclusiveGroupStruct>(_Groups);
 
         public static ExclusiveBuildGroup BuildGroup => new ExclusiveBuildGroup(_Groups[0]);
 
         static GroupTag()
         {
             var group = new ExclusiveGroup();
             _Groups.Add(group);
 
 #if DEBUG
             GroupMap.idToName[(uint) group] = $"Compound: {typeof(T).Name} ID {(uint)group}";
 #endif
         }
 
         //Each time a new combination of group tags is found a new group is added.
         internal static void Add(ExclusiveGroupStruct group)
         {
             for (var i = 0; i < _Groups.count; ++i)
                 if (_Groups[i] == group)
                     throw new Exception("temporary must be transformed in unit test");
 
             _Groups.Add(group);
         }
     }
}