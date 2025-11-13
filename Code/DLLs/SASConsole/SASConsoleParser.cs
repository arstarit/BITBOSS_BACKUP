using System;
using System.Linq;
using SASComms;
using System.Collections.Generic;

namespace SASConsole
{
    public class LongPollPrinter
    {
        byte[] array; // Array to hold the initial bytes
        int currentindex = 0; // Current index within the array
        int currentfieldlength = 0; // Length of the current field

        // Constructor for LongPollPrinter class
        public LongPollPrinter(byte[] init, int startindex)
        {
            array = init; // Initializing the array
            currentindex = startindex; // Initializing the current index
        }

        // Prints the specified subarray and updates the current index
        public void PrintAndNext(string name, int length)
        {
            if (length >= 0 && currentindex + length <= array.Length - 2) // Checking the length and array boundaries
            {
                List<byte> subarray = new List<byte>();
                for (int i = currentindex; i < currentindex + length; i++) // Creating a subarray
                {
                    subarray.Add(array[i]);
                }
                Console.WriteLine(name + "->" + BitConverter.ToString(subarray.ToArray())); // Printing the subarray
                currentfieldlength = length; // Updating the current field length
                currentindex += currentfieldlength; // Moving the current index to the next field
            }
        }

        // Prints the specified subarray without updating the current index
        public void Print(string name, int length)
        {
            if (length >= 0 && currentindex + length <= array.Length - 2) // Checking the length and array boundaries
            {
                List<byte> subarray = new List<byte>();
                for (int i = currentindex; i < currentindex + length; i++) // Creating a subarray
                {
                    subarray.Add(array[i]);
                }
                Console.WriteLine(name + "->" + BitConverter.ToString(subarray.ToArray())); // Printing the subarray
                currentfieldlength = length; // Updating the current field length
            }
        }

        // Moves the current index to the next position
        public void Next()
        {
            currentindex += currentfieldlength; // Moving the current index to the next field
        }

        // Retrieves the current value at the current index in the array
        public byte GetCurrentValue()
        {
            if (currentindex < array.Length) // Checking the array boundary
            {
                return array[currentindex]; // Returning the value at the current index
            }
            return 0x00; // Returning 0x00 if the current index is out of bounds
        }
    }

    public class SASConsoleParser
    {
        public SASConsoleParser()
        {
            // Constructor for the SASConsoleParser class
        }

        // Converts a string of hexadecimal characters to a byte array
        private static byte[] HexStringToBytes(string s)
        {
            const string HEX_CHARS = "0123456789ABCDEF"; // Valid hexadecimal characters

            if (s.Length == 0)
                return new byte[0]; // Return an empty byte array if the string is empty

            if ((s.Length + 1) % 3 != 0) // Checking if the string length is valid for conversion
                throw new FormatException(); // Throw a format exception if the length is invalid

            byte[] bytes = new byte[(s.Length + 1) / 3]; // Initializing the byte array for the converted values
            int state = 0; // 0 = expect first digit, 1 = expect second digit, 2 = expect hyphen
            int currentByte = 0;
            int x;
            int value = 0;

            foreach (char c in s) // Iterating through each character in the string
            {
                switch (state) // Handling the state transitions for character processing
                {
                    case 0: // Expecting the first digit
                        x = HEX_CHARS.IndexOf(Char.ToUpperInvariant(c)); // Obtaining the index of the character
                        if (x == -1)
                            throw new FormatException(); // Throw an exception if the character is not a valid hexadecimal character
                        value = x << 4; // Shifting the value by 4 bits
                        state = 1; // Transitioning to the next state
                        break;
                    case 1: // Expecting the second digit
                        x = HEX_CHARS.IndexOf(Char.ToUpperInvariant(c)); // Obtaining the index of the character
                        if (x == -1)
                            throw new FormatException(); // Throw an exception if the character is not a valid hexadecimal character
                        bytes[currentByte++] = (byte)(value + x); // Storing the calculated byte value
                        state = 2; // Transitioning to the next state
                        break;
                    case 2: // Expecting a hyphen
                        if (c != '-')
                            throw new FormatException(); // Throw an exception if the character is not a hyphen
                        state = 0; // Transitioning to the initial state
                        break;
                }
            }

            return bytes; // Return the resulting byte array
        }


        // Generates an array of indexes starting from 'start' with the specified 'length'
        private int[] GetIndexes(int start, int length)
        {
            List<int> indexes = new List<int>(); // Initializing a list to store the indexes
            for (int i = start; i < start + length; i++) // Generating indexes based on the 'start' and 'length'
            {
                indexes.Add(i); // Adding the indexes to the list
            }
            return indexes.ToArray(); // Converting the list to an array and returning it
        }

        // Prints the field specified by the indexes from the 'response' byte array with the given 'name'
        private void printfield(byte[] response, int[] indexes, string name)
        {
            List<byte> subresponse = new List<byte>(); // Initializing a list to store the subresponse
            for (int i = 0; i < indexes.Length; i++) // Iterating through the indexes
            {
                subresponse.Add(response[indexes[i]]); // Adding the corresponding bytes to the subresponse list
            }
            Console.WriteLine(name + "->" + BitConverter.ToString(subresponse.ToArray())); // Printing the subresponse
        }

        // Empty print method
        private void print(byte[] response, string name)
        {
            // Add your implementation here if needed
        }

        // Displays a start header for parsing information
        private void startheader()
        {
            Console.WriteLine("****************Parsing info*************"); // Printing the start header
        }

        // Displays a finish header for parsing information
        private void finishheader()
        {
            Console.WriteLine("****************************************"); // Printing the finish header
        }



        /// <summary>
        /// Función privada, que retorna el tamaño mínimo que puede tomar el meter de código code_.
        /// Se usa para determinar el límite de parseo y de lectura de las respuestas de meters. Y para determinar cuantos bytes responder cuando consultan determinado meter
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private int MinSize(byte code)
        {
            /* If the code is between 00 and 0D, return 4 */
            if (0x00 <= code && code < 0x0D)
                return 4;
            /* If the code is between 0D and 10, return 5 */
            if (0x0D <= code && code < 0x11)
                return 5;
            /* If the code is between 11 and 7E, return 4*/
            if (0x11 <= code && code <= 0x7F)
                return 4;
            byte[] arr = new byte[] {0x80, 0x82, 0x84, 0x86, 0x88, 0x8A, 0x8C, 0x8E,
                             0x90, 0x92, 0xA0, 0xA2, 0xA4, 0xA6, 0xA8, 0xAA,
                             0xAC, 0xAE, 0xB0, 0xB8, 0xBA, 0xBC};
            /* If the code is among 80, 82, 84, 86, 88, 8A, 8C, 8E, 90, 92 */
            if (arr.Contains(code))
            {
                return 5; /* Return 5 */
            }
            else
            {
                return 4; /* Otherwise, return 4 */
            }

        }


        public void ParseLongPoll(string resp1)
        {
            byte[] response = HexStringToBytes(resp1); // Converting the hex string to a byte array
            if (response.Length >= 2) // Checking the value at index 1 of the response
            {
                switch (response[1])
                {
                    case 0x1E:// If the value is 0x1E
                        {
                            startheader(); // Printing the start header for parsing information
                            LongPollPrinter printer = new LongPollPrinter(response, 2); // Creating an instance of LongPollPrinter
                            printer.PrintAndNext("$1 Bills meter accepted", 4); // Printing and moving to the next field
                            printer.PrintAndNext("$5 Bills meter accepted", 4); // Printing and moving to the next field
                            printer.PrintAndNext("$10 Bills meter accepted", 4); // Printing and moving to the next field
                            printer.PrintAndNext("$20 Bills meter accepted", 4); // Printing and moving to the next field
                            printer.PrintAndNext("$50 Bills meter accepted", 4); // Printing and moving to the next field
                            printer.PrintAndNext("$100 Bills meter accepted", 4); // Printing and moving to the next field
                            finishheader(); // Printing the finish header for parsing information
                            break; // Breaking out of the switch statement           
                        }
                    case 0x1F:
                        {
                            startheader(); // Printing the start header for parsing information
                            LongPollPrinter printer = new LongPollPrinter(response, 2); // Creating an instance of LongPollPrinter
                            printer.PrintAndNext("Game ID, in ASCII", 2); // Printing and moving to the next field
                            printer.PrintAndNext("Additional ID", 3); // Printing and moving to the next field
                            printer.PrintAndNext("Denomination", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Max Bet", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Progressive Group", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Game Options", 2); // Printing and moving to the next field
                            printer.PrintAndNext("Paytable ID", 6); // Printing and moving to the next field
                            printer.PrintAndNext("Base%", 4); // Printing and moving to the next field
                            finishheader(); // Printing the finish header for parsing information
                            break; // Breaking out of the switch statement
                        }
                    case 0x2F:
                        {
                            startheader(); // Printing the start header for parsing information
                            LongPollPrinter printer = new LongPollPrinter(response, 2); // Creating an instance of LongPollPrinter
                            printer.Print("Length", 1); // Printing the length and getting the current value
                            int length = printer.GetCurrentValue(); // Storing the current value as 'length'
                            printer.Next(); // Moving to the next field
                            printer.PrintAndNext("Game Number", 2); // Printing the game number and moving to the next field
                            length = length - 2; // Updating the remaining length

                            // Processing multiple fields while the length is greater than 1
                            while (length > 1)
                            {
                                printer.Print("Meter Code", 1); // Printing the meter code and getting the current value
                                int maxsize = MinSize(printer.GetCurrentValue()); // Getting the maximum size based on the current value
                                printer.Next(); // Moving to the next field
                                printer.PrintAndNext("Meter Value", maxsize); // Printing the meter value and moving to the next field
                                length = length - 1 - maxsize; // Updating the remaining length
                            }

                            finishheader(); // Printing the finish header for parsing information
                            break; // Breaking out of the switch statement
                        }
                    case 0x4D:
                        {
                            startheader(); // Printing the start header for parsing information
                            LongPollPrinter printer = new LongPollPrinter(response, 2); // Creating an instance of LongPollPrinter
                            printer.PrintAndNext("Validation Type", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Index Number", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Date", 4); // Printing and moving to the next field
                            printer.PrintAndNext("Time", 3); // Printing and moving to the next field
                            printer.PrintAndNext("Validation Number", 8); // Printing and moving to the next field
                            printer.PrintAndNext("Amount", 5); // Printing and moving to the next field
                            printer.PrintAndNext("Ticket Number", 2); // Printing and moving to the next field
                            printer.PrintAndNext("Validation System ID", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Expiration", 4); // Printing and moving to the next field
                            printer.PrintAndNext("Pool ID", 2); // Printing and moving to the next field
                            finishheader(); // Printing the finish header for parsing information
                            break; // Breaking out of the switch statement
                        }
                    case 0x51:
                        {
                            startheader(); // Printing the start header for parsing information
                            LongPollPrinter printer = new LongPollPrinter(response, 2); // Creating an instance of LongPollPrinter
                            printer.PrintAndNext("Number of games implemented", 2); // Printing and moving to the next field
                            finishheader(); // Printing the finish header for parsing information
                            break; // Breaking out of the switch statement
                        }
                    case 0x53:
                        {
                            startheader(); // Printing the start header for parsing information
                            LongPollPrinter printer = new LongPollPrinter(response, 2); // Creating an instance of LongPollPrinter
                            printer.PrintAndNext("Game number", 2); // Printing and moving to the next field
                            printer.PrintAndNext("Game N ID", 2); // Printing and moving to the next field
                            printer.PrintAndNext("Additional ID", 3); // Printing and moving to the next field
                            printer.PrintAndNext("Denomination", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Max bet", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Progressive group", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Game options", 2); // Printing and moving to the next field
                            printer.PrintAndNext("Paytable", 6); // Printing and moving to the next field
                            printer.PrintAndNext("Base%", 4); // Printing and moving to the next field
                            finishheader(); // Printing the finish header for parsing information
                            break; // Breaking out of the switch statement
                        }
                    case 0x54:
                        {
                            startheader(); // Printing the start header for parsing information
                            LongPollPrinter printer = new LongPollPrinter(response, 2); // Creating an instance of LongPollPrinter
                            printer.Print("Length", 1); // Printing the length
                            int length = printer.GetCurrentValue(); // Storing the current value as 'length'
                            printer.Next(); // Moving to the next field
                            printer.PrintAndNext("SAS Version", 3); // Printing and moving to the next field
                            printer.PrintAndNext("Gaming machine serial number", length - 3); // Printing the gaming machine serial number
                            finishheader(); // Printing the finish header for parsing information
                            break; // Breaking out of the switch statement

                        }
                    case 0x55:
                        {
                            startheader(); // Printing the start header for parsing information
                            LongPollPrinter printer = new LongPollPrinter(response, 2); // Creating an instance of LongPollPrinter
                            printer.PrintAndNext("Game Number", 2); // Printing and moving to the next field
                            finishheader(); // Printing the finish header for parsing information
                            break; // Breaking out of the switch statement
                        }
                    case 0x56:
                        {
                            startheader(); // Printing the start header for parsing information
                            LongPollPrinter printer = new LongPollPrinter(response, 2); // Creating an instance of LongPollPrinter
                            printer.PrintAndNext("Length", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Number of games", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Game number ", 2); // Printing and moving to the next field
                            finishheader(); // Printing the finish header for parsing information
                            break; // Breaking out of the switch statement

                        }
                    case 0x57: // If the value is 0x57
                        {
                            startheader(); // Printing the start header for parsing information
                            LongPollPrinter printer = new LongPollPrinter(response, 2); // Creating an instance of LongPollPrinter
                            printer.PrintAndNext("Cashout Type", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Amount", 5); // Printing and moving to the next field
                            finishheader(); // Printing the finish header for parsing information
                            break; // Breaking out of the switch statement
                        }
                    case 0x58: // If the value is 0x58
                        {
                            startheader(); // Printing the start header for parsing information
                            LongPollPrinter printer = new LongPollPrinter(response, 2); // Creating an instance of LongPollPrinter
                            printer.PrintAndNext("Status", 1); // Printing and moving to the next field
                            finishheader(); // Printing the finish header for parsing information
                            break; // Breaking out of the switch statement
                        }
                    case 0x6F: // If the value is 0x6F
                        {
                            startheader(); // Printing the start header for parsing information
                            LongPollPrinter printer = new LongPollPrinter(response, 2); // Creating an instance of LongPollPrinter
                            printer.Print("Length", 1); // Printing the length and getting the current value
                            int length = printer.GetCurrentValue(); // Storing the current value as 'length'
                            printer.Next(); // Moving to the next field
                            printer.PrintAndNext("Game number", 2); // Printing and moving to the next field
                            length = length - 2; // Updating the remaining length

                            // Processing multiple fields while the length is greater than 1
                            while (length > 1)
                            {
                                printer.PrintAndNext("Meter code", 2); // Printing and moving to the next field
                                printer.Print("Meter size", 1); // Printing the meter size and getting the current value
                                int size = printer.GetCurrentValue(); // Storing the current value as 'size'
                                printer.Next(); // Moving to the next field
                                printer.PrintAndNext("Meter value", size); // Printing the meter value and moving to the next field
                                length = length - 2 - 1 - size; // Updating the remaining length
                            }

                            finishheader(); // Printing the finish header for parsing information
                            break; // Breaking out of the switch statement
                        }
                    case 0x70: // If the value is 0x70
                        {
                            startheader(); // Printing the start header for parsing information
                            LongPollPrinter printer = new LongPollPrinter(response, 2); // Creating an instance of LongPollPrinter
                            printer.Print("Length", 1); // Printing the length and getting the current value
                            int length = printer.GetCurrentValue(); // Storing the current value as 'length'
                            printer.Next(); // Moving to the next field
                            printer.PrintAndNext("Ticket Status", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Ticket Amount", 5); // Printing and moving to the next field
                            printer.PrintAndNext("Parsing Code", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Validation Data", length - 1 - 5 - 1); // Printing the validation data
                            finishheader(); // Printing the finish header for parsing information
                            break; // Breaking out of the switch statement
                        }
                    case 0x71: // If the value is 0x71
                        {
                            startheader(); // Printing the start header for parsing information
                            LongPollPrinter printer = new LongPollPrinter(response, 2); // Creating an instance of LongPollPrinter
                            printer.Print("Length", 1); // Printing the length and getting the current value
                            int length = printer.GetCurrentValue(); // Storing the current value as 'length'
                            printer.Next(); // Moving to the next field
                            printer.PrintAndNext("Machine status", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Transfer amount", 5); // Printing and moving to the next field
                            printer.PrintAndNext("Parsing code", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Validation Data", length - 1 - 5 - 1); // Printing the validation data
                            finishheader(); // Printing the finish header for parsing information
                            break; // Breaking out of the switch statement
                        }
                    case 0x72: // If the value is 0x72
                        {
                            startheader(); // Printing the start header for parsing information
                            LongPollPrinter printer = new LongPollPrinter(response, 2); // Creating an instance of LongPollPrinter
                            printer.PrintAndNext("Length", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Transaction buffer position", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Transfer status", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Receipt status", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Transfer type", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Cashable Amount", 5); // Printing and moving to the next field
                            printer.PrintAndNext("Restricted Amount", 5); // Printing and moving to the next field
                            printer.PrintAndNext("Non Restricted Amount", 5); // Printing and moving to the next field
                            printer.PrintAndNext("Transfer Flags", 1); // Printing and moving to the next field
                            printer.PrintAndNext("Asset Number", 4); // Printing and moving to the next field
                            printer.Print("Transaction ID Length", 1); // Printing the transaction ID length and getting the current value
                            int TIDlength = printer.GetCurrentValue(); // Storing the current value as 'TIDlength'
                            printer.Next(); // Moving to the next field
                            printer.PrintAndNext("Transaction ID", TIDlength); // Printing the transaction ID and moving to the next field
                            printer.PrintAndNext("Transaction Date", 4); // Printing and moving to the next field
                            printer.PrintAndNext("Transaction Time", 3); // Printing and moving to the next field
                            printer.PrintAndNext("Expiration", 4); // Printing and moving to the next field
                            printer.PrintAndNext("Pool ID", 2); // Printing and moving to the next field
                            printer.Print("Cumulative cashable amount meter size", 1); // Printing the cumulative cashable amount meter size and getting the current value
                            int CCLength = printer.GetCurrentValue(); // Storing the current value as 'CCLength'
                            printer.Next(); // Moving to the next field
                            printer.PrintAndNext("Cumulative cashable amount meter", CCLength); // Printing the cumulative cashable amount meter and moving to the next field
                            printer.Print("Cumulative restricted amount meter size", 1); // Printing the cumulative restricted amount meter size and getting the current value
                            int CRLength = printer.GetCurrentValue(); // Storing the current value as 'CRLength'
                            printer.Next(); // Moving to the next field
                            printer.PrintAndNext("Cumulative restricted amount meter", CRLength); // Printing the cumulative restricted amount meter and moving to the next field
                            printer.Print("Cumulative non restricted amount meter size", 1); // Printing the cumulative non restricted amount meter size and getting the current value
                            int CNRLength = printer.GetCurrentValue(); // Storing the current value as 'CNRLength'
                            printer.Next(); // Moving to the next field
                            printer.PrintAndNext("Cumulative non restricted amount meter", CNRLength); // Printing the cumulative non restricted amount meter and moving to the next field
                            finishheader(); // Printing the finish header for parsing information
                            break; // Breaking out of the switch statement
                        }
                    case 0x73:
                        {
                            startheader();
                            LongPollPrinter printer = new LongPollPrinter(response, 2);
                            printer.PrintAndNext("Length", 1);
                            printer.PrintAndNext("Registration status", 1);
                            printer.PrintAndNext("Asset number", 4);
                            printer.PrintAndNext("Registration key", 20);
                            printer.PrintAndNext("POS ID", 4);
                            finishheader();
                            break;
                        }
                    case 0x74:
                        {
                            startheader();
                            LongPollPrinter printer = new LongPollPrinter(response, 2);
                            printer.PrintAndNext("Length", 1);
                            printer.PrintAndNext("Asset Number", 4);
                            printer.PrintAndNext("Game Lock Status", 1);
                            printer.PrintAndNext("Available Transfers", 1);
                            printer.PrintAndNext("Host Cashout Status", 1);
                            printer.PrintAndNext("AFT Status", 1);
                            printer.PrintAndNext("Max Buffer Index", 1);
                            printer.PrintAndNext("Current Cashable Amount", 5);
                            printer.PrintAndNext("Current Restricted Amount", 5);
                            printer.PrintAndNext("Current Non Restricted Amount", 5);
                            printer.PrintAndNext("Gaming Machine Transfer Limit", 5);
                            printer.PrintAndNext("Restricted Expiration", 4);
                            printer.PrintAndNext("Restricted Pool ID ", 2);
                            finishheader();
                            break;
                        }
                    case 0x7b:
                        {
                            startheader();
                            LongPollPrinter printer = new LongPollPrinter(response, 2);
                            printer.PrintAndNext("Length", 1);
                            printer.PrintAndNext("Control Mask", 2);
                            printer.PrintAndNext("Status bit control states", 2);
                            printer.PrintAndNext("Cashable ticket and receipt expiration", 2);
                            printer.PrintAndNext("Restricted ticket default expiration", 2);
                            finishheader();
                            break;
                        }
                    case 0x7C:
                        {
                            startheader();
                            LongPollPrinter printer = new LongPollPrinter(response, 2);
                            printer.PrintAndNext("Length", 1);
                            printer.PrintAndNext("Ticket data status flag", 1);
                            finishheader();
                            break;
                        }
                    case 0x83:
                        {
                            startheader();
                            LongPollPrinter printer = new LongPollPrinter(response, 2);
                            printer.PrintAndNext("Game Number", 2);
                            printer.PrintAndNext("Cumulative progressive wins", 2);
                            finishheader();
                            break;
                        }
                    case 0x84:
                        {
                            startheader();
                            LongPollPrinter printer = new LongPollPrinter(response, 2);
                            printer.PrintAndNext("Group", 1);
                            printer.PrintAndNext("Level", 1);
                            printer.PrintAndNext("Amount", 5);
                            finishheader();
                            break;
                        }
                    case 0x85:
                        {
                            startheader();
                            LongPollPrinter printer = new LongPollPrinter(response, 2);
                            printer.PrintAndNext("Group", 1);
                            printer.PrintAndNext("Level", 1);
                            printer.PrintAndNext("Amount", 5);
                            finishheader();
                            break;
                        }
                    case 0x86:
                        {
                            startheader();
                            LongPollPrinter printer = new LongPollPrinter(response, 2);
                            printer.Print("Length", 1); int length = printer.GetCurrentValue(); printer.Next();
                            printer.PrintAndNext("Group", 1);
                            printer.PrintAndNext("Level", 1);
                            printer.PrintAndNext("Amount", 5); length = length - 5 - 1 - 1;
                            while (length > 0)
                            {
                                printer.PrintAndNext("Level", 1);
                                printer.PrintAndNext("Amount", 5); length = length - 5 - 1;
                            }
                            finishheader();
                            break;
                        }
                    case 0x87:
                        {
                            startheader();
                            LongPollPrinter printer = new LongPollPrinter(response, 2);
                            printer.Print("Length", 1); int length = printer.GetCurrentValue(); printer.Next();
                            printer.PrintAndNext("Group", 1);
                            printer.PrintAndNext("Number of levels", 1);
                            printer.PrintAndNext("Level", 1);
                            printer.PrintAndNext("Amount", 5); length = length - 5 - 1 - 1 - 1;
                            while (length > 0)
                            {
                                printer.PrintAndNext("Level", 1);
                                printer.PrintAndNext("Amount", 5); length = length - 5 - 1;
                            }
                            finishheader();
                            break;
                        }
                    case 0xAF:
                        {
                            startheader();
                            LongPollPrinter printer = new LongPollPrinter(response, 2);
                            printer.Print("Length", 1); int length = printer.GetCurrentValue(); printer.Next();
                            printer.PrintAndNext("Game number", 2); length = length - 2;
                            while (length > 1)
                            {
                                printer.PrintAndNext("Meter code", 2);
                                printer.Print("Meter size", 1); int size = printer.GetCurrentValue(); printer.Next();
                                printer.PrintAndNext("Meter value", size); length = length - 2 - 1 - size;



                            }
                            finishheader();
                            break;
                        }
                    case 0xB1:
                        {
                            startheader();
                            LongPollPrinter printer = new LongPollPrinter(response, 2);
                            printer.PrintAndNext("Current player denomination", 2);

                            finishheader();
                            break;
                        }
                    case 0xB5:
                        {
                            startheader();
                            LongPollPrinter printer = new LongPollPrinter(response, 2);
                            printer.PrintAndNext("Length", 1);
                            printer.PrintAndNext("Game number", 2);
                            printer.PrintAndNext("Max bet", 2);
                            printer.PrintAndNext("Progressive group", 1);
                            printer.PrintAndNext("Progressive levels", 4);
                            printer.Print("Game name length", 1); int gamenamelength = printer.GetCurrentValue(); printer.Next();
                            printer.PrintAndNext("Game name", gamenamelength);
                            printer.Print("Paytable name length", 1); int paytablenamelength = printer.GetCurrentValue(); printer.Next();
                            printer.PrintAndNext("Paytable name", paytablenamelength);
                            printer.PrintAndNext("Wager categories", 2);
                            finishheader();
                            break;
                        }
                }

            }
            else if (response.Length == 1)
            {
                switch (response[0])
                {
                    case 0x13:
                        Console.WriteLine("$13/ Drop door opened");
                        break;
                    case 0x14:
                        Console.WriteLine("$14/ Drop door was closed");
                        break;
                    case 0x15:
                        Console.WriteLine("$15/ Card cage was opened");
                        break;
                    case 0x16:
                        Console.WriteLine("$16/ Card cage was closed");
                        break;
                    case 0x19:
                        Console.WriteLine("$19/ Cashbox door was opened");
                        break;
                    case 0x1A:
                        Console.WriteLine("$1A/ Cashbox door was closed");
                        break;
                    case 0x1B:
                        Console.WriteLine("$1B/ Cashbox was removed");
                        break;
                    case 0x1C:
                        Console.WriteLine("$1C/ Cashbox was installed");
                        break;
                    case 0x1D:
                        Console.WriteLine("$1D/ Belly door was opened");
                        break;
                    case 0x1E:
                        Console.WriteLine("$1E/ Belly door was closed");
                        break;
                    case 0x20:
                        Console.WriteLine("$20/ General tilt");
                        break;
                    case 0x21:
                        Console.WriteLine("$21/ Coin in tilt");
                        break;
                    case 0x22:
                        Console.WriteLine("$22/ Coin out tilt");
                        break;
                    case 0x23:
                        Console.WriteLine("$23/ Hopper empty detected");
                        break;
                    case 0x24:
                        Console.WriteLine("$24/ Extra coin paid");
                        break;
                    case 0x25:
                        Console.WriteLine("$25/ Diverter malfunction (controls coins to drop or hopper)");
                        break;
                    case 0x27:
                        Console.WriteLine("$27/ Cashbox full detected");
                        break;
                    case 0x28:
                        Console.WriteLine("$28/ Bill jam");
                        break;
                    case 0x29:
                        Console.WriteLine("$29/ Bill acceptor hardware failure");
                        break;
                    case 0x2A:
                        Console.WriteLine("$2A/ Reverse Bill detected");
                        break;
                    case 0x47:
                        Console.WriteLine("$47/ $1.00 bill accepted");
                        break;
                    case 0x48:
                        Console.WriteLine("$47/ $5.00 bill accepted ");
                        break;
                    case 0x49:
                        Console.WriteLine("$49/ $10.00 bill accepted");
                        break;
                    case 0x4A:
                        Console.WriteLine("$4A/ $20.00 bill accepted");
                        break;
                    case 0x4B:
                        Console.WriteLine("$4B/ $50.00 bill accepted");
                        break;
                    case 0x4C:
                        Console.WriteLine("$4C/ $100.00 bill accepted");
                        break;
                    case 0x4D:
                        Console.WriteLine("$4D/ $2.00 bill accepted");
                        break;
                    case 0x4E:
                        Console.WriteLine("$4E/ $500.00 bill accepted");
                        break;
                    case 0x4F:
                        Console.WriteLine("$4F/ Bill accepted");
                        break;
                    case 0x50:
                        Console.WriteLine("$50/ $200.00 bill accepted");
                        break;
                    case 0x6F:
                        Console.WriteLine("$6F/ Game Locked");
                        break;
                    // case 0x11:
                    //     return "$11/ Slot door opened";
                    // case 0x12:
                    //     return "$12/ Slot door closed";
                    // case 0x17:
                    //     return "$17/ AC power was applied to gaming machine";
                    // case 0x18:
                    //     return "$18/ AC power was lost from gaming machine";

                    // case 0x7E:
                    //     return "$7E/ Game has started";
                    // case 0x7F:
                    //     return "$7F/ Game has ended";

                    // case 0x69:
                    //     return "$69/ AFT transfer complete";



                    default:
                        break;
                }
            }

        }

    }
}
