namespace IoTPlatform.IoTComponents
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ComponentHandler : MonoBehaviour
    {

        static ComponentHandler instanceInternal = null;
        public static ComponentHandler Instance
        {
            get { return instanceInternal; }
        }


        public List<IoTComponent> components = new List<IoTComponent>();

        private void Awake()
        {
            if (instanceInternal != null && instanceInternal != this)
            {
                Destroy(gameObject);
            }
            instanceInternal = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            components.Add(new LED());
            components.Add(new Button());
        }

        public IoTComponent GetIoTComponent(string id)
        {
            foreach (IoTComponent c in components)
            {
                if (c.ComponentID == id)
                {
                    return c;
                }
            }
            return null;
        }
    }
}