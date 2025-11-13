using System;
using System.IO.Ports;
using System.Threading;
using System.Timers;
using System.Linq;
using BitbossCardReaderController.Responses;
using System.Collections.Generic;

namespace BitbossCardReaderController
{
    public class ResponseOf<T>
    {
        public T Response;
    }



    /// <summary>
    /// La clase CardReaderController
    /// </summary>
    public class ResponseHandler
    {

            public delegate void ResponseReceivedHandler<T>(T Response, EventArgs e);

            public event ResponseReceivedHandler<GetReaderStatus_Response> GetReaderStatus_Received; 


            /// <summary>
            /// Acceso a índice con handleo de errores: buf[index]
            /// </summary>
            /// <param name="buf"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            private byte? GetByteFromArrayIndex(byte[] buf, int index)
            {
                try
                {
                    return buf[index];
                }
                catch
                {
                    return null;
                }
            }

            /// <summary>
            /// Acceso a índice con handleo de errores: buf[index]
            /// </summary>
            /// <param name="buf"></param>
            /// <param name="start"></param>
            /// <param name="final"></param>
            /// <returns></returns>
            private byte[] GetSubArrayFromIndex(byte[] buf, int start, int final)
            {
                try
                {
                    List<byte> list = new List<byte>();
                    for(int i = start; i < final+1; i++)
                    {
                        list.Add(GetByteFromArrayIndex(buf, i).Value);
                    }
                    return list.ToArray();
               }
                catch
                {
                    return null;
                }
            }


            public ResponseHandler()
            {

            }

            /// <summary>
            /// Método asincrónico que analiza las responses de la EGM (EN ORDEN POR LONG POLL CODE)
            /// </summary>
            /// <param name="response"></param>
            /// <returns></returns>
            public int Analyze(byte[] response)
            {
                /* En el caso de que la response tenga un tamaño mayor a 1..*/
                if (response.Length > 1)
                {
                    /* Lee el primer byte */
                    switch (GetByteFromArrayIndex(response, 1))
                    {
                        /* Response from FF*/
                        case 0x01:
                            GetReaderStatus_Response result = new GetReaderStatus_Response();
                            int start = 3;
                            result.CardReaderStatus = GetByteFromArrayIndex(response, start).Value;
                            if (result.CardReaderStatus == 0x02)
                            {
                                // Card Type
                                result.CardType = GetByteFromArrayIndex(response, start+1).Value;
                                // Track 1
                                result.Track1Status = GetByteFromArrayIndex(response, start+2);
                                result.Track1Len = GetByteFromArrayIndex(response, start+3);
                                result.Track1Data = GetSubArrayFromIndex(response, start+4, start+4+(int)result.Track1Len.Value-1);
                                // Track 2
                                int j = start+4+(int)result.Track1Len.Value;
                                result.Track2Status = GetByteFromArrayIndex(response, j);
                                result.Track2Len = GetByteFromArrayIndex(response, j+1);
                                result.Track2Data = GetSubArrayFromIndex(response, j+2, j+2+(int)result.Track2Len.Value-1);
                            }
                            try { GetReaderStatus_Received(result, null); } catch {}
                            break;                                       
                        default:
                            break;
                    }
                }
                return 0;
            }
    }
}
