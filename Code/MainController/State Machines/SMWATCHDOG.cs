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
    public delegate void ResetRedemptionEvent(EventArgs e);
    public delegate void ResetValidationEvent(EventArgs e);
    public delegate void ResetAFTEvent(EventArgs e);
    // Definición de la clase InterfacedRedemption
    // InterfacedRedemption class definition
    public class SMWATCHDOG
     {

        private static SMWATCHDOG _instance = null;

        public static event ResetRedemptionEvent ResetRedemption;
        public static event ResetValidationEvent ResetValidation;
        public static event ResetAFTEvent ResetAFT;

        public static System.Timers.Timer WatchDogTimer; // Un timer que corre cada 30 segundos en busca de alguna State Machine atascada // A timer that runs every 30 seconds in search of a stuck State Machine. 

        protected SMWATCHDOG()
        {

        }

        // La instancia del singleton
        // The instance of the singleton
        public static SMWATCHDOG Instance()
        {
            // Si aún no se instanció
            // If not yet installed
            if (_instance == null)
            {
                try
                {
                    // Leo de la persistencia en el xml
                    // I Read from the persistence in the xml
                    _instance =  new SMWATCHDOG();
                    // Lanzo el timer
                    // I launch the timer
                    WatchDogTimer = new System.Timers.Timer (30000);
                    WatchDogTimer.Elapsed += WatchDogTimerElapsed;
                }
                catch // No existe el archivo!! // The file does not exist! 
                {

                }
                
                
            }
            return _instance;   
        }
    
        // Start WatchDog
        public void StartWatchDog()
        {
            WatchDogTimer.Start();
        }


        // La acción que se lanza al transcurrir 30 segundos
        // The action to be triggered after 30 seconds
        private static void WatchDogTimerElapsed (Object source, System.Timers.ElapsedEventArgs e) 
        {           
            WatchDogTimer.Stop();
            DateTime now = DateTime.Now;

            /* Transfer and Cashout Transactions */
            /* Si el estado intermedio quedó durante más de dos minutos */
            /* If the intermediate status was left for more than two minutes */
            if (((now - AFTCurrentTransaction.Instance().LastTransitionTS).TotalMinutes > 1)
              && AFTCurrentTransaction.Instance().WorkInProgress())
            {
                AFTCurrentTransaction.Instance().ResetState();
            }

            /* Redemptions */
            /* Si el estado intermedio quedó durante más de dos minutos */
            /* If the intermediate status was left for more than two minutes */
            if (((now - InterfacedRedemption.Instance().LastTransitionTS).TotalMinutes > 1)
              && InterfacedRedemption.Instance().WorkInProgress())
            {
                InterfacedRedemption.Instance().ResetState();
                ResetRedemption(e);// Program.ResetRedemption();
            }

            /* Validations */
            /* Si el estado intermedio quedó durante más de dos minutos */
            /* If the intermediate status was left for more than two minutes */
            if (((now - InterfacedValidation.Instance().LastTransitionTS).TotalMinutes > 1)
              && InterfacedValidation.Instance().WorkInProgress())
            {
                InterfacedValidation.Instance().ResetState();
                ResetValidation(e);// Program.ResetValidation();
            }

            /* AFT */
            /* Si el estado intermedio quedó durante más de dos minutos */
            /* If the intermediate status was left for more than two minutes */
            if (((now - InterfacedAFT.Instance().LastTransitionTS).TotalMinutes > 1)
              && InterfacedAFT.Instance().WorkInProgress())
            {
                InterfacedAFT.Instance().ResetState();
                ResetAFT(e);// Program.ResetAFT();
            }

            /* Send Long Poll */
            /* Si el estado intermedio quedó durante más de dos minutos */
            /* If the intermediate status was left for more than two minutes */
            if (((now - SendingLongPollSM.Instance().LastTransitionTS).TotalMinutes > 1)
              && SendingLongPollSM.Instance().WorkInProgress())
            {
                SendingLongPollSM.Instance().ResetState();
            }


            WatchDogTimer.Start();

        }


        // Método público que rompe la instancia
        // Public method that breaks the instance
        public void Destroy()
        {
            _instance = null;
        }

    }
}
