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
    // La enumeración InterfacedValidationStatus: Los estados de una interfaced redemption
    // The InterfacedValidationStatus enumeration: The statuses of an interfaced redemption
    public enum InterfacedValidationStatus
    {
        Idle,
        Phy57Received,
        PhyLP57ResponseReceived,
        PhyLP58ResponseReceived,
        PhyLP4DResponseReceived,
        Phy3DReceived,
        Phy3EReceived,
        Completed
    }

    public class Validation
    {

         public byte validationType;
         public byte indexNumber;
         public byte[] date;
         public byte[] time;
         public byte[] validationNumber;
         public byte[] ticketNumber;
         public byte validationSystemId;
         public byte[] expiration;
         public byte[] poolId;
         public byte[] amount;

         public Validation()
         {

         }
    }





    // Definición de la clase InterfacedValidation
    // Definition of the InterfacedValidation class
    public class InterfacedValidation
     {
         public byte cashoutType;
         public byte[] amount;
         public byte validationType;
         public byte indexNumber;
         public byte[] date;
         public byte[] time;
         public byte[] validationNumber;
         public byte[] ticketNumber;
         public byte validationSystemId;
         public byte[] expiration;
         public byte[] poolId;
         public List<Validation> ValidationArray = new List<Validation>();

        // Se modela la máquina de estados con todas sus transiciones
        // The state machine is modeled with all its transitions.
        private ValidTransition<InterfacedValidationStatus>[] state_machine = new ValidTransition<InterfacedValidationStatus>[] {
                new ValidTransition<InterfacedValidationStatus> { /* FROM */ status = InterfacedValidationStatus.Idle ,                                                                          
                                                                  /* TO */ next_status = new InterfacedValidationStatus[] { InterfacedValidationStatus.Phy57Received, InterfacedValidationStatus.Phy3DReceived, InterfacedValidationStatus.Phy3EReceived}},

                new ValidTransition<InterfacedValidationStatus> { /* FROM */ status = InterfacedValidationStatus.Phy57Received ,                                                                
                                                                  /* TO */ next_status = new InterfacedValidationStatus[] { InterfacedValidationStatus.Phy57Received, InterfacedValidationStatus.PhyLP57ResponseReceived}},

                new ValidTransition<InterfacedValidationStatus> { /* FROM */ status = InterfacedValidationStatus.PhyLP57ResponseReceived ,                                                               
                                                                  /* TO */ next_status = new InterfacedValidationStatus[] { InterfacedValidationStatus.PhyLP58ResponseReceived}},

                new ValidTransition<InterfacedValidationStatus> { /* FROM */ status = InterfacedValidationStatus.PhyLP58ResponseReceived ,                                                               
                                                                  /* TO */ next_status = new InterfacedValidationStatus[] { InterfacedValidationStatus.Phy3DReceived}},

                new ValidTransition<InterfacedValidationStatus> { /* FROM */ status = InterfacedValidationStatus.Phy3DReceived ,                                                               
                                                                  /* TO */ next_status = new InterfacedValidationStatus[] { InterfacedValidationStatus.Phy3DReceived, InterfacedValidationStatus.PhyLP4DResponseReceived}},   

                new ValidTransition<InterfacedValidationStatus> { /* FROM */ status = InterfacedValidationStatus.Phy3EReceived ,                                                               
                                                                  /* TO */ next_status = new InterfacedValidationStatus[] { InterfacedValidationStatus.Phy3EReceived, InterfacedValidationStatus.PhyLP4DResponseReceived}},  

                new ValidTransition<InterfacedValidationStatus> { /* FROM */ status = InterfacedValidationStatus.PhyLP4DResponseReceived ,                                                               
                                                                  /* TO */ next_status = new InterfacedValidationStatus[] { InterfacedValidationStatus.Completed}},   
                                                 
                new ValidTransition<InterfacedValidationStatus> { /* FROM */ status = InterfacedValidationStatus.Completed ,                                                               
                                                                  /* TO */  next_status = new InterfacedValidationStatus[] { InterfacedValidationStatus.Idle }},

         };

        /* El status de la Validation */
        /* The status of the Validation */
        public InterfacedValidationStatus status;
        /* El timestamp de la última transición */
        /* The timestamp of the last transition */
        public DateTime LastTransitionTS;
        /* La exception a hacer relay al client. Default 0x3D */
        /* The exception to relay to the client. Default 0x3D */
        public byte exception = 0x3D;
        /*  La instancia del singleton */
        /*  The instance of the singleton */
        private static InterfacedValidation _instance = null;
        protected InterfacedValidation()
        {

        }

        // This method is used to insert a Validation object into the ValidationArray if it doesn't already exist.
        public void InsertValidation(Validation v)
        {
            // Check if the ValidationArray contains a Validation object with the same indexNumber as the given Validation object.
            if (ValidationArray.Where(v_ => v_.indexNumber == v.indexNumber).Count() == 0)
            {
                // If no such Validation object exists, add the given Validation object to the ValidationArray.
                ValidationArray.Add(v);
            }
            else
            {
                // If a Validation object with the same indexNumber exists, replace it with the given Validation object.
                Validation v1 = ValidationArray.Where(v_ => v_.indexNumber == v.indexNumber).FirstOrDefault();
                int idx = ValidationArray.IndexOf(v1);
                ValidationArray.RemoveAt(idx);
                ValidationArray.Add(v);
            }
            // Save the data after the operation is complete.
            SaveData();
        }


        // This method is used to retrieve a Validation object from the ValidationArray based on the provided indexNumber.
        public Validation GetValidation(byte indexNumber)
        {
            // Check if the ValidationArray contains a Validation object with the specified indexNumber.
            if (ValidationArray.Where(v_ => v_.indexNumber == indexNumber).Count() > 0)
            {
                // If a Validation object with the specified indexNumber exists, retrieve and return it.
                Validation v = ValidationArray.Where(v_ => v_.indexNumber == indexNumber).FirstOrDefault();
                return v;
            }
            else
                // If no such Validation object exists, return null.
                return null;
        }



        // Función de transición. Retorna true si transicionó bien, retorna false si no pudo transicionar.
        // Transition function. Returns true if it transitioned well, returns false if it failed to transition.
        public bool Transition(InterfacedValidationStatus status_)
        {
            // Obtiene la transición guardada de status, de la SM
            // Gets the saved status transition, from the GI
            ValidTransition<InterfacedValidationStatus> transition = state_machine.Where(t => t.status == status).FirstOrDefault();
            // Si puede transicionar.. retorna true y el status actual es el status nuevo
            // If it can transition... it returns true and the current status is the new status.
            if (transition.next_status.Contains(status_))
            {
                    status = status_;
                    if (status == InterfacedValidationStatus.Phy3DReceived)
                    {
                        exception = 0x3D;
                    }
                    else if (status == InterfacedValidationStatus.Phy3EReceived)
                    {
                        exception = 0x3E;
                    }
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
            if (_instance.status == InterfacedValidationStatus.Phy57Received
             || _instance.status == InterfacedValidationStatus.PhyLP57ResponseReceived
             || _instance.status == InterfacedValidationStatus.PhyLP58ResponseReceived
             || _instance.status == InterfacedValidationStatus.PhyLP4DResponseReceived
             || _instance.status == InterfacedValidationStatus.Phy3DReceived
             || _instance.status == InterfacedValidationStatus.Phy3EReceived)
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
            status = InterfacedValidationStatus.Idle;
            SaveData();
        }

        // La instancia del singleton
        // The instance of the singleton
        public static InterfacedValidation Instance()
        {
            // Si aún no se instanció
            // If not yet installed
            if (_instance == null)
            {
                  try
                {
                    // Leo de la persistencia en el xml
                    // I read the persistence in the xml
                    _instance =  XmlFileSerializer.Deserialize<InterfacedValidation>("InterfacedValidation.xml");

                    // Si el estado está en uno de estos estados no iniciales
                    // If the state is in one of the following non-initial states
                    if (_instance.status >= InterfacedValidationStatus.Phy57Received && _instance.status <=  InterfacedValidationStatus.Completed)
                    {
                        // Lo fuerzo a Idle
                        // I force it to Idle
                        _instance.status =  InterfacedValidationStatus.Idle;
                        SaveData();

                    }

                }
                catch // No existe el archivo!! // The file does not exist! 
                {
                    // Creo una nueva InterfacedValidation
                    // I create a new InterfacedValidation
                    _instance = new InterfacedValidation();
                    // La persisto a través de SaveData
                    // I persist it through SaveData
                    _instance.status = InterfacedValidationStatus.Idle;
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
            XmlFileSerializer.SaveXml<InterfacedValidation>(_instance, "InterfacedValidation.xml");
        }

    }
}
