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
    // La enumeración InterfacedRedemptionStatus: Los estados de una interfaced redemption
    // The InterfacedRedemptionStatus enumeration: The statuses of an interfaced redemption
    public enum InterfacedRedemptionStatus
    {
        Idle,
        Phy67Received,
        PhyLP70Sent,
        PhyLP70ResponseReceived,
        Vir67Sent,
        VirLP71Received,
        VirLP71ResponsePending,
        PhyLP71Sent,
        PhyLP71ResponsePending,
        Vir68Sent,
        Phy68Received,
        PhyLP71FFSent,
        Rejected,
        Accepted,
        Completed
    }

    // Definición de la clase InterfacedRedemption
    // InterfacedRedemption class definition
    public class InterfacedRedemption
    {
        // Se modela la máquina de estados con todas sus transiciones
        // The state machine is modeled with all its transitions.
        private ValidTransition<InterfacedRedemptionStatus>[] state_machine = new ValidTransition<InterfacedRedemptionStatus>[] {
                new ValidTransition<InterfacedRedemptionStatus> { /* FROM */ status = InterfacedRedemptionStatus.Idle ,                                                                          
                                                                  /* TO */ next_status = new InterfacedRedemptionStatus[] { InterfacedRedemptionStatus.Phy67Received}},

                new ValidTransition<InterfacedRedemptionStatus> { /* FROM */ status = InterfacedRedemptionStatus.Phy67Received ,                                                                
                                                                  /* TO */ next_status = new InterfacedRedemptionStatus[] { InterfacedRedemptionStatus.PhyLP70Sent }},

                new ValidTransition<InterfacedRedemptionStatus> { /* FROM */ status = InterfacedRedemptionStatus.PhyLP70Sent ,                                                                
                                                                  /* TO */ next_status = new InterfacedRedemptionStatus[] { InterfacedRedemptionStatus.PhyLP70ResponseReceived }},

                new ValidTransition<InterfacedRedemptionStatus> { /* FROM */ status = InterfacedRedemptionStatus.PhyLP70ResponseReceived ,                                                                
                                                                  /* TO */ next_status = new InterfacedRedemptionStatus[] { InterfacedRedemptionStatus.Vir67Sent }},

                new ValidTransition<InterfacedRedemptionStatus> { /* FROM */ status = InterfacedRedemptionStatus.Vir67Sent ,                                                               
                                                                  /* TO */ next_status = new InterfacedRedemptionStatus[] { InterfacedRedemptionStatus.VirLP71Received }},

                new ValidTransition<InterfacedRedemptionStatus> { /* FROM */ status = InterfacedRedemptionStatus.VirLP71Received ,                                                               
                                                                  /* TO */ next_status = new InterfacedRedemptionStatus[] { InterfacedRedemptionStatus.VirLP71ResponsePending }},

                new ValidTransition<InterfacedRedemptionStatus> { /* FROM */ status = InterfacedRedemptionStatus.VirLP71ResponsePending ,                                                               
                                                                  /* TO */ next_status = new InterfacedRedemptionStatus[] { InterfacedRedemptionStatus.PhyLP71Sent }},

                new ValidTransition<InterfacedRedemptionStatus> { /* FROM */ status = InterfacedRedemptionStatus.PhyLP71Sent ,                                                               
                                                                  /* TO */ next_status = new InterfacedRedemptionStatus[] { InterfacedRedemptionStatus.PhyLP71ResponsePending, InterfacedRedemptionStatus.Rejected  }},

                new ValidTransition<InterfacedRedemptionStatus> { /* FROM */ status = InterfacedRedemptionStatus.Rejected ,                                                               
                                                                  /* TO */  next_status = new InterfacedRedemptionStatus[] { InterfacedRedemptionStatus.Vir68Sent }},

                new ValidTransition<InterfacedRedemptionStatus> { /* FROM */ status = InterfacedRedemptionStatus.Accepted ,                                                               
                                                                  /* TO */  next_status = new InterfacedRedemptionStatus[] { InterfacedRedemptionStatus.Vir68Sent }},

                new ValidTransition<InterfacedRedemptionStatus> { /* FROM */ status = InterfacedRedemptionStatus.Vir68Sent ,                                                               
                                                                  /* TO */  next_status = new InterfacedRedemptionStatus[] { InterfacedRedemptionStatus.Completed }},

                new ValidTransition<InterfacedRedemptionStatus> { /* FROM */ status = InterfacedRedemptionStatus.Completed ,                                                               
                                                                  /* TO */  next_status = new InterfacedRedemptionStatus[] { InterfacedRedemptionStatus.Idle }},

                new ValidTransition<InterfacedRedemptionStatus> { /* FROM */ status = InterfacedRedemptionStatus.PhyLP71ResponsePending ,                                                               
                                                                  /* TO */ next_status = new InterfacedRedemptionStatus[] { InterfacedRedemptionStatus.Phy68Received}},

                new ValidTransition<InterfacedRedemptionStatus> { /* FROM */ status = InterfacedRedemptionStatus.Phy68Received ,                                                               
                                                                  /* TO */ next_status = new InterfacedRedemptionStatus[] { InterfacedRedemptionStatus.PhyLP71FFSent}},

                new ValidTransition<InterfacedRedemptionStatus> { /* FROM */ status = InterfacedRedemptionStatus.PhyLP71FFSent ,                                                               
                                                                  /* TO */ next_status = new InterfacedRedemptionStatus[] { InterfacedRedemptionStatus.Accepted, InterfacedRedemptionStatus.Rejected }},


         };

        /* El status de la Redemption */
        /* Redemption status */
        public InterfacedRedemptionStatus status;
        /* El timestamp de la última transición*/
        /* The timestamp of the last transition*/
        public DateTime LastTransitionTS;
        /*  La instancia del singleton */
        /*  The instance of the singleton */
        private static InterfacedRedemption _instance = null;
        protected InterfacedRedemption()
        {

        }

        // Función de transición. Retorna true si transicionó bien, retorna false si no pudo transicionar.
        // Transition function. Returns true if it transitioned well, returns false if it failed to transition.
        public bool Transition(InterfacedRedemptionStatus status_)
        {
            // Obtiene la transición guardada de status, de la SM
            // Gets the saved status transition, from the GI
            ValidTransition<InterfacedRedemptionStatus> transition = state_machine.Where(t => t.status == status).FirstOrDefault();
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
            if (_instance.status == InterfacedRedemptionStatus.Phy67Received
             || _instance.status == InterfacedRedemptionStatus.PhyLP70Sent
             || _instance.status == InterfacedRedemptionStatus.PhyLP70ResponseReceived
             || _instance.status == InterfacedRedemptionStatus.Vir67Sent
             || _instance.status == InterfacedRedemptionStatus.VirLP71Received
             || _instance.status == InterfacedRedemptionStatus.VirLP71ResponsePending
             || _instance.status == InterfacedRedemptionStatus.PhyLP71Sent
             || _instance.status == InterfacedRedemptionStatus.PhyLP71ResponsePending
             || _instance.status == InterfacedRedemptionStatus.Vir68Sent
             || _instance.status == InterfacedRedemptionStatus.Phy68Received
             || _instance.status == InterfacedRedemptionStatus.PhyLP71FFSent
             || _instance.status == InterfacedRedemptionStatus.Rejected
             || _instance.status == InterfacedRedemptionStatus.Accepted)
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
            status = InterfacedRedemptionStatus.Idle;
            SaveData();
        }

        // La instancia del singleton
        // The instance of the singleton
        public static InterfacedRedemption Instance()
        {
            // Si aún no se instanció
            // If not yet installed
            if (_instance == null)
            {
                try
                {
                    // Leo de la persistencia en el xml
                    // I read the persistence in the xml
                    _instance = XmlFileSerializer.Deserialize<InterfacedRedemption>("InterfacedRedemption.xml");
                    // Si el estado está en uno de estos estados no iniciales
                    // If the state is in one of the following non-initial states
                    if (_instance.status >= InterfacedRedemptionStatus.Phy67Received && _instance.status <= InterfacedRedemptionStatus.Completed)
                    {
                        // Lo fuerzo a Idle
                        // I force it to Idle
                        _instance.status = InterfacedRedemptionStatus.Idle;
                        SaveData();
                    }
                }
                catch // No existe el archivo!! // The file does not exist! 
                {
                    // Creo una nueva InterfacedRedemption
                    // I create a new InterfacedRedemption
                    _instance = new InterfacedRedemption();
                    _instance.status = InterfacedRedemptionStatus.Idle;
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
        public static void Update()
        {
            SaveData();
        }
        // Método privado para guardar y persistir datos
        // Private method to store and persist data
        private static void SaveData()
        {
            XmlFileSerializer.SaveXml<InterfacedRedemption>(_instance, "InterfacedRedemption.xml");
        }

    }
}
