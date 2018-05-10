namespace IoTPlatform.Events {

    using System.Collections.Generic;

    public class GameEvent {

    }

    public class IoTEventHandler {

        static IoTEventHandler instanceInternal = null;
        public static IoTEventHandler Instance {
            get {
                if (instanceInternal == null) {
                    instanceInternal = new IoTEventHandler();
                }
                return instanceInternal;
            }
        }

        public delegate void EventDelegate<T>(T e) where T : GameEvent;
        private delegate void EventDelegate(GameEvent e);

        private Dictionary<System.Type, EventDelegate> delegates = new Dictionary<System.Type, EventDelegate>();
        private Dictionary<System.Delegate, EventDelegate> delegateLookup = new Dictionary<System.Delegate, EventDelegate>();

        public void AddListener<T>(EventDelegate<T> del) where T : GameEvent {
            if (delegateLookup.ContainsKey(del)) {
                return;
            }
            EventDelegate internalDelegate = (e) => del((T)e);
            delegateLookup[del] = internalDelegate;

            EventDelegate tempDel;
            if (delegates.TryGetValue(typeof(T), out tempDel)) {
                delegates[typeof(T)] = tempDel += internalDelegate;
            } else {
                delegates[typeof(T)] = internalDelegate;
            }
        }

        public void RemoveListener<T>(EventDelegate<T> del) where T : GameEvent {
            EventDelegate internalDelegate;
            if (delegateLookup.TryGetValue(del, out internalDelegate)) {
                return;
            }
            EventDelegate tempDel;
            if (delegates.TryGetValue(typeof(T), out tempDel)) {
                tempDel -= internalDelegate;
                if (tempDel == null) {
                    delegates.Remove(typeof(T));
                } else {
                    delegates[typeof(T)] = tempDel;
                }
            }
            delegateLookup.Remove(del);
        }

        public void Raise(GameEvent e) {
            EventDelegate del;
            if (!delegates.TryGetValue(e.GetType(), out del)) {
                return;
            }
            del.Invoke(e);
        }
    }
}
