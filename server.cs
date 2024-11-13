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
