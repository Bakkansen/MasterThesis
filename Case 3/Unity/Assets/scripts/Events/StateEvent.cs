namespace IoTPlatform.Events {
    using IoTPlatform.Master.Utility;
    using uPLibrary.Networking.M2Mqtt.Messages;


    public class StateEvent : MQTTMessageEvent {
        public bool isConnected;
        public ComponentState[] components;
        
    }
}
