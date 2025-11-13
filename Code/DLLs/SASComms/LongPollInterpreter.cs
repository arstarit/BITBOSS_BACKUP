using System;
using System.Collections.Generic;
using System.Text;

namespace SASComms
{
    /// <summary>
    /// Singleton. Un único objeto que se encarga de interpretar long polls (Camino a ser deprecated debido a SASResponseHandler)
    /// Singleton. The unique object that gives a semantic of long polls. (This is going to be deprecated due SASResponseHandler)
    /// </summary>
    public sealed class LongPollInterpreter
    {

        private LongPollInterpreter()
        {
        }

        private static readonly Lazy<LongPollInterpreter> lazy = new Lazy<LongPollInterpreter>(() => new LongPollInterpreter());
        public static LongPollInterpreter Singleton
        {
            get
            {
                return lazy.Value;
            }
        }

        /// <summary>
        /// Construye un mensaje con la info de toda la respuesta en bytes del 1C
        /// Builds a message with the byte array response info of 1C
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string GetLP1C(byte[] bytes)
        {
            byte[] _totalCoinInMeter = { bytes[2], bytes[3], bytes[4], bytes[5] };
            byte[] _totalCoinOutMeter = { bytes[6], bytes[7], bytes[8], bytes[9] };
            byte[] _totalDropMeter = { bytes[10], bytes[11], bytes[12], bytes[13] };
            byte[] _totalJackpotMeter = { bytes[14], bytes[15], bytes[16], bytes[17] };
            byte[] _gamesPlayedMeter = { bytes[18], bytes[19], bytes[20], bytes[21] };
            byte[] _gamesWonMeter = { bytes[22], bytes[23], bytes[24], bytes[25] };
            byte[] _doorOpenMeter = { bytes[26], bytes[27], bytes[28], bytes[29] };
            byte[] _powerResetMeter = { bytes[30], bytes[31], bytes[32], bytes[33] };

            return string.Format (@"$1C/ Total Coin In Meter   = {0}
                              $1C/ Total Coin Out Meter = {1}
                              $1C/ Total Drop Meter = {2}
                              $1C/ Total Jackpot Meter = {3}
                              $1C/ Games Played Meter = {4}
                              $1C/ Games Won Meter = {5}
                              $1C/ Door Open Meter = {6}
                              $1C/ Power Reset Meter = {7}
", 
                               BitConverter.ToString (_totalCoinInMeter).Replace ("-", ""),
                               BitConverter.ToString(_totalCoinOutMeter).Replace("-", ""),
                               BitConverter.ToString(_totalDropMeter).Replace("-", ""),
                               BitConverter.ToString(_totalJackpotMeter).Replace("-", ""),
                               BitConverter.ToString(_gamesPlayedMeter).Replace("-", ""),
                               BitConverter.ToString(_gamesWonMeter).Replace("-", ""),
                               BitConverter.ToString(_doorOpenMeter).Replace("-", ""),
                               BitConverter.ToString(_powerResetMeter).Replace("-", ""));
        }



        /// <summary>
        /// Dado una exception, retorna en string el nombre de la exception
        /// Given an exception, return as string the name of the exception
        /// </summary>
        /// <param name="exceptionByte"></param>
        /// <returns></returns>
        public string GetException(byte exceptionByte)
        {

            switch (exceptionByte)
            {
                case 0x11:
                    return "$11/ Slot door opened";
                case 0x12:
                    return "$12/ Slot door closed";
                case 0x17:
                    return "$17/ AC power was applied to gaming machine";
                case 0x18:
                    return "$18/ AC power was lost from gaming machine";

                case 0x7E:
                    return "$7E/ Game has started";
                case 0x7F:
                    return "$7F/ Game has ended";

                case 0x69:
                    return "$69/ AFT transfer complete";



                default:
                    return "";
            }
          
        }
    }
}
