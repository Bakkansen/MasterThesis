using System.Collections;
using System.Net;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using IoTPlatform.Events;
using IoTPlatform.Master.Utility;

using System;


public class MQTTHandler : MonoBehaviour
{

    // Singleton
    static MQTTHandler instanceInternal = null;
    public static MQTTHandler Instance
    {
        get { return instanceInternal; }
    }

    public delegate void OnConnectRequest();

    public OnConnectRequest onConnectRequest;


    public bool AutoConnectNewDevices = true;

    private MqttClient client;
    private string mqttBrokerAddress = "mqtt.idi.ntnu.no";
    private int mqttBrokerPort = 1883;
    private string eventSubTopic = "FtRD/Event/House/#";
    private string actionSubTopic = "FtRD/Action/House/#";
    private string stateReqSubTopic = "FtRD/State_Req/House/#";
    private string stateSubTopic = "FtRD/State/House/#";
    private string interfaceReqSubTopic = "FtRD/Interface_Req/House/#";
    private string interfaceSubTopic = "FtRD/Interface/House/#";
    private string connectReqSubTopic = "FtRD/ConnectReq/House/#";
    private string connectSubTopic = "FtRD/Connect/House/#";
    private string AllTopicLabel = "#";
    private string WildcardLabel = "+";

    public enum MQTTMsgType { Event, Action, State_Req, State, Interface_Req, Interface, ConnectReq, Connect, ALL };
    public enum MQTTMsgEnvironment { House, ALL };


    private void Awake()
    {
        if (instanceInternal != null && instanceInternal != this)
        {
            Destroy(gameObject);
        }
        instanceInternal = this;
        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    void Start()
    {
        client = new MqttClient(mqttBrokerAddress, mqttBrokerPort, false, null);

        client.MqttMsgPublishReceived += MqttMsgPublishReceived;

        string clientId = Guid.NewGuid().ToString();
        client.Connect(clientId);

        client.Subscribe(new string[] { actionSubTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        Debug.Log("Subscribed to: [" + actionSubTopic + "]");

        client.Subscribe(new string[] { stateSubTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        Debug.Log("Subscribed to: [" + stateSubTopic + "]");

        client.Subscribe(new string[] { interfaceSubTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        Debug.Log("Subscribed to: [" + interfaceSubTopic + "]");

        client.Subscribe(new string[] { connectReqSubTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        Debug.Log("Subscribed to: [" + connectReqSubTopic + "]");

        Debug.Log("Start Called " + client.ToString());
    }

    internal void MqttPublishMsg(MQTTMsgType action, MQTTMsgEnvironment house, object rPI, object iD, string v)
    {
        throw new NotImplementedException();
    }

    void MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string[] topics = e.Topic.Split('/');
        if (!Enum.IsDefined(typeof(MQTTMsgType), topics[1]))
        {
            Debug.Log("Received message from different topic: " + e.Topic);
            return;
        }

        MQTTMsgType topic = (MQTTMsgType)Enum.Parse(typeof(MQTTMsgType), topics[1]);
        switch (topic)
        {
            case MQTTMsgType.Event:
                ActionPerformed(e);
                break;
            case MQTTMsgType.Interface:
                InterfaceReceived(e);
                break;
            case MQTTMsgType.State:
                StateReceived(e);
                break;
            case MQTTMsgType.ConnectReq:
                ConnectReqReceived(e);
                break;
        }


    }

    private void ConnectReqReceived(MqttMsgPublishEventArgs e)
    {
        string[] topics = e.Topic.Split('/');
        string env = "#";
        string rpi = "#";
        string dev = "#";
        if (topics.Length > 2)
        {
            env = topics[2];
        }
        if (topics.Length > 3)
        {
            rpi = topics[3];
        }
        if (topics.Length > 4)
        {
            dev = topics[4];
        }

        if (AutoConnectNewDevices)
        {
            DevHandler.Instance.AddNewDevice(env, rpi, dev);
            Debug.Log("Topic to publish to: " + interfaceReqSubTopic + " : ['REQ']");
            Byte[] bytes = System.Text.Encoding.UTF8.GetBytes("REQ");
            client.Publish(interfaceReqSubTopic, bytes);
        } else
        {
            onConnectRequest();
        }
    }

    private void StateReceived(MqttMsgPublishEventArgs e)
    {
        string[] topics = e.Topic.Split('/');
        string env = "#";
        string rpi = "#";
        string dev = "#";
        if (topics.Length > 2)
        {
            env = topics[2];
        }
        if (topics.Length > 3)
        {
            rpi = topics[3];
        }
        if (topics.Length > 4)
        {
            dev = topics[4];
        }

        Debug.Log("[State]: " + env + "/" + rpi + "/" + dev);
        IoTState iotState = JsonUtility.FromJson<IoTState>(System.Text.Encoding.UTF8.GetString(e.Message));
        IoTEventHandler.Instance.Raise(new StateEvent()
        {
            Device = dev,
            RPI = rpi,
            Environment = env,
            isConnected = iotState.connected,
            components = iotState.components
        });
    }

    private void InterfaceReceived(MqttMsgPublishEventArgs e)
    {
        string[] topics = e.Topic.Split('/');
        string env = "#";
        string rpi = "#";
        string dev = "#";
        if (topics.Length > 2)
        {
            env = topics[2];
        }
        if (topics.Length > 3)
        {
            rpi = topics[3];
        }
        if (topics.Length > 4)
        {
            dev = topics[4];
        }

        Debug.Log("[Interface]: " + env + "/" + rpi + "/" + dev);
        IoTInterface ioTInterface = JsonUtility.FromJson<IoTInterface>(System.Text.Encoding.UTF8.GetString(e.Message));
        IoTEventHandler.Instance.Raise(new InterfaceEvent()
        {
            Device = dev,
            RPI = rpi,
            Environment = env,
            components = ioTInterface.components
        });
    }


    private void ActionPerformed(MqttMsgPublishEventArgs e)
    {
        // Debug.Log("[Action]: " + System.Text.Encoding.UTF8.GetString(e.Message));
        string[] topics = e.Topic.Split('/');
        string env = "#";
        string rpi = "#";
        string dev = "#";
        if (topics.Length > 2)
        {
            env = topics[2];
        }
        if (topics.Length > 3)
        {
            rpi = topics[3];
        }
        if (topics.Length > 4)
        {
            dev = topics[4];
        }
        Debug.Log("[Action] " + env + "/" + rpi + "/" + dev + ": " + System.Text.Encoding.UTF8.GetString(e.Message));

        IoTEventHandler.Instance.Raise(new ActionEvent
        {
            Environment = env,
            RPI = rpi,
            Device = dev,
            Msg = System.Text.Encoding.UTF8.GetString(e.Message)
        });
    }

    public void MqttPublishMsg(MQTTMsgType type, MQTTMsgEnvironment area, string RPI, string device, string msg)
    {
        string topic = "FtRD/";
        if (type == MQTTMsgType.ALL)
        {
            topic += "#";
        } else
        {
            topic += type.ToString() + "/";

            if (area == MQTTMsgEnvironment.ALL)
            {
                topic += "#";
            } else
            {
                topic += area.ToString() + "/";

                if (RPI == AllTopicLabel)
                {
                    topic += AllTopicLabel;
                } else
                {
                    topic += RPI + "/";

                    if (device == AllTopicLabel)
                    {
                        topic += AllTopicLabel;
                    } else
                    {
                        topic += device;
                    }
                }
            }
        }
        Debug.Log("Topic to publish to: " + topic + " : [" + msg + "]");
        Byte[] bytes = System.Text.Encoding.UTF8.GetBytes(msg);
        client.Publish(topic, bytes);
    }

    private void OnDestroy()
    {
        client.Disconnect();
    }
}
