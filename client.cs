using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

class UDPClient
{

    static void Main()
    {
        string serverName = "10.11.66.151";
        int serverPort = 2222;

        IPEndPoint serverAddress = new IPEndPoint(IPAddress.Parse(serverName), serverPort);
        byte[] receivedData;

        Console.WriteLine("Choose the type of connection:");
        Console.WriteLine("1. Connect as CLIENT");
        Console.WriteLine("2. Connect as ADMIN");

        Console.Write("Enter your choice (1 or 2): ");
        string connectionChoice = Console.ReadLine();

        string connectionMessage = "";
        if (connectionChoice == "1")
        {
            connectionMessage = "CONNECT:CLIENT";
        }
        else if (connectionChoice == "2")
        {
            Console.Write("Enter the ADMIN username: ");
            string username = Console.ReadLine();

            Console.Write("Enter the ADMIN password: ");
            string password = Console.ReadLine();

            connectionMessage = $"CONNECT:ADMIN{username}:{password}";
        }
        else
        {
            Console.WriteLine("Invalid choice. Please enter 1 or 2.");
            return;
        }

        for (int i = 0; i < 5; i++)
        {
            using (UdpClient clientS = new UdpClient())
            {
                try
                {
                    byte[] connectionData = Encoding.UTF8.GetBytes(connectionMessage);
                    clientS.Send(connectionData, connectionData.Length, serverName, serverPort);

                    while (true)
                    {
                        receivedData = clientS.Receive(ref serverAddress);
                        string connectionResponse = Encoding.UTF8.GetString(receivedData);

                       
                        bool hasFullAccess = connectionResponse.Contains("FULL_ACCESS");


                        while (true)
                        {
                            if (hasFullAccess)
                            {

                                Console.WriteLine("Choose an option:");
                                Console.WriteLine("1. Enter a message");
                                Console.WriteLine("2. Read a file");
                                Console.WriteLine("3. Write to a file");
                                Console.WriteLine("4. Execute a command");
                                Console.WriteLine("5. Exit");
                            }

                            else if (!hasFullAccess)
                            {
                                Console.WriteLine("Choose an option:");
                                Console.WriteLine("1. Enter a message");
                                Console.WriteLine("2. Read a file");
                                Console.WriteLine("3. Exit");
                            }

                            Console.Write("Enter your choice: ");
                            string choice = Console.ReadLine();

                            // Handle client options based on the choice
                            // (Note: You may need to adjust the logic based on your specific requirements.)

                            if (hasFullAccess)
                            {

                                // Handle options for clients with full access
                                if (choice == "1")
                                {
                                    Console.Write("Enter a message to send to the server: ");
                                    string message = Console.ReadLine();

                                    byte[] data = Encoding.UTF8.GetBytes(message);
                                    clientS.Send(data, data.Length, serverName, serverPort);
                                }
                                else if (choice == "2")
                                {
                                    Console.Write("Enter the file name (e.g., hello.txt): ");
                                    string fileName = Console.ReadLine();
                                    string filePath = Path.Combine(@"C:\Users\Admin\Desktop", "hello.txt");

                                    if (File.Exists(filePath))
                                    {
                                        string fileContent = File.ReadAllText(filePath);
                                        byte[] data = Encoding.UTF8.GetBytes(fileContent);
                                        clientS.Send(data, data.Length, serverName, serverPort);
                                    }
                                    else
                                    {
                                        string errorFile = $"File '{fileName}' not found";
                                        Console.WriteLine(errorFile);
                                        byte[] data = Encoding.UTF8.GetBytes(errorFile);
                                        clientS.Send(data, data.Length, serverName, serverPort);
                                    }
                                }
