using System;
using SASComms;
using BitbossInterface;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace MainController
{
    using TransactionCollection = System.Collections.Generic.List<MainController.TransactionLogLine>;

    /// <summary>
    /// Transaction log line.
    /// </summary>
    public class TransactionLogLine
    {
        public string TransferStatus;
        public string ReceiptStatus;
        public string TransferType;
        public int CashableAmount;
        public int RestrictedAmount;
        public string TransactionID;
        public DateTime TransactionDateTime;
        public int Position;
    }
    /// <summary>
    /// Transactions controller. Gestiona las transacciones (transferencias) agregandolos en un histórico con N posiciones fijas
    /// Transactions controller. Manages transactions (transfers) by aggregating them into a history with N fixed positions
    /// </summary>
    public class TransactionsController
    {
        /// <summary>
        /// The transactions.
        /// </summary>
        private static TransactionCollection transactions = new TransactionCollection();

        /// <summary>
        /// Gets the transactions.
        /// </summary>
        /// <returns>The transactions.</returns>
        public TransactionCollection GetTransactions()
        {
            return transactions;
        }

        /// <summary>
        /// Inserts the transaction.
        /// </summary>
        /// <returns>The transaction.</returns>
        /// <param name="t">T.</param>
        /// <param name="collection">Collection.</param>
        private TransactionCollection insertTransaction(TransactionLogLine t, TransactionCollection collection)
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

                TransactionLogLine first = collection.FirstOrDefault();
                // t == x -> return t:xs
                if (t.Position == first.Position)
                {
                    TransactionCollection singleton = new TransactionCollection();
                    singleton.Add(t);
                    collection = singleton.Concat(collection.Skip(1).ToList()).ToList();
                    return collection;

                }
                // t < x -> return t:x:xs
                else if (t.Position < first.Position)
                {
                    TransactionCollection singleton = new TransactionCollection();
                    singleton.Add(t);
                    collection = singleton.Concat(collection).ToList();
                    return collection;
                }
                // t > x -> return x:Insert(t,xs)
                else
                {
                    TransactionCollection singleton = new TransactionCollection();
                    singleton.Add(first);
                    collection = singleton.Concat(insertTransaction(t, collection.Skip(1).ToList())).ToList();
                    return collection;
                }
            }

        }

        /// <summary>
        /// Adds the transaction.
        /// </summary>
        /// <param name="t">T.</param>
        public void AddTransaction(TransactionLogLine t)
        {
            transactions = insertTransaction(t, transactions);
            XmlFileSerializer.SaveXml<TransactionCollection>(transactions, "Transactions.xml");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MainController.TransactionsController"/> class.
        /// </summary>
        public TransactionsController()
        {
            try
            {
                transactions = XmlFileSerializer.Deserialize<TransactionCollection>("Transactions.xml");
            }
            catch
            {
                transactions = new TransactionCollection();
                XmlFileSerializer.SaveXml<TransactionCollection>(transactions, "Transactions.xml");
            }

        }
    }
}
