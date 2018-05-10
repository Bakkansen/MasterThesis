namespace IoTPlatform.Master.Utility
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ComponentInterface
    {
        public string componentID;
        public string componentType;
        public string[] states;
    }

    public class ComponentState
    {
        public string componentID;
        public string currentState;
    }

    public class IoTInterface
    {
        public ComponentInterface[] components;
    }

    public class IoTState
    {
        public bool connected;
        public ComponentState[] components;
    }

}
