using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Xml.Serialization;
using System.Linq;

namespace BitbossInterface
{
    [XmlType]
    public class EGMException
    {
        [XmlAttribute]
        public byte exception;
        [XmlAttribute]
        public byte[] data;

        public EGMException()
        {

        }
    }

    [XmlType]
    public class EGMTransferLog
    {
        [XmlAttribute]
        public byte commandID;
        [XmlAttribute]
        public byte transactionID;
        [XmlAttribute]
        public byte ackFlag;
        [XmlAttribute]
        public byte machineStatus;
        [XmlAttribute]
        public byte[] amount;
        public EGMTransferLog()
        {
            commandID = 0x00;
            transactionID = 0x00;
            ackFlag = 0x00;
            machineStatus = 0x00;
            amount = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 };
        }
    }

    [XmlType]
    public class EGMStatus
    {
        [XmlAttribute]
        public byte regStatus;
        [XmlAttribute]
        public byte[] regKey;
        [XmlAttribute]
        public byte[] regPOSId;
        [XmlAttribute]
        public bool MainDoorOpen;
        [XmlAttribute]
        public bool LogicDoorOpen;
        [XmlAttribute]
        public byte availableTransfers;
        [XmlAttribute]
        public byte[] currentCashableAmount;
        [XmlAttribute]
        public byte[] currentRestrictedAmount;
        [XmlAttribute]
        public byte[] currentNonRestrictedAmount;
        [XmlAttribute]
        public byte gameLockStatus;
        [XmlAttribute]
        public byte hostCashoutStatus;
        [XmlAttribute]
        public byte aftStatus;
        [XmlAttribute]
        public byte[] restrictedExpiration;
        [XmlAttribute]
        public bool BillAcceptorEnabled;
        [XmlAttribute]
        public long EGMTime;
        [XmlAttribute]
        public long EGMLastPolledTime;
        [XmlAttribute]
        public byte[] CurrentGameNumber;
        [XmlAttribute]
        public byte CurrentPlayerDenomination;
        [XmlAttribute]
        public byte NumberOfDenominations;
        [XmlAttribute]
        public byte[] PlayerDenominations;
        [XmlAttribute]
        public byte TokenDenomination;
        [XmlAttribute]
        public byte[] LastBillInformation;
        [XmlAttribute]
        public DateTime LastEGMResponseReceivedAt;
        [XmlElement]
        public AFTOperationCollection aftcollection = new AFTOperationCollection();
        [XmlIgnore]
        public ConcurrentQueue<Handpay> Handpayqueue = new ConcurrentQueue<Handpay>();
        [XmlElement]
        public List<Handpay> HandpayqueueList = new List<Handpay>();
        [XmlIgnore]
        public ConcurrentQueue<EGMException> eventqueue = new ConcurrentQueue<EGMException>();
        [XmlElement]
        public List<EGMException> eventqueueList = new List<EGMException>();
        [XmlIgnore]
        public ConcurrentQueue<EGMException> priorityeventqueue = new ConcurrentQueue<EGMException>();
        [XmlElement]
        public List<EGMException> priorityeventqueueList = new List<EGMException>();

        public EGMStatus()
        {
            // Initializing the values for the EGMStatus class.
            regStatus = 0x80; // Setting the initial value for regStatus.
            regKey = new byte[] { }; // Initializing regKey as an empty byte array.
            regPOSId = new byte[] { }; // Initializing regPOSId as an empty byte array.
            restrictedExpiration = new byte[] { }; // Initializing restrictedExpiration as an empty byte array.
            currentRestrictedAmount = new byte[] { }; // Initializing currentRestrictedAmount as an empty byte array.
            currentNonRestrictedAmount = new byte[] { }; // Initializing currentNonRestrictedAmount as an empty byte array.
            currentCashableAmount = new byte[] { }; // Initializing currentCashableAmount as an empty byte array.
        }

        // This method converts a list to a concurrent queue.
        public ConcurrentQueue<T> ToConcurrentQueue<T>(List<T> list)
        {
            ConcurrentQueue<T> q = new ConcurrentQueue<T>(); // Initializing a new ConcurrentQueue.
            foreach (T t in list)
            {
                q.Enqueue(t); // Adding each item from the list to the ConcurrentQueue.
            }
            return q; // Returning the converted ConcurrentQueue.
        }

        // This method sets the status.
        public void setStatus(byte status)
        {
            regStatus = status; // Setting the regStatus with the provided status value.
        }

        // This method sets the registration key.
        public void setregistrationKey(byte[] regK)
        {
            regKey = regK; // Setting the regKey with the provided byte array regK.
        }

        // This method sets the registration POS Id.
        public void setregistrationPOSId(byte[] regPId)
        {
            regPOSId = regPId; // Setting the regPOSId with the provided byte array regPId.
        }


    }
}
