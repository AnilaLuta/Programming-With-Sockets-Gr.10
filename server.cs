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
            }

            else if (message.StartsWith("WRITE:"))
            {
                try
                {
                    string[] parts = message.Substring(6).Split(new[] { ':' }, 2); // Ndarje në dy pjesë: fileName dhe fileContent
                    if (parts.Length == 2)
                    {
                        string fileName = parts[0];
                        string fileContent = parts[1];

                        string filePath = Path.Combine(baseDirectory, fileName);
                        File.WriteAllText(filePath, fileContent);
                        Console.WriteLine($"File '{fileName}' successfully written to server.");

                        byte[] response = Encoding.UTF8.GetBytes($"File '{fileName}' written successfully.");
                        await serverS.SendAsync(response, response.Length, clientAddress);
                    }
                    else
                    {
                        string errorMessage = "Invalid WRITE format. Use WRITE:<filename>:<content>";
                        Console.WriteLine(errorMessage);

                        byte[] errorResponse = Encoding.UTF8.GetBytes(errorMessage);
                        await serverS.SendAsync(errorResponse, errorResponse.Length, clientAddress);
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = $"Error writing file: {ex.Message}";
                    Console.WriteLine(errorMessage);

                    byte[] errorResponse = Encoding.UTF8.GetBytes(errorMessage);
                    await serverS.SendAsync(errorResponse, errorResponse.Length, clientAddress);
                }
            }
            else if (message.StartsWith("READ:"))
            {
                string fileName = message.Substring(5);
                Console.WriteLine($"Received request to read file: {fileName}");

                try
                {
                    string filePath = Path.Combine(baseDirectory, fileName);
                    if (File.Exists(filePath))
                    {
                        string fileContent = File.ReadAllText(filePath);
                        byte[] fileData = Encoding.UTF8.GetBytes(fileContent);
                        await serverS.SendAsync(fileData, fileData.Length, clientAddress);
                    }
                    else
                    {
                        string errorMessage = $"File '{fileName}' not found";
                        byte[] errorData = Encoding.UTF8.GetBytes(errorMessage);
                        await serverS.SendAsync(errorData, errorData.Length, clientAddress);
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = $"Error reading file: {ex.Message}";
                    byte[] errorData = Encoding.UTF8.GetBytes(errorMessage);
                    await serverS.SendAsync(errorData, errorData.Length, clientAddress);
                }
            } else if (message.StartsWith("EXECUTE:")) {
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
                        string[] files = Directory.GetFiles(@"");
                        output = string.Join(Environment.NewLine, files);
                    }
                    else
                    {
                        ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", $"/c {command}")
                        {
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            WorkingDirectory = baseDirectory 
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
                    string errorMessage = $"Error executing command: {ex.Message}";
                    byte[] errorData = Encoding.UTF8.GetBytes(errorMessage);
                    await serverS.SendAsync(errorData, errorData.Length, clientAddress);
                }
            }
            else
            {
                Console.WriteLine($"Request from client {clients.Count}: {message}");
                byte[] responseData = Encoding.UTF8.GetBytes(message); // Echo the message back
                await serverS.SendAsync(responseData, responseData.Length, clientAddress);
            }
        }

        // Notify all clients that the server is full
        foreach (var client in clients)
        {
            string fullMessage = "Server: The client list is full!";
            byte[] fullMessageData = Encoding.UTF8.GetBytes(fullMessage);
            await serverS.SendAsync(fullMessageData, fullMessageData.Length, client);
        }
    }
}
