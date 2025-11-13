using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SASComms
{
    /// <summary>
    /// Clase que se encarga del cálculo del CRC
    /// Class that its function is the calc of CRC
    /// </summary>
    public class CyclicalRedundancyCheck
    {
        /// <summary>
        /// La matriz para alojar los valores que se usará para calcular el CRC
        /// Matrix to save all the values which will be used to calc the CRC
        /// </summary>
        readonly ushort[] table = new ushort[256];

        /// <summary>
        /// Instanciar un objeto de tipo CyclicalRedundancyCheck
        /// Instantiates an object of type CyclicalRedundancyCheck
        /// </summary>
        public CyclicalRedundancyCheck()
        {
            //CcittKermit = 0x8408
            ushort polynomial = 0x8408;
            ushort value;
            ushort temp;
            for (ushort i = 0; i < table.Length; ++i)
            {
                value = 0;
                temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort)((value >> 1) ^ polynomial);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                table[i] = value;
            }
        }

        /// <summary>
        /// Función que calcula el CRC (en un número) dado un array de bytes
        /// Function that calcs the CRC (in a number) given a byte array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public ushort GetCRC(params byte[] bytes)
        {
            ushort crc = 0;

            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(crc ^ bytes[i]);
                crc = (ushort)((crc >> 8) ^ table[index]);
            }

            return crc;
        }

        /// <summary>
        /// Función que calcula el CRC (en bytes) dado un array de bytes
        /// Function that computes the CRC (in bytes) given a byte array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public byte[] GetCRCBytes(params byte[] bytes)
        {
            ushort crc = GetCRC(bytes);
            return BitConverter.GetBytes(crc);
        }


        /// <summary>
        /// Función que calcula el CRC (en una string) dado un array de bytes
        /// Function that computes the CRC (in a string) given a byte array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string GetCRCBytesStr(params byte[] bytes)
        {
            return BitConverter.ToString(GetCRCBytes(bytes));
        }

    }
}
