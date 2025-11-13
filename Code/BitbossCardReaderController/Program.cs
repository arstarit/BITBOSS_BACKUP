using System;
using System.IO.Ports;
using System.Threading;
using System.Timers;
using BitbossCardReaderController;
using BitbossCardReaderController.Responses;
namespace MainProgram
{
    public static class StringExt
    {
        public static string Left(this string @this, int count)
        {
            if (@this.Length <= count)
            {
                return @this;
            }
            else
            {
                return @this.Substring(0, count);
            }
        }
    }

    class Program
    {

            static string input = "";

            public static void ReadInput()
            {
                    input = "";

                    var ch = Console.ReadKey();
                    do
                    {
                        if (ch.Key == ConsoleKey.Backspace)
                        {
                            if (input.Length > 0)
                            {
                                input = input.Left(input.Length-1);
                            }
                        }
                        else
                        {
                            try
                            {
                                input = $"{input}{ch.Key.ToString().ToLower()}";
                            }
                            catch {

                            }
                        }
                        ch = Console.ReadKey();
                   } while (ch.Key != ConsoleKey.Enter);
                   Console.WriteLine("");
            }


            public static void StartConsole(CardReaderController cardReaderController)
            {
                 Console.Write("Enter Command to Test: ");
                 ReadInput();
                 do
                 {
                     switch (input)
                     {
                         case "start":
                             cardReaderController.Start();
                             Console.WriteLine("Started");
                             break;
                         case "stop":
                             cardReaderController.Stop();
                             Console.WriteLine("Stopped");
                             break;
                         case "setegmid":
                             cardReaderController.SetEGMId(new byte[] {0x01, 0x02, 0x03, 0x04},
                                                          new byte[] {0x10, 0x20, 0x30, 0x40},
                                                          new byte[] {0x12, 0x34, 0x56},
                                                          new byte[] {0x78, 0x90, 0x01});
                             break;
                         case "getcardreaderstatus":
                             cardReaderController.GetCardReaderStatus();
                             break;
                         default:
                             Console.WriteLine("Default case\n");
                             break;
                     }

                     Console.Write("Enter Command: ");
                     ReadInput();
                 } while (input != "exit");
            }


            public static void Main(string[] args)
            {
                string port = "/dev/ttyS0";
                 // Opción de EnableSASTrace
                if (args.Length >= 1)
                    port = args[0];
                else
                {
                    Console.WriteLine("You must set a port, usage: ./CardReaderController [Port]");
                    return;
                }

                CardReaderController cardReaderController = new CardReaderController(port);
                cardReaderController.CommandSent += new CardReaderController.CommandSentHandler(cmdSent);
                cardReaderController.CommandReceived += new CardReaderController.CommandReceivedHandler(cmdReceived);
                cardReaderController.responseHandler.GetReaderStatus_Received += new ResponseHandler.ResponseReceivedHandler<GetReaderStatus_Response>(getReaderStatusReceived);
                StartConsole(cardReaderController);
                //cardReaderController.Stop();
            }

            static void getReaderStatusReceived(GetReaderStatus_Response resp, EventArgs e)
            {
              Console.WriteLine("");
              Console.WriteLine($"CardReaderStatus: {BitConverter.ToString(new byte[] {resp.CardReaderStatus})}");
              if (resp.CardType != null)  Console.WriteLine($"CardType: {BitConverter.ToString(new byte[] {resp.CardType.Value})}"); 
              if (resp.Track1Status != null)  Console.WriteLine($"Track1Status: {BitConverter.ToString(new byte[] {resp.Track1Status.Value})}"); 
              if (resp.Track1Len != null)  Console.WriteLine($"Track1Len: {BitConverter.ToString(new byte[] {resp.Track1Len.Value})}");
              if (resp.Track1Data != null)  if (resp.Track1Data.Length > 0) Console.WriteLine($"Track1Data: {resp.Track1Data}"); 
              if (resp.Track2Status != null)  Console.WriteLine($"Track2Status: {BitConverter.ToString(new byte[] {resp.Track2Status.Value})}"); 
              if (resp.Track2Len != null)  Console.WriteLine($"Track2Len: {BitConverter.ToString(new byte[] {resp.Track2Len.Value})}");
              if (resp.Track2Data != null)  if (resp.Track2Data.Length > 0) Console.WriteLine($"Track2Data: {resp.Track2Data}"); 
              Console.Write($"Enter Command: {input}");

            }

            static void cmdSent(string cmd, EventArgs e)
            {
              Console.WriteLine("");
              Console.Write("Sent: ");
              Console.WriteLine(cmd);
              Console.Write($"Enter Command: {input}");

            }

            static void cmdReceived(string cmd, EventArgs e)
            {
              Console.WriteLine("");
              Console.Write("Received: ");
              Console.WriteLine(cmd);
              Console.Write($"Enter Command: {input}");
                
            }
    }
}
