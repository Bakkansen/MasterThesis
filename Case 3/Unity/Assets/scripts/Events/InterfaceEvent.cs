namespace IoTPlatform.Events {
    using uPLibrary.Networking.M2Mqtt.Messages;
    using IoTPlatform.Master.Utility;

    public class InterfaceEvent : MQTTMessageEvent {
        public ComponentInterface[] components;
    }
}
