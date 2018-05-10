using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IoTPlatform.Master.Utility;

public class DevHandler : MonoBehaviour {

    static DevHandler instanceInternal = null;
    public static DevHandler Instance
    {
        get { return instanceInternal; }
    }

    public List<IoTDevice> devices = new List<IoTDevice>();

    public delegate void OnDeviceAdded(IoTDevice device);

    // Delegate function used to add functions to the event DeviceAdded
    public OnDeviceAdded onDeviceAdded;

    private void Awake()
    {
        if (instanceInternal != null && instanceInternal != this)
        {
            Destroy(gameObject);
        }
        instanceInternal = this;
        DontDestroyOnLoad(gameObject);
    }

    public List<IoTDevice> GetAllDevices()
    {
        return devices;
    }

    public bool AddNewDevice(string env, string rpi, string id)
    {
        GameObject newDevice = new GameObject("MQTT" + id);
        IoTDevice dev = newDevice.AddComponent<IoTDevice>();
        dev.Environment = env;
        dev.RPI = rpi;
        dev.ID = id;
        devices.Add(dev);
        onDeviceAdded(dev);
        return false;
    }

    public bool RemoveDevice(string ID)
    {
        foreach (IoTDevice dev in devices)
        {
            if (dev.ID == ID)
            {
                devices.Remove(dev);
                Destroy(dev.gameObject);
                return true;
            }
        }
        return false;
    }
}
