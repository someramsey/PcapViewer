# Pcap Viewer

This is a simple console app running on Dotnet that displays sent and received packets on your device using [SharpPap](https://github.com/dotpcap/sharppcap).

After you launch the app you will be prompted to select a network device to listen on from the available list of devices. Once you select a device, the app will start to display and record sent and received packets. You can press the enter key on your keyboard to stop the capture at any time. This will clear the console and display a list of all the hosts that were contacted and a list of all the apps that used those hosts.

### Sample Output

```sh {"id":"01J64GAS82TMAZHQ3QBRBRMGN6"}
Hosts found: 3
    84.17.53.155 => root-zrh-01.zerotier.com
        zerotier-one_x64
    140.82.113.26 => lb-140-82-113-26-iad.github.com
        Discord
    44.205.171.153 => ec2-44-205-171-153.compute-1.amazonaws.com
        Discord

```

### How to install and run

> #### Download the Release
>
> -   Download the latest release from the [releases page](https://github.com/someramsey/PcapViewer/releases)
> -   Extract the zip file
> -   Open the extracted exe file

> #### Build from source
>
> -   Clone the repository
> -   Open the project folder in your terminal
> -   Run `dotnet build`
> -   Run `dotnet run`

_Might not work for all devices..._
