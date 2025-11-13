using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Serialization;

namespace BitbossInterface
{

    [XmlType]
    public class EGMAccounting
    {
        // Función privada, que retorna el tamaño mínimo que puede tomar el meter de código code_
        // Se usa para determinar el límite de parseo y de lectura de las respuestas de meters. Y para determinar cuantos bytes responder cuando 
        // consultan determinado meter

        // Private function, that returns the minimum length which can take the meter of code 'code_'
        // It is used to determine the parsing and reading limits of the meters responses and to determine how many bytes are used for the response
        private int MinSize(byte[] code_)
        {
            byte code = 0xFF;
            /*Si el tamaño del code es 2, me quedo con el segundo byte, si no, con el primer byte*/
            /*If the code length is 2, we take the second byte. Otherwise, we take the first byte */
            if (code_.Length == 2)
            {
                code = code_[1];
            }
            else
            {
                code = code_[0];
            }
            /*Si el code está entre el 00 y el 0D, retorno 4 */
            /*If the code is between 00 and 0D, return 4 */
            if (0x00 <= code && code < 0x0D)
                return 4;
            /* Si el code está entre el 0D y el 10, retorno 5 */
            /* If the code is between 0D and 10, return 5*/
            if (0x0D <= code && code < 0x11)
                return 5;
            /* Si el code está entre el 11 y el 7E, retorno 4*/
            /* If the code is betwenn 11 and 7E, return 4*/
            if (0x11 <= code && code <= 0x7F)
                return 4;
            byte[] arr = new byte[] {0x80, 0x82, 0x84, 0x86, 0x88, 0x8A, 0x8C, 0x8E,
                                     0x90, 0x92, 0xA0, 0xA2, 0xA4, 0xA6, 0xA8, 0xAA,
                                     0xAC, 0xAE, 0xB0, 0xB8, 0xBA, 0xBC};
            /* Si el array está entre los meters pares */
            /* If the code is among the codes of the array*/
            if (arr.Contains(code))
            {
                return 5; /* Retorno 5 */ /* Return 5*/
            }
            else
            {
                return 4; /* Si no, retorno 4 */ /* Otherwise, return 4*/
            }

        }

        // Función privada, que retorna el tamaño mínimo que puede tomar el meter de código code_
        // Private function, that returns the minimum length which can take the meter of code 'code_'
        private int MinSize(string code_)
        {
            return 4;
        }

        private byte[] CutCeros(byte[] b)
        {
            if (b.FirstOrDefault() == 0x00 && b.Length > 1)
            {
                return CutCeros(b.Skip(1).ToArray());
            }
            else
            {
                return b;
            }
        }

        // Rutina privada, que toma un entero unsigned, y retorna como trend de bytes su equivalente. Ejemplo 12345 = 0x01, 0x23, 0x45
        // Private routine, that takes an unsigned integer, and returns as byte trend its equivalent. Example 12345 = 0x01, 0x23, 0x45
        private byte[] intToBCD5(uint numericvalue, int bytesize = 5)
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

        [XmlAttribute]
        public uint Credits;
        private GameAccounting gm;  // Game info por default, usado para ser devuelto en el caso de que no se encuentre un game solicitado
                                    // Game info by default, used in case that a requested game doesn't exist

        [XmlIgnore]
        public Dictionary<int, GameAccounting> gameAccountings; // All game accountings




        // Rutina de inicialización
        // Initialization routine
        public EGMAccounting()
        {
            Credits = 15005;
            gameAccountings = new Dictionary<int, GameAccounting>();
        }

        /* Seteo de games, toma un booleano isPhysical, que concatena como prefijo 'Physical' a los archivos correspondientes a los games.
            Si no existen, los crea en memoria y cuando se requiera guardar, se persiste en el disco */
        /*Games setting, it takes a boolean isPhysical, which concatenates the 'Physical' prefix to the files corresponding to the games.
            If they do not exist, they are created in memory and when they are required to be saved, they are persisted to disk. */
        public void SetGames(bool isPhysical)
        {
            string prefix = "";
            if (isPhysical)
            {
                prefix = "Physical";
            }
            else
            {

            }
            XmlFileSerializer xmlfile_serializer = new XmlFileSerializer();

            // La EGMAccounting, saca los datos persistidos. Si no existe, instancia una nueva y la persiste
            //EGMAccounting, retrieves the persisted data. If it doesn't exist, instantiate a new one and persist it
            try { gameAccountings.Add(0, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game0Accounting.xml")); } catch { gameAccountings.Add(0, new GameAccounting()); }
            gameAccountings[0].SetFilePath(prefix + "Game0Accounting.xml");
            try { gameAccountings.Add(1, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game1Accounting.xml")); } catch { gameAccountings.Add(1, new GameAccounting()); }
            gameAccountings[1].SetFilePath(prefix + "Game1Accounting.xml");
            try { gameAccountings.Add(2, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game2Accounting.xml")); } catch { gameAccountings.Add(2, new GameAccounting()); }
            gameAccountings[2].SetFilePath(prefix + "Game2Accounting.xml");
            try { gameAccountings.Add(3, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game3Accounting.xml")); } catch { gameAccountings.Add(3, new GameAccounting()); }
            gameAccountings[3].SetFilePath(prefix + "Game3Accounting.xml");
            try { gameAccountings.Add(4, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game4Accounting.xml")); } catch { gameAccountings.Add(4, new GameAccounting()); }
            gameAccountings[4].SetFilePath(prefix + "Game4Accounting.xml");
            try { gameAccountings.Add(5, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game5Accounting.xml")); } catch { gameAccountings.Add(5, new GameAccounting()); }
            gameAccountings[5].SetFilePath(prefix + "Game5Accounting.xml");
            try { gameAccountings.Add(6, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game6Accounting.xml")); } catch { gameAccountings.Add(6, new GameAccounting()); }
            gameAccountings[6].SetFilePath(prefix + "Game6Accounting.xml");
            try { gameAccountings.Add(7, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game7Accounting.xml")); } catch { gameAccountings.Add(7, new GameAccounting()); }
            gameAccountings[7].SetFilePath(prefix + "Game7Accounting.xml");
            try { gameAccountings.Add(8, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game8Accounting.xml")); } catch { gameAccountings.Add(8, new GameAccounting()); }
            gameAccountings[8].SetFilePath(prefix + "Game8Accounting.xml");
            try { gameAccountings.Add(9, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game9Accounting.xml")); } catch { gameAccountings.Add(9, new GameAccounting()); }
            gameAccountings[9].SetFilePath(prefix + "Game9Accounting.xml");
            try { gameAccountings.Add(10, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game10Accounting.xml")); } catch { gameAccountings.Add(10, new GameAccounting()); }
            gameAccountings[10].SetFilePath(prefix + "Game10Accounting.xml");
            try { gameAccountings.Add(11, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game11Accounting.xml")); } catch { gameAccountings.Add(11, new GameAccounting()); }
            gameAccountings[11].SetFilePath(prefix + "Game11Accounting.xml");
            try { gameAccountings.Add(12, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game12Accounting.xml")); } catch { gameAccountings.Add(12, new GameAccounting()); }
            gameAccountings[12].SetFilePath(prefix + "Game12Accounting.xml");
            try { gameAccountings.Add(13, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game13Accounting.xml")); } catch { gameAccountings.Add(13, new GameAccounting()); }
            gameAccountings[13].SetFilePath(prefix + "Game13Accounting.xml");
            try { gameAccountings.Add(14, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game14Accounting.xml")); } catch { gameAccountings.Add(14, new GameAccounting()); }
            gameAccountings[14].SetFilePath(prefix + "Game14Accounting.xml");
            try { gameAccountings.Add(15, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game15Accounting.xml")); } catch { gameAccountings.Add(15, new GameAccounting()); }
            gameAccountings[15].SetFilePath(prefix + "Game15Accounting.xml");
            try { gameAccountings.Add(16, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game16Accounting.xml")); } catch { gameAccountings.Add(16, new GameAccounting()); }
            gameAccountings[16].SetFilePath(prefix + "Game16Accounting.xml");
            try { gameAccountings.Add(17, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game17Accounting.xml")); } catch { gameAccountings.Add(17, new GameAccounting()); }
            gameAccountings[17].SetFilePath(prefix + "Game17Accounting.xml");
            try { gameAccountings.Add(18, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game18Accounting.xml")); } catch { gameAccountings.Add(18, new GameAccounting()); }
            gameAccountings[18].SetFilePath(prefix + "Game18Accounting.xml");
            try { gameAccountings.Add(19, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game19Accounting.xml")); } catch { gameAccountings.Add(19, new GameAccounting()); }
            gameAccountings[19].SetFilePath(prefix + "Game19Accounting.xml");
            try { gameAccountings.Add(20, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game20Accounting.xml")); } catch { gameAccountings.Add(20, new GameAccounting()); }
            gameAccountings[20].SetFilePath(prefix + "Game20Accounting.xml");
            try { gameAccountings.Add(21, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game21Accounting.xml")); } catch { gameAccountings.Add(21, new GameAccounting()); }
            gameAccountings[21].SetFilePath(prefix + "Game21Accounting.xml");
            try { gameAccountings.Add(22, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game22Accounting.xml")); } catch { gameAccountings.Add(22, new GameAccounting()); }
            gameAccountings[22].SetFilePath(prefix + "Game22Accounting.xml");
            try { gameAccountings.Add(23, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game23Accounting.xml")); } catch { gameAccountings.Add(23, new GameAccounting()); }
            gameAccountings[23].SetFilePath(prefix + "Game23Accounting.xml");
            try { gameAccountings.Add(24, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game24Accounting.xml")); } catch { gameAccountings.Add(24, new GameAccounting()); }
            gameAccountings[24].SetFilePath(prefix + "Game24Accounting.xml");
            try { gameAccountings.Add(25, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game25Accounting.xml")); } catch { gameAccountings.Add(25, new GameAccounting()); }
            gameAccountings[25].SetFilePath(prefix + "Game25Accounting.xml");
            try { gameAccountings.Add(26, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game26Accounting.xml")); } catch { gameAccountings.Add(26, new GameAccounting()); }
            gameAccountings[26].SetFilePath(prefix + "Game26Accounting.xml");
            try { gameAccountings.Add(27, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game27Accounting.xml")); } catch { gameAccountings.Add(27, new GameAccounting()); }
            gameAccountings[27].SetFilePath(prefix + "Game27Accounting.xml");
            try { gameAccountings.Add(28, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game28Accounting.xml")); } catch { gameAccountings.Add(28, new GameAccounting()); }
            gameAccountings[28].SetFilePath(prefix + "Game28Accounting.xml");
            try { gameAccountings.Add(29, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game29Accounting.xml")); } catch { gameAccountings.Add(29, new GameAccounting()); }
            gameAccountings[29].SetFilePath(prefix + "Game29Accounting.xml");
            try { gameAccountings.Add(30, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game30Accounting.xml")); } catch { gameAccountings.Add(30, new GameAccounting()); }
            gameAccountings[30].SetFilePath(prefix + "Game30Accounting.xml");
            try { gameAccountings.Add(31, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game31Accounting.xml")); } catch { gameAccountings.Add(31, new GameAccounting()); }
            gameAccountings[31].SetFilePath(prefix + "Game31Accounting.xml");
            try { gameAccountings.Add(32, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game32Accounting.xml")); } catch { gameAccountings.Add(32, new GameAccounting()); }
            gameAccountings[32].SetFilePath(prefix + "Game32Accounting.xml");
            try { gameAccountings.Add(33, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game33Accounting.xml")); } catch { gameAccountings.Add(33, new GameAccounting()); }
            gameAccountings[33].SetFilePath(prefix + "Game33Accounting.xml");
            try { gameAccountings.Add(34, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game34Accounting.xml")); } catch { gameAccountings.Add(34, new GameAccounting()); }
            gameAccountings[34].SetFilePath(prefix + "Game34Accounting.xml");
            try { gameAccountings.Add(35, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game35Accounting.xml")); } catch { gameAccountings.Add(35, new GameAccounting()); }
            gameAccountings[35].SetFilePath(prefix + "Game35Accounting.xml");
            try { gameAccountings.Add(36, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game36Accounting.xml")); } catch { gameAccountings.Add(36, new GameAccounting()); }
            gameAccountings[36].SetFilePath(prefix + "Game36Accounting.xml");
            try { gameAccountings.Add(37, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game37Accounting.xml")); } catch { gameAccountings.Add(37, new GameAccounting()); }
            gameAccountings[37].SetFilePath(prefix + "Game37Accounting.xml");
            try { gameAccountings.Add(38, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game38Accounting.xml")); } catch { gameAccountings.Add(38, new GameAccounting()); }
            gameAccountings[38].SetFilePath(prefix + "Game38Accounting.xml");
            try { gameAccountings.Add(39, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game39Accounting.xml")); } catch { gameAccountings.Add(39, new GameAccounting()); }
            gameAccountings[39].SetFilePath(prefix + "Game39Accounting.xml");
            try { gameAccountings.Add(40, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game40Accounting.xml")); } catch { gameAccountings.Add(40, new GameAccounting()); }
            gameAccountings[40].SetFilePath(prefix + "Game40Accounting.xml");
            try { gameAccountings.Add(41, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game41Accounting.xml")); } catch { gameAccountings.Add(41, new GameAccounting()); }
            gameAccountings[41].SetFilePath(prefix + "Game41Accounting.xml");
            try { gameAccountings.Add(42, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game42Accounting.xml")); } catch { gameAccountings.Add(42, new GameAccounting()); }
            gameAccountings[42].SetFilePath(prefix + "Game42Accounting.xml");
            try { gameAccountings.Add(43, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game43Accounting.xml")); } catch { gameAccountings.Add(43, new GameAccounting()); }
            gameAccountings[43].SetFilePath(prefix + "Game43Accounting.xml");
            try { gameAccountings.Add(44, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game44Accounting.xml")); } catch { gameAccountings.Add(44, new GameAccounting()); }
            gameAccountings[44].SetFilePath(prefix + "Game44Accounting.xml");
            try { gameAccountings.Add(45, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game45Accounting.xml")); } catch { gameAccountings.Add(45, new GameAccounting()); }
            gameAccountings[45].SetFilePath(prefix + "Game45Accounting.xml");
            try { gameAccountings.Add(46, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game46Accounting.xml")); } catch { gameAccountings.Add(46, new GameAccounting()); }
            gameAccountings[46].SetFilePath(prefix + "Game46Accounting.xml");
            try { gameAccountings.Add(47, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game47Accounting.xml")); } catch { gameAccountings.Add(47, new GameAccounting()); }
            gameAccountings[47].SetFilePath(prefix + "Game47Accounting.xml");
            try { gameAccountings.Add(48, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game48Accounting.xml")); } catch { gameAccountings.Add(48, new GameAccounting()); }
            gameAccountings[48].SetFilePath(prefix + "Game48Accounting.xml");
            try { gameAccountings.Add(49, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game49Accounting.xml")); } catch { gameAccountings.Add(49, new GameAccounting()); }
            gameAccountings[49].SetFilePath(prefix + "Game49Accounting.xml");
            try { gameAccountings.Add(50, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game50Accounting.xml")); } catch { gameAccountings.Add(50, new GameAccounting()); }
            gameAccountings[50].SetFilePath(prefix + "Game50Accounting.xml");
            try { gameAccountings.Add(51, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game51Accounting.xml")); } catch { gameAccountings.Add(51, new GameAccounting()); }
            gameAccountings[51].SetFilePath(prefix + "Game51Accounting.xml");
            try { gameAccountings.Add(52, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game52Accounting.xml")); } catch { gameAccountings.Add(52, new GameAccounting()); }
            gameAccountings[52].SetFilePath(prefix + "Game52Accounting.xml");
            try { gameAccountings.Add(53, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game53Accounting.xml")); } catch { gameAccountings.Add(53, new GameAccounting()); }
            gameAccountings[53].SetFilePath(prefix + "Game53Accounting.xml");
            try { gameAccountings.Add(54, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game54Accounting.xml")); } catch { gameAccountings.Add(54, new GameAccounting()); }
            gameAccountings[54].SetFilePath(prefix + "Game54Accounting.xml");
            try { gameAccountings.Add(55, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game55Accounting.xml")); } catch { gameAccountings.Add(55, new GameAccounting()); }
            gameAccountings[55].SetFilePath(prefix + "Game55Accounting.xml");
            try { gameAccountings.Add(56, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game56Accounting.xml")); } catch { gameAccountings.Add(56, new GameAccounting()); }
            gameAccountings[56].SetFilePath(prefix + "Game56Accounting.xml");
            try { gameAccountings.Add(57, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game57Accounting.xml")); } catch { gameAccountings.Add(57, new GameAccounting()); }
            gameAccountings[57].SetFilePath(prefix + "Game57Accounting.xml");
            try { gameAccountings.Add(58, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game58Accounting.xml")); } catch { gameAccountings.Add(58, new GameAccounting()); }
            gameAccountings[58].SetFilePath(prefix + "Game58Accounting.xml");
            try { gameAccountings.Add(59, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game59Accounting.xml")); } catch { gameAccountings.Add(59, new GameAccounting()); }
            gameAccountings[59].SetFilePath(prefix + "Game59Accounting.xml");
            try { gameAccountings.Add(60, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game60Accounting.xml")); } catch { gameAccountings.Add(60, new GameAccounting()); }
            gameAccountings[60].SetFilePath(prefix + "Game60Accounting.xml");
            try { gameAccountings.Add(61, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game61Accounting.xml")); } catch { gameAccountings.Add(61, new GameAccounting()); }
            gameAccountings[61].SetFilePath(prefix + "Game61Accounting.xml");
            try { gameAccountings.Add(62, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game62Accounting.xml")); } catch { gameAccountings.Add(62, new GameAccounting()); }
            gameAccountings[62].SetFilePath(prefix + "Game62Accounting.xml");
            try { gameAccountings.Add(63, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game63Accounting.xml")); } catch { gameAccountings.Add(63, new GameAccounting()); }
            gameAccountings[63].SetFilePath(prefix + "Game63Accounting.xml");
            try { gameAccountings.Add(64, xmlfile_serializer.Deserialize<GameAccounting>(prefix + "Game64Accounting.xml")); } catch { gameAccountings.Add(64, new GameAccounting()); }
            gameAccountings[64].SetFilePath(prefix + "Game64Accounting.xml");



        }


        /// <summary>
        /// Función que mapea gameNumber con gameAccounting
        ///  Function that maps gameNumber with gameAccounting
        /// </summary>
        /// <param name="gameNumber"> Takes a gameNumber, as byte array</param>
        /// <returns> Returns a gameAccounting</returns>
        public GameAccounting GetGameAccounting(byte[] gameNumber)
        {
            try
            {
                int gn = int.Parse(BitConverter.ToString(gameNumber).Replace("-", ""));
                if (gameAccountings.Keys.Contains(gn))
                    return gameAccountings[gn];
                else
                    return gm;
            }
            catch
            {
                return gm;
            }
        }

        // Reseteo de meters
        // Meters reset
        public void ResetMeters()
        {
            for (int i = 0; i < 65; i++)
            {
                byte[] gn = intToBCD5((uint)i, 2);
                GameAccounting gameAccounting = GetGameAccounting(gn);
                gameAccounting.ResetMeters();
                /* Guardo el gameAccounting */
                // Save the gameAccounting 
                XmlFileSerializer xmlfile_serializer = new XmlFileSerializer();
                xmlfile_serializer.SaveXml<GameAccounting>(gameAccounting, gameAccounting.GetFilePath());
            }
        }

        /// <summary>
        ///  Rutina que toma un gameAccounting, una meter string, un value y setea el value al code correspondiente
        ///  Hay meters que en lugar de código, al no existir en la lista de codes tienen un nombre representantivo

        ///  Routine that takes a gameAccounting, a meter string and a value and sets the value to the corresponding code
        ///  There are meters that instead of a code, since they do not exist in the list of codes, they have a representative name
        /// </summary>
        /// <param name="gameAccounting">un gameAccounting por referencia </param>
        /// <param name="code"> Una meter string </param>
        /// <param name="value"> El valor</param>
        public void UpdateMeter(byte[] gameNumber, string code, int value)
        {
            int gn = -1;
            try { gn = int.Parse(BitConverter.ToString(gameNumber).Replace("-", "")); } catch { }
            if (gameAccountings.Keys.Contains(gn))
            {
                GameAccounting gameAccounting = GetGameAccounting(gameNumber);
                switch (code)
                {
                    case "PowerReset": // -----------------
                        gameAccounting.Meter_Basic_PowerReset.Value = value;
                        break;
                    case "TotalDrop": // -----------------
                        gameAccounting.Meter_Basic_TotalDrop.Value = value;
                        break;
                    case "SlotDoorOpen": // -----------------
                        gameAccounting.Meter_Basic_SlotDoorOpen.Value = value;
                        break;
                    case "TotalBillsInDollars": // --------------
                        gameAccounting.Meter_TotalBillMeterInDollars.Value = value;
                        break;
                    case "TrueCoinIn": // ---------------
                        gameAccounting.Meter_TrueCoinInMeter.Value = value;
                        break;
                    case "TrueCoinOut": // --------------
                        gameAccounting.Meter_TrueCoinOutMeter.Value = value;
                        break;
                    case "BonusingDeductible": // --------------
                        gameAccounting.Meter_BonusingDeductible.Value = value;
                        break;
                    case "BonusingNoDeductible": // --------------
                        gameAccounting.Meter_BonusingNoDeductible.Value = value;
                        break;
                    case "BonusingWagerMatch": // --------------
                        gameAccounting.Meter_BonusingWagerMatch.Value = value;
                        break;
                }
                /* Save el gameAccounting */
                XmlFileSerializer xmlfile_serializer = new XmlFileSerializer();
                xmlfile_serializer.SaveXml<GameAccounting>(gameAccounting, gameAccounting.GetFilePath());
                gameAccountings[gn] = gameAccounting;
            }
        }

        /// <summary>
        ///  Rutina que toma un gameAccounting, un meter code y un value y setea el value al code correspondiente
        ///  Routine that takes a gameAccounting, a meter code and a value and sets the value to the corresponding code
        /// </summary>
        /// <param name="gameAccounting">a gameAccounting, by reference</param>
        /// <param name="code"> A meter code </param>
        /// <param name="value"> The value </param>
        public void UpdateMeter(byte[] gameNumber, byte code, int value)
        {
            int gn = -1;
            try { gn = int.Parse(BitConverter.ToString(gameNumber).Replace("-", "")); } catch { }
            if (gameAccountings.Keys.Contains(gn))
            {
                GameAccounting gameAccounting = GetGameAccounting(gameNumber);
                switch (code)
                {
                    case 0x00: // Total Coin In
                        gameAccounting.Meter_0000.Value = value;
                        gameAccounting.Meter_Basic_TotalCoinIn.Value = value;
                        break;
                    case 0x01: // Total Coin Out
                        gameAccounting.Meter_0001.Value = value;
                        gameAccounting.Meter_Basic_TotalCoinOut.Value = value;
                        break;
                    case 0x02: // Total Jack Pot
                        gameAccounting.Meter_0002.Value = value;
                        gameAccounting.Meter_Basic_TotalJackPot.Value = value;
                        break;
                    case 0x03: // Total Hand Pay Cancelled credits
                        gameAccounting.Meter_0003.Value = value;
                        break;
                    case 0x04:
                        gameAccounting.Meter_0004.Value = value;
                        break;
                    case 0x05: // Games Played
                        gameAccounting.Meter_0005.Value = value;
                        gameAccounting.Meter_Basic_GamesPlayed.Value = value;
                        break;
                    case 0x06: // Games Won
                        gameAccounting.Meter_0006.Value = value;
                        gameAccounting.Meter_Basic_GamesWon.Value = value;
                        break;
                    case 0x07: // Games Won
                        gameAccounting.Meter_0007.Value = value;
                        break;
                    case 0x0B: // Total credits from bills accepted
                        gameAccounting.Meter_000B.Value = value;
                        break;
                    case 0x0C:
                        gameAccounting.Meter_000C.Value = value;
                        break;
                    case 0x0D: // Total cashable ticket in (cents)
                        gameAccounting.Meter_000D.Value = value;
                        break;
                    case 0x0E: // Total cashable ticket out (cents)
                        gameAccounting.Meter_000E.Value = value;
                        break;
                    case 0x0F: // Total restricted ticket in (cents)
                        gameAccounting.Meter_000F.Value = value;
                        break;
                    case 0x10: // Total restricted ticket out (cents)
                        gameAccounting.Meter_0010.Value = value;
                        break;
                    case 0x11: // Total SAS cashable ticket in, including nonrestricted tickets (quantity)
                        gameAccounting.Meter_0011.Value = value;
                        break;
                    case 0x12: // Total SAS cashable ticket out, including nonrestricted tickets (quantity)
                        gameAccounting.Meter_0012.Value = value;
                        break;
                    case 0x13:
                        gameAccounting.Meter_0013.Value = value;
                        break;
                    case 0x14:
                        gameAccounting.Meter_0014.Value = value;
                        break;
                    case 0x15: // Total tickets in credits
                        gameAccounting.Meter_0015.Value = value;
                        break;
                    case 0x16: // Total tickets out credits
                        gameAccounting.Meter_0016.Value = value;
                        break;
                    case 0x17:
                        gameAccounting.Meter_0017.Value = value;
                        break;
                    case 0x18:
                        gameAccounting.Meter_0018.Value = value;
                        break;
                    case 0x19: // Total restricted amount played (credits)
                        gameAccounting.Meter_0019.Value = value;
                        break;
                    case 0x1A: // Total nonrestricted amount played (credits)
                        gameAccounting.Meter_001A.Value = value;
                        break;
                    case 0x1B: // Total restricted credits 
                        gameAccounting.Meter_001B.Value = value;
                        break;
                    case 0x1C:
                        gameAccounting.Meter_001C.Value = value;
                        break;
                    case 0x1D:
                        gameAccounting.Meter_001D.Value = value;
                        break;
                    case 0x1E: // Total machine paid external bonus win (credits)
                        gameAccounting.Meter_001E.Value = value;
                        break;
                    case 0x1F:
                        gameAccounting.Meter_001F.Value = value;
                        break;
                    case 0x20:
                        gameAccounting.Meter_0020.Value = value;
                        break;
                    case 0x21: // Total attendant paid external bonus win (credits)
                        gameAccounting.Meter_0021.Value = value;
                        break;
                    case 0x22:
                        gameAccounting.Meter_0022.Value = value;
                        break;
                    case 0x23:
                        gameAccounting.Meter_0023.Value = value;
                        break;
                    case 0x24: // Total drops credits
                        gameAccounting.Meter_0024.Value = value;
                        break;
                    case 0x25: // Games since last power up
                        gameAccounting.Meter_0025.Value = value;
                        break;
                    case 0x26: // Games since last door closure
                        gameAccounting.Meter_0026.Value = value;
                        break;
                    case 0x27:
                        gameAccounting.Meter_0027.Value = value;
                        break;
                    case 0x28:  // Total cashable in credits, including non-restricted
                        gameAccounting.Meter_0028.Value = value;
                        break;
                    case 0x29: // Total regular cashable ticket in credits
                        gameAccounting.Meter_0029.Value = value;
                        break;
                    case 0x2A: // Total regular cashable ticket in credits
                        gameAccounting.Meter_002A.Value = value;
                        break;
                    case 0x2B: // Total nonrestricted promotional ticket out credits
                        gameAccounting.Meter_002B.Value = value;
                        break;
                    case 0x2C: // Total cashable ticket out credits
                        gameAccounting.Meter_002C.Value = value;
                        break;
                    case 0x2D: // Total restricted promotional ticket out credits
                        gameAccounting.Meter_002D.Value = value;
                        break;
                    case 0x2E:
                        gameAccounting.Meter_002E.Value = value;
                        break;
                    case 0x2F:
                        gameAccounting.Meter_002F.Value = value;
                        break;
                    case 0x30:
                        gameAccounting.Meter_0030.Value = value;
                        break;
                    case 0x31:
                        gameAccounting.Meter_0031.Value = value;
                        break;
                    case 0x32:
                        gameAccounting.Meter_0032.Value = value;
                        break;
                    case 0x33:
                        gameAccounting.Meter_0033.Value = value;
                        break;
                    case 0x34:
                        gameAccounting.Meter_0034.Value = value;
                        break;
                    case 0x35: // Total regular cashable ticket in count
                        gameAccounting.Meter_0035.Value = value;
                        break;
                    case 0x36: // Total restricted promotional ticket out credits
                        gameAccounting.Meter_0036.Value = value;
                        break;
                    case 0x37: // Total nonrestricted ticket in count
                        gameAccounting.Meter_0037.Value = value;
                        break;
                    case 0x38: // Total cashable out count, including debit ticket
                        gameAccounting.Meter_0038.Value = value;
                        break;
                    case 0x39: // Total restricted promotional ticket out count
                        gameAccounting.Meter_0039.Value = value;
                        break;
                    case 0x3A:
                        gameAccounting.Meter_003A.Value = value;
                        break;
                    case 0x3B:
                        gameAccounting.Meter_003B.Value = value;
                        break;
                    case 0x3C:
                        gameAccounting.Meter_003C.Value = value;
                        break;
                    case 0x3D:
                        gameAccounting.Meter_003D.Value = value;
                        break;
                    case 0x3E: // Number of bills currently in stacker
                        gameAccounting.Meter_003E.Value = value;
                        break;
                    case 0x3F: // Total value of bills currently in stacker (Credits)
                        gameAccounting.Meter_003F.Value = value;
                        break;
                    case 0x40: //
                        gameAccounting.Meter_0040.Value = value;
                        break;
                    case 0x41: // 
                        gameAccounting.Meter_0041.Value = value;
                        break;
                    case 0x42: // 
                        gameAccounting.Meter_0042.Value = value;
                        break;
                    case 0x43: //
                        gameAccounting.Meter_0043.Value = value;
                        break;
                    case 0x44: //
                        gameAccounting.Meter_0044.Value = value;
                        break;
                    case 0x45: //
                        gameAccounting.Meter_0045.Value = value;
                        break;
                    case 0x46: //
                        gameAccounting.Meter_0046.Value = value;
                        break;
                    case 0x47: //
                        gameAccounting.Meter_0047.Value = value;
                        break;
                    case 0x48: //
                        gameAccounting.Meter_0048.Value = value;
                        break;
                    case 0x49: //
                        gameAccounting.Meter_0049.Value = value;
                        break;
                    case 0x4A:
                        gameAccounting.Meter_004A.Value = value;
                        break;
                    case 0x4B:
                        gameAccounting.Meter_004B.Value = value;
                        break;
                    case 0x4C:
                        gameAccounting.Meter_004C.Value = value;
                        break;
                    case 0x4D:
                        gameAccounting.Meter_004D.Value = value;
                        break;
                    case 0x4E:
                        gameAccounting.Meter_004E.Value = value;
                        break;
                    case 0x4F:
                        gameAccounting.Meter_004F.Value = value;
                        break;
                    case 0x50: //
                        gameAccounting.Meter_0050.Value = value;
                        break;
                    case 0x51: //
                        gameAccounting.Meter_0051.Value = value;
                        break;
                    case 0x52: //
                        gameAccounting.Meter_0052.Value = value;
                        break;
                    case 0x53: //
                        gameAccounting.Meter_0053.Value = value;
                        break;
                    case 0x54: //
                        gameAccounting.Meter_0054.Value = value;
                        break;
                    case 0x55: //
                        gameAccounting.Meter_0055.Value = value;
                        break;
                    case 0x56: //
                        gameAccounting.Meter_0056.Value = value;
                        break;
                    case 0x57: //
                        gameAccounting.Meter_0057.Value = value;
                        break;
                    case 0x58: //
                        gameAccounting.Meter_0058.Value = value;
                        break;
                    case 0x59: //
                        gameAccounting.Meter_0059.Value = value;
                        break;
                    case 0x5A:
                        gameAccounting.Meter_005A.Value = value;
                        break;
                    case 0x5B:
                        gameAccounting.Meter_005B.Value = value;
                        break;
                    case 0x5C:
                        gameAccounting.Meter_005C.Value = value;
                        break;
                    case 0x5D: //
                        gameAccounting.Meter_005D.Value = value;
                        break;
                    case 0x5E:
                        gameAccounting.Meter_005E.Value = value;
                        break;
                    case 0x5F:
                        gameAccounting.Meter_005F.Value = value;
                        break;
                    case 0x60:
                        gameAccounting.Meter_0060.Value = value;
                        break;
                    case 0x61:
                        gameAccounting.Meter_0061.Value = value;
                        break;
                    case 0x62:
                        gameAccounting.Meter_0062.Value = value;
                        break;
                    case 0x63:
                        gameAccounting.Meter_0063.Value = value;
                        break;
                    case 0x64:
                        gameAccounting.Meter_0064.Value = value;
                        break;
                    case 0x65:
                        gameAccounting.Meter_0065.Value = value;
                        break;
                    case 0x66:
                        gameAccounting.Meter_0066.Value = value;
                        break;
                    case 0x67:
                        gameAccounting.Meter_0067.Value = value;
                        break;
                    case 0x68:
                        gameAccounting.Meter_0068.Value = value;
                        break;
                    case 0x69:
                        gameAccounting.Meter_0069.Value = value;
                        break;
                    case 0x6A:
                        gameAccounting.Meter_006A.Value = value;
                        break;
                    case 0x6B:
                        gameAccounting.Meter_006B.Value = value;
                        break;
                    case 0x6C:
                        gameAccounting.Meter_006C.Value = value;
                        break;
                    case 0x6D:
                        gameAccounting.Meter_006D.Value = value;
                        break;
                    case 0x6E:
                        gameAccounting.Meter_006E.Value = value;
                        break;
                    case 0x6F:
                        gameAccounting.Meter_006F.Value = value;
                        break;
                    case 0x70: //
                        gameAccounting.Meter_0070.Value = value;
                        break;
                    case 0x71: //
                        gameAccounting.Meter_0071.Value = value;
                        break;
                    case 0x72: //
                        gameAccounting.Meter_0072.Value = value;
                        break;
                    case 0x73: //
                        gameAccounting.Meter_0073.Value = value;
                        break;
                    case 0x74: //
                        gameAccounting.Meter_0074.Value = value;
                        break;
                    case 0x75: //
                        gameAccounting.Meter_0075.Value = value;
                        break;
                    case 0x76: //
                        gameAccounting.Meter_0076.Value = value;
                        break;
                    case 0x77: //
                        gameAccounting.Meter_0077.Value = value;
                        break;
                    case 0x78: //
                        gameAccounting.Meter_0078.Value = value;
                        break;
                    case 0x79: //
                        gameAccounting.Meter_0079.Value = value;
                        break;
                    case 0x7A: //
                        gameAccounting.Meter_007A.Value = value;
                        break;
                    case 0x7B: //
                        gameAccounting.Meter_007B.Value = value;
                        break;
                    case 0x7C: //
                        gameAccounting.Meter_007C.Value = value;
                        break;
                    case 0x7D: //
                        gameAccounting.Meter_007D.Value = value;
                        break;
                    case 0x7E: //
                        gameAccounting.Meter_007E.Value = value;
                        break;
                    case 0x7F: //
                        gameAccounting.Meter_007F.Value = value;
                        break;
                    case 0x80:  // Regular cashable ticket in cents
                        gameAccounting.Meter_0080.Value = value;
                        break;
                    case 0x81:  // Regular cashable ticket in count
                        gameAccounting.Meter_0081.Value = value;
                        break;
                    case 0x82: // Restricted ticket in cent
                        gameAccounting.Meter_0082.Value = value;
                        break;
                    case 0x83:  // Restricted ticket in count
                        gameAccounting.Meter_0083.Value = value;
                        break;
                    case 0x84:  // Nonrestricted ticket in cents
                        gameAccounting.Meter_0084.Value = value;
                        break;
                    case 0x85: // Nonrestricted ticket in count
                        gameAccounting.Meter_0085.Value = value;
                        break;
                    case 0x86: // Regular cashable ticket out cents
                        gameAccounting.Meter_0086.Value = value;
                        break;
                    case 0x87: // Regular cashable ticket out count
                        gameAccounting.Meter_0087.Value = value;
                        break;
                    case 0x88: // Restricted ticket out cents
                        gameAccounting.Meter_0088.Value = value;
                        break;
                    case 0x89: // Restricted ticket out counts
                        gameAccounting.Meter_0089.Value = value;
                        break;
                    case 0x8A: // Debit ticket out cents
                        gameAccounting.Meter_008A.Value = value;
                        break;
                    case 0x8B: // Debit ticket out counts
                        gameAccounting.Meter_008B.Value = value;
                        break;
                    case 0x8C: // Validated cancelled credit handpay, receipt printed cents
                        gameAccounting.Meter_008C.Value = value;
                        break;
                    case 0x8D: // Validated cancelled credit handpay, receipt printed counts
                        gameAccounting.Meter_008D.Value = value;
                        break;
                    case 0x8E: // Validated jackpot handpay, receipt printed cents
                        gameAccounting.Meter_008E.Value = value;
                        break;
                    case 0x8F: // Validated jackpot handpay, receipt printed counts
                        gameAccounting.Meter_008F.Value = value;
                        break;
                    case 0x90: // Validated cancelled credit handpay, no receipt cents
                        gameAccounting.Meter_0090.Value = value;
                        break;
                    case 0x91: //  Validated cancelled credit handpay, no receipt counts
                        gameAccounting.Meter_0091.Value = value;
                        break;
                    case 0x92: // Validated jackpot handpay, no receipt cents
                        gameAccounting.Meter_0092.Value = value;
                        break;
                    case 0x93: // Validated jackpot handpay, no receipt counts
                        gameAccounting.Meter_0093.Value = value;
                        break;
                    case 0xA0:  // AFT in House cashable transfer to gaming machine (cents)
                        gameAccounting.Meter_00A0.Value = value;
                        break;
                    case 0xA1: // AFT in House cashable transfer to gaming machine (quantity)
                        gameAccounting.Meter_00A1.Value = value;
                        break;
                    case 0xA2: // AFT in House restricted transfer to gaming machine cents
                        gameAccounting.Meter_00A2.Value = value;
                        break;
                    case 0xA3: // AFT in House restricted transfer to gaming machine counts
                        gameAccounting.Meter_00A3.Value = value;
                        break;
                    case 0xA4: // AFT in House nonrestricted transfer to gaming machine (cents)
                        gameAccounting.Meter_00A4.Value = value;
                        break;
                    case 0xA5: // AFT in House nonrestricted transfer to gaming machine (quantity)
                        gameAccounting.Meter_00A5.Value = value;
                        break;
                    case 0xA6: // AFT debit transfer to gaming machine (cents)
                        gameAccounting.Meter_00A6.Value = value;
                        break;
                    case 0xA7: // AFT debit transfer to gaming machine (quantity)
                        gameAccounting.Meter_00A7.Value = value;
                        break;
                    case 0xA8: // AFT In House cashable transfer to ticket (cents)
                        gameAccounting.Meter_00A8.Value = value;
                        break;
                    case 0xA9: // AFT In House cashable transfer to ticket (quantity)
                        gameAccounting.Meter_00A9.Value = value;
                        break;
                    case 0xAA: // AFT In House restricted transfer to ticket (cents)
                        gameAccounting.Meter_00AA.Value = value;
                        break;
                    case 0xAB: // AFT In House restricted transfer to ticket (quantity)
                        gameAccounting.Meter_00AB.Value = value;
                        break;
                    case 0xAC: // AFT Debit transfer to ticket (cents)
                        gameAccounting.Meter_00AC.Value = value;
                        break;
                    case 0xAD: // AFT Debit transfer to ticket (quantity)
                        gameAccounting.Meter_00AD.Value = value;
                        break;
                    case 0xAE: // AFT Bonus cashable transfer to gaming machine (cents)
                        gameAccounting.Meter_00AE.Value = value;
                        break;
                    case 0xAF: // AFT Bonus cashable transfer to gaming machine (quantity) 
                        gameAccounting.Meter_00AF.Value = value;
                        break;
                    case 0xB0: // AFT Bonus nonrestricted transfer to gaming machine (cents)
                        gameAccounting.Meter_00B0.Value = value;
                        break;
                    case 0xB1: // AFT Bonus nonrestricted transfer to gaming machine (quantity)
                        gameAccounting.Meter_00B1.Value = value;
                        break;
                    case 0xB8: // AFT In House cashable transfer to host (cents)
                        gameAccounting.Meter_00B8.Value = value;
                        break;
                    case 0xB9: // AFT In House cashable transfer to host (quantity)
                        gameAccounting.Meter_00B9.Value = value;
                        break;
                    case 0xBA: // AFT In House restricted transfer to host (cents)
                        gameAccounting.Meter_00BA.Value = value;
                        break;
                    case 0xBB: // AFT In House restricted transfer to host (quantity)
                        gameAccounting.Meter_00BB.Value = value;
                        break;
                    case 0xBC: // AFT In House nonrestricted transfer to host (cents)
                        gameAccounting.Meter_00BC.Value = value;
                        break;
                    case 0xBD: // AFT In House nonrestricted transfer to host (quantity)
                        gameAccounting.Meter_00BD.Value = value;
                        break;
                    case 0xFA: // 
                        gameAccounting.Meter_00FA.Value = value;
                        break;
                    case 0xFB: // 
                        gameAccounting.Meter_00FB.Value = value;
                        break;
                    case 0xFC: //
                        gameAccounting.Meter_00FC.Value = value;
                        break;
                    case 0xFD: //
                        gameAccounting.Meter_00FD.Value = value;
                        break;
                    case 0xFE: //
                        gameAccounting.Meter_00FE.Value = value;
                        break;
                    case 0xFF: //
                        gameAccounting.Meter_00FF.Value = value;
                        break;
                    default:
                        break;
                }
                /* Guardo el gameAccounting */
                XmlFileSerializer xmlfile_serializer = new XmlFileSerializer();
                xmlfile_serializer.SaveXml<GameAccounting>(gameAccounting, gameAccounting.GetFilePath());
                gameAccountings[gn] = gameAccounting;
            }
        }


        /// <summary>
        /// Función que mapea un meter code (en string) y un gameAccounting con el meter correspondiente.
        /// Hay meters que en lugar de código tienen un nombre representantivo. 

        /// Function that maps a meter code (in string) and a gameAccounting with the corresponding meter.
        /// There are meters that instead of a code have a representative name.
        /// </summary>
        /// <param name="meter_string"> A meter string </param>
        /// <param name="gameAccounting"> A gameAccounting </param>
        /// <returns>Returns the value in a byte array or BCD with a fixed size for strings</returns>
        public byte[] GetValueOfMeter(string meter_string, GameAccounting gameAccounting)
        {
            byte[] value = new byte[] { };
            if (meter_string == "PowerReset")
            {
                value = intToBCD5((uint)gameAccounting.Meter_Basic_PowerReset.Value, MinSize(meter_string));
            }
            else if (meter_string == "TotalDrop")
            {
                value = intToBCD5((uint)gameAccounting.Meter_Basic_TotalDrop.Value, MinSize(meter_string));
            }
            else if (meter_string == "SlotDoorOpen")
            {
                value = intToBCD5((uint)gameAccounting.Meter_Basic_SlotDoorOpen.Value, MinSize(meter_string));
            }
            else if (meter_string == "TotalBillsInDollars")
            {
                value = intToBCD5((uint)gameAccounting.Meter_TotalBillMeterInDollars.Value, MinSize(meter_string));
            }
            else if (meter_string == "TrueCoinIn")
            {
                value = intToBCD5((uint)gameAccounting.Meter_TrueCoinInMeter.Value, MinSize(meter_string));
            }
            else if (meter_string == "TrueCoinOut")
            {
                value = intToBCD5((uint)gameAccounting.Meter_TrueCoinOutMeter.Value, MinSize(meter_string));
            }
            else if (meter_string == "BonusingDeductible")
            {
                value = intToBCD5((uint)gameAccounting.Meter_BonusingDeductible.Value, MinSize(meter_string));
            }
            else if (meter_string == "BonusingNoDeductible")
            {
                value = intToBCD5((uint)gameAccounting.Meter_BonusingNoDeductible.Value, MinSize(meter_string));
            }
            else if (meter_string == "BonusingWagerMatch")
            {
                value = intToBCD5((uint)gameAccounting.Meter_BonusingWagerMatch.Value, MinSize(meter_string));
            }

            return value;

        }

        //
        /// <summary>
        ///  Función que mapea un meterCode (original), un gameAccounting y devuelve el meter correspondiente
        ///  Function that maps an (original) meterCode, a gameAccounting and returns the corresponding meter
        /// </summary>
        /// <param name="requestedMeterCodeOriginal"> The original, a byte array. It is clarified that it is original because in the function the array is reversed </param>
        /// <param name="gameAccounting"> The game Accounting </param>
        /// <param name="buildMeterResponse"> The boolean that determines if the value comes in the structure code + length + value if true
        /// or if it comes only with the value if it is false</param>
        /// <returns></returns>
        public byte[] GetValueOfMeter(byte[] requestedMeterCodeOriginal, GameAccounting gameAccounting, bool buildMeterResponse)
        {
            byte[] meters = new byte[] { };
            byte[] value = new byte[] { };


            byte[] requestedMeterCode = requestedMeterCodeOriginal.Reverse().ToArray();
            if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x00 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_Basic_TotalCoinIn.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x01 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_Basic_TotalCoinOut.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x02 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_Basic_TotalJackPot.Value, MinSize(requestedMeterCode));

            }
            // AFT In House nonrestricted transfer to host (quantity)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x03 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0003.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x04 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0004.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x05 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_Basic_GamesPlayed.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x06 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_Basic_GamesWon.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x07 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0007.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x0B }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_000B.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x0C }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_000C.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x0D }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_000D.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x0E }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_000E.Value, MinSize(requestedMeterCode));

            } // Total cashable ticket out (cents)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x0F }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_000F.Value, MinSize(requestedMeterCode));

            } // Total restricted ticket in (cents)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x10 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0010.Value, MinSize(requestedMeterCode));

            } // Total restricted ticket out (cents)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x11 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0011.Value, MinSize(requestedMeterCode));

            } // Total SAS cashable ticket in, including nonrestricted tickets (quantity)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x12 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0012.Value, MinSize(requestedMeterCode));

            } // Total SAS cashable ticket out, including nonrestricted tickets (quantity)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x13 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0013.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x14 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0014.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x15 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0015.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x16 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0016.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x17 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0017.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x18 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0018.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x19 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0019.Value, MinSize(requestedMeterCode));

            } // Total restricted amount played (credits)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x1A }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_001A.Value, MinSize(requestedMeterCode));

            } // Total nonrestricted amount played (credits)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x1B }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_001B.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x1C }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_001C.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x1D }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_001D.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x1E }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_001E.Value, MinSize(requestedMeterCode));

            } // Total machine paid external bonus win (credits)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x1F }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_001F.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x20 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0020.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x21 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0021.Value, MinSize(requestedMeterCode));

            } // Total attendant paid external bonus win (credits)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x22 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0022.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x23 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0023.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x24 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0024.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x25 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0025.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x26 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0026.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x27 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0027.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x28 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0028.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x29 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0029.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x2A }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_002A.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x2B }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_002B.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x2C }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_002C.Value, MinSize(requestedMeterCode));

            } // Total cashable ticket out credits
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x2D }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_002D.Value, MinSize(requestedMeterCode));

            } // Total restricted promotional ticket out credits
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x2E }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_002E.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x2F }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_002F.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x30 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0030.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x31 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0031.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x32 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0032.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x33 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0033.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x34 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0034.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x35 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0035.Value, MinSize(requestedMeterCode));

            } // Total regular cashable ticket in count
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x36 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0036.Value, MinSize(requestedMeterCode));

            } // Total restricted promotional ticket out credits
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x37 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0037.Value, MinSize(requestedMeterCode));

            } // Total nonrestricted ticket in count
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x38 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0038.Value, MinSize(requestedMeterCode));

            } // Total cashable out count, including debit ticket
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x39 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0039.Value, MinSize(requestedMeterCode));

            } // Total restricted promotional ticket out count
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x3A }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_003B.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x3B }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_003B.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x3C }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_003C.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x3D }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_003D.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x3E }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_003E.Value, MinSize(requestedMeterCode));

            } // Number of bills currently in stacker
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x3F }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_003F.Value, MinSize(requestedMeterCode));

            } // Total value of bills currently in stacker (Credits)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x40 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0040.Value, MinSize(requestedMeterCode));
            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x41 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0041.Value, MinSize(requestedMeterCode));
            } // 
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x42 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0042.Value, MinSize(requestedMeterCode));
            } // 
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x43 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0043.Value, MinSize(requestedMeterCode));
            } //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x44 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0044.Value, MinSize(requestedMeterCode));
            }  //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x45 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0045.Value, MinSize(requestedMeterCode));
            }  //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x46 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0046.Value, MinSize(requestedMeterCode));
            }  //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x47 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0047.Value, MinSize(requestedMeterCode));
            }  //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x48 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0048.Value, MinSize(requestedMeterCode));
            }  //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x49 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0049.Value, MinSize(requestedMeterCode));
            }  //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x4A }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_004A.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x4B }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_004B.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x4C }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_004C.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x4D }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_004D.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x4E }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_004E.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x4F }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_004F.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x50 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0050.Value, MinSize(requestedMeterCode));
            }  //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x51 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0051.Value, MinSize(requestedMeterCode));
            }  //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x52 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0052.Value, MinSize(requestedMeterCode));
            }  //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x53 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0053.Value, MinSize(requestedMeterCode));
            }  //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x54 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0054.Value, MinSize(requestedMeterCode));
            }  //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x55 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0055.Value, MinSize(requestedMeterCode));
            }  //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x56 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0056.Value, MinSize(requestedMeterCode));
            }  //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x57 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0057.Value, MinSize(requestedMeterCode));
            }  //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x58 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0058.Value, MinSize(requestedMeterCode));
            }  //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x59 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0059.Value, MinSize(requestedMeterCode));
            }  //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x5A }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_005A.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x5B }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_005B.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x5C }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_005C.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x5D }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_005D.Value, MinSize(requestedMeterCode));
            }  //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x5E }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_005E.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x5F }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_005F.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x60 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0060.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x61 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0061.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x62 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0062.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x63 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0063.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x64 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0064.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x65 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0065.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x66 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0066.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x67 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0067.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x68 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0068.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x69 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0069.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x6A }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_006A.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x6B }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_006B.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x6C }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_006C.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x6D }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_006D.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x6E }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_006E.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x6F }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_006F.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x70 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0070.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x71 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0071.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x72 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0072.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x73 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0073.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x74 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0074.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x75 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0075.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x76 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0076.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x77 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0077.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x78 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0078.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x79 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0079.Value, MinSize(requestedMeterCode));
            }  //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x7A }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_007A.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x7B }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_007B.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x7C }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_007C.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x7D }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_007D.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x7E }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_007E.Value, MinSize(requestedMeterCode));

            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x7F }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_007F.Value, MinSize(requestedMeterCode));
            }  //
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x80 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0080.Value, MinSize(requestedMeterCode));
            } // Regular cashable ticket in cents
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x81 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0081.Value, MinSize(requestedMeterCode));

            } // Regular cashable ticket in count
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x82 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0082.Value, MinSize(requestedMeterCode));

            } // Restricted ticket in cent
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x83 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0083.Value, MinSize(requestedMeterCode));

            } // Restricted ticket in count
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x84 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0084.Value, MinSize(requestedMeterCode));

            } // Nonrestricted ticket in cents
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x85 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0085.Value, MinSize(requestedMeterCode));

            } // Nonrestricted ticket in count
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x86 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0086.Value, MinSize(requestedMeterCode));

            } // Regular cashable ticket out cents
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x87 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0087.Value, MinSize(requestedMeterCode));

            } // Regular cashable ticket out count
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x88 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0088.Value, MinSize(requestedMeterCode));

            } // Restricted ticket out cents
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x89 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0089.Value, MinSize(requestedMeterCode));

            } // Restricted ticket out counts
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x8A }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_008A.Value, MinSize(requestedMeterCode));

            } // Debit ticket out cents
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x8B }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_008B.Value, MinSize(requestedMeterCode));

            } // Debit ticket out counts
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x8C }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_008C.Value, MinSize(requestedMeterCode));

            } // Validated cancelled credit handpay, receipt printed cents
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x8D }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_008D.Value, MinSize(requestedMeterCode));

            } // Validated cancelled credit handpay, receipt printed counts
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x8E }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_008E.Value, MinSize(requestedMeterCode));

            } // Validated jackpot handpay, receipt printed cents
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x8F }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_008F.Value, MinSize(requestedMeterCode));

            } // Validated jackpot handpay, receipt printed counts
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x90 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0090.Value, MinSize(requestedMeterCode));

            } // Validated cancelled credit handpay, no receipt cents
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x91 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0091.Value, MinSize(requestedMeterCode));

            } //  Validated cancelled credit handpay, no receipt counts
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x92 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0092.Value, MinSize(requestedMeterCode));

            } // Validated jackpot handpay, no receipt cents
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0x93 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_0093.Value, MinSize(requestedMeterCode));

            } // Validated jackpot handpay, no receipt counts
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xA0 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00A0.Value, MinSize(requestedMeterCode));

            } // AFT in House cashable transfer to gaming machine (cents)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xA1 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00A1.Value, MinSize(requestedMeterCode));

            } // AFT in House cashable transfer to gaming machine (quantity)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xA2 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00A2.Value, MinSize(requestedMeterCode));

            } // AFT in House restricted transfer to gaming machine cents
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xA3 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00A3.Value, MinSize(requestedMeterCode));

            } // AFT in House restricted transfer to gaming machine counts
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xA4 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00A4.Value, MinSize(requestedMeterCode));

            } // AFT in House nonrestricted transfer to gaming machine (cents)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xA5 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00A5.Value, MinSize(requestedMeterCode));

            } // AFT in House nonrestricted transfer to gaming machine (quantity)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xA6 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00A6.Value, MinSize(requestedMeterCode));

            } // AFT debit transfer to gaming machine (cents)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xA7 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00A7.Value, MinSize(requestedMeterCode));

            } // AFT debit transfer to gaming machine (quantity)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xA8 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00A8.Value, MinSize(requestedMeterCode));

            } // AFT In House cashable transfer to ticket (cents)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xA9 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00A9.Value, MinSize(requestedMeterCode));

            } // AFT In House cashable transfer to ticket (quantity)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xAA }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00AA.Value, MinSize(requestedMeterCode));

            } // AFT In House restricted transfer to ticket (cents)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xAB }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00AB.Value, MinSize(requestedMeterCode));

            } // AFT In House restricted transfer to ticket (quantity)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xAC }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00AC.Value, MinSize(requestedMeterCode));

            } // AFT Debit transfer to ticket (cents)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xAD }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00AD.Value, MinSize(requestedMeterCode));

            } // AFT Debit transfer to ticket (quantity)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xAE }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00AE.Value, MinSize(requestedMeterCode));

            } // AFT Bonus cashable transfer to gaming machine (cents)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xAF }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00AF.Value, MinSize(requestedMeterCode));

            } // AFT Bonus cashable transfer to gaming machine (quantity) 
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xB0 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00B0.Value, MinSize(requestedMeterCode));

            } // AFT Bonus nonrestricted transfer to gaming machine (cents)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xB1 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00B1.Value, MinSize(requestedMeterCode));

            } // AFT Bonus nonrestricted transfer to gaming machine (quantity)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xB8 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00B8.Value, MinSize(requestedMeterCode));

            } // AFT In House cashable transfer to host (cents)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xB9 }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00B9.Value, MinSize(requestedMeterCode));

            } // AFT In House cashable transfer to host (quantity)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xBA }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00BA.Value, MinSize(requestedMeterCode));

            } // AFT In House restricted transfer to host (cents)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xBB }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00BB.Value, MinSize(requestedMeterCode));

            } // AFT In House restricted transfer to host (quantity)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xBC }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00BC.Value, MinSize(requestedMeterCode));

            } // AFT In House nonrestricted transfer to host (cents)
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xBD }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00BD.Value, MinSize(requestedMeterCode));
            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xFA }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00FA.Value, MinSize(requestedMeterCode));
            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xFB }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00FB.Value, MinSize(requestedMeterCode));
            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xFC }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00FC.Value, MinSize(requestedMeterCode));
            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xFD }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00FD.Value, MinSize(requestedMeterCode));
            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xFE }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00FE.Value, MinSize(requestedMeterCode));
            }
            else if (requestedMeterCode.SequenceEqual(new byte[] { 0x00, 0xFF }))
            {
                value = intToBCD5((uint)gameAccounting.Meter_00FF.Value, MinSize(requestedMeterCode));
            }
            else
            {
                value = intToBCD5((uint)0, MinSize(requestedMeterCode));

            }
            if (buildMeterResponse)
            {
                meters = meters.ToList().Concat(requestedMeterCodeOriginal.ToList()
                                        .Concat(new byte[] { (byte)value.Length }).ToList()
                                        .Concat(value).ToList()).ToArray();
                return meters;
            }
            else
                return value;
        }

    }
}

