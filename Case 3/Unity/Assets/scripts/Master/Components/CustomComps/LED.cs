namespace IoTPlatform.IoTComponents {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class LED : IoTComponent {
        public override void Interact(string state)
        {
            SetCurrentState(state);
            onInteract();
        }

        public override void Setup() {
            this.ComponentType = "LED";
            this.ComponentID = "LED";
            this.States = new List<string>() { "ON", "OFF", "Disabled" };
        }
    }
}
