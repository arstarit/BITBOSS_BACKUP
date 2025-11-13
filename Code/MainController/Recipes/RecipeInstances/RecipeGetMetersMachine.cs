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
    /* Recipe que contiene una sóla acción, y consiste en enviar el 2F con meters a nivel machine */
    /* Recipe containing a single action, which consists of sending the 2F with meters to the machine level. */
    /// </summary>
    public class RecipeGetMetersMachine : RecipeInterface
    {
        // La lista de acciones, donde cada acción es una función que no acepta parámetros, 
        // pero devuelve un ActionStatus
        // The list of actions, where each action is a function that does not accept parameters, 
        // but returns an ActionStatus
        private List<Func<ActionStatus>> actions;
        // El controller // The controller
        public PhysicalEGMBehaviourController controller;
        // Permite añadir una acción
        // Allows you to add an action
        public override void AddAction(Func<ActionStatus> action)
        {
        }

        public override void Init(PhysicalEGMBehaviourController controller_) // Inicialización // Initialization
        {
            controller = controller_;
            actions = new List<Func<ActionStatus>>();
            // Enviamos el 2F, recibimos sus meters
            // We sent the 2F, we received your meters
            actions.Add(() =>
                                  {
                                      controller.SendSelectedMeter(new byte[] { 0x00, 0x01, 0x02, 0x05, 0x25, 0x26, 0x06, 0x24, 0x0C, 0x1B /* Llega el 1B a lo ùltimo*/});
                                      return ActionStatus.Completed;
                                  });

        }

        public override bool InProgress()
        {
            return false;
        }

        // Permite ejecutar toda la lista
        // Allows you to run the entire list
        public override ActionStatus Execute()
        {
            bool failed = false;
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
            if (failed)
                return ActionStatus.Failed; // Retorna Failed // Return Failed
            else
                return ActionStatus.Completed; // Retorna Completed // Return Completed
        }
    }
}
