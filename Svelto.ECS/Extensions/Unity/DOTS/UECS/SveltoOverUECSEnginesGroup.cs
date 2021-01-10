#if UNITY_ECS
using Svelto.Common;
using Svelto.ECS.Schedulers;
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    /// <summary>
    /// This is a high level class to abstract the complexity of creating a Svelto ECS application that interacts
    /// with UECS. However this is designed to make it work almost out of the box, but it should be eventually
    /// substituted by project customized code.
    /// This is a JobifiedEngine and as such it expect to be ticked. Normally it must be executed in a
    /// SortedEnginesGroup as step that happens after the Svelto jobified engines run. The flow should be:
    /// Svelto Engines Run
    /// This Engine runs, which causeS:
    /// Jobs to be completed (it's a sync point)
    /// Synchronizations engines to be executed (Svelto to UECS)
    /// Submission of Entities to be executed
    /// Svelto Add/Remove callbacks to be called
    /// ISubmissionEngines to be executed
    /// UECS engines to executed
    /// Synchronizations engines to be executed (UECS To Svelto)
    /// </summary>
    [Sequenced(nameof(JobifiedSveltoEngines.SveltoOverUECS))]
    public class SveltoOverUECSEnginesGroup: IJobifiedEngine
    {
        public SveltoOverUECSEnginesGroup(EnginesRoot enginesRoot)
        {
            DBC.ECS.Check.Require(enginesRoot.scheduler is SimpleEntitiesSubmissionScheduler, "The Engines root must use a EntitiesSubmissionScheduler scheduler implementation");

            CreateUnityECSWorldForSvelto(enginesRoot.scheduler as SimpleEntitiesSubmissionScheduler, enginesRoot);
        }

        public World world { get; private set; }

        void CreateUnityECSWorldForSvelto(SimpleEntitiesSubmissionScheduler scheduler, EnginesRoot enginesRoot)
        {
            world = new World("Svelto<>UECS world");

            var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);
            World.DefaultGameObjectInjectionWorld = world;

            //This is the UECS group that takes care of all the UECS systems that creates entities
            //it also submits Svelto entities
            _sveltoUecsEntitiesSubmissionGroup = new SveltoUECSEntitiesSubmissionGroup(scheduler, world);
            //This is the group that handles the UECS sync systems that copy the svelto entities values to UECS entities
            _syncSveltoToUecsGroup = new SyncSveltoToUECSGroup();
            enginesRoot.AddEngine(_syncSveltoToUecsGroup);
            _syncUecsToSveltoGroup = new SyncUECSToSveltoGroup();
            enginesRoot.AddEngine(_syncUecsToSveltoGroup);
            //This is the group that handles the UECS sync systems that copy the UECS entities values to svelto entities
            //enginesRoot.AddEngine(new SveltoUECSEntitiesSubmissionGroup(scheduler, world));
            enginesRoot.AddEngine(this);

            _enginesRoot = enginesRoot;
        }

        public JobHandle Execute(JobHandle inputDeps)
        {
            //this is a sync point, there won't be pending jobs after this
            _sveltoUecsEntitiesSubmissionGroup.SubmitEntities(inputDeps);

            //Mixed explicit job dependency and internal automatic ECS dependency system
            //Write in to UECS entities so the UECS dependencies react on the components touched
            var handle = _syncSveltoToUecsGroup.Execute(default);

            //As long as pure UECS systems do not use external containers (like native arrays and so) the Unity
            //automatic dependencies system will guarantee that there won't be race conditions
            world.Update();

            //this svelto group of UECS SystemBase systems
            return _syncUecsToSveltoGroup.Execute(handle);
        }

        public void AddUECSSubmissionEngine(SubmissionEngine spawnUnityEntityOnSveltoEntityEngine)
        {
            _sveltoUecsEntitiesSubmissionGroup.Add(spawnUnityEntityOnSveltoEntityEngine);
            _enginesRoot.AddEngine(spawnUnityEntityOnSveltoEntityEngine);
        }

        public void AddSveltoToUECSEngine(SyncSveltoToUECSEngine engine)
        {
            //it's a Svelto Engine/UECS SystemBase so it must be added in the UECS world AND svelto enginesRoot
            world.AddSystem(engine);
            _enginesRoot.AddEngine(engine);

            _syncSveltoToUecsGroup.Add(engine);
        }

        public void AddUECSToSveltoEngine(SyncUECSToSveltoEngine engine)
        {
            //it's a Svelto Engine/UECS SystemBase so it must be added in the UECS world AND svelto enginesRoot
            world.AddSystem(engine);
            _enginesRoot.AddEngine(engine);

            _syncUecsToSveltoGroup.Add(engine);
        }

        public void Dispose()
        {
            world.Dispose();
        }

        public string name => nameof(SveltoOverUECSEnginesGroup);
        
        SveltoUECSEntitiesSubmissionGroup _sveltoUecsEntitiesSubmissionGroup;
        SyncSveltoToUECSGroup             _syncSveltoToUecsGroup;
        SyncUECSToSveltoGroup             _syncUecsToSveltoGroup;
        EnginesRoot                       _enginesRoot;
    }
}
#endif