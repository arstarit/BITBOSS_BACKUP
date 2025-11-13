using System;
using System.IO.Ports;
using System.Threading;
using System.Timers;
using System.Linq;

namespace BitbossCardReaderController.Responses
{
    /// <summary>
    /// La clase CardReaderController
    /// </summary>
    public class GetReaderStatus_Response : IEquatable<GetReaderStatus_Response>
    {
        public byte CardReaderStatus;
        public byte? CardType;
        public byte? Track1Status;
        public byte? Track1Len;
        public byte[] Track1Data;
        public byte? Track2Status;
        public byte? Track2Len;
        public byte[] Track2Data;

        public bool Equals(GetReaderStatus_Response other)
        {
            if (this.CardReaderStatus != other.CardReaderStatus) return false;
            if (this.CardType != other.CardType) return false;
            if (this.Track1Status != other.Track1Status) return false;
            if (this.Track1Len != other.Track1Len) return false;
            if (this.Track1Data != other.Track1Data) return false;
            if (this.Track2Status != other.Track2Status) return false;
            if (this.Track2Len != other.Track2Len) return false;
            if (this.Track2Data != other.Track2Data) return false;

            return true;

        }
           
    }
}
