namespace IoTPlatform.IoTComponents
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class IoTComponent : MonoBehaviour
    {
        public string ComponentType { get; set; }
        public string ComponentID { get; set; }
        private int CurrentState { get; set; }
        public List<string> States { get; set; }


        public delegate void OnInteract();

        // Delegate function used to add functions to the event DeviceAdded
        public OnInteract onInteract;


        public abstract void Setup();

        private void OnEnable()
        {
            Setup();
            SetCurrentState("Disabled");
        }

        public string GetCurrentState()
        {
            return States[CurrentState];
        }

        public void SetCurrentState(string state)
        {
            if (States.Contains(state))
            {
                CurrentState = States.IndexOf(state);
            } else
            {
                Debug.Log("[" + this.ComponentID + "] Tried to set to illegal state: (" + state + ")");
            }
        }

        public abstract void Interact(string state);        
    }
}
