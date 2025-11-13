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

    public enum InterfacedValidationType
    {
        SystemValidation,

        EnhancedValidation
    }

    /// <summary>
    /// El archivo de InterfacingSettings, tiene diferentes booleanos, que activan o desactivan 
    /// el interfaceo de los long polls que vienen del client(sastest) hacia el host(EGM)
    ///
    /// The InterfacingSettings file has different booleans, that activate or deactivate 
    /// the interface of the long polls coming from the client(sastest) to the host(EGM)
    /// </summary>
    public class InterfacingSettingsFile
    {
        /// <summary>
        /// Bandera de lp01 desde el Client al Host
        /// lp01 flag from the Client to the Host
        /// </summary>
        public bool passthrough_lp01;
        /// <summary>
        /// Bandera de lp02 desde el Client al Host
        /// lp02 flag from the Client to the Host
        /// </summary>
        public bool passthrough_lp02;
        /// <summary>
        /// Bandera de lp03 desde el Client al Host
        /// lp03 flag from Client to Host
        /// </summary>
        public bool passthrough_lp03;
        /// <summary>
        /// Bandera de lp04 desde el Client al Host
        /// lp04 flag from the Client to the Host
        /// </summary>
        public bool passthrough_lp04;
        /// <summary>
        /// Bandera de lp06 desde el Client al Host
        /// Flag of lp06 from Client to Host
        /// </summary>
        public bool passthrough_lp06;
        /// <summary>
        /// Bandera de lp07 desde el Client al Host
        /// Flag of lp07 from Client to Host
        /// </summary>
        public bool passthrough_lp07;
        /// <summary>
        /// Bandera de lp08 desde el Client al Host
        /// Flag of lp08 from Client to Host
        /// </summary>
        public bool passthrough_lp08;
        /// <summary>
        /// Bandera de lp0E desde el Client al Host
        /// lp0E flag from Client to Host
        /// </summary>
        public bool passthrough_lp0E;
        /// <summary>
        /// Bandera de lp2E desde el Client al Host
        /// lp2E flag from Client to Host
        /// </summary>
        public bool passthrough_lp2E;
        /// <summary>
        /// Bandera de lp4C desde el Client al Host
        /// lp4C flag from Client to Host
        /// </summary>
        public bool passthrough_lp4c;
        /// <summary>
        /// Bandera de lp7C desde el Client al Host
        /// lp7C flag from Client to Host
        /// </summary>
        public bool passthrough_lp7C;
        /// <summary>
        /// Bandera de lp7D desde el Client al Host
        /// lp7D banner from Client to Host
        /// </summary>
        public bool passthrough_lp7D;
        /// <summary>
        /// Bandera de lp7F desde el Client al Host
        /// lp7F banner from Client to Host
        /// </summary>
        public bool passthrough_lp7f;
        /// <summary>
        /// Bandera de lp80 desde el Client al Host
        /// lp80 flag from Client to Host
        /// </summary>
        public bool passthrough_lp80;
        /// <summary>
        /// Bandera de lp86 desde el Client al Host
        /// lp86 banner from Client to Host
        /// </summary>
        public bool passthrough_lp86;
        /// <summary>
        /// Bandera de lp8A desde el Client al Host
        /// lp8A flag from Client to Host
        /// </summary>
        public bool passthrough_lp8A;
        /// <summary>
        /// Bandera de lp94 desde el Client al Host
        /// Flag of lp94 from Client to Host
        /// </summary>
        public bool passthrough_lp94;
        /// <summary>
        /// Bandera de lpa8 desde el Client al Host
        /// lpa8 banner from Client to Host
        /// </summary>
        public bool passthrough_lpa8;
        /// <summary>
        /// Bandera que permite forzar una registración desde la PhysicalEGM
        /// Flag that allow force a registration from PhysicalEGM
        /// </summary>
        public bool ForceDummyRegistrationOnStartup;
        /// <summary>
        /// Bandera que permite hacer relay de cualquier long poll con una address de broadcast
        /// Flag that allows to relay any long poll with a broadcast address.
        /// </summary>
        public bool SASProgressiveBroadcastPassThrough;
        /// <summary>
        ///  Current Selected Game Number
        /// </summary>
        public byte[] SelectedGameNumber;
        /// <summary>
        /// AFT Asset Number
        /// </summary>
        public int AFT_AssetNumber;
        /// <summary>
        /// La Validation Type
        /// Validation Type
        /// </summary>
        public InterfacedValidationType validationType;
        /// <summary>
        /// Booleano que indica que las extensiones de la validación están soportadas
        /// Boolean indicating that validation extensions are supported.
        /// </summary>
        public bool ValidationExtensionsSupported;
        /// <summary>
        /// Booleano que indica que el bonusing está soportado
        /// Boolean indicating that bonusing is supported
        /// </summary>
        public bool BonusingEnabled;
        public InterfacingSettingsFile()
        {

        }
    }


    /// <summary>
    /// La clase Interfacing Settings, que establece cuando reenviar determinados longpolls desde la smib a la EGM
    /// The Interfacing Settings class, which sets when to forward certain longpolls from the smib to the EGM.
    /// </summary>
    public class InterfacingSettings
    {
        /// <summary>
        /// El archivo de configuración 
        /// The configuration file 
        /// </summary>
        private static InterfacingSettingsFile settings;
        private InterfacingSettings()
        {

        }
        // El Singleton de la InterfacingSettings
        // The Singleton of the InterfacingSettings
        public static InterfacingSettingsFile Singleton()
        {
            try
            {
                // Leo el archivo InterfacingSettings.xml
                // I read the InterfacingSettings.xml file
                settings = XmlFileSerializer.Deserialize<InterfacingSettingsFile>("InterfacingSettings.xml");
                return settings;
            }
            catch // No existe el archivo // No existe el archivo 
            {
                // Instancio una nueva InterfacingSettings
                // I install a new InterfacingSettings
                settings = new InterfacingSettingsFile();
                settings.passthrough_lp4c = true; //
                settings.passthrough_lp7C = true; // 
                settings.passthrough_lp7D = true; //
                settings.passthrough_lp08 = true; //
                settings.passthrough_lp0E = true; //
                settings.passthrough_lp01 = true; //
                settings.passthrough_lp02 = true; //
                settings.passthrough_lp03 = true; //
                settings.passthrough_lp04 = true; // Todos en true
                settings.passthrough_lp06 = true; //
                settings.passthrough_lp07 = true; //
                settings.passthrough_lp7f = true; // 
                settings.passthrough_lp94 = true; //
                settings.passthrough_lpa8 = true; //
                settings.passthrough_lp80 = true; //
                settings.passthrough_lp86 = true; //
                settings.passthrough_lp8A = true; //
                settings.passthrough_lp2E = true; //
                settings.SASProgressiveBroadcastPassThrough = true;
                settings.AFT_AssetNumber = 12345; // 12345 por default // 12345 by default 
                settings.validationType = InterfacedValidationType.EnhancedValidation; // En principio es Enhanced // In principle it is Enhanced 
                settings.ForceDummyRegistrationOnStartup = false;
                settings.ValidationExtensionsSupported = false;
                settings.BonusingEnabled = false;
                XmlFileSerializer.SaveXml<InterfacingSettingsFile>(settings, "InterfacingSettings.xml"); // Persisto el archivo // I persist the file 
                return settings;
            }
        }

        /// <summary>
        /// Actualizo el ValidationType, en base a la validation type que me viene como parámetro
        /// I update the ValidationType, based on the validation type that comes to me as parameter
        /// </summary>
        /// <param name="type"></param>
        public static void UpdateValidationType(InterfacedValidationType type)
        {
            settings = Singleton(); // Me aseguro que la settings sea la Singleton() // I make sure that the settings are Singleton()
            if (type == InterfacedValidationType.EnhancedValidation) // Si la validation es Enhanced // If the validation is Enhanced
                settings.passthrough_lp4c = true; // Seteo en true el passthrough para el long poll 4C // Set the passthrough for long poll 4C to true 
            else if (type == InterfacedValidationType.SystemValidation) // Si la validation es System // If validation is System
                settings.passthrough_lp4c = false; // Seteo en false el passthrough para el long poll 4C // Set the passthrough for long poll 4C to false
            settings.validationType = type; // Guardo el type // I keep the type
            XmlFileSerializer.SaveXml<InterfacingSettingsFile>(settings, "InterfacingSettings.xml"); // Persisto el archivo // I persist the file

        }


        /// <summary>
        /// Actualizo la Validation Extensions, en base al booleano b que me viene como parámetro
        /// I update the Validation Extensions, based on the boolean b that comes as parameter
        /// </summary>
        /// <param name="b"></param>
        public static void UpdateValidationExtensions(bool b)
        {
            settings = Singleton(); // Me aseguro que la settings sea la Singleton() // I make sure that the settings are Singleton()
            settings.ValidationExtensionsSupported = b;
            XmlFileSerializer.SaveXml<InterfacingSettingsFile>(settings, "InterfacingSettings.xml"); // Persisto el archivo // I persist the file
        }

        /// <summary>
        /// Actualizo el Asset Number, en base al entero que me viene como parámetro
        /// I update the Asset Number, based on the integer that comes to me as parameter
        /// </summary>
        /// <param name="assetNumber"></param>
        public static void UpdateAssetNumber(int assetNumber)
        {
            settings = Singleton(); // Me aseguro que la settings sea la Singleton() // I make sure that the settings are Singleton() 
            settings.AFT_AssetNumber = assetNumber;
            XmlFileSerializer.SaveXml<InterfacingSettingsFile>(settings, "InterfacingSettings.xml"); // Persisto el archivo // I persist the file 

        }

          /// <summary>
        /// I update force registration property
        /// </summary>
        /// <param name="assetNumber"></param>
        public static void UpdateForceRegistrationOnStartup(bool force)
        {
            settings = Singleton(); // Me aseguro que la settings sea la Singleton() // I make sure that the settings are Singleton() 
            settings.ForceDummyRegistrationOnStartup = force;
            XmlFileSerializer.SaveXml<InterfacingSettingsFile>(settings, "InterfacingSettings.xml"); // Persisto el archivo // I persist the file 

        }


        /// <summary>
        /// Guardar y persistir datos
        /// Save and persist data
        /// </summary>
        public static void SaveData()
        {
            XmlFileSerializer.SaveXml<InterfacingSettingsFile>(settings, "InterfacingSettings.xml"); // Persisto el archivo // I persist the file
        }

    }
}
