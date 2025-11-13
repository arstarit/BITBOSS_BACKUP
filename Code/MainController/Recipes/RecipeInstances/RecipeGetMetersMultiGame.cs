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
    /// Recipe que contiene un método que agrega una acción de recipe. Dicho método es parametrizado por el game. La acción consulta meters a la EGM para determinado game  
    /// Recipe that contains a method that adds a recipe action. This method is parameterized by the game. The action queries meters to the EGM for a given game.  
    /// </summary>
    public class RecipeGetMetersMultiGame : RecipeInterface
    {
        // La lista de acciones, donde cada acción es una función que no acepta parámetros, 
        // pero devuelve un ActionStatus
        // The list of actions, where each action is a function that does not accept parameters, 
        // but returns an ActionStatus
        private List<Func<ActionStatus>> actions;
        private byte[] gameNumber;
        // El controller // The Controller
        public PhysicalEGMBehaviourController controller;

        public void SetGameForMeters(byte[] game)
        {
            gameNumber = game;
            // Enviamos el 2F, recibimos sus meters
            // We sent the 2F, we received your meters
            actions.Add(() =>
            {
                controller.SendSelectedMeter(gameNumber, new byte[] { 0x00, 0x01, 0x02, 0x05, 0x06, 0x07, 0x08, 0x1C, 0x1D, 0x1E });
                controller.SendSelectedMeter(gameNumber, new byte[] { 0x1F, 0x20, 0x21, 0x22, 0x79, 0x7F });
                return ActionStatus.Completed;
            });
        }

        public override bool InProgress()
        {
            return false;
        }

        // Permite añadir una acción
        // Allows you to add an action
        public override void AddAction(Func<ActionStatus> action)
        {
        }

        public override void Init(PhysicalEGMBehaviourController controller_) // Inicialización // Initialization 
        {
            controller = controller_;
            actions = new List<Func<ActionStatus>>();

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
