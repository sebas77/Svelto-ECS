namespace Svelto.ECS
{
    /// <summary>
    /// still experimental alternative to ExclusiveGroup, use this like:
    /// use this like:
    /// public class TriggersGroup : ExclusiveGroup<TriggersGroup> {}
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class NamedExclusiveGroup<T>
    {
        public static ExclusiveGroup Group = new ExclusiveGroup();
        public static string         name  = typeof(T).FullName;

        static NamedExclusiveGroup()
        {
#if DEBUG        
            GroupMap.idToName[(uint) Group] = $"{name} ID {(uint)Group}";
#endif            
        }
        //      protected NamedExclusiveGroup(string recognizeAs) : base(recognizeAs)  {}
        //    protected NamedExclusiveGroup(ushort range) : base(range) {}
    }
}