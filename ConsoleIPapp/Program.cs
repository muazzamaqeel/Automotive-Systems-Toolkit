using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Principal;

namespace ConsoleIPapp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Check if the application is running with administrative privileges
            if (!IsAdministrator())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("This application requires administrative privileges to modify network settings.");
                Console.ResetColor();
                Console.Write("Do you want to restart the application with administrative privileges? (y/n): ");
                string restartChoice = Console.ReadLine().Trim().ToLower();

                if (restartChoice == "y" || restartChoice == "yes")
                {
                    try
                    {
                        // Restart the application with admin privileges
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = Process.GetCurrentProcess().MainModule.FileName,
                            UseShellExecute = true,
                            Verb = "runas" // Triggers the UAC prompt
                        };
                        Process.Start(startInfo);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Failed to restart as administrator. Error: {ex.Message}");
                        Console.ResetColor();
                    }

                    return; // Exit the current instance
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Cannot continue without administrative privileges. Exiting...");
                    Console.ResetColor();
                    return;
                }
            }

            // Main application loop
            while (true)
            {
                Console.Clear();
                Console.WriteLine("All Network Interfaces:");

                // Retrieve all network interfaces using both .NET and PowerShell
                Console.WriteLine("Fetching network interfaces using .NET:");
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                for (int i = 0; i < interfaces.Length; i++)
                {
                    var networkInterface = interfaces[i];
                    Console.WriteLine($"{i + 1}. {networkInterface.Name} | Description: {networkInterface.Description} | Status: {networkInterface.OperationalStatus} | Type: {networkInterface.NetworkInterfaceType}");
                }

                Console.WriteLine("\nFetching network interfaces using PowerShell:");
                FetchNetworkInterfacesWithPowerShell();

                Console.WriteLine($"{interfaces.Length + 1}. Exit");

                // Prompt user to select an interface
                int selection = -1;
                while (true)
                {
                    Console.Write("\nEnter the number of the network interface to view/manage its IP addresses: ");
                    string input = Console.ReadLine();

                    if (int.TryParse(input, out selection) &&
                        selection >= 1 &&
                        selection <= interfaces.Length + 1)
                    {
                        break;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid selection. Please enter a valid number.");
                        Console.ResetColor();
                    }
                }

                if (selection == interfaces.Length + 1)
                {
                    // Exit option selected
                    break;
                }

                // Get the selected network interface
                NetworkInterface selectedInterface = interfaces[selection - 1];
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
                        string ipVersion = addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? "IPv4" :
                                           addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 ? "IPv6" :
                                           "Unknown";

                        Console.WriteLine($"{ipVersion}: {addr.Address}");
                    }
                }

                Console.Write("\nDo you want to manage IP protocols for this interface? (y/n): ");
                string manageChoice = Console.ReadLine().Trim().ToLower();
                if (manageChoice != "y" && manageChoice != "yes")
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
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid choice. Please select 1, 2, or 3.");
                        Console.ResetColor();
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
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid action. Please enter 'e' or 'd'.");
                        Console.ResetColor();
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        continue;
                    }

                    // Confirm the action
                    Console.Write($"\nAre you sure you want to {(enable ? "enable" : "disable")} {protocol} on {selectedInterface.Name}? (y/n): ");
                    string confirm = Console.ReadLine().Trim().ToLower();
                    if (confirm != "y" && confirm != "yes")
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Action canceled.");
                        Console.ResetColor();
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        continue;
                    }

                    // Execute the PowerShell command
                    string cmd = enable ? "Enable-NetAdapterBinding" : "Disable-NetAdapterBinding";
                    string argsCommand = $"-Command \"{cmd} -Name '{selectedInterface.Name}' -ComponentID {componentId} -Confirm:$false\"";

                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = argsCommand,
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
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"\n{protocol} has been {(enable ? "enabled" : "disabled")} successfully.");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"\nFailed to {(enable ? "enable" : "disable")} {protocol}.");
                                if (!string.IsNullOrEmpty(error))
                                {
                                    Console.WriteLine($"Error: {error}");
                                }
                                Console.ResetColor();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\nAn error occurred: {ex.Message}");
                        Console.ResetColor();
                    }

                    Console.WriteLine("\nPress any key to return to the protocol menu...");
                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// Fetches network interfaces using PowerShell commands.
        /// </summary>
        static void FetchNetworkInterfacesWithPowerShell()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "Get-NetAdapter | Format-Table -AutoSize",
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

                    if (!string.IsNullOrEmpty(output))
                    {
                        Console.WriteLine(output);
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error: {error}");
                        Console.ResetColor();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An error occurred while fetching interfaces via PowerShell: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Checks if the current process is running with administrative privileges.
        /// </summary>
        /// <returns>True if running as administrator; otherwise, false.</returns>
        static bool IsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
