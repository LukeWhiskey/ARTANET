using System;
using SharpPcap;
using PacketDotNet;
using PacketDotNet.Ieee80211;
using PacketDotNet.Tcp;
using System.Net.Sockets;

class Program
{
    static void Main(string[] args)
    {
        // Retrieve the list of available network interfaces
        var devices = CaptureDeviceList.Instance;
        if (devices.Count == 0)
        {
            Console.WriteLine("No capture devices found.");
            return;
        }

        Console.WriteLine(devices.Count);
        // Choose the first network interface
        ICaptureDevice device = devices[5];

        // Set the filter expression to capture only TCP packets
        string filterExpression = "";

        try
        {
            // Open the device for capturing
            device.OnPacketArrival += (sender, e) => PacketHandler(sender, e);
            device.Open(DeviceModes.Promiscuous);
            device.Filter = filterExpression;
        
            // Start capturing packets
            Console.WriteLine($"Listening on {device.Description}...");
            device.StartCapture();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            // Ensure device is closed and resources are released
            device?.Close();
            return;
        }

        // Wait for user input to stop capturing
        Console.WriteLine("Press any key to stop capturing...");
        Console.ReadKey();

        // Stop capturing packets and release resources
        device.StopCapture();
        device.Close();
    }

    static void PacketHandler(object sender, PacketCapture e)
    {
        try
        {
            // Parse the packet
            var packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.Data.ToArray());

            // Get Packet Protoocl Name
            var protocol = packet.PayloadPacket.GetType().Name;
            // Get Packet Data Type
            var packetType = packet.PayloadPacket;

            // IPv4 / TCP Packets
            if (packetType is IPv4Packet IPv4Packet)
            {
                string IPv4protocol = IPv4Packet.Protocol.ToString();
                string IPv4source = IPv4Packet.SourceAddress.ToString();
                string IPv4destination = IPv4Packet.DestinationAddress.ToString();
                //Console.WriteLine($"Protocol: {IPv4protocol} \n IPv4 Source Address: {IPv4source} \n IPv4 Destination Address: {IPv4destination}");
            }
             
            // IPv6 / TCP Packets
            else if (packetType is IPv6Packet IPv6Packet)
            {
                string IPv6protocol = IPv6Packet.Protocol.ToString();
                string IPv6source = IPv6Packet.SourceAddress.ToString();
                string IPv6destination = IPv6Packet.DestinationAddress.ToString();
                // Console.WriteLine($"Protocol: {IPv6protocol} \n IPv6 Source Address: {IPv6source} \n IPv6 Destination Address: {IPv6destination}");
            }

            // ARP packets
            else if (packetType is ArpPacket ArpPacket)
            {
                string ARPsource = ArpPacket.SenderProtocolAddress.ToString();
                string ARPhardsource = ArpPacket.SenderHardwareAddress.ToString();
                string ARPtarget = ArpPacket.TargetProtocolAddress.ToString();
                string ARPhardtarget = ArpPacket.TargetHardwareAddress.ToString();
                // Console.WriteLine($"Protocol: {protocol} \n ARP Source Address: {ARPsource} \n ARP Hardware Source Address: {ARPhardsource} \n ARP Target Address {ARPtarget} \n ARP Hardware Target Address: {ARPhardtarget}");
            }

            // DHCP packets
            else if (packetType is DhcpV4Packet DhcpV4Packet)
            {
                string DhcpV4hardsource = DhcpV4Packet.HardwareType.ToString();
                string DhcpV4hardclient = DhcpV4Packet.ClientHardwareAddress.ToString();
                string DhcpV4source = DhcpV4Packet.ServerAddress.ToString();
                string DhcpV4client = DhcpV4Packet.ClientAddress.ToString();
                // Console.WriteLine($"Protocol: {protocol}");
            }


        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing packet: {ex.Message}");
        }
    }
}
