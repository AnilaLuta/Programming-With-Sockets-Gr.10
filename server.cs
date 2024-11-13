using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class UDPServer
{
    private static IPEndPoint clientWithFullAccess = null;

    static void Main()
    {
        Task.Run(async () => await StartServerAsync());

        Console.ReadLine();
    }

    private static bool VerifyClientAccess(IPEndPoint clientAddress, string authMessage)
    {
        if (authMessage.StartsWith("CONNECT:ADMIN"))
        {
            string[] adminCredentials = authMessage.Substring(13).Split(':');

            if (adminCredentials.Length == 2)
            {
                string adminUsername = adminCredentials[0];
                string adminPassword = adminCredentials[1];

                if (adminUsername == "admin" && adminPassword == "admin123")
                {
                    return true; 
                }
            }

            
        }

        return false;
    }

     static async Task StartServerAsync()
    {
        string serverName = "";
        int serverPort = 1200;

        IPAddress ipv4Address = IPAddress.Parse("192.168.0.23");
        UdpClient serverS = new UdpClient(new IPEndPoint(ipv4Address, serverPort));
        Console.WriteLine($"Serveri eshte startuar ne IP adresen: {ipv4Address}, portin: {serverPort}");


        List<IPEndPoint> clients = new List<IPEndPoint>();
        int maxClients = 5;

        while (clients.Count < maxClients)
        {
            UdpReceiveResult receiveResult = await serverS.ReceiveAsync();
            IPEndPoint clientAddress = receiveResult.RemoteEndPoint;
            byte[] data = receiveResult.Buffer;

