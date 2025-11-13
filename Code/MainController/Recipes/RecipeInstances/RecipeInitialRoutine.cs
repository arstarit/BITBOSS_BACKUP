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
    /// Recipe correspondiente a la rutina inicial. Se ejecuta cuando hay link restored de parte de la SMIB
    /// Recipe corresponding to the initial routine. It is executed when there is a link restored from the SMIB.
    /// </summary>
    public class RecipeInitialRoutine : RecipeInterface
    {
        // La lista de acciones, donde cada acción es una función que no acepta parámetros, 
        // pero devuelve un ActionStatus
        // The list of actions, where each action is a function that does not accept parameters, 
        // but returns an ActionStatus
        private List<Func<ActionStatus>> actions;
        // El controller
        // The controller
        public PhysicalEGMBehaviourController controller;
        // Un booleano que indica que está en ejecución la recipe
        // A boolean indicating that the recipe is running.
        private bool InExecution;
        // Permite añadir una acción
        // Permite añadir una acción
        public override void AddAction(Func<ActionStatus> action)
        {
        }

        public override void Init(PhysicalEGMBehaviourController controller_) // Inicialización // Initialization
        {
            InExecution = false;
            controller = controller_;
            actions = new List<Func<ActionStatus>>();
            actions.Add(() =>
            {
                controller.MLaunchLog(new string[] {}, "Initial routine started");
                return ActionStatus.Completed;
            });
            // Enviamos el 54
            // We ship 54
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetSASVersionAndMachineSerialNumber(controller.GetAddress())));
            // Enviamos el 1C, recibimos sus meters
            // We sent the 1C, we received your meters
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetSendMeters(controller.GetAddress())));
            // Envio distintos meters
            // I send different meters
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetSendSelectedMeters(controller.GetAddress(), new byte[] { 0x00, 0x00 }, new byte[] { 0x03, 0x04, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0D, 0x0E, 0x11 })));
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetSendSelectedMeters(controller.GetAddress(), new byte[] { 0x00, 0x00 }, new byte[] { 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1C })));
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetSendSelectedMeters(controller.GetAddress(), new byte[] { 0x00, 0x00 }, new byte[] { 0x1D, 0x1E, 0x1F, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26 })));
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetSendSelectedMeters(controller.GetAddress(), new byte[] { 0x00, 0x00 }, new byte[] { 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F, 0x30, 0x31 })));
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetSendSelectedMeters(controller.GetAddress(), new byte[] { 0x00, 0x00 }, new byte[] { 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3E, 0x3F })));
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetSendSelectedMeters(controller.GetAddress(), new byte[] { 0x00, 0x00 }, new byte[] { 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49 })));
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetSendSelectedMeters(controller.GetAddress(), new byte[] { 0x00, 0x00 }, new byte[] { 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F, 0x50, 0x51, 0x52, 0x53 })));
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetSendSelectedMeters(controller.GetAddress(), new byte[] { 0x00, 0x00 }, new byte[] { 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D })));
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetSendSelectedMeters(controller.GetAddress(), new byte[] { 0x00, 0x00 }, new byte[] { 0x5E, 0x5F, 0x79, 0x7A, 0x7F, 0x80, 0x81, 0x82, 0x83, 0x84 })));
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetSendSelectedMeters(controller.GetAddress(), new byte[] { 0x00, 0x00 }, new byte[] { 0x85, 0x86, 0x87, 0x88, 0x89, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E })));
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetSendSelectedMeters(controller.GetAddress(), new byte[] { 0x00, 0x00 }, new byte[] { 0x8F, 0x90, 0x91, 0x92, 0x93, 0xA0, 0xA1, 0xA2, 0xA3, 0xA4 })));
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetSendSelectedMeters(controller.GetAddress(), new byte[] { 0x00, 0x00 }, new byte[] { 0xA5, 0xA8, 0xA9, 0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF, 0xB0 })));
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetSendSelectedMeters(controller.GetAddress(), new byte[] { 0x00, 0x00 }, new byte[] { 0xB1, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD, 0xFA, 0xFB, 0xFC })));
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetSendSelectedMeters(controller.GetAddress(), new byte[] { 0x00, 0x00 }, new byte[] { 0xFD, 0xFE, 0xFF })));



            // Envía el long poll 74
            // Send the long poll 74
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetLockLP74(controller.GetAddress(), 0xFF, 0x00, 0x00, 0x00)));
            // Interrogamos
            // We question
            actions.Add(() => { if (!AFTCurrentTransaction.Instance().WorkInProgress() && 
                                    !InterfacedAFT.Instance().WorkInProgress())
                                    {
                                        controller.AFTCurrentTransferInterrogated = true; 
                                        return controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetAFTInt(controller.GetAddress(), 0x00));
                                    }
                                else 
                                    {
                                        return ActionStatus.Completed;
                                    }
                                });
        
            // Query all transfer indexes
            actions.Add(() =>
            {
                controller.MLaunchLog(new string[] {}, "AFT Buffer resync started");
                return ActionStatus.Completed;
            });

            actions.Add(() => {
                ActionStatus status = ActionStatus.Completed;
                for(byte i = 0x01; i <= controller.maxBufferIndexForInterrogate; i++)
                {
                  status = controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetAFTInt(controller.GetAddress(), i));
                }
                return status;
            });

            actions.Add(() =>
            {
                controller.MLaunchLog(new string[] {}, "AFT Buffer resync completed");
                return ActionStatus.Completed;
            });
            // Consultamos los créditos
            // We consult the credits
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetSendSelectedMeters(controller.GetAddress(), new byte[] { 0x00, 0x00 }, new byte[] { 0x0C, 0x1B })));
            // Obtenemos el MachineID y la información
            // We obtain MachineID and information
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetGamingMachineIDAndInformation(controller.GetAddress())));
            // Solicitamos las features 
            // We request the following features 
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.BuildSendEnabledFeaturesCommand(controller.GetAddress(), new byte[] { 0x00, 0x00 })));
            // Send Number Of Games Implemented
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetLP51(controller.GetAddress())));
            // Envia el long poll B2 
            // Send the long poll B2 
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.BuildLPB2(controller.GetAddress())));
            // Envia el long poll B3 
            // Send the long poll B3 
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.BuildLPB3(controller.GetAddress())));
            // Envia el long poll B4 
            // Send the long poll B4 
            actions.Add(() => 
            { 
                controller.SendWagerCategoryInformation(new byte[] { 0x00, 0x00 }); 
                return ActionStatus.Completed;
            });
            // Envía el long poll 48
            // Send the long poll 48
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetSendLastBillAcceptedInformation(controller.GetAddress())));
            // Envía el long poll 7B
            // Send the long poll 7B
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.ExtendedValidationStatus(controller.GetAddress(), 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00)));
            // Envía el long poll 73
            // Send the long poll 73
            actions.Add(() => controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.GetAFTRegistration(controller.GetAddress(), 0xFF)));
            // Envía el long poll 9A
            // Send long poll 9A
            actions.Add(() =>
            {
                if (InterfacingSettings.Singleton().BonusingEnabled == true)
                    return controller.SendLongPollWaitingResponse(LongPollFactory.Singleton.BuildLP9A(controller.GetAddress(), new byte[] { 0x00, 0x00 }));
                else
                    return ActionStatus.Completed;
            });


            actions.Add(() => { controller.LaunchInitialRoutineFinished(); return ActionStatus.Completed; });
            actions.Add(() =>
            {
                controller.MLaunchLog(new string[] {}, "Initial routine finished");
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
