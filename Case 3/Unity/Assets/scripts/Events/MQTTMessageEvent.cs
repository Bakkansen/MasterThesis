namespace IoTPlatform.Events {
    using uPLibrary.Networking.M2Mqtt.Messages;


    public class MQTTMessageEvent : GameEvent {
        public string Environment;
        public string RPI;
        public string Device;
        public string Msg;
    }
}
