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

/// <summary>
/* Definición de la Recipe. Instancia una Recipe en base al argumento. */
/* Recipe Definition. Instantiate a Recipe based on the argument. */
/// </summary>
namespace Recipes
{


    // Definición del Recipe
    // Recipe Definition
    public class Recipe
    {

        // El RecipeInterface
        // The RecipeInterface
        public RecipeInterface Instance;

        // La instanciación de la Recipe 
        // The instantiation of the Recipe 
        public Recipe(string name)
        {
            // De acuerdo al modo de dejar logs, el name
            // According to the way logs are left, the name
            switch (name)
            {
                case "Default": // name es Default // name is Default
                    {
                        Instance = new RecipeDefault(); // Instancio la Recipe Default // I install the Recipe Default 
                        break;
                    }
                case "InitialRoutine": // name es InitialRoutine // The name is InitialRoutine
                    {
                        Instance = new RecipeInitialRoutine(); // Instancia la Recipe de la rutina Inicial // Instantiate the Recipe of the Initial routine
                        break;
                    }
                case "Registration": // name es Registration // name is Registration
                    {
                        Instance = new RecipeRegistration(); // Instancio la Recipe Registration // I install the Recipe Registration
                        break;
                    }
                case "GetMetersMachine": // name es GetMetersMachine // name is GetMetersMachine
                    {
                        Instance = new RecipeGetMetersMachine(); // Instancio la Recipe GetMetersMachine // I install the Recipe GetMetersMachine
                        break;
                    }
                case "GetMetersMultiGame": // name es GetMetersMultiGame // name is GetMetersMultiGame 
                    {
                        Instance = new RecipeGetMetersMultiGame(); // Instancio la Recipe GetMetersMultiGame // I install the GetMetersMultiGame Recipe
                        break;
                    }
                case "ValidationBuffer": // name es ValidationBuffer // name is ValidationBuffer
                    {
                        Instance = new RecipeValidationBuffer(); // Instancio la Recipe ValidationBuffer // I install the Recipe ValidationBuffer
                        break;
                    }
                case "ShortValidationBuffer": // name es ValidationBuffer // name is ValidationBuffer
                    {
                        Instance = new RecipeShortValidationBuffer(); // Instancio la Recipe ValidationBuffer // I install the Recipe ValidationBuffer 
                        break;
                    }
                default:
                    {
                        Instance = new RecipeDefault();
                        break;
                    }
            }
        }

    }

}
