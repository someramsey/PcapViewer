﻿using System.Net;
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

Console.Clear();
Console.WriteLine($"Hosts found: {hostMap.Count}");

foreach (var (ip, host) in hostMap) {
    Console.WriteLine($"{ip} =>\n\t{host ?? "Unknown"}");
}


return;

async void RecordIp(string ip) {
    if (hostMap.ContainsKey(ip)) {
        return;
    }

    hostMap[ip] = null;

    using var cts = new CancellationTokenSource(1000);

    try {
        var hostEntry = await Dns.GetHostEntryAsync(ip);
        hostMap[ip] = hostEntry.HostName;
        Console.WriteLine($"{ip} resolved to {hostEntry.HostName}");
    } catch (OperationCanceledException) {
        hostMap[ip] = null;
        Console.WriteLine($"DNS resolution for {ip} timed out.");
    } catch (Exception ex) {
        Console.WriteLine($"DNS resolution for {ip} failed: {ex.Message}");
    }
}

int GetSourcePacketPort(Packet packet) {
    return packet switch {
        TcpPacket tcp => tcp.SourcePort,
        UdpPacket udp => udp.SourcePort,
        _ => -1
    };
}

void DeviceOnPacketArrival(object sender, PacketCapture captureEvent) {
    var rawPacket = captureEvent.GetPacket();
    var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
    var ipPacket = packet.Extract<IPPacket>();

    if (ipPacket != null) {
        
        // RecordIp(ipPacket.SourceAddress.ToString());
        // RecordIp(ipPacket.DestinationAddress.ToString());
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