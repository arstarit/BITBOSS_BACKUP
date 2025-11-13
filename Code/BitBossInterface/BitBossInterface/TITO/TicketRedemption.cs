using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BitbossInterface
{
    // La enumeración TicketRedemptionsStatus: Los estados de una ticket redemption
    // The TicketRedemptionsStatus enumeration: The statuses of a ticket redemption
    public enum TicketRedemptionStatus
    {
        TicketInserted,
        TimedOut,
        Received70,
        WaitingAcceptance,
        RejectedByHost,
        Stacking,
        Sending68,
        AckByHost
    }

    /// <summary>
    /// Representa una transición. Dado un estado status, se lista todos los estados a los cuales se puede transicionar en next_status
    /// It represents a transition. Given a status 'status', all states that can be transitioned to are listed in next_status
    /// </summary>
    public class TicketRedemptionStatus_ValidTransition
     {
         public TicketRedemptionStatus status;
         public TicketRedemptionStatus[] next_status;
     }

    /// <summary>
    /// Se define la state machine
    /// We define the state machine
    /// </summary>
    public class SM
    {
        private static SM sm_;

        // Todas las transiciones posibles
        // All possible transitions
        public TicketRedemptionStatus_ValidTransition[] state_machine = new TicketRedemptionStatus_ValidTransition[] {
                new TicketRedemptionStatus_ValidTransition { /* FROM */ status = TicketRedemptionStatus.TicketInserted , 
                                                            next_status = 
                                                            /* TO */ new TicketRedemptionStatus[] { TicketRedemptionStatus.Received70, TicketRedemptionStatus.TimedOut}},  
                new TicketRedemptionStatus_ValidTransition { /* FROM */ status = TicketRedemptionStatus.Received70 , 
                                                            next_status = 
                                                            /* TO */ new TicketRedemptionStatus[] { TicketRedemptionStatus.WaitingAcceptance}},
                new TicketRedemptionStatus_ValidTransition { /* FROM */ status = TicketRedemptionStatus.WaitingAcceptance , 
                                                            next_status = 
                                                            /* TO */ new TicketRedemptionStatus[] { TicketRedemptionStatus.RejectedByHost, TicketRedemptionStatus.Stacking}},
                 new TicketRedemptionStatus_ValidTransition { /* FROM */ status = TicketRedemptionStatus.Stacking , 
                                                            next_status = 
                                                            /* TO */ new TicketRedemptionStatus[] { TicketRedemptionStatus.Sending68}},
                new TicketRedemptionStatus_ValidTransition { /* FROM */ status = TicketRedemptionStatus.Sending68 , 
                                                            next_status = 
                                                            /* TO */ new TicketRedemptionStatus[] { TicketRedemptionStatus.AckByHost}},
                new TicketRedemptionStatus_ValidTransition { /* FROM */ status = TicketRedemptionStatus.AckByHost , 
                                                            next_status = 
                                                            /* TO */ new TicketRedemptionStatus[] { }}
            };
        
        protected SM()
        {

        }

        public static SM Instance()
        {
            if (sm_ == null)
            {
                sm_ = new SM();
            }
            return sm_;
        }
    }
   
    /// <summary>
    /// La clase que representa a una redemption
    /// The class that represents a validation
    /// </summary>
    public class TicketRedemption
    {
        public TicketRedemptionStatus status;
        public DateTime LastTransitionTS;
        public byte machineStatus;
        public int amount;
        public byte[] amountBytes;
        public byte parsingCode;
        public byte[] validationData;
        public byte[] restrictedexpiration;
        public byte[] poolId;



        /// <summary>
        /// Función de transición.
        /// Transition function
        /// </summary>
        /// <param name="status_">
        /// El estado al cual se quiere transicionar
        /// The state to which you want to transition </param>
        /// <returns>   Returns true if it transitioned successfully, returns false if it failed to transition.  </returns>
        public bool Transition(TicketRedemptionStatus status_)
        {
            // Obtiene la transición guardada de status, de la SM
            // Gets the saved transition of status, from the SM
            TicketRedemptionStatus_ValidTransition transition = SM.Instance().state_machine.Where(t => t.status == status).FirstOrDefault();
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
