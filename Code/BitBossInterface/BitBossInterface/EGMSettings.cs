using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Linq;

namespace BitbossInterface
{

    // The enumeration EGMValidationType
    public enum EGMValidationType
    {
            System,

            Enhanced
    }
    [XmlType]
    public class EGMSettings
    {
        [XmlAttribute]
        public  byte address; // The direction of the EGM
        [XmlAttribute]
        public  string LocationName; // The lcoation name
        [XmlAttribute]
        public  string LocationAddress1; // The direction 1 
        [XmlAttribute]
        public  string LocationAddress2; // The direction 2
        [XmlAttribute]
        public  string RestrictedTicketTitle; 
        [XmlAttribute]
        public  string DebitTicketTitle; 
        [XmlAttribute]
        public  int expirationDays;
        [XmlAttribute]
        public  EGMValidationType ValidationType; // The type of validation
        [XmlAttribute]
        public  byte features1; //Temp
        [XmlAttribute]
        public  byte features2; //Temp
        [XmlAttribute]
        public  byte features3; //Temp
        [XmlAttribute]
        public  bool EnabledRealTime; // EnabledRealTime
        [XmlAttribute]
        public bool ValidationExtensions; // Validation Extensions
        [XmlAttribute]
        public  byte[] controlMask; 
        [XmlAttribute]
        public  byte[] statusBitControlStates;
        [XmlAttribute]
        public  byte[] cashableTicketAndReceiptExpiration;
        [XmlAttribute]
        public  byte[] restrictedTicketDefaultExpiration;
        [XmlAttribute]
        public  byte[] assetId;
        [XmlAttribute]
        public byte[] gmTransferLimit;
        [XmlAttribute]
        public byte[] restrictedPoolID;
        [XmlAttribute]
        public byte gmMaxBufferIndex;
        [XmlAttribute]
        public  byte MaxBet;
        [XmlAttribute]
        public  byte ProgressiveGroup;
        [XmlAttribute]
        public  byte[] ProgressiveLevels;
        [XmlAttribute]
        public  byte[] GameOptions;
        [XmlAttribute]
        public  List<byte[]> EnabledGameNumbers = new List<byte[]>();

         public GameSettings gs; // Default game setting, used to be returned in case a requested game is not found
         /*The remaining game settings*/
         [XmlElement]
        public  GameSettings game0Settings;
         [XmlElement]
        public  GameSettings game1Settings;
         [XmlElement]
        public  GameSettings game2Settings;
         [XmlElement]
        public  GameSettings game3Settings;
         [XmlElement]
        public  GameSettings game4Settings;
         [XmlElement]
        public  GameSettings game5Settings;
         [XmlElement]
        public  GameSettings game6Settings;
         [XmlElement]
        public  GameSettings game7Settings;
         [XmlElement]
        public  GameSettings game8Settings;
         [XmlElement]
        public  GameSettings game9Settings;
         [XmlElement]
        public  GameSettings game10Settings;
         [XmlElement]
        public  GameSettings game11Settings;
         [XmlElement]
        public  GameSettings game12Settings;
         [XmlElement]
        public  GameSettings game13Settings;
         [XmlElement]
        public  GameSettings game14Settings;
         [XmlElement]
        public  GameSettings game15Settings;
         [XmlElement]
        public  GameSettings game16Settings;
         [XmlElement]
        public  GameSettings game17Settings;
         [XmlElement]
        public  GameSettings game18Settings;
         [XmlElement]
        public  GameSettings game19Settings;
         [XmlElement]
        public  GameSettings game20Settings;
         [XmlElement]
        public  GameSettings game21Settings;
         [XmlElement]
        public  GameSettings game22Settings;
         [XmlElement]
        public  GameSettings game23Settings;
         [XmlElement]
        public  GameSettings game24Settings;
         [XmlElement]
        public  GameSettings game25Settings;
         [XmlElement]
        public  GameSettings game26Settings;
         [XmlElement]
        public  GameSettings game27Settings;
         [XmlElement]
        public  GameSettings game28Settings;
         [XmlElement]
        public  GameSettings game29Settings;
         [XmlElement]
        public  GameSettings game30Settings;
         [XmlElement]
        public  GameSettings game31Settings;
         [XmlElement]
        public  GameSettings game32Settings;
         [XmlElement]
        public  GameSettings game33Settings;
         [XmlElement]
        public  GameSettings game34Settings;
         [XmlElement]
        public  GameSettings game35Settings;
         [XmlElement]
        public  GameSettings game36Settings;
         [XmlElement]
        public  GameSettings game37Settings;
         [XmlElement]
        public  GameSettings game38Settings;
         [XmlElement]
        public  GameSettings game39Settings;
         [XmlElement]
        public  GameSettings game40Settings;
         [XmlElement]
        public  GameSettings game41Settings;
         [XmlElement]
        public  GameSettings game42Settings;
         [XmlElement]
        public  GameSettings game43Settings;
         [XmlElement]
        public  GameSettings game44Settings;
         [XmlElement]
        public  GameSettings game45Settings;
         [XmlElement]
        public  GameSettings game46Settings;
         [XmlElement]
        public  GameSettings game47Settings;
         [XmlElement]
        public  GameSettings game48Settings;
         [XmlElement]
        public  GameSettings game49Settings;
         [XmlElement]
        public  GameSettings game50Settings;
         [XmlElement]
        public  GameSettings game51Settings;
         [XmlElement]
        public  GameSettings game52Settings;
         [XmlElement]
        public  GameSettings game53Settings;
         [XmlElement]
        public  GameSettings game54Settings;
         [XmlElement]
        public  GameSettings game55Settings;
         [XmlElement]
        public  GameSettings game56Settings;
         [XmlElement]
        public  GameSettings game57Settings;
         [XmlElement]
        public  GameSettings game58Settings;
         [XmlElement]
        public  GameSettings game59Settings;
         [XmlElement]
        public  GameSettings game60Settings;
         [XmlElement]
        public  GameSettings game61Settings;
         [XmlElement]
        public  GameSettings game62Settings;
         [XmlElement]
        public  GameSettings game63Settings;
         [XmlElement]
        public  GameSettings game64Settings;

        // Función que toma un gameNumber y devuelve un GameSettings // // Function that takes a gameNumber and returns a GameSettings //
        public ref GameSettings GetGameSettings(byte[] gameNumber)
        {
             if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x00 }))
               return ref game0Settings;  
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x01 }))
               return ref game1Settings;
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x02 }))
               return ref game2Settings;
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x03 }))
               return ref game3Settings;
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x04 }))
               return ref game4Settings;
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x05 }))
               return ref game5Settings;
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x06 }))
               return ref game6Settings;
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x07 }))
               return ref game7Settings;
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x08 }))
               return ref game8Settings;
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x09 }))
               return ref game9Settings;
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x10 }))
               return ref game10Settings;   
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x11 }))
               return ref game11Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x12 }))
               return ref game12Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x13 }))
               return ref game13Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x14 }))
               return ref game14Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x15 }))
               return ref game15Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x16 }))
               return ref game16Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x17 }))
               return ref game17Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x18 }))
               return ref game18Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x19 }))
               return ref game19Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x20 }))
               return ref game20Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x21 }))
               return ref game21Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x22 }))
               return ref game22Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x23 }))
               return ref game23Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x24 }))
               return ref game24Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x25 }))
               return ref game25Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x26 }))
               return ref game26Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x27 }))
               return ref game27Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x28 }))
               return ref game28Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x29 }))
               return ref game29Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x30 }))
               return ref game30Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x31 }))
               return ref game31Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x32 }))
               return ref game32Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x33 }))
               return ref game33Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x34 }))
               return ref game34Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x35 }))
               return ref game35Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x36 }))
               return ref game36Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x37 }))
               return ref game37Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x38 }))
               return ref game38Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x39 }))
               return ref game39Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x40 }))
               return ref game40Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x41 }))
               return ref game41Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x42 }))
               return ref game42Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x43 }))
               return ref game43Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x44 }))
               return ref game44Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x45 }))
               return ref game45Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x46 }))
               return ref game46Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x47 }))
               return ref game47Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x48 }))
               return ref game48Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x49 }))
               return ref game49Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x50 }))
               return ref game50Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x51 }))
               return ref game51Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x52 }))
               return ref game52Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x53 }))
               return ref game53Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x54 }))
               return ref game54Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x55 }))
               return ref game55Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x56 }))
               return ref game56Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x57 }))
               return ref game57Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x58 }))
               return ref game58Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x59 }))
               return ref game59Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x60 }))
               return ref game60Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x61 }))
               return ref game61Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x62 }))
               return ref game62Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x63 }))
               return ref game63Settings;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x64 }))
               return ref game64Settings;         
             else
             {
                 Console.WriteLine($"Warning, game settings {BitConverter.ToString(gameNumber)} doesn't exist. Default to a game");
                 return ref gs;
             }
        }

        // Rutina de inicialización
        // Initial routine
        public EGMSettings() // New instance
        {
            address = 0x01; // El address por default será 01 -- The address by default will be 01
            assetId = new byte[] { 0x12, 0x43, 0x65, 0x12}; // El assetId será el 12 43 65 12 -- The asset id will be 12 43 65 12
            EnabledRealTime = false; // El EnabledRealTime será falso al principio -- The EnabledRealTime will be false 
            ValidationExtensions = false;
            statusBitControlStates = new byte[] { 0x34, 0x00}; // El statusBitControlStates será 34 00 por default -- The statusBitControlStates will be 34 00 by default
            gmTransferLimit = new byte[] {};
            restrictedPoolID = new byte[] {};
            game0Settings = new GameSettings();
            game1Settings = new GameSettings();
            game2Settings = new GameSettings();
            game3Settings = new GameSettings();
            game4Settings = new GameSettings();
            game5Settings = new GameSettings();
            game6Settings = new GameSettings();
            game7Settings = new GameSettings();
            game8Settings = new GameSettings();
            game9Settings = new GameSettings();
            game10Settings = new GameSettings();
            game11Settings = new GameSettings();;
         	game12Settings = new GameSettings();;
         	game13Settings = new GameSettings();;
         	game14Settings = new GameSettings();;
         	game15Settings = new GameSettings();;
         	game16Settings = new GameSettings();;
         	game17Settings = new GameSettings();;
         	game18Settings = new GameSettings();;
         	game19Settings = new GameSettings();;
         	game20Settings = new GameSettings();;
         	game21Settings = new GameSettings();;
         	game22Settings = new GameSettings();;
         	game23Settings = new GameSettings();;
         	game24Settings = new GameSettings();;
         	game25Settings = new GameSettings();;
         	game26Settings = new GameSettings();;
         	game27Settings = new GameSettings();;
         	game28Settings = new GameSettings();;
         	game29Settings = new GameSettings();;
         	game30Settings = new GameSettings();;
         	game31Settings = new GameSettings();;
         	game32Settings = new GameSettings();;
         	game33Settings = new GameSettings();;
         	game34Settings = new GameSettings();;
         	game35Settings = new GameSettings();;
         	game36Settings = new GameSettings();;
         	game37Settings = new GameSettings();;
         	game38Settings = new GameSettings();;
         	game39Settings = new GameSettings();;
         	game40Settings = new GameSettings();;
         	game41Settings = new GameSettings();;
         	game42Settings = new GameSettings();;
         	game43Settings = new GameSettings();;
         	game44Settings = new GameSettings();;
         	game45Settings = new GameSettings();;
         	game46Settings = new GameSettings();;
         	game47Settings = new GameSettings();;
         	game48Settings = new GameSettings();;
         	game49Settings = new GameSettings();;
         	game50Settings = new GameSettings();;
         	game51Settings = new GameSettings();;
         	game52Settings = new GameSettings();;
         	game53Settings = new GameSettings();;
         	game54Settings = new GameSettings();;
         	game55Settings = new GameSettings();;
         	game56Settings = new GameSettings();;
         	game57Settings = new GameSettings();;
         	game58Settings = new GameSettings();;
         	game59Settings = new GameSettings();;
         	game60Settings = new GameSettings();;
         	game61Settings = new GameSettings();;
         	game62Settings = new GameSettings();;
         	game63Settings = new GameSettings();;
         	game64Settings = new GameSettings();;
        }

        public byte GetAddress()
        {
            return address;
        }

        public bool GetEnabledRealTime()
        {
            return EnabledRealTime;
        }

    }
}
