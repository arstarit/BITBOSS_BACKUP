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
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace MainController
{
    // La enumeración InterfacedAFTStatus: Los estados de una interfaced redemption
    // The InterfacedAFTStatus enumeration: The statuses of an interfaced redemption
    public enum InterfacedAFTStatus
    {
        Idle,
        SmibAFTOperationIncoming,
        HostAFTOperationSent,
        EGMAFTOperationPending,
        EGMAFTOperationRejected,
        EGMAFTOperationCompleted,
        EGMAFTInterrogated,
        ClientException69,
        SmibAFTRequestedInterrogate,
        SmibAFTInterrogateCompleted
    }


    // Definición de la clase InterfacedAFT
    // Definition of the InterfacedAFT class
    public class InterfacedAFT
     {

        // Se modela la máquina de estados con todas sus transiciones
        // The state machine is modeled with all its transitions.
        private ValidTransition<InterfacedAFTStatus>[] state_machine = new ValidTransition<InterfacedAFTStatus>[] {
                new ValidTransition<InterfacedAFTStatus> { /* FROM */ status = InterfacedAFTStatus.Idle ,                                                                          
                                                           /* TO */ next_status = new InterfacedAFTStatus[] { InterfacedAFTStatus.SmibAFTOperationIncoming }},

                new ValidTransition<InterfacedAFTStatus> { /* FROM */ status = InterfacedAFTStatus.SmibAFTOperationIncoming ,                                                                
                                                          /* TO */ next_status = new InterfacedAFTStatus[] { InterfacedAFTStatus.HostAFTOperationSent}},

                new ValidTransition<InterfacedAFTStatus> { /* FROM */ status = InterfacedAFTStatus.HostAFTOperationSent ,                                                                
                                                          /* TO */ next_status = new InterfacedAFTStatus[] { InterfacedAFTStatus.EGMAFTOperationPending, InterfacedAFTStatus.EGMAFTOperationRejected}},

                new ValidTransition<InterfacedAFTStatus> { /* FROM */ status = InterfacedAFTStatus.EGMAFTOperationPending ,                                                                
                                                          /* TO */ next_status = new InterfacedAFTStatus[] { InterfacedAFTStatus.EGMAFTOperationCompleted}},

                new ValidTransition<InterfacedAFTStatus> { /* FROM */ status = InterfacedAFTStatus.EGMAFTOperationCompleted ,                                                                
                                                          /* TO */ next_status = new InterfacedAFTStatus[] { InterfacedAFTStatus.EGMAFTInterrogated}},
                                                        
                new ValidTransition<InterfacedAFTStatus> { /* FROM */ status = InterfacedAFTStatus.EGMAFTInterrogated ,                                                                
                                                          /* TO */ next_status = new InterfacedAFTStatus[] { InterfacedAFTStatus.ClientException69}},
                                                    
                new ValidTransition<InterfacedAFTStatus> { /* FROM */ status = InterfacedAFTStatus.EGMAFTOperationRejected ,                                                                
                                                          /* TO */ next_status = new InterfacedAFTStatus[] { InterfacedAFTStatus.ClientException69 }},

                new ValidTransition<InterfacedAFTStatus> { /* FROM */ status = InterfacedAFTStatus.ClientException69 ,                                                                
                                                          /* TO */ next_status = new InterfacedAFTStatus[] { InterfacedAFTStatus.ClientException69, InterfacedAFTStatus.SmibAFTRequestedInterrogate}},

                new ValidTransition<InterfacedAFTStatus> { /* FROM */ status = InterfacedAFTStatus.SmibAFTRequestedInterrogate ,                                                                
                                                          /* TO */ next_status = new InterfacedAFTStatus[] { InterfacedAFTStatus.SmibAFTInterrogateCompleted }},

                new ValidTransition<InterfacedAFTStatus> { /* FROM */ status = InterfacedAFTStatus.SmibAFTInterrogateCompleted ,                                                                
                                                          /* TO */ next_status = new InterfacedAFTStatus[] { InterfacedAFTStatus.Idle, InterfacedAFTStatus.SmibAFTOperationIncoming }}
         };

        /* El status de la AFT */
        /* The status of the AFT */
        public InterfacedAFTStatus status;
        /* El timestamp de la última transición */
        /* The timestamp of the last transition */
        public DateTime LastTransitionTS;
        /*  La instancia del singleton */
        /*  The instance of the singleton */
        private static InterfacedAFT _instance = null;
        protected InterfacedAFT()
        {

        }


        // Función de transición. Retorna true si transicionó bien, retorna false si no pudo transicionar.
        // Transition function. Returns true if it transitioned well, returns false if it failed to transition.
        public bool Transition(InterfacedAFTStatus status_)
        {
            // Obtiene la transición guardada de status, de la SM
            // Gets the saved status transition, from the GI
            ValidTransition<InterfacedAFTStatus> transition = state_machine.Where(t => t.status == status).FirstOrDefault();
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
            if (_instance.status == InterfacedAFTStatus.SmibAFTOperationIncoming
             || _instance.status == InterfacedAFTStatus.HostAFTOperationSent
             || _instance.status == InterfacedAFTStatus.EGMAFTOperationPending
             || _instance.status == InterfacedAFTStatus.EGMAFTOperationCompleted
             || _instance.status == InterfacedAFTStatus.EGMAFTInterrogated
             || _instance.status == InterfacedAFTStatus.ClientException69
             || _instance.status == InterfacedAFTStatus.SmibAFTRequestedInterrogate)
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
            status = InterfacedAFTStatus.Idle;
            SaveData();
        }

        // La instancia del singleton
        // The instance of the singleton
        public static InterfacedAFT Instance()
        {
            // Si aún no se instanció
            // If not yet installed
            if (_instance == null)
            {
                  try
                {
                    // Leo de la persistencia en el xml
                    // I read the persistence in the xml
                    _instance =  XmlFileSerializer.Deserialize<InterfacedAFT>("InterfacedAFT.xml");

                    // Si el estado está en uno de estos estados no iniciales
                    // If the state is in one of the following non-initial states
                    if (_instance.status >= InterfacedAFTStatus.SmibAFTOperationIncoming && _instance.status <=  InterfacedAFTStatus.SmibAFTInterrogateCompleted)
                    {
                        // Lo fuerzo a Idle
                        // I force it to Idle
                        _instance.status =  InterfacedAFTStatus.Idle;
                        SaveData();

                    }

                }
                catch // No existe el archivo!! // The file does not exist! 
                {
                    // Creo una nueva InterfacedAFT
                    // I create a new InterfacedAFT
                    _instance = new InterfacedAFT();
                    // La persisto a través de SaveData
                    // I persist it through SaveData
                    _instance.status = InterfacedAFTStatus.Idle;
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
            XmlFileSerializer.SaveXml<InterfacedAFT>(_instance, "InterfacedAFT.xml");
        }

    }
}
