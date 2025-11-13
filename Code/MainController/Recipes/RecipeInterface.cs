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
    /* Interfaz de la Recipe. Tipo abstracto del que las demás recipe heredan*/
    /* Recipe interface. Abstract type from which the other recipes inherit.*/
    /// </summary>
    public abstract class RecipeInterface
    {
        // El controller, para hacer llamadas a la physicalEGMController
        // The controller, to make calls to the physicalEGMController
        public PhysicalEGMBehaviourController controller;
        // Agregar una Action
        // Add an Action
        public abstract void AddAction(Func<ActionStatus> action);
        // Está en InProgress
        // You are in InProgress
        public abstract bool InProgress();
        // Inicializar la recipe, pasandole como parámetro el controller
        // Initialize the recipe, passing it as parameter the controller
        public abstract void Init(PhysicalEGMBehaviourController controller_);
        // Permite ejecutar toda la lista
        // Allows you to run the entire list
        public abstract ActionStatus Execute();
        
    }

}
