using System.Net;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;


var deviceList = LibPcapLiveDeviceList.Instance;
var devicesBeingCaptured = new List<LibPcapLiveDevice>();
var hostMap = new Dictionary<string, string?>();


StartCaptureDevice(deviceList[4]);

Console.WriteLine("Press Enter to stop capturing...");
Console.ReadLine();

foreach (var device in devicesBeingCaptured) {
    StopCaptureDevice(device);
}

void DeviceOnPacketArrival(object sender, PacketCapture captureEvent) {
    var rawPacket = captureEvent.GetPacket();
    var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
    var ipPacket = packet.Extract<IPPacket>();

    if (ipPacket != null) {
        Console.WriteLine($"Packet from {ipPacket.SourceAddress} to {ipPacket.DestinationAddress}");
    }
}

void StartCaptureDevice(LibPcapLiveDevice device) {
    Console.WriteLine($"Capturing '{device.Description}'");

    device.OnPacketArrival += DeviceOnPacketArrival;
    device.Open(DeviceModes.Promiscuous);
    device.StartCapture();
    devicesBeingCaptured.Add(device);
}

void StopCaptureDevice(LibPcapLiveDevice device) {
    device.StopCapture();
    device.Close();
}