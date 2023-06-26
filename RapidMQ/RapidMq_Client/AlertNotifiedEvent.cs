using RapidMQ.Models;

namespace RapidMq_Client;

public class AlertNotifiedEvent : MqMessage
{
    public AlertNotifiedEvent(int deviceId, string serialNumber, object[] alerts)
    {
        DeviceId = deviceId;
        SerialNumber = serialNumber;
        Alerts = alerts;
    }

    public int DeviceId { get; set; }
    public string SerialNumber { get; set; }
    public object[] Alerts { get; set; } 
}