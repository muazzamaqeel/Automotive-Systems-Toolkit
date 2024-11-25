using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ConsoleIPapp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                // Retrieve all network interfaces
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                // Filter interfaces that are operational
                var operationalInterfaces = interfaces
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
                    .ToArray();

                Console.Clear();
                Console.WriteLine("Active Network Interfaces:");
                for (int i = 0; i < operationalInterfaces.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. {operationalInterfaces[i].Name}");
                }
                Console.WriteLine($"{operationalInterfaces.Length + 1}. Exit");

                int selection = -1;
                while (true)
                {
                    Console.Write("\nEnter the number of the network interface to view/manage its IP addresses: ");
                    string input = Console.ReadLine();

                    if (int.TryParse(input, out selection) &&
                        selection >= 1 &&
                        selection <= operationalInterfaces.Length + 1)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid selection. Please enter a valid number.");
                    }
                }

                if (selection == operationalInterfaces.Length + 1)
                {
                    // Exit option selected
                    break;
                }

                // Get the selected network interface
                NetworkInterface selectedInterface = operationalInterfaces[selection - 1];
                IPInterfaceProperties ipProps = selectedInterface.GetIPProperties();

                var unicastAddresses = ipProps.UnicastAddresses;

                Console.Clear();
                Console.WriteLine($"IP Addresses for {selectedInterface.Name}:");

                if (unicastAddresses.Count == 0)
                {
                    Console.WriteLine("No IP addresses assigned to this interface.");
                }
                else
                {
                    foreach (var addr in unicastAddresses)
                    {
                        string ipVersion = addr.Address.AddressFamily == AddressFamily.InterNetwork ? "IPv4" :
                                           addr.Address.AddressFamily == AddressFamily.InterNetworkV6 ? "IPv6" :
                                           "Unknown";

                        Console.WriteLine($"{ipVersion}: {addr.Address}");
                    }
                }

                Console.WriteLine("\nDo you want to manage IP protocols for this interface? (y/n): ");
                string manageChoice = Console.ReadLine().Trim().ToLower();
                if (manageChoice != "y")
                {
                    continue; // Return to main menu
                }

                // Sub-menu for managing IP protocols
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine($"Managing IP Protocols for {selectedInterface.Name}:");
                    Console.WriteLine("1. IPv4");
                    Console.WriteLine("2. IPv6");
                    Console.WriteLine("3. Return to Main Menu");

                    Console.Write("\nEnter your choice: ");
                    string protocolChoice = Console.ReadLine();

                    if (protocolChoice == "3")
                    {
                        break; // Return to main menu
                    }

                    string protocol = "";
                    string componentId = "";

                    if (protocolChoice == "1")
                    {
                        protocol = "IPv4";
                        componentId = "ms_tcpip";
                    }
                    else if (protocolChoice == "2")
                    {
                        protocol = "IPv6";
                        componentId = "ms_tcpip6";
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice. Please select 1, 2, or 3.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        continue;
                    }

                    Console.WriteLine($"\nSelected Protocol: {protocol}");
                    Console.WriteLine("Choose an action:");
                    Console.WriteLine("e - Enable");
                    Console.WriteLine("d - Disable");
                    Console.Write("Enter your choice (e/d): ");
                    string action = Console.ReadLine().Trim().ToLower();

                    bool enable = false;
                    if (action == "e")
                    {
                        enable = true;
                    }
                    else if (action == "d")
                    {
                        enable = false;
                    }
                    else
                    {
                        Console.WriteLine("Invalid action. Please enter 'e' or 'd'.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        continue;
                    }

                    // Confirm the action
                    Console.WriteLine($"\nAre you sure you want to {(enable ? "enable" : "disable")} {protocol} on {selectedInterface.Name}? (y/n): ");
                    string confirm = Console.ReadLine().Trim().ToLower();
                    if (confirm != "y")
                    {
                        Console.WriteLine("Action canceled.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        continue;
                    }

                    // Execute the PowerShell command
                    string command = enable ? "Enable-NetAdapterBinding" : "Disable-NetAdapterBinding";
                    string argsCommand = $"-Name \"{selectedInterface.Name}\" -ComponentID {componentId}";

                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = $"{command} {argsCommand} -Confirm:$false",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using (Process process = Process.Start(psi))
                        {
                            string output = process.StandardOutput.ReadToEnd();
                            string error = process.StandardError.ReadToEnd();
                            process.WaitForExit();

                            if (process.ExitCode == 0)
                            {
                                Console.WriteLine($"\n{protocol} has been {(enable ? "enabled" : "disabled")} successfully.");
                            }
                            else
                            {
                                Console.WriteLine($"\nFailed to {(enable ? "enable" : "disable")} {protocol}.");
                                if (!string.IsNullOrEmpty(error))
                                {
                                    Console.WriteLine($"Error: {error}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\nAn error occurred: {ex.Message}");
                    }

                    Console.WriteLine("\nPress any key to return to the protocol menu...");
                    Console.ReadKey();
                }
            }

            Console.WriteLine("Exiting the application. Goodbye!");
        }
    }
}
