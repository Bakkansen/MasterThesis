using IoTPlatform.IoTComponents;
using IoTPlatform.Events;
using System.Collections.Generic;
using UnityEngine;
using IoTPlatform.Master.Utility;

public class IoTDevice : MonoBehaviour
{


    public string Environment;
    public string RPI;
    public string ID;
    private List<IoTComponent> components = new List<IoTComponent>();

    // Use this for initialization
    void Start()
    {
        IoTComponent[] comps = GetComponents<IoTComponent>();
        foreach (IoTComponent i in comps)
        {
            components.Add(i);
            if (i.ComponentID == "LED")
            {
                i.SetCurrentState("OFF");
            }
            if (i.ComponentID == "BUTTON")
            {
                i.SetCurrentState("Lifted");
            }
        }
    }

    private void OnEnable()
    {
        IoTEventHandler.Instance.AddListener<ActionEvent>(ActionEventReceived);
        IoTEventHandler.Instance.AddListener<InterfaceEvent>(InterfaceEventReceived);
        IoTEventHandler.Instance.AddListener<StateEvent>(StateEventRecieved);
    }

    private void OnDisable()
    {
        IoTEventHandler.Instance.RemoveListener<ActionEvent>(ActionEventReceived);
        IoTEventHandler.Instance.RemoveListener<InterfaceEvent>(InterfaceEventReceived);
        IoTEventHandler.Instance.RemoveListener<StateEvent>(StateEventRecieved);
    }

    private void ActionEventReceived(ActionEvent e)
    {
        if (IsItMe(e.Environment, e.RPI, e.Device))
        {
            string[] msg = e.Msg.Split(':');
            if (msg.Length != 2)
            {
                Debug.Log("Invalid msg format: [" + msg + "]");
                return;
            }
            if (components.Count == 0)
            {
                Debug.Log("No compoents attached to Gameobject: [" + gameObject.name + "]");
                return;
            }
            foreach (IoTComponent comp in components)
            {
                if (comp.ComponentID == msg[0])
                {
                    comp.SetCurrentState(msg[1]);
                    Debug.Log("Set state: [" + ID + "]" + ":" + msg[1]);
                    return;
                }
            }
        }
    }


    private void StateEventRecieved(StateEvent e)
    {
        if (IsItMe(e.Environment, e.RPI, e.Device))
        {
            foreach (ComponentState c in e.components)
            {
                foreach (IoTComponent comp in components)
                {
                    if (comp.ComponentID == c.componentID)
                    {
                        comp.SetCurrentState(c.currentState);
                    }
                }
            }
        }
    }


    private void InterfaceEventReceived(InterfaceEvent e)
    {
        if (IsItMe(e.Environment, e.RPI, e.Device))
        {
            if (components.Count > 0)
            {
                foreach (IoTComponent c in components)
                {
                    Destroy(c);
                }
            }
            foreach (ComponentInterface c in e.components)
            {
                if (c.componentID == "LED")
                {
                    gameObject.AddComponent<LED>();
                }
                if (c.componentID == "BUTTON")
                {
                    gameObject.AddComponent<IoTPlatform.IoTComponents.Button>();
                }
            }
            MQTTHandler.Instance.MqttPublishMsg(MQTTHandler.MQTTMsgType.State_Req, MQTTHandler.MQTTMsgEnvironment.House, e.RPI, e.Device, "REQ");


        }
    }

    private bool IsItMe(string environment, string rpi, string id)
    {
        return (this.Environment == environment && this.RPI == rpi && (this.ID == id || id == "#"));
    }


}
