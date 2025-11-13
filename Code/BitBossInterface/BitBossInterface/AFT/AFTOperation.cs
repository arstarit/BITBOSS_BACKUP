using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Serialization;

namespace BitbossInterface
{
        // The enumeration AFTOperationStatus. Created -> Finished -> Acknowledged
    public enum AFTOperationStatus
    {
        OpCreated,
        OpFinished,
        OpAcknowledged
    }

    /// <summary>
    /// The class AFTOperation
    /// </summary>
    [XmlType]
    public class AFTOperation
    {
        [XmlAttribute]
        public byte InternalStatus; //
        [XmlAttribute]
        public byte ReceiptStatus; //
        [XmlAttribute]
        public byte[] TransactionID; //
        [XmlAttribute]
        public byte[] Amount; // 
        [XmlAttribute]
        public byte[] RestrictedAmount; // 
        [XmlAttribute]  
        public byte[] NonRestrictedAmount; // 
        [XmlAttribute] 
        public byte transferCode;
        [XmlAttribute]
        public byte transferFlags; //
        [XmlAttribute]
        public byte TransferType; // 
        [XmlAttribute] 
        public byte[] expiration;
        [XmlAttribute]
        public byte[] poolID;
        [XmlAttribute]
        public byte Position;
        [XmlAttribute]
        public DateTime TransactionDate;
        [XmlAttribute]
        public AFTOperationStatus AckStatus;

        public AFTOperation()
        {
            AckStatus = AFTOperationStatus.OpCreated;
        }

    }
   
}
