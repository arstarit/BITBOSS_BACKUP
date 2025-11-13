using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BitbossInterface
{
    // La enumeración TicketValidationStatus: Los estados de una ticket redemption
    // The TicketValidationStatus enumeration: The statuses of a ticket redemption
    public enum TicketValidationStatus
    {
        Sending57,
        ReceivedLP57,
        ResponseToLP57,
        ReceivedLP58,
        ResponseToLP58,
        Printing,
        ReceivedLP4D,
        Completed
    }

    /// <summary>
    /// Representa una transición. Dado un estado status, se lista todos los estados a los cuales se puede transicionar en next_status
    /// It represents a transition. Given a status 'status', all states that can be transitioned to are listed in next_status
    /// </summary>
    public class TicketValidationStatus_ValidTransition
     {
         public TicketValidationStatus status;
         public TicketValidationStatus[] next_status;
     }

    /// <summary>
    ///  Se define la state machine
    ///  We define the state machine
    /// </summary>
    public class SM1
    {
        private static SM1 sm_;
        // Todas las transiciones posibles
        // All possible transitions
        public TicketValidationStatus_ValidTransition[] state_machine = new TicketValidationStatus_ValidTransition[] {
               new TicketValidationStatus_ValidTransition { /* FROM */ status = TicketValidationStatus.Sending57 , 
                                                            /* TO   */ next_status = new TicketValidationStatus[] { TicketValidationStatus.ReceivedLP57 }},
               new TicketValidationStatus_ValidTransition { /* FROM */ status = TicketValidationStatus.ReceivedLP57 , 
                                                            /* TO   */ next_status = new TicketValidationStatus[] { TicketValidationStatus.ResponseToLP57 }},
               new TicketValidationStatus_ValidTransition { /* FROM */ status = TicketValidationStatus.ResponseToLP57 , 
                                                            /* TO   */ next_status = new TicketValidationStatus[] { TicketValidationStatus.ReceivedLP58 }}, 
               new TicketValidationStatus_ValidTransition { /* FROM */ status = TicketValidationStatus.ReceivedLP58 , 
                                                            /* TO   */ next_status = new TicketValidationStatus[] { TicketValidationStatus.ResponseToLP58 }}, 
               new TicketValidationStatus_ValidTransition { /* FROM */ status = TicketValidationStatus.ResponseToLP58 , 
                                                            /* TO   */ next_status = new TicketValidationStatus[] { TicketValidationStatus.Printing }}, 
               new TicketValidationStatus_ValidTransition { /* FROM */ status = TicketValidationStatus.Printing , 
                                                            /* TO   */ next_status = new TicketValidationStatus[] { TicketValidationStatus.ReceivedLP4D }},  
               new TicketValidationStatus_ValidTransition { /* FROM */ status = TicketValidationStatus.ReceivedLP4D , 
                                                            /* TO   */ next_status = new TicketValidationStatus[] { TicketValidationStatus.Completed }},   
        };
        
        protected SM1()
        {

        }

        public static SM1 Instance()
        {
            if (sm_ == null)
            {
                sm_ = new SM1();
            }
            return sm_;
        }
    }


    /// <summary>
    /// La clase que representa a una validation
    /// The class that represents a validation
    /// </summary>
    public class TicketValidation
    {
        public TicketValidationStatus status;
        public DateTime LastTransitionTS;
        public byte cashoutType;
        public uint amount;
        public byte[] validationNumber;
        public byte validationSystemID;
        public byte validationStatus;
        public byte validationType;
        public byte indexNumber;
        public byte[] date;
        public byte[] time;
        public byte[] ticketNumber;
        public byte[] expiration;
        public byte[] poolId;



        /// <summary>
        /// Función de transición.
        /// Transition function
        /// </summary>
        /// <param name="status_">
        ///  El estado al cual se quiere transicionar 
        ///  The state to which you want to transition
        /// </param>
        /// <returns>  Returns true if it transitioned successfully, returns false if it failed to transition. </returns>
        public bool Transition(TicketValidationStatus status_)
        {
            // Obtiene la transición guardada de status, de la SM
            // Gets the saved transition of status, from the SM
            TicketValidationStatus_ValidTransition transition = SM1.Instance().state_machine.Where(t => t.status == status).FirstOrDefault();
            // Si puede transicionar.. retorna true y el status actual es el status nuevo
            // If it can transition.. returns true and the current status is the new status
            if (transition.next_status.Contains(status_))
            {
                    status = status_;
                    LastTransitionTS = DateTime.Now;
                    return true;
            }

            return false;
        }
    }
   
}
