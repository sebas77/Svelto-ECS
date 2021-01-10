#if UNITY_5 || UNITY_5_3_OR_NEWER
using System;
using Object = UnityEngine.Object;
using System.Collections;
using UnityEngine;

namespace Svelto.ECS.Schedulers.Unity
{
    //The EntitySubmissionScheduler has been introduced to make the entity components submission logic platform independent
    //You can customize the scheduler if you wish
    public class UnityEntitiesSubmissionScheduler : EntitiesSubmissionScheduler
    {
        class Scheduler : MonoBehaviour
        {
            public Scheduler()
            {
                _coroutine = Coroutine();
            }

            void Update()
            {
                _coroutine.MoveNext();
            }
            
            IEnumerator Coroutine()
            {
                while (true)
                {
                    yield return _wait;
                    
                    onTick();
                }
            }

            readonly WaitForEndOfFrame _wait = new WaitForEndOfFrame();
            readonly IEnumerator       _coroutine;
            
            public System.Action onTick;
        }

        public UnityEntitiesSubmissionScheduler(string name)
        {
            _scheduler = new GameObject(name).AddComponent<Scheduler>();
            GameObject.DontDestroyOnLoad(_scheduler.gameObject);
            _scheduler.onTick = SubmitEntities;
        }

        public override void Dispose()
        {
            if (_scheduler != null && _scheduler.gameObject != null)
            {
                Object.Destroy(_scheduler.gameObject);
            }
        }

        public override bool paused { get; set; }

        void SubmitEntities()
        {
            if (paused == false)
            {
                var enumerator = _onTick.Invoke(UInt32.MaxValue);
                while (enumerator.MoveNext()) ;
            }
        }

        protected internal override EnginesRoot.EntitiesSubmitter onTick
        {
            set => _onTick = value;
        }

        readonly Scheduler       _scheduler;
        EnginesRoot.EntitiesSubmitter _onTick;
    }
}
#endif