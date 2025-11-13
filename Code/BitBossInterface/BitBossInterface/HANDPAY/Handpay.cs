using System;
using SASComms;
using BitbossInterface;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace BitbossInterface
{
    // La enumeración HandpaySMStatus: Los estados de una interfaced redemption
    // The HandpaySMStatus enumeration: The statuses of a ticket redemption
    public enum HandpaySMStatus
    {
        HandpayOcurred,
        HandpayReset
    }


    /// <summary>
    /// Definición de ValidTransition.     
    /// Representa una transición. Dado un estado status de tipo T, se lista todos los estados a los cuales se puede transicionar en next_statusde tipo T
    /// Definition of ValidTransition.
    /// Represents a transition. Given a state status of type T, lists all the states that can be transitioned to in next_status of type T
    /// </summary>
    /// <typeparam name="T">
    /// El tipo T genérico que representa al estado</typeparam>

    public class ValidTransition<T>
    {
        public T status;
        public T[] next_status;
    }


    /// <summary>
    /// Definición de la clase HandpaySM
    /// Definition of the class HandpaySM
    /// </summary>
    [XmlType]
    public class Handpay
    {
        // Se modela la máquina de estados con todas sus transiciones
        // We model the state machine with all its transitions
        private ValidTransition<HandpaySMStatus>[] state_machine = new ValidTransition<HandpaySMStatus>[] {


                new ValidTransition<HandpaySMStatus> { /* FROM */ status = HandpaySMStatus.HandpayOcurred ,                                                                
                                                                  /* TO */ next_status = new HandpaySMStatus[] { HandpaySMStatus.HandpayReset}},

                new ValidTransition<HandpaySMStatus> { /* FROM */ status = HandpaySMStatus.HandpayReset ,                                                               
                                                                  /* TO */ next_status = new HandpaySMStatus[] {  HandpaySMStatus.HandpayOcurred}}
         };

        [XmlAttribute]
        public  HandpaySMStatus status;
        [XmlAttribute]
        public DateTime LastTransitionTS;
        [XmlAttribute]
        public  bool LP1BAcknowledged = false;
        [XmlAttribute]
        public  byte[] data;

        public Handpay()
        {
            status = HandpaySMStatus.HandpayOcurred;
        }



  /// <summary>
        /// Función de transición.
        /// Transition function
        /// </summary>
        /// <param name="status_">
        ///  El estado al cual se quiere transicionar 
        ///  The state to which you want to transition
        /// </param>
        /// <returns>  Returns true if it transitioned successfully, returns false if it failed to transition. </returns>
        public bool Transition(HandpaySMStatus status_)
        {
            // Obtiene la transición guardada de status, de la SM
            // Gets the saved transition of status, from the SM
            ValidTransition<HandpaySMStatus> transition = state_machine.Where(t => t.status == status).FirstOrDefault();
            // Si puede transicionar.. retorna true y el status actual es el status nuevo
            // If it can transition.. returns true and the current status is the new status
            if (transition.next_status.Contains(status_))
            {
                status = status_;
                LastTransitionTS = DateTime.Now;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
