namespace Svelto.ECS
{
    /// <summary>
    /// use INeedEGID on an IEntityComponent only if you need the EGID. consider using EGIDComponent instead
    /// </summary>
    public interface INeedEGID
    {
        //The set is used only for the framework, but it must stay there
        EGID ID { get; set; }
    }
}