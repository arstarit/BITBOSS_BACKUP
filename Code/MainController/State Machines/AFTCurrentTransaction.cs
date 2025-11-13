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
    // La enumeración AFTCurrentTransactionStatus: Los estados de una transacción actual
    // AFTCurrentTransactionStatus enumeration: The status of a current transaction
    public enum AFTCurrentTransactionStatus
    {
        Created,
        Sent,
        Pending,
        Rejected,
        Interrogated,
        Acknowledged,
        Completed
    }

    // Definición de la clase AFTCurrentTransaction
    // Definition of the class AFTCurrentTransaction
    public class AFTCurrentTransaction
    {
        public byte InternalStatus;
        public byte ReceiptStatus;
        public string TransactionID;
        public int Amount;
        public int RestrictedAmount;
        public byte TransferType;
        public int Position;
        public DateTime TransactionDate;

        // Se modela la máquina de estados con todas sus transiciones
        // The state machine is modeled with all its transitions.
        private ValidTransition<AFTCurrentTransactionStatus>[] state_machine = new ValidTransition<AFTCurrentTransactionStatus>[] {
                new ValidTransition<AFTCurrentTransactionStatus> { /* FROM */ status = AFTCurrentTransactionStatus.Created ,                                                                          
                                                                  /* TO    */ next_status = new AFTCurrentTransactionStatus[] { AFTCurrentTransactionStatus.Sent}},
                new ValidTransition<AFTCurrentTransactionStatus> { /* FROM */ status = AFTCurrentTransactionStatus.Sent ,                                                                          
                                                                  /* TO    */ next_status = new AFTCurrentTransactionStatus[] { AFTCurrentTransactionStatus.Rejected, AFTCurrentTransactionStatus.Pending}},
                new ValidTransition<AFTCurrentTransactionStatus> { /* FROM */ status = AFTCurrentTransactionStatus.Pending ,                                                                          
                                                                  /* TO    */ next_status = new AFTCurrentTransactionStatus[] { AFTCurrentTransactionStatus.Interrogated }},
                new ValidTransition<AFTCurrentTransactionStatus> { /* FROM */ status = AFTCurrentTransactionStatus.Rejected ,                                                                          
                                                                  /* TO    */ next_status = new AFTCurrentTransactionStatus[] { AFTCurrentTransactionStatus.Interrogated }},
                new ValidTransition<AFTCurrentTransactionStatus> { /* FROM */ status = AFTCurrentTransactionStatus.Interrogated ,                                                                          
                                                                   /* TO   */ next_status = new AFTCurrentTransactionStatus[] { AFTCurrentTransactionStatus.Acknowledged }},
                new ValidTransition<AFTCurrentTransactionStatus> { /* FROM */ status = AFTCurrentTransactionStatus.Acknowledged ,                                                                          
                                                                   /* TO   */ next_status = new AFTCurrentTransactionStatus[] { AFTCurrentTransactionStatus.Completed }},
                new ValidTransition<AFTCurrentTransactionStatus> { /* FROM */ status = AFTCurrentTransactionStatus.Completed ,                                                                          
                                                                   /* TO   */ next_status = new AFTCurrentTransactionStatus[] { AFTCurrentTransactionStatus.Created }},

         };
        /* El status de la Transaction */
        /* The status of the Transaction */
        public AFTCurrentTransactionStatus status;
        /* El timestamp de la última transición*/
        /* The timestamp of the last transition*/
        public DateTime LastTransitionTS;
        /*  La instancia del singleton */
        /*  The instance of the singleton */
        private static AFTCurrentTransaction _instance = null;
        protected AFTCurrentTransaction()
        {

        }

        // Función de transición. Retorna true si transicionó bien, retorna false si no pudo transicionar.
        // Transition function. Returns true if it transitioned well, returns false if it failed to transition.
        public bool Transition(AFTCurrentTransactionStatus status_)
        {
            // Obtiene la transición guardada de status, de la SM
            // Gets the saved status transition, from the GI
            ValidTransition<AFTCurrentTransactionStatus> transition = state_machine.Where(t => t.status == status).FirstOrDefault();
            // Si puede transicionar.. retorna true y el status actual es el status nuevo
            // If it can transition... it returns true and the current status is the new status.
            if (transition.next_status.Contains(status_))
            {
                status = status_;
                LastTransitionTS = DateTime.Now;
                SaveData();
                return true;
            }
            else
            {
                return false;
            }
        }

        /* Determina cuando la state machine está en proceso, en algún estado intermedio */
        /* Determines when the state machine is in process, in some intermediate state. */
        public bool WorkInProgress()
        {
            // Si el estado está en uno de estos estados no iniciales
            // If the state is in one of the following non-initial states
            if (_instance.status == AFTCurrentTransactionStatus.Sent
             || _instance.status == AFTCurrentTransactionStatus.Pending
             || _instance.status == AFTCurrentTransactionStatus.Rejected
             || _instance.status == AFTCurrentTransactionStatus.Interrogated
             || _instance.status == AFTCurrentTransactionStatus.Acknowledged)
            {
                return true;
            }
            else
                return false;
        }

        /* Reseteo el state  */
        /* Reset the state  */
        public void ResetState()
        {
            status = AFTCurrentTransactionStatus.Created;
            LastTransitionTS = DateTime.Now;
            SaveData();
        }

        // La instancia del singleton
        // The instance of the singleton
        public static AFTCurrentTransaction Instance()
        {
            // Si aún no se instanció
            // If not yet installed
            if (_instance == null)
            {
                try
                {
                    // Leo de la persistencia en el xml
                    // I Read from the persistence in the xml
                    _instance = XmlFileSerializer.Deserialize<AFTCurrentTransaction>("AFTCurrentTransaction.xml");
                    // Si el estado está en uno de estos estados no iniciales
                    // If the state is in one of the following non-initial states
                    if (_instance.status == AFTCurrentTransactionStatus.Sent)
                    {
                        // Lo fuerzo a Created
                        // I force it to Created
                        _instance.status = AFTCurrentTransactionStatus.Created;
                        SaveData();
                    }
                }
                catch  // No existe el archivo!! // The file does not exist! 
                {
                    // Creo una nueva InterfacedRedemption
                    // I create a new InterfacedRedemption
                    _instance = new AFTCurrentTransaction();
                    _instance.status = AFTCurrentTransactionStatus.Created;
                    // La persisto a través de SaveData
                    // I persist it through SaveData
                    SaveData();

                }


            }
            return _instance;

        }

        // Método público que rompe la instancia
        // Public method that breaks the instance
        public void Destroy()
        {
            _instance = null;
        }

        // Método público para guardar y persistir datos
        // Public method to store and persist data
        public void Update()
        {
            SaveData();
        }
        // Método privado para guardar y persistir datos
        // Private method to store and persist data
        private static void SaveData()
        {
            XmlFileSerializer.SaveXml<AFTCurrentTransaction>(_instance, "AFTCurrentTransaction.xml");
        }

    }
}
