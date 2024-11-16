using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

class UDPServer
{
    private static IPEndPoint clientWithFullAccess = null;
     private static string baseDirectory = (@"");//Pathi se ku ruhet sedari

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
        string serverName = "";//IpAddress me te cilen jemi lidh ne rrjete 
        int serverPort = 2222;//Porti

        IPAddress ipv4Address = IPAddress.Parse(serverName);
        UdpClient serverS = new UdpClient(new IPEndPoint(ipv4Address, serverPort));
        Console.WriteLine($"Server started at IP address: {ipv4Address}, port: {serverPort}");


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
                Console.WriteLine($"Client {clients.Count} connected from {clientAddress.Address}:{clientAddress.Port}");
            }

            string message = Encoding.UTF8.GetString(data);
            if (message.StartsWith("CONNECT:"))
            {
                bool hasFullAccess = VerifyClientAccess(clientAddress, message);

                if (hasFullAccess)
                {
                    Console.WriteLine($"Klienti {clientAddress.Address} has full access.");
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

                if (message.Equals("CONNECT:CLIENT"))
                {
                    Console.WriteLine($"Client connected from {clientAddress.Address} on port {clientAddress.Port}");
                }
                else if (message.StartsWith("CONNECT:ADMIN"))
                {
                    string[] adminCredentials = message.Substring(13).Split(':');
                    string adminUsername = adminCredentials[0];
                    string adminPassword = adminCredentials[1];

                    if (adminUsername == "admin" && adminPassword == "admin123")
                    {
                        Console.WriteLine($"Admin connected from {clientAddress.Address} on port {clientAddress.Port}");
                    }
                    else
                    {
                        Console.WriteLine($"Invalid ADMIN credentials from {clientAddress.Address} on port {clientAddress.Port}");

                        byte[] invalidCredentialsMsg = Encoding.UTF8.GetBytes("Invalid credentials, accessing as a regular client");
                        await serverS.SendAsync(invalidCredentialsMsg, invalidCredentialsMsg.Length, clientAddress);

                        continue;
                    }

                }
                else
                {
                    Console.WriteLine($"Invalid connection request from {clientAddress.Address} on port {clientAddress.Port}");
                    byte[] invalidConnectionMsg = Encoding.UTF8.GetBytes("Invalid connection request");
                    await serverS.SendAsync(invalidConnectionMsg, invalidConnectionMsg.Length, clientAddress);
                    continue;
                }
            }

            if (message.StartsWith("FILE:"))
            {
                string fileName = message.Substring(5); // Remove the "FILE:" prefix
                Console.WriteLine($"Received file content from client {clients.Count} for file: {fileName}");

            }
            else
            {
                Console.WriteLine($"Kerkesa nga klienti {clients.Count}: {message}");
                if (message.StartsWith("WRITE:"))
                {
                    string fileName = message.Substring(6); // Remove the "WRITE:" prefix
                    Console.WriteLine($"Received a request to write content to file: {fileName}");

                    
                }

                else if (message.StartsWith("OPEN:"))
                {
                    string fileName = message.Substring(5); 
                    Console.WriteLine($"Received a request to open file: {fileName}");

                    try
                    {
                        string filePath = Path.Combine(@"C:\Users\Dell\Desktop\Rrjeta-projekti\Anila_Rrjeta", fileName);

                        if (File.Exists(filePath))
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = filePath,
                                UseShellExecute = true
                            });
                            Console.WriteLine($"File '{fileName}' opened successfully.");
                        }
                        else
                        {
                            Console.WriteLine($"File '{fileName}' not found in the server folder.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error opening file: {ex.Message}");
                    }
                }

                else if (message.StartsWith("EXECUTE:"))
                {
                    string command = message.Substring(8);
                    Console.WriteLine($"Received a request to execute command: {command}");

                    try
                    {
                        string output = "";

                        if (command.StartsWith("mkdr"))
                        {
                            string dirName = command.Substring(5).Trim();

                            Directory.CreateDirectory(dirName);

                            output = $"Directory '{dirName}' created successfully.";
                        }
                        else if (command.StartsWith("ls"))
                        {
                            string[] files = Directory.GetFiles(@"C:\Users\Dell\Desktop\Rrjeta-projekti\Anila_Rrjeta");
                            output = string.Join(Environment.NewLine, files);
                        }
                        else
                        {
                            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", $"/c {command}")
                            {
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };

                            using (Process process = new Process() { StartInfo = psi })
                            {
                                process.Start();

                                output = process.StandardOutput.ReadToEnd();
                                string error = process.StandardError.ReadToEnd();

                                if (!string.IsNullOrEmpty(error))
                                    output += $"{Environment.NewLine}Error:{Environment.NewLine}{error}";
                            }
                        }

                        string response = $"Output:{Environment.NewLine}{output}";
                        byte[] responseData = Encoding.UTF8.GetBytes(response);
                        await serverS.SendAsync(responseData, responseData.Length, clientAddress);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error executing command: {ex.Message}");
                    }
                }

                else
                {
                    string messageK = message.ToUpper();
                    Console.WriteLine($"Pergjigja nga serveri: {messageK}");

                    byte[] responseData = Encoding.UTF8.GetBytes(messageK);
                    await serverS.SendAsync(responseData, responseData.Length, clientAddress);
                }
            }
        }


        foreach (var client in clients)
        {
            string fullMessage = "Server: Lista e klientave eshte mbushur!";
            byte[] fullMessageData = Encoding.UTF8.GetBytes(fullMessage);
            await serverS.SendAsync(fullMessageData, fullMessageData.Length, client);
        }
    }
}
