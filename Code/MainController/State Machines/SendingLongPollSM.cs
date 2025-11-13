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
    // La enumeración InterfacedValidationStatus: Los estados de una interfaced redemption
    // The InterfacedValidationStatus enumeration: The statuses of an interfaced redemption
    public enum SendingLongPollSMStatus
    {
        Idle,
        LongPollSentToHost,
        LongPollSentToEGM,
        LongPollResponseSuccesfull,
        LongPollNoResponse
    }


    // Definición de la clase InterfacedValidation
    // Definition of the InterfacedValidation class
    public class SendingLongPollSM
     {
         public string longpoll;

        // Se modela la máquina de estados con todas sus transiciones
        // The state machine is modeled with all its transitions.
        private ValidTransition<SendingLongPollSMStatus>[] state_machine = new ValidTransition<SendingLongPollSMStatus>[] {
                new ValidTransition<SendingLongPollSMStatus> { /* FROM */ status = SendingLongPollSMStatus.Idle ,                                                                          
                                                               /* TO */ next_status = new SendingLongPollSMStatus[] { SendingLongPollSMStatus.LongPollSentToHost}},

                new ValidTransition<SendingLongPollSMStatus> { /* FROM */ status = SendingLongPollSMStatus.LongPollSentToHost ,                                                                
                                                               /* TO */ next_status = new SendingLongPollSMStatus[] { SendingLongPollSMStatus.LongPollSentToEGM}},

                new ValidTransition<SendingLongPollSMStatus> { /* FROM */ status = SendingLongPollSMStatus.LongPollSentToEGM ,                                                               
                                                               /* TO */ next_status = new SendingLongPollSMStatus[] { SendingLongPollSMStatus.LongPollResponseSuccesfull, SendingLongPollSMStatus.LongPollNoResponse }},

                new ValidTransition<SendingLongPollSMStatus> { /* FROM */ status = SendingLongPollSMStatus.LongPollResponseSuccesfull ,                                                               
                                                               /* TO */ next_status = new SendingLongPollSMStatus[] { SendingLongPollSMStatus.Idle }},

                new ValidTransition<SendingLongPollSMStatus> { /* FROM */ status = SendingLongPollSMStatus.LongPollNoResponse ,                                                               
                                                               /* TO */ next_status = new SendingLongPollSMStatus[] { SendingLongPollSMStatus.Idle }},

         };
        /* El status del envío sincrónico del long poll */
        /* The status of the synchronous sending of the long poll */
        public SendingLongPollSMStatus status;
        /* El timestamp de la última transición*/
        /* The timestamp of the last transition*/
        public DateTime LastTransitionTS;
        /*  La instancia del singleton */
        /*  The instance of the singleton */
        private static SendingLongPollSM _instance = null;
        protected SendingLongPollSM()
        {

        }

        // Función de transición. Retorna true si transicionó bien, retorna false si no pudo transicionar.
        // Transition function. Returns true if it transitioned well, returns false if it failed to transition.
        public bool Transition(SendingLongPollSMStatus status_)
        {
            // Obtiene la transición guardada de status, de la SM
            // Gets the saved status transition, from the GI
            ValidTransition<SendingLongPollSMStatus> transition = state_machine.Where(t => t.status == status).FirstOrDefault();
            // Si puede transicionar.. retorna true y el status actual es el status nuevo
            // If it can transition... it returns true and the current status is the new status.
            if (transition.next_status.Contains(status_))
            {
                    status = status_;
                    LastTransitionTS = DateTime.Now;
                    // SaveData();
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
            if (_instance.status == SendingLongPollSMStatus.LongPollSentToHost
             || _instance.status == SendingLongPollSMStatus.LongPollSentToEGM
             || _instance.status == SendingLongPollSMStatus.LongPollResponseSuccesfull
             || _instance.status == SendingLongPollSMStatus.LongPollNoResponse)
            {
                return true;
            }
            else
                return false;
        }

        /* Reseteo el state  */
        /* Reset state  */
        public void ResetState()
        {
            status = SendingLongPollSMStatus.Idle;
            SaveData();
        }


        // La instancia del singleton
        // The instance of the singleton
        public static SendingLongPollSM Instance()
        {
            // Si aún no se instanció
            // If not yet installed
            if (_instance == null)
            {
                  try
                {
                    // // Leo de la persistencia en el xml
                    // read from the persistence in the xml
                    // _instance =  XmlFileSerializer.Deserialize<InterfacedValidation>("InterfacedValidation.xml");
                    // // If the state is in one of the following non-initial states
                    // if (_instance.status >= InterfacedValidationStatus.Phy57Received && _instance.status <=  InterfacedValidationStatus.Completed)
                    // {
                    //     // Lo fuerzo a Idle // I force it to Idle 
                    //     _instance.status =  InterfacedValidationStatus.Idle;
                    //     SaveData();

                    // }
                    // Creo una nueva InterfacedValidation
                    // I create a new InterfacedValidation
                    _instance = new SendingLongPollSM();
                    // La persisto a través de SaveData
                    // I persist it through SaveData
                    _instance.status = SendingLongPollSMStatus.Idle;

                }
                catch // No existe el archivo!! // The file does not exist! 
                {
                    // Creo una nueva InterfacedValidation
                    // I create a new InterfacedValidation
                    _instance = new SendingLongPollSM();
                    // La persisto a través de SaveData
                    // I persist it through SaveData
                    _instance.status = SendingLongPollSMStatus.Idle;
                    // SaveData();

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
            // XmlFileSerializer.SaveXml<InterfacedValidation>(_instance, "InterfacedValidation.xml");
        }

    }
}
