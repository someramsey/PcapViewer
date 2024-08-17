using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
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

async void Record(string ip) {
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

int FindProcessByPort(ushort srcPort, ushort dstPort) {
    var startInfo = new ProcessStartInfo {
        FileName = "netstat",
        Arguments = "-ano",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using var process = Process.Start(startInfo)!;
    using var reader = process.StandardOutput;

    string output = reader.ReadToEnd();
    string[] lines = output.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

    foreach (string line in lines) {
        if (line.Contains($":{srcPort}")) {
            string[] parts = line.Split([" "], StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 4 && int.TryParse(parts[^1], out int pid)) {
                return pid;
            }
        }
    }


    return -1;
}

void DeviceOnPacketArrival(object sender, PacketCapture captureEvent) {
    var rawPacket = captureEvent.GetPacket();
    var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);


    if (packet.PayloadPacket is IPv4Packet { PayloadPacket: TransportPacket transportPacket } ipPacket) {
        Console.WriteLine($"{ipPacket.SourceAddress}:{transportPacket.SourcePort} =>\n\t{ipPacket.DestinationAddress}:{transportPacket.DestinationPort}");

        var senderDevice = (LibPcapLiveDevice)sender;
        var senderIp = senderDevice.Addresses[0].Addr.ipAddress;

        if (Equals(ipPacket.SourceAddress, senderIp)) {
            var pid = FindProcessByPort(transportPacket.SourcePort, transportPacket.DestinationPort);
            
            if (pid != -1) {
                Console.WriteLine($"Process ID: {pid}");
            }
            
            var process = Process.GetProcessById(pid);
            Console.WriteLine($"Process: {process.ProcessName}");
        } else if (Equals(ipPacket.DestinationAddress, senderIp)) {
            
        }
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