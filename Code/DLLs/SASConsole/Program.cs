using System;
using System.Linq;
using SASComms;
using System.Collections.Generic;
using System.Text;

namespace SASConsole
{
    class Program
    {
        static SASConsoleParser parser = new SASConsoleParser();
        static SASHost _sashost;

        /// <summary>
        ///  Generación de ID de transacciones. Genera aleatoriamente ids de longitud 11
        /// </summary>
        /// <returns></returns>
        private static byte[] generateTransactionID()
        {
            Encoding encoding = Encoding.Default;
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[11];
            var random = new Random();
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            var finalString = new String(stringChars);
            return encoding.GetBytes(finalString);
        }

         /*
        * InttoBCD5, si tengo 0xAB 0xCD y 0XEF. El entero resultante será ABCDEF
        */
        static private byte[] intToBCD5_v2(uint numericvalue, int bytesize = 5)
        {
            byte[] bcd = new byte[bytesize];
            for (int byteNo = 0; byteNo < bytesize; ++byteNo)
                bcd[byteNo] = 0;
            for (int digit = 0; digit < bytesize * 2; ++digit)
            {
                uint hexpart = numericvalue % 10;
                bcd[digit / 2] |= (byte)(hexpart << ((digit % 2) * 4));
                numericvalue /= 10;
            }

            bcd = bcd.Reverse().ToArray();
            return bcd;
        }

        static void Main(string[] args)
        {
            _sashost = new SASHost();

            _sashost.CommandSent += new SASHost.CommandSentHandler(cmdSent);
            _sashost.CommandReceived += new SASHost.CommandReceivedHandler(cmdReceived);
            _sashost.DataReceived += new SASHost.ShowMessageHandler(dataReceived);

            //_sashost.StartPolling();

            ////_sashost.LockEGM();
            Console.WriteLine(_sashost.GetHostInfo());

            Console.Write("Enter Command: ");
            string cmd = Console.ReadLine();

            do
            {
                switch (cmd)
                {
                    case "start":
                    {
                        _sashost.StartPolling();
                        break;
                    }
                    case "stop":
                    {
                        _sashost.StopPolling();
                        break;
                    }
                    case "gethostinfo":
                    {
                        Console.Write(_sashost.GetHostInfo());
                        break;
                    }
                    case "setserialport":
                    {
                        Console.Write("     Enter serial port name: "); _sashost.SetSerialPort(Console.ReadLine());
                        break;
                    }                   
                    case "pf":
                    {   
                        Console.Write("     Enter polling frequency: "); 
                        try
                        {
                            int mlscnds = _sashost.setPFMiliseconds(int.Parse(Console.ReadLine()));
                            if (mlscnds >= 0)
                            {
                                Console.WriteLine($"   New polling frequency set to {mlscnds} ms");
                            }
                            else
                            {
                                Console.WriteLine("   Host running, please stop it");
                            }
                        }
                        catch
                        {
                            Console.WriteLine("    Syntax error");
                        }
                        break;
                    }
                    case "setassetnumber":
                    {
                        Console.Write("     Enter Asset Number: "); _sashost.SetAssetNumber(Int32.Parse(Console.ReadLine()));
                        break;
                    }                   
                    case "lp01":
                    {
                        _sashost.LockOutPlay();
                        break;
                    }
                    case "lpbadcrc":
                    {
                        // Example of bad crc long poll
                        _sashost.SendLongPoll(new byte[] { 0x01, 0x01, 0x51, 0x09 });
                        break;
                    }
                    case "lp02":
                    {
                        _sashost.EnablePlay();
                        break;
                    }                   
                    case "lp0A":
                    {
                        _sashost.EnterMaintenanceMode(); 
                        break;
                    }
                    case "lp0B":
                    {
                        _sashost.ExitMaintenanceMode();
                        break;
                    }                                       
                    case "lp1c":
                    { 
                        Console.Write(_sashost.SendMeters());
                        break;
                    }
                    case "lp1E":
                    {
                        _sashost.SendMultipleMeterLongPollGMLP1E();
                        break;
                    }
                    case "lp1f":
                    {
                        _sashost.SendGamingMachineIDAndInformation();
                        break;
                    }                   
                    case "lp2f":
                    {
                        Console.Write("    Game Number: ");
                        uint gameNumber = Convert.ToUInt32(Console.ReadLine());
                        Console.Write("     Meters (MeterCodes separated with comma (,) ): ");
                        string text = Console.ReadLine();
                         byte[] meters = text.Split(',').Select(x => Convert.ToByte(x,16)).ToArray();
                         Console.WriteLine(BitConverter.ToString(meters));
                        _sashost.SendSelectedMeter(intToBCD5_v2(gameNumber, 2), meters);
                        break;
                    }
                    case "getcredits": // lp2f
                    {
                        _sashost.SendCredits();
                        break;
                    }             
                    case "lp4d":
                    {
                        Console.Write("     Function Code (00 to 1F, FF): ");
                        Byte function_code = Convert.ToByte(Console.ReadLine(), 16);
                        _sashost.SendEnhancedValidationInformation(function_code);
                        break;
                    }                   
                    case "lp51":
                    {
                        _sashost.SendNumberOfGamesImplemented();
                        break;
                    }
                    case "lp53":
                    {
                         Console.Write("    Game Number: ");
                        uint gameNumber53 = Convert.ToUInt32(Console.ReadLine());
                        _sashost.SendGameNConfiguration(intToBCD5_v2(gameNumber53, 2));
                        break;

                    }
                    case "lp54":
                    { 
                        _sashost.SendSASVersionAndMachineSerialNumber();
                        break;
                    }                   
                    case "lp55":
                    {
                        _sashost.SendSelectedGameNumber();
                        break;
                    }
                    case "lp56":
                    {
                        _sashost.SendEnabledGameNumbers();
                        break;
                    }               
                    case "lp57":
                    {
                        _sashost.SendPendingCashoutInformation();
                        break;
                    }
                    case "lp58":
                    {
                            try
                            {
                                Console.Write("     Validation SystemID: ");
                                Byte validationSystemID = Convert.ToByte(Console.ReadLine(), 16);
                                byte[] validationNumber = { };
                                Console.Write("     Validation Number: ");
                                string vn = Console.ReadLine();
                                validationNumber = vn.Split(new char[] { '-' }).ToList().Select(s => Convert.ToByte(s, 16)).ToArray();
                                _sashost.SendReceiveValidationNumber(validationSystemID, validationNumber);

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                            break;

                    }
                    case "aftgetextendmeters": // lp6f
                    {
                            byte[] gameNumber = { };
                            byte[] meterCodes = { };
                            Console.Write("     Enter Game Number: ");
                            uint gn = Convert.ToUInt32(Console.ReadLine());
                            Console.Write("     Meters (MeterCodes separated with comma (,) ): ");
                            string text = Console.ReadLine();
                            text = text.Replace(",", ",00,");
                            text = text+",00";
                            meterCodes = text.Split(',').Select(x => Convert.ToByte(x,16)).ToArray();
                            _sashost.SendExtendedMeters(intToBCD5_v2(gn, 2), meterCodes);
                            break;
                    }                   
                    case "lp70":
                    { 
                        _sashost.SendTicketValidationData();
                        break;
                    }
                    case "lp71":
                    {
                            Console.Write("     Transfer Code (byte): ");
                            Byte transferCode = Convert.ToByte(Console.ReadLine(), 16);
                            Console.Write("     Transfer Amount (number): ");
                            uint transferAmount = Convert.ToUInt32(Console.ReadLine());
                            Console.Write("     Parsing Code (byte): ");
                            Byte parsingCode = Convert.ToByte(Console.ReadLine(), 16);
                            Console.Write("     Validation Data (byte collection, each byte separated with slash [-], up to 32 bytes): ");
                            string vn = Console.ReadLine();
                            byte[] validationData = { };
                            validationData = vn.Split(new char[] { '-' }).ToList().Select(s => Convert.ToByte(s, 16)).ToArray();
                            Console.Write("     Expiration Date (YYYY-MM-DD): ");
                            DateTime expirationDate = Convert.ToDateTime(Console.ReadLine());
                            Console.Write("     Pool ID (two bytes, separated with slash [-]): ");
                            string vn1 = Console.ReadLine();
                            byte[] poolId = { };
                            poolId = vn1.Split(new char[] { '-' }).ToList().Select(s => Convert.ToByte(s, 16)).ToArray();
                            _sashost.SendtRedeemTicketCommand(transferCode, transferAmount, parsingCode, validationData, expirationDate, poolId);
                            break;
                    }
                    case "afttransferfunds": // lp72
                    {
                        Console.Write("     Enter cashable amount: "); _sashost.AFTTransferFunds(0x00, Int32.Parse(Console.ReadLine()), generateTransactionID());
                        //_sashost.AFTTransferFunds(0x00, Console.ReadLine());
                        break;
                    }                
                    case "aftcashoutfunds": // lp72
                    {
                        Console.Write("     Enter cashable amount: "); _sashost.AFTTransferFunds(0x80, Int32.Parse(Console.ReadLine()), generateTransactionID());
                        break;
                    }               
                    case "aftint": // lp72
                    {
                        _sashost.AFTInit();
                        break;
                    }                   
                    case "aftreginit": // lp73
                    {
                        _sashost.AFTRegistration(0x00);
                        break;
                    }
                    case "aftregreg": // lp73
                    {
                        _sashost.AFTRegistration(0x01);
                        break;
                    }
                    case "aftregunreg": // lp73
                    {
                        _sashost.AFTRegistration(0x80);
                        break;
                    }
                    case "aftregread": // lp73
                    {
                        _sashost.AFTRegistration(0xFF);
                        break;
                    }                   
                    case "lp74":
                    {
                        _sashost.LockEGM();
                        break;
                    }
                    case "lp74lock":
                    {
                            Console.Write("     Lock Code: ");
                            Byte LockCode = Convert.ToByte(Console.ReadLine(), 16);
                            Console.Write("     Transfer Condition: ");
                            Byte transferCondition = Convert.ToByte(Console.ReadLine(), 16);
                            Console.Write("     Lock Timeout (In hundredths of a second): ");
                            uint lockTimeout = Convert.ToUInt32(Console.ReadLine());
                            try{ _sashost.SendLP74(LockCode, transferCondition, intToBCD5_v2(lockTimeout,2)); } catch { }
                            break;
                    }                   
                    case "lp7b":
                    {
                            Console.Write("     Control Mask 1: ");
                            Byte control_mask1 = Convert.ToByte(Console.ReadLine(), 16);
                            Console.Write("     Control Mask 2: ");
                            Byte control_mask2 = Convert.ToByte(Console.ReadLine(), 16);
                            Console.Write("     Status Bit Control States 1: ");
                            Byte status_bit_control_states1 = Convert.ToByte(Console.ReadLine(), 16);
                            Console.Write("     Status Bit Control States 2: ");
                            Byte status_bit_control_states2 = Convert.ToByte(Console.ReadLine(), 16);
                            Console.Write("     Cashable Ticket and Receipt Expiration 1: ");
                            Byte cashable_ticket_and_receipt_expiration1 = Convert.ToByte(Console.ReadLine(), 16);
                            Console.Write("     Cashable Ticket and Receipt Expiration 2: ");
                            Byte cashable_ticket_and_receipt_expiration2 = Convert.ToByte(Console.ReadLine(), 16);
                            Console.Write("     Restricted Ticket Default Expiration 1: ");
                            Byte restricted_ticket_default_expiration1 = Convert.ToByte(Console.ReadLine(), 16);
                            Console.Write("     Restricted Ticket Default Expiration 2: ");
                            Byte restricted_ticket_default_expiration2 = Convert.ToByte(Console.ReadLine(), 16);
                            _sashost.ExtendedValidationStatus(control_mask1, control_mask2, status_bit_control_states1, status_bit_control_states2, cashable_ticket_and_receipt_expiration1, cashable_ticket_and_receipt_expiration2, restricted_ticket_default_expiration1, restricted_ticket_default_expiration2);
                            break;
                    }
                    case "lp7c":
                    {
                            Console.Write("     Code: ");
                            Byte code = Convert.ToByte(Console.ReadLine(), 16);
                            Console.Write("     Data: ");
                            string data = Console.ReadLine();
                            _sashost.SetExtendedTicketData(code, data);
                            break;
                    }
                    case "lp80":
                    {
                        Console.Write("     Group: ");
                        Byte group = Convert.ToByte(Console.ReadLine(), 16);
                        Console.Write("     Level: ");
                        Byte level = Convert.ToByte(Console.ReadLine(), 16);
                        Console.Write("     Amount: ");
                        uint amount = Convert.ToUInt32(Console.ReadLine());
                        _sashost.SendLP80(false, group, level, intToBCD5_v2(amount,5));
                        break;
                    }
                    case "lp83":
                    {
                        Console.Write("     Game Number: ");
                        uint amount1 = Convert.ToUInt32(Console.ReadLine());
                        _sashost.SendLP83(intToBCD5_v2(amount1,2));
                        break;
                    }
                    case "lp84":
                    {
                        _sashost.SendLP84();
                        break;
                    }
                    case "lp85":
                    {
                        _sashost.SendLP85();
                        break;
                    }
                    case "lp86":
                    {
                        Console.Write("     Group: ");
                        Byte group2 = Convert.ToByte(Console.ReadLine(), 16);
                        Console.Write("     Level Count (max 32):");
                        uint levelCount =  Convert.ToUInt32(Console.ReadLine());
                        int i = 1;
                        List<Tuple<byte,byte[]>> AmountsAndLevels = new List<Tuple<byte,byte[]>>();
                        if (levelCount <= 32)
                        {
                            while (i <= levelCount)
                            {
                                Console.Write($"----Level {i}: ");
                                Console.Write("     Level: ");
                                Byte level2 = Convert.ToByte(Console.ReadLine(), 16);
                                Console.Write("     Amount: ");
                                uint amount2 = Convert.ToUInt32(Console.ReadLine());
                                Tuple<byte,byte[]> t = new Tuple<byte,byte[]>(level2, intToBCD5_v2(amount2));
                                AmountsAndLevels.Add(t);
                                i++;
                            }
                        }
                        _sashost.SendLP86(false, group2, AmountsAndLevels);
                        break;
                    }
                    case "lp87":
                    {
                        _sashost.SendLP87();
                        break;
                    }
                    case "lp8C":
                    {
                        Console.Write("     Game Number: ");
                        uint gameNumber5 = Convert.ToUInt32(Console.ReadLine());
                        Console.Write("     Time: ");
                        uint time5 = Convert.ToUInt32(Console.ReadLine());
                        Console.Write("     Credits: ");
                        uint credits5 = Convert.ToUInt32(Console.ReadLine());
                        Console.Write("     Pulses: ");
                        Byte pulses5 = Convert.ToByte(Console.ReadLine(), 16);
                        _sashost.SendLP8C(intToBCD5_v2(gameNumber5,2), intToBCD5_v2(time5,2), intToBCD5_v2(credits5,4), pulses5);
                        break;
                    }
                    case "lp95":
                    {
                        Console.Write("     Game Number: ");
                        uint gameNumber6 = Convert.ToUInt32(Console.ReadLine());
                        _sashost.SendTournamentGamesPlayed(intToBCD5_v2(gameNumber6,2));
                        break;
                    }
                    case "lp96":
                    {
                        Console.Write("     Game Number: ");
                        uint gameNumber7 = Convert.ToUInt32(Console.ReadLine());
                        _sashost.SendTournamentGamesWon(intToBCD5_v2(gameNumber7,2));
                        break;
                    }
                    case "lp97":
                    {
                        Console.Write("     Game Number: ");
                        uint gameNumber8 = Convert.ToUInt32(Console.ReadLine());
                        _sashost.SendTournamentCreditsWagered(intToBCD5_v2(gameNumber8,2));
                        break;
                    }
                    case "lp98":
                    {                
                        Console.Write("     Game Number: ");
                        uint gameNumber9 = Convert.ToUInt32(Console.ReadLine());
                        _sashost.SendTournamentCreditsWon(intToBCD5_v2(gameNumber9,2));
                        break;
                    }
                    case "lp99":
                    {
                        Console.Write("     Game Number: ");
                        uint gameNumber10 = Convert.ToUInt32(Console.ReadLine());
                        _sashost.SendTournamentMeters(intToBCD5_v2(gameNumber10,2));
                        break;
                    }                  
                    case "lpb1":
                    {
                        _sashost.SendCurrentPlayerDenomination();
                        break;
                    }
                    case "lpb5":
                    {
                        Console.Write("    Game Number: ");
                        uint gameNumber = Convert.ToUInt32(Console.ReadLine());
                        _sashost.SentExtendedGameNInformation(intToBCD5_v2(gameNumber,2));
                        break;
                    }
                    case "customlongpoll":
                    {
                        Console.Write("     Enter Long poll with space between bytes");
                        string customlpstr = Console.ReadLine();
                        string[] customlparray = customlpstr.Split(' ');
                        List<byte> customlpList = new List<byte>();
                        byte[] customlp = new byte[] {};
                        foreach (var b in customlparray)
                        {
                            customlpList.Add(Convert.ToByte(b,16));
                        }
                        customlp = customlpList.ToArray();
                        _sashost.SendLongPoll(customlp);
                        break;
                    }
                    default:
                    {
                        Console.WriteLine("Default case");
                        break;
                    }
                }

                Console.Write("Enter Command: ");
                cmd = Console.ReadLine();

            } while (cmd != "exit");

        }
     
        static void cmdSent(string cmd, bool crc, bool isRetry, EventArgs e)
        {
           // Console.Write("Command sent: ");
           //Console.WriteLine(cmd);
        }

        // Handles the received command with optional CRC checking
        static void cmdReceived(string cmd, bool crc, EventArgs e)
        {
            if (cmd.Length > 0) // Checking if the command length is greater than 0
            {
                if (crc == false) // If CRC is false, indicating an error
                {
                    Console.Write("ERROR CRC: "); // Printing error message for CRC
                    Console.WriteLine(cmd); // Printing the command
                    Console.Write("Enter Command: "); // Prompting to enter a command
                }
                else if (crc == true && cmd != "00" && cmd != "1F") // If CRC is true and the command is not "00" or "1F"
                {
                    Console.Write("Command Received: "); // Indicating a received command
                    Console.WriteLine(cmd); // Printing the received command
                    parser.ParseLongPoll(cmd); // Parsing the received command using the parser
                    Console.Write("Enter Command: "); // Prompting to enter a command
                }

                //cmd = Console.ReadLine(); // Uncomment to allow user input for the command
            }
        }

        // Handles the received data
        static void dataReceived(string cmd, EventArgs e)
        {
            if (cmd.Length > 0) // Checking if the data length is greater than 0
            {
                Console.Write(@"Data received: "); // Indicating the received data
                Console.WriteLine(cmd); // Printing the received data
                Console.Write("Enter Command: "); // Prompting to enter a command
            }
        }





    }
}
