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
    // Ruhet klienti me qasje të plotë në server 
    private static IPEndPoint clientWithFullAccess = null;
    // Drejtoria bazë për ruajtjen e skedarëve që krijohen ose lexohen
    private static string baseDirectory = (@"C:\Users\Dell\Desktop\Server_Final\Server\Server\");

    static void Main()
    {
        // Fillon serveri në një detyrë asinkrone dhe pret për hyrje nga përdoruesi
        Task.Run(async () => await StartServerAsync());
        Console.ReadLine();
    }

    // Kontrollon nëse klienti ka qasje të plotë bazuar në kredencialet e dhëna
    private static bool VerifyClientAccess(IPEndPoint clientAddress, string authMessage)
    {
        // Kontrollo nëse mesazhi përmban formatin e duhur për lidhjen me qasje të plotë
        if (authMessage.StartsWith("CONNECT:ADMIN"))
        {
            // Merr emrin dhe fjalëkalimin nga mesazhi
            string[] adminCredentials = authMessage.Substring(13).Split(':');
            if (adminCredentials.Length == 2)
            {
                string adminUsername = adminCredentials[0];
                string adminPassword = adminCredentials[1];

                // Verifikimi i kredencialeve
                if (adminUsername == "admin" && adminPassword == "admin123")
                {
                    return true; 
                }
            }
        }
        return false;
    }

    // Funksioni kryesor që ekzekuton serverin UDP
     static async Task StartServerAsync()
    {
         // IP dhe porta që serveri do të përdorë për të pritur klientët
        string serverName = "192.168.0.27";//IP Addressa
        int serverPort = 2222;//Porti

        IPAddress ipv4Address = IPAddress.Parse(serverName);
        UdpClient serverS = new UdpClient(new IPEndPoint(ipv4Address, serverPort));
        Console.WriteLine($"Server started at IP address: {ipv4Address}, port: {serverPort}");

         // Lista e klientëve të lidhur me serverin
        List<IPEndPoint> clients = new List<IPEndPoint>();
        int maxClients = 5;

        while (clients.Count < maxClients)
        {
            // Pranon të dhënat nga një klient
            UdpReceiveResult receiveResult = await serverS.ReceiveAsync();
            IPEndPoint clientAddress = receiveResult.RemoteEndPoint;
            byte[] data = receiveResult.Buffer;

            // Shto klientin nëse nuk është në listë
            if (!clients.Contains(clientAddress))
            {
                clients.Add(clientAddress);
                Console.WriteLine($"Client {clients.Count} connected from {clientAddress.Address}:{clientAddress.Port}");
            }

            // Përkthen mesazhin në tekst
            string message = Encoding.UTF8.GetString(data);

            // Kontrollo mesazhet që fillojnë me "CONNECT:"
            if (message.StartsWith("CONNECT:"))
            {
                // Verifikimi i qasjes së plotë
                bool hasFullAccess = VerifyClientAccess(clientAddress, message);

                if (hasFullAccess)
                {
                    Console.WriteLine($"Klienti {clientAddress.Address} has full access.");
                    clientWithFullAccess = clientAddress;

                    // Dërgon mesazh për qasje të plotë te klienti
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

            // Procesimi i komandës "WRITE:"
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

            // Procesimi i komandës "READ:"
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
            } 

            // Procesimi i komandës "EXECUTE:"
            else if (message.StartsWith("EXECUTE:")) {
                // Komanda ekzekutohet dhe dërgohet rezultati tek klienti
                string command = message.Substring(8);
                Console.WriteLine($"Received a request to execute command: {command}");

                try
                {
                    string output = "";

                    // Komanda për të krijuar një direktor
                    if (command.StartsWith("mkdr"))
                    {
                        string dirName = command.Substring(5).Trim();

                        Directory.CreateDirectory(dirName);

                        output = $"Directory '{dirName}' created successfully.";
                    }
                    else if (command.StartsWith("ls"))
                    {
                        // Liston të gjitha skedarët në direktor
                        string[] files = Directory.GetFiles(@"C:\Users\Dell\Desktop\Server_Final\Server\Server\");
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
            // Përpunon çdo mesazh tjetër
            else
            {
                Console.WriteLine($"Request from client {clients.Count}: {message}");
                byte[] responseData = Encoding.UTF8.GetBytes(message);
                await serverS.SendAsync(responseData, responseData.Length, clientAddress);
            }
        }

         // Njofton të gjithë klientët që lista është mbushur
        foreach (var client in clients)
        {
            string fullMessage = "Server: The client list is full!";
            byte[] fullMessageData = Encoding.UTF8.GetBytes(fullMessage);
            await serverS.SendAsync(fullMessageData, fullMessageData.Length, client);
        }
    }
}
