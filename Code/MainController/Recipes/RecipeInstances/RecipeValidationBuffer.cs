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
    /// Recipe correspondiente al envío del long poll 4D para un validation buffer largo. Va desde la function code 1F a la funcion code 01
    /// Recipe corresponding to the sending of the 4D long poll for a long validation buffer. It goes from function code 1F to function code 01.
    /// </summary> 
    public class RecipeValidationBuffer : RecipeInterface
    {
        // La lista de acciones, donde cada acción es una función que no acepta parámetros, // The list of actions, where each action is a function that does not accept parameters,
        // pero devuelve un ActionStatus // but returns an ActionStatus
        private List<Func<ActionStatus>> actions;
        // El controller // The Controller
        public  PhysicalEGMBehaviourController controller;
        // Permite añadir una acción // Allows you to add an action 
        public override void AddAction(Func<ActionStatus> action)
        {
        }

        public override void Init(PhysicalEGMBehaviourController controller_) // Inicialización // Initialization
        {
               controller = controller_;
               actions = new List<Func<ActionStatus>>();
                // AFT Registration 0x00 (Registramos) // 

                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x1F);
                                      return ActionStatus.Completed; 
                                  });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x1E);
                                      return ActionStatus.Completed; 
                                  });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x1D);
                                      return ActionStatus.Completed; 
                                  });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x1C);
                                      return ActionStatus.Completed; 
                                  });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x1B);
                                      return ActionStatus.Completed; 
                                  });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x1A);
                                      return ActionStatus.Completed; 
                                  });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x19);
                                      return ActionStatus.Completed; 
                                  });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x18);
                                      return ActionStatus.Completed; 
                                  });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x17);
                                      return ActionStatus.Completed; 
                                  });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x16);
                                      return ActionStatus.Completed; 
                                  });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x15);
                                      return ActionStatus.Completed; 
                                  });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x14);
                                      return ActionStatus.Completed; 
                                  });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x13);
                                      return ActionStatus.Completed; 
                                 });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x12);
                                      return ActionStatus.Completed; 
                                 });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x11);
                                      return ActionStatus.Completed; 
                                 });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x10);
                                      return ActionStatus.Completed; 
                                 });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x0F);
                                      return ActionStatus.Completed; 
                                 });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x0E);
                                      return ActionStatus.Completed; 
                                 });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x0D);
                                      return ActionStatus.Completed; 
                                 });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x0C);
                                      return ActionStatus.Completed; 
                                 });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x0B);
                                      return ActionStatus.Completed; 
                                 });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x0A);
                                      return ActionStatus.Completed; 
                                 });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x09);
                                      return ActionStatus.Completed; 
                                  });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x08);
                                      return ActionStatus.Completed; 
                                  });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x07);
                                      return ActionStatus.Completed; 
                                  });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x06);
                                      return ActionStatus.Completed; 
                                  });
                actions.Add(() =>  {  controller.ValidationSendEnhancedValidationInformationFunctionCode(0x05);
                                      return ActionStatus.Completed; 
                                  });
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

        // Permite ejecutar toda la lista // Allows you to run the entire list // 
        public override ActionStatus Execute()
        {
            bool failed = false;
            // Recorriendo todas las acciones // Going through all the actions
            foreach (Func<ActionStatus> act in actions)
            {
                // ejecuta act, y si es Failed // execute act, and if Failed
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
