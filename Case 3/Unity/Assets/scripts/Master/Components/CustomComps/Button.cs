namespace IoTPlatform.IoTComponents {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Button : IoTComponent {
        public override void Interact(string state)
        {
            SetCurrentState(state);
            onInteract();
        }

        public override void Setup() {
            this.ComponentType = "BUTTON";
            this.ComponentID = "BUTTON";
            this.States = new List<string>() { "Pressed", "Lifted", "Disabled" };
        }
    }
}
