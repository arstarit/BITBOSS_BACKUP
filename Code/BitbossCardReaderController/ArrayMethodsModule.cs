using System;
using System.IO.Ports;
using System.Threading;
using System.Timers;
using System.Linq;

namespace BitbossCardReaderController
{
    public static class ArrayMethodsModule
    {

        #region ArrayMethodsModule
        public static byte[] join(params byte[][] arr)
        {
            byte[] b = new byte[] { };
            foreach (byte[] b_ in arr)
            {
                b = b.ToList().Concat(b_.ToList()).ToArray();
            }
            return b;
        }

        public static byte sum(params byte[] arr)
        {
            byte b = 0x00;
            foreach (byte a in arr)
            {
                b = (byte)((int)b+(int)a);
            }

            return b;
        }
        #endregion
    }
}
