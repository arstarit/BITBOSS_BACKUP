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
using MainController;

namespace Recipes
{
    /// <summary>
    /// Recipe correspondiente a la registración. Contiene dos acciones que envía el long poll 73 a la EGM, con registration code 00 y 01 respectivamente. 
    /// Recipe corresponding to the registration. It contains two actions sent by the long poll 73 to the EGM, with registration code 00 and 01 respectively. 
    /// </summary>
    public class RecipeRegistration : RecipeInterface
    {
        // La lista de acciones, donde cada acción es una función que no acepta parámetros, 
        // pero devuelve un ActionStatus
        // The list of actions, where each action is a function that does not accept parameters, 
        // but returns an ActionStatus
        private List<Func<ActionStatus>> actions;
        // El controller //  The controller
        public PhysicalEGMBehaviourController controller;
        // Un booleano que indica que está en ejecución la recipe
        // A boolean indicating that the recipe is running.
        private bool InExecution;
        // Permite añadir una acción
        // Allows you to add an action
        public override void AddAction(Func<ActionStatus> action)
        {
        }

        public override void Init(PhysicalEGMBehaviourController controller_) // Inicialización // Initialization 
        {
               InExecution = false;
               controller = controller_;
               actions = new List<Func<ActionStatus>>();
               actions.Add(() =>  { Console.WriteLine($"registration started {DateTime.Now}");
                                    return ActionStatus.Completed; 
                                  });
                // AFT Registration 0x00 (Registramos)
               actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetAFTRegistration(controller.GetAddress(), 0x00, InterfacingSettings.Singleton().AFT_AssetNumber, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x20 }, 0)));
                // AFT Registration 0x01
               actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetAFTRegistration(controller.GetAddress(), 0x01, InterfacingSettings.Singleton().AFT_AssetNumber, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05,0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x20 }, 0)));
               actions.Add(() =>  { Console.WriteLine($"registration finished {DateTime.Now}");
                                    return ActionStatus.Completed; 
                                  });
        }

        public override bool InProgress()
        {
          return InExecution;
        }

        // Permite ejecutar toda la lista
        // Allows you to run the entire list
        public override ActionStatus Execute()
        {
            bool failed = false;
             if (InExecution == false)
            {
              InExecution = true;
                // Recorriendo todas las acciones
                // Going through all the actions
                foreach (Func<ActionStatus> act in actions)
              {
                    // ejecuta act, y si es Failed
                    // execute act, and if Failed
                    if (act() == ActionStatus.Failed)
                {
                    failed = true;
                }
              }
              InExecution = false;
            }
            if (failed)
              return ActionStatus.Failed; // Retorna Failed // Return Failed 
            else 
              return ActionStatus.Completed; // Retorna Completed // Return Completed
        }
    }
}
