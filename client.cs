using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class UDPClient
{
    static void Main()
    {
        string serverName = "172.20.10.2";
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

        using (UdpClient clientS = new UdpClient())
        {
            try
            {
                byte[] connectionData = Encoding.UTF8.GetBytes(connectionMessage);
                clientS.Send(connectionData, connectionData.Length, serverName, serverPort);

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
                        Console.WriteLine("4.Execute commands");
                        Console.WriteLine("5. Exit");
                    }
                    else
                    {
                        Console.WriteLine("Choose an option:");
                        Console.WriteLine("1. Enter a message");
                        Console.WriteLine("2. Read a file");
                        Console.WriteLine("3. Exit");
                    }

                    Console.Write("Enter your choice: ");
                    string choice = Console.ReadLine();
                                
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
                                    string filePath = Path.Combine(@"C:\Users\Admin\Desktop\Hello.txt", "hello.txt");

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


                                else if (choice == "3")
                                {
                                    Console.Write("Enter the file name to write (e.g., newfile.txt): ");
                                    string fileName = Console.ReadLine();
                                    string filePath = Path.Combine(@"C:\Users\Admin\Desktop\Hello.txt\", fileName);

                                    Console.Write("Enter the content for the file: ");
                                    string fileContent = Console.ReadLine();

                                    File.WriteAllText(filePath, fileContent);
                                    byte[] data = Encoding.UTF8.GetBytes($"WRITE:{fileName}");
                                    clientS.Send(data, data.Length, serverAddress);

                                    receivedData = clientS.Receive(ref serverAddress);
                                    string confirmationMessage = Encoding.UTF8.GetString(receivedData);

                                    Console.WriteLine("Server response: " + confirmationMessage);
                                }
                                else if (choice == "4")
                                {
                                    Console.WriteLine("Choose a command to execute:");
                                    Console.WriteLine("1. mkdr [directory_name]");
                                    Console.WriteLine("2. ls");

                                    Console.Write("Enter your choice (1 or 2): ");
                                    string subChoice = Console.ReadLine();

                                    string command = "";
                                    if (subChoice == "1")
                                    {
                                        Console.Write("Enter the directory name to create: ");
                                        string dirName = Console.ReadLine();
                                        command = $"EXECUTE:mkdr {dirName}";
                                    }
                                    else if (subChoice == "2")
                                    {
                                        command = "EXECUTE:ls";
                                    }
                                    else
                                    {
                                        Console.WriteLine("Invalid sub-choice. Please enter 1 or 2.");
                                        continue; 
                                    }

                                    byte[] data = Encoding.UTF8.GetBytes(command);
                                    clientS.Send(data, data.Length, serverAddress);

                                    receivedData = clientS.Receive(ref serverAddress);
                                    string response = Encoding.UTF8.GetString(receivedData);

                                    Console.WriteLine("Server response: ");
                                    Console.WriteLine(response);
                                }
                                else if (choice == "5")
                                {
                                    Console.WriteLine("Exiting the client.");
                                    return;
                                }
                                else
                                {
                                    Console.WriteLine("Invalid choice. Please enter a number between 1 and 5.");
                                }
                            }
                            else if (!hasFullAccess)
                            {
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
                                    string filePath = Path.Combine(@"C:\Users\Admin\Desktop\Hello.txt\", fileName);

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
                                else if (choice == "3")
                                {
                                    Console.WriteLine("Exiting the client.");
                                    return;
                                }
                                else
                                {
                                    Console.WriteLine("Invalid choice. Please enter a number between 1 and 3.");
                                }
                            }
                            
                            byte[] choiceData = Encoding.UTF8.GetBytes(choice);
                            clientS.Send(choiceData, choiceData.Length, serverAddress);

                            receivedData = clientS.Receive(ref serverAddress);
                            string modifiedMessage = Encoding.UTF8.GetString(receivedData);

                            Console.WriteLine("Response from the server: " + modifiedMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error communicating with the server: {ex.Message}");
                }
            }
        }
    }
}
