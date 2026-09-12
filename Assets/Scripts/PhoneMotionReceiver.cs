using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class PhoneMotionReceiver : MonoBehaviour
{
    public int port = 5055;

    [Header("Live Phone Readings")]
    public float pitch;
    public float roll;
    public float ax;
    public float ay;
    public int packetsReceived;
    public bool receiving;

    public double Timestamp { get; private set; }
    public string CentreID { get; private set; }

    private UdpClient client;
    private float lastPacketTime;

    [Serializable]
    private class MotionPacket
    {
        public float pitch;
        public float roll;
        public float ax;
        public float ay;
        public double timestamp;
        public string centreID;
    }

    void OnEnable()
    {
        packetsReceived = 0;
        receiving = false;
        Timestamp = 0;
        CentreID = null;

        try
        {
            client = new UdpClient(port);
            client.Client.Blocking = false;
            Debug.Log("Phone receiver listening on " + port);
        }
        catch (Exception error)
        {
            Debug.LogError(error.Message);
            CloseReceiver();
            enabled = false;
        }
    }

    void Update()
    {
        if (client == null)
            return;

        for (int i = 0; i < 120; i++)
        {
            try
            {
                if (client.Available <= 0)
                    break;

                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                byte[] bytes = client.Receive(ref sender);

                MotionPacket packet = JsonUtility.FromJson<MotionPacket>(
                    Encoding.UTF8.GetString(bytes)
                );

                if (packet == null ||
                    string.IsNullOrEmpty(packet.centreID) ||
                    !Finite(packet.pitch) || !Finite(packet.roll) ||
                    !Finite(packet.ax) || !Finite(packet.ay) ||
                    double.IsNaN(packet.timestamp) ||
                    double.IsInfinity(packet.timestamp))
                    continue;

                // Ignore duplicate or older readings from this calibration.
                if (packet.centreID == CentreID &&
                    packet.timestamp <= Timestamp)
                    continue;

                pitch = packet.pitch;
                roll = packet.roll;
                ax = packet.ax;
                ay = packet.ay;
                Timestamp = packet.timestamp;
                CentreID = packet.centreID;

                packetsReceived++;
                lastPacketTime = Time.unscaledTime;
            }
            catch (SocketException error)
            {
                if (error.SocketErrorCode != SocketError.WouldBlock)
                    Debug.LogWarning(error.Message);

                break;
            }
            catch (ArgumentException)
            {
                // Ignore malformed packets.
            }
        }

        receiving = packetsReceived > 0 &&
            Time.unscaledTime - lastPacketTime < 0.25f;
    }

    private bool Finite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private void CloseReceiver()
    {
        client?.Close();
        client = null;
        receiving = false;
    }

    void OnDisable()
    {
        CloseReceiver();
    }

    void OnDestroy()
    {
        CloseReceiver();
    }
}