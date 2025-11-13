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

    // La enumeración ActionStatus
    // The ActionStatus enumeration
    public enum ActionStatus
    {
        Completed,
        Failed
    }




    /// <summary>
    /* Recipe que no tiene ninguna acción. Por lo general no se usa o se usa como elemento neutro sin ningún efecto en la ejecución del maincontroller  */
    /* Recipe that has no action. Usually not used or used as a neutral element with no effect on maincontroller execution.  */
    /// </summary>
    public class RecipeDefault : RecipeInterface
    {
        // La lista de acciones, donde cada acción es una función que no acepta parámetros, 
        // pero devuelve un ActionStatus
        // The list of actions, where each action is a function that does not accept parameters, 
        // but returns an ActionStatus
        private List<Func<ActionStatus>> actions;
        // El controller // The Controller
        public PhysicalEGMBehaviourController controller;
        // Permite añadir una acción // Allows you to add an action
        public override void AddAction(Func<ActionStatus> action)
        {
            actions.Add(action);
        }

        public override void Init(PhysicalEGMBehaviourController controller_) // Inicialización // 
        {
               controller = controller_;
               actions = new List<Func<ActionStatus>>();
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
