namespace Svelto.ECS
{
    public interface IStepEngine : IEngine
    {
        void Step();
        
        string name { get; }
    }
    
    public interface IStepEngine<T> : IEngine
    {
        void Step(in T _param);
        
        string name { get; }
    }
    
    //this must stay IStep Engine as it may be part of a group itself
    public interface IStepGroupEngine : IStepEngine
    {
    }
    
    public interface IStepGroupEngine<T> : IStepEngine<T>
    {
    }
}