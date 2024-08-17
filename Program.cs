using System.Diagnostics;
using System.Net;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;



var deviceList = LibPcapLiveDeviceList.Instance;
var devicesBeingCaptured = new List<LibPcapLiveDevice>();
var hostMap = new Dictionary<string, RequestInfo>();

StartCaptureDevice(deviceList[4]);

Console.WriteLine("Press Enter to stop capturing...");
Console.ReadLine();

foreach (var device in devicesBeingCaptured) {
    StopCaptureDevice(device);
}

Console.Clear();
Console.WriteLine($"Hosts found: {hostMap.Count}");

foreach (var entry in hostMap) {
    if(entry.Value.Host is not null) {
        Console.WriteLine($"{entry.Key} => {entry.Value.Host}");
        Console.WriteLine($"\t{string.Join(", ", entry.Value.Processes)}");
    }
}
return;


async Task<IPHostEntry?> ResolveHostEntry(string ip) {
    using var cts = new CancellationTokenSource(1000);

    try {
        return await Dns.GetHostEntryAsync(ip);
    } catch (OperationCanceledException) {
        Console.WriteLine($"DNS resolution for {ip} timed out.");
    } catch (Exception ex) {
        Console.WriteLine($"DNS resolution for {ip} failed: {ex.Message}");
    }
    
    return null;
}

async void Record(string ip, ushort port, string process) {
    string key = $"{ip}:{port}";

    if (!hostMap.TryGetValue(key, out var requestInfo)) {
        var hostEntry = await ResolveHostEntry(ip);

        requestInfo = new RequestInfo {
            Processes = [],
            Host = hostEntry?.HostName
        };

        hostMap[ip] = requestInfo;
    }

    if (!requestInfo.Processes.Contains(process)) {
        requestInfo.Processes.Add(process);

        Console.WriteLine($"({process}) =>\n\t{ip}:{port}\n\t{requestInfo.Host}");
    }
}

bool FindProcessByPort(ushort srcPort, ushort dstPort, out int processId) {
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
        if (line.Contains($":{srcPort}") || line.Contains($":{dstPort}")) {
            string[] parts = line.Split([" "], StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 4 && int.TryParse(parts[^1], out int pid)) {
                processId = pid;


                return true;
            }
        }
    }

    processId = -1;


    return false;
}

void DeviceOnPacketArrival(object sender, PacketCapture captureEvent) {
    var rawPacket = captureEvent.GetPacket();
    var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);

    if (packet.PayloadPacket is IPv4Packet { PayloadPacket: TransportPacket transportPacket } ipPacket) {
        // Console.WriteLine($"{ipPacket.SourceAddress}:{transportPacket.SourcePort} =>\n\t{ipPacket.DestinationAddress}:{transportPacket.DestinationPort}");

        if (FindProcessByPort(transportPacket.SourcePort, transportPacket.DestinationPort, out int pid)) {
            var process = Process.GetProcessById(pid);

            var senderDevice = (LibPcapLiveDevice)sender;
            var senderIp = senderDevice.Addresses[0].Addr.ipAddress;

            var packetSourceIp = ipPacket.SourceAddress;
            var packetDestIp = ipPacket.DestinationAddress;

            if (Equals(packetSourceIp, senderIp)) {
                Record(ipPacket.DestinationAddress.ToString(), transportPacket.DestinationPort, process.ProcessName);
            } else if (Equals(packetDestIp, senderIp)) {
                Record(ipPacket.SourceAddress.ToString(), transportPacket.SourcePort, process.ProcessName);
            }


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

public struct RequestInfo {
    public string? Host;
    public List<string> Processes;
}