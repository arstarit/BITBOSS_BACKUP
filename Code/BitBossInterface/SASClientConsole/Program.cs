using BitbossInterface;
using System;
using System.Linq;

namespace SASClientConsole
{
    class Program
    {
        // Static VirtualEGM instance for the program.
        static VirtualEGM virtualEGM;

        // Entry point of the program.
        static void Main(string[] args)
        {
            // Checking if port is specified through command line arguments.
            if (args.Length == 0)
                Console.WriteLine("ERROR: Port not specified!!");
            else
            {
                // Initializing the VirtualEGM.
                virtualEGM = new VirtualEGM(true);
                Console.WriteLine("Press 1 to start reading, 2 to stop");
                ConsoleKeyInfo key = Console.ReadKey(true);
                bool exit = false;
                // Main loop for user input handling.
                do
                {
                    switch (key.KeyChar)
                    {
                        // Starting the virtual EGM reading.
                        case '1':
                            virtualEGM.StartVirtualEGM(args[0]);
                            Console.WriteLine("Started");
                            break;
                        // Stopping the virtual EGM.
                        case '2':
                            virtualEGM.StopVirtualEGM();
                            Console.WriteLine("Stopped");
                            break;
                        // Handling handpay initiation and reset.
                        case 'h':
                            if (virtualEGM.HandpayPending())
                            {
                                Console.WriteLine("HANDPAY INITIATED");
                            }
                            else
                            {
                                Console.WriteLine("HANDPAY IN PROCESS. PLEASE RESET");
                            }
                            break;
                        // Resetting the handpay.
                        case 'r':
                            if (virtualEGM.HandpayReset())
                            {
                                Console.WriteLine("HANDPAY RESET");
                            }
                            break;
                        // Simulating ticket insertion with specified parameters.
                        case 'i':
                            virtualEGM.TicketHasBeenInserted(3000, 0x00, 0x00, new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 });
                            break;
                        // Triggering a chirp.
                        case 'c':
                            virtualEGM.Chirp();
                            break;
                        // Exiting the program.
                        case 'q':
                            exit = true;
                            break;
                        // Handling the default case for unknown inputs.
                        default:
                            Console.WriteLine("Default case");
                            break;
                    }
                    // Receiving the next key if the exit condition is not met.
                    if (!exit) key = Console.ReadKey(true);
                } while (key.KeyChar != 'q'); // Exiting the loop when 'q' is pressed.

            }

        }
    }
}

