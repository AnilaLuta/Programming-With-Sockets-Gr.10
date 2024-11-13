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

                   if (!clients.Contains(clientAddress))
            {
                clients.Add(clientAddress);
                Console.WriteLine($"Klienti {clients.Count} u lidh me {clientAddress.Address} ne portin {clientAddress.Port}");
            }

            string message = Encoding.UTF8.GetString(data);
            if (message.StartsWith("CONNECT:"))
            {
                bool hasFullAccess = VerifyClientAccess(clientAddress, message);

                if (hasFullAccess)
                {
                    Console.WriteLine($"Client {clientAddress.Address} has full access.");
                    clientWithFullAccess = clientAddress;

                    byte[] fullAccessMessage = Encoding.UTF8.GetBytes("FULL_ACCESS");
                    await serverS.SendAsync(fullAccessMessage, fullAccessMessage.Length, clientAddress);
                }
                else
                {
                    Console.WriteLine($"Client {clientAddress.Address} does not have full access.");

                    byte[] restrictedAccessMessage = Encoding.UTF8.GetBytes("RESTRICTED_ACCESS");
                    await serverS.SendAsync(restrictedAccessMessage, restrictedAccessMessage.Length, clientAddress);
             
                    continue;
                }
