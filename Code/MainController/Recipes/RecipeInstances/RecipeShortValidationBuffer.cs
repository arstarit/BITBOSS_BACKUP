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
    /// Recipe correspondiente al envío del long poll 4D para un validation buffer corto. Va desde la function code 04 a la funcion code 01
    /// Recipe corresponding to the sending of the 4D long poll for a short validation buffer. It goes from function code 04 to function code 01.
    /// </summary>    
    public class RecipeShortValidationBuffer : RecipeInterface
    {
        // La lista de acciones, donde cada acción es una función que no acepta parámetros, 
        // pero devuelve un ActionStatus
        // The list of actions, where each action is a function that does not accept parameters, 
        // but returns an ActionStatus
        private List<Func<ActionStatus>> actions;
        // El controller // The controller 
        public PhysicalEGMBehaviourController controller;
        // Permite añadir una acción // Allows you to add an action
        public override void AddAction(Func<ActionStatus> action)
        {
        }

        public override void Init(PhysicalEGMBehaviourController controller_) // Inicialización // Initialization
        {
               controller = controller_;
               actions = new List<Func<ActionStatus>>();

                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x04);
                                      return ActionStatus.Completed; 
                                  });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x03);
                                      return ActionStatus.Completed; 
                                  });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x02);
                                      return ActionStatus.Completed; 
                                  });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x01);
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
