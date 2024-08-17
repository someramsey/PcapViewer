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

    using var cts = new CancellationTokenSource(1000);

    hostMap[ip] = null;

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

void DeviceOnPacketArrival(object sender, PacketCapture captureEvent) {
    var rawPacket = captureEvent.GetPacket();
    var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
    
    
    
    if(packet.PayloadPacket is IPv4Packet ipPacket) {
        if(ipPacket.PayloadPacket is TcpPacket tcpPacket) {
               
        } else if(ipPacket.PayloadPacket is UdpPacket udpPacket) {
            
        }
    }
    
    //
    // var ipPacket = packet.Extract<IPPacket>();
    //
    // if (ipPacket != null) {
    //     LibPcapLiveDevice senderDevice = (LibPcapLiveDevice)sender;
    //     var senderIp = senderDevice.Addresses[0].Addr.ipAddress;
    //
    //     var packetSourceIp = ipPacket.SourceAddress;
    //     var packetDestIp = ipPacket.DestinationAddress;
    //
    //     if (Equals(packetSourceIp, senderIp)) {
    //         
    //         
    //         
    //         
    //         
    //         
    //         
    //
    //
    //         Console.WriteLine($"Packet sent from {packetSourceIp} to {packetDestIp}");
    //     } else if (Equals(packetDestIp, senderIp)) {
    //         Console.WriteLine($"Packet received from {packetSourceIp} to {packetDestIp}");
    //     }
    //
    //     // RecordIp(ipPacket.SourceAddress.ToString());
    //     // RecordIp(ipPacket.DestinationAddress.ToString());
    // }
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