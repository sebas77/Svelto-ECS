using System;

namespace Svelto.ECS
{
    public class DispatchOnSet<T>
    {
        public DispatchOnSet(EGID senderID, Action<EGID, T> callback):this(senderID)
        {
            NotifyOnValueSet(callback);
        }
        public DispatchOnSet(EGID senderID) { _senderID = senderID; }

        public T value
        {
            set
            {
                _value = value;

                if (_paused == false)
                    _subscriber(_senderID, value);
            }
        }

        public void NotifyOnValueSet(Action<EGID, T> action)
        {
#if DEBUG && !PROFILE_SVELTO
            DBC.ECS.Check.Require(_subscriber == null, $"{this.GetType().Name}: listener already registered");
#endif
            _subscriber = action;
            _paused     = false;
        }

        public void StopNotify()
        {
            _subscriber = null;
            _paused     = true;
        }

        public void PauseNotify()  { _paused = true; }
        public void ResumeNotify() { _paused = false; }

        protected T    _value;
        readonly  EGID _senderID;

        Action<EGID, T> _subscriber;
        bool            _paused;
    }
}