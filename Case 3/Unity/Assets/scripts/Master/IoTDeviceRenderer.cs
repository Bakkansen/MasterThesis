using IoTPlatform.IoTComponents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IoTDeviceRenderer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private float buttonPressedRequired = 0.2f;
    private float buttonPressedTimer = 0.0f;
    private bool isButtonPressed;

    private List<IoTComponent> components = new List<IoTComponent>();

    public string RPI { get; private set; }
    public string ID { get; private set; }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnPointerDown(PointerEventData eventData)
    {
        isButtonPressed = true;
        if (eventData.pointerId == -1)
        {
            // Debug.Log("Left Click");
            if (components.Count == 0)
            {
                Debug.Log("No compoents attached to Gameobject: [" + gameObject.name + "]");
                return;
            }
            string msg = "LED:";
            foreach (IoTComponent comp in components)
            {
                if (comp.ComponentID == "LED")
                {
                    if (comp.GetCurrentState() == "ON")
                    {
                        msg += "OFF";
                    } else if (comp.GetCurrentState() == "OFF")
                    {
                        msg += "ON";
                    } else if (comp.GetCurrentState() == "Disabled")
                    {
                        Debug.Log("LED-component disabled [" + gameObject.name + "]");
                        return;
                    }
                } /*else if (comp.ComponentID == "BUTTON" && comp.GetCurrentState() == "Lifted") {
                    MQTTHandler.Instance.client_PublishMsg(MQTTHandler.MQTTMsgType.Action, MQTTHandler.MQTTMsgEnvironment.House, RPI, ID, "BUTTON:Pressed");
                }*/
            }

            MQTTHandler.Instance.MqttPublishMsg(MQTTHandler.MQTTMsgType.Action, MQTTHandler.MQTTMsgEnvironment.House, RPI, ID, msg);
        } else if (eventData.pointerId == -3)
        {
            Debug.Log("Middle Click");
        } else if (eventData.pointerId == -2)
        {
            Debug.Log("Right Click");
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (buttonPressedTimer > buttonPressedRequired)
        {
            foreach (IoTComponent comp in components)
            {
                if (comp.ComponentID == "BUTTON" && comp.GetCurrentState() == "Pressed")
                {
                    MQTTHandler.Instance.MqttPublishMsg(MQTTHandler.MQTTMsgType.Action, MQTTHandler.MQTTMsgEnvironment.House, RPI, ID, "BUTTON:Lifted");
                }
            }
        }
        isButtonPressed = false;
        buttonPressedTimer = 0.0f;
    }
}
