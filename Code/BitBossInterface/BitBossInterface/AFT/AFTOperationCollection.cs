using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Serialization;

namespace BitbossInterface
{
    /// <summary>
    /// Class representing a collection of virtual operations
    /// </summary>
    [XmlType]
    public class AFTOperationCollection
    {
        [XmlAttribute]
        public byte aftcurrentindex = 0x00; // AFT Current operation index
        [XmlElement]
        public List<AFTOperation> aftoperations = new List<AFTOperation>(); // List of AFT Operations
        [XmlElement]
        public AFTOperation currentoperation = null; // Current Operation
        [XmlElement]
        public byte maxBufferSize = 0x6f; // Max Buffer Size
        // Constructor
        public AFTOperationCollection()
        {

        }


        // This method updates the internal status of an AFTOperation object with the provided status.
        private void updatestatus(ref AFTOperation t, byte status)
        {
            t.InternalStatus = status; // Updating the InternalStatus property with the provided status.
        }

        // This method updates the acknowledgment status of an AFTOperation object with the provided status.
        private void updateackstatus(ref AFTOperation t, AFTOperationStatus status)
        {
            t.AckStatus = status; // Updating the AckStatus property with the provided status.
        }


        /// <summary>
        /// Update the status for the aft current operation
        /// </summary>
        public void UpdateStatusForCurrentOperation(byte status)
        {
            // Set the status
            updatestatus(ref currentoperation, status);
            // If status is 0x00 (completed) or 0x01, we take the operation as correctly finished
            if (status == 0x00 || status == 0x01)
            {
                //Add Current Operation
                //aftcurrentindex calc
                if (aftcurrentindex == maxBufferSize + 1)
                {
                    currentoperation.Position = 0;
                    aftcurrentindex = 0;
                }
                else
                {
                    aftcurrentindex++;
                    currentoperation.Position = (byte)aftcurrentindex;

                }
                // Stamp currentindex to Position
                currentoperation.Position = (byte)aftcurrentindex;
                // Insert the current operation to the collection
                aftoperations = insertTransaction(currentoperation, aftoperations);
            }
        }

        /// <summary>
        /// Update the acknoledgment status for the current operation
        /// </summary>
        public void UpdateAckStatusForCurrentOperation(AFTOperationStatus status)
        {
            updateackstatus(ref currentoperation, status);
        }


        /// <summary>
        /// Get the operation of specific index
        /// </summary>
        public AFTOperation GetOperationForIndex(byte index)
        {
            AFTOperation op = aftoperations.Where(o => o.Position == index).FirstOrDefault();
            return op;
        }


        /// <summary>
        /// Adds a transaction to transaction queue with a completed info. 
        /// Generally it is used when a lp72 response from EGM arrives to Host and processed by MainController
        /// </summary>
        public void AddTransaction(byte status, byte? receiptstatus, byte[] transactionId, byte[] cashableAmount, byte[] restrictedAmount, byte[] nonRestrictedAmount, byte? transferFlags, byte? transferType, byte[] expiration, byte[] poolId, byte? position, DateTime? transactionDate)
        {
            AFTOperation op = new AFTOperation();
            // If status is 0x00 (completed) or 0x01, we take the operation as correctly finished
            if (status == 0x00 || status == 0x01)
            {
                // We set all info to the operation attributes
                op.InternalStatus = status;
                op.ReceiptStatus = receiptstatus.Value;
                op.TransactionID = transactionId;
                op.Amount = cashableAmount;
                op.RestrictedAmount = restrictedAmount;
                op.NonRestrictedAmount = nonRestrictedAmount;
                op.transferFlags = transferFlags.Value;
                op.expiration = expiration;
                op.poolID = poolId;
                op.Position = position.Value;
                op.TransactionDate = transactionDate.Value;
                op.TransferType = transferType.Value;
                aftcurrentindex = op.Position;
                // Insert the operation to the collection
                aftoperations = insertTransaction(op, aftoperations);
            }
        }

        /// <summary>
        /// Inserts the transaction.
        /// </summary>
        /// <returns>The transaction.</returns>
        /// <param name="t">T.</param>
        /// <param name="collection">Collection.</param>
        private static List<AFTOperation> insertTransaction(AFTOperation t, List<AFTOperation> collection)
        {
            // Pattern Matching (Insert(t, []))

            // return [t]
            if (collection.Count == 0)
            {
                collection.Add(t);
                return collection;
            }
            else
            {
                // Pattern Matching (Insert(t, (x:xs)))

                AFTOperation first = collection.FirstOrDefault();
                // t == x -> return t:xs
                if (t.Position == first.Position)
                {
                    List<AFTOperation> singleton = new List<AFTOperation>();
                    singleton.Add(t);
                    collection = singleton.Concat(collection.Skip(1).ToList()).ToList();
                    return collection;

                }
                // t < x -> return t:x:xs
                else if (t.Position < first.Position)
                {
                    List<AFTOperation> singleton = new List<AFTOperation>();
                    singleton.Add(t);
                    collection = singleton.Concat(collection).ToList();
                    return collection;
                }
                // t > x -> return x:Insert(t,xs)
                else
                {
                    List<AFTOperation> singleton = new List<AFTOperation>();
                    singleton.Add(first);
                    collection = singleton.Concat(insertTransaction(t, collection.Skip(1).ToList())).ToList();
                    return collection;
                }
            }

        }

    }

}
