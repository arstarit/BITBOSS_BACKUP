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
    [XmlType]
    public class EGMInfo
    {
        [XmlAttribute]
        public  string GameID;
        [XmlAttribute]
        public  string AdditionalID;
        [XmlAttribute]
        public  byte Denomination;
        [XmlAttribute]
        public  string PayTableID;
        [XmlAttribute]
        public  string BasePercentage;
        [XmlAttribute]
        public  byte[] SASVersion;
        [XmlAttribute]
        public  byte[] GMSerialNumber;
         [XmlAttribute]
        public  int NumberOfGamesImplemented;
         [XmlAttribute]
        public  string Error;
         public GameInfo gi; // Game info by default, used to be returned in case a requested game is not found
         /*Los game info restantes*/
         [XmlElement]
        public  GameInfo game0Info;
         [XmlElement]
        public  GameInfo game1Info;
         [XmlElement]
        public  GameInfo game2Info;
         [XmlElement]
        public  GameInfo game3Info;
         [XmlElement]
        public  GameInfo game4Info;
         [XmlElement]
        public  GameInfo game5Info;
         [XmlElement]
        public  GameInfo game6Info;
         [XmlElement]
        public  GameInfo game7Info;
         [XmlElement]
        public  GameInfo game8Info;
         [XmlElement]
        public  GameInfo game9Info;
         [XmlElement]
        public  GameInfo game10Info;
         [XmlElement]
        public  GameInfo game11Info;
         [XmlElement]
        public  GameInfo game12Info;
         [XmlElement]
        public  GameInfo game13Info;
         [XmlElement]
        public  GameInfo game14Info;
         [XmlElement]
        public  GameInfo game15Info;
         [XmlElement]
        public  GameInfo game16Info;
         [XmlElement]
        public  GameInfo game17Info;
         [XmlElement]
        public  GameInfo game18Info;
         [XmlElement]
        public  GameInfo game19Info;
         [XmlElement]
        public  GameInfo game20Info;
         [XmlElement]
        public  GameInfo game21Info;
         [XmlElement]
        public  GameInfo game22Info;
         [XmlElement]
        public  GameInfo game23Info;
         [XmlElement]
        public  GameInfo game24Info;
         [XmlElement]
        public  GameInfo game25Info;
         [XmlElement]
        public  GameInfo game26Info;
         [XmlElement]
        public  GameInfo game27Info;
         [XmlElement]
        public  GameInfo game28Info;
         [XmlElement]
        public  GameInfo game29Info;
         [XmlElement]
        public  GameInfo game30Info;
         [XmlElement]
        public  GameInfo game31Info;
         [XmlElement]
        public  GameInfo game32Info;
         [XmlElement]
        public  GameInfo game33Info;
         [XmlElement]
        public  GameInfo game34Info;
         [XmlElement]
        public  GameInfo game35Info;
         [XmlElement]
        public  GameInfo game36Info;
         [XmlElement]
        public  GameInfo game37Info;
         [XmlElement]
        public  GameInfo game38Info;
         [XmlElement]
        public  GameInfo game39Info;
         [XmlElement]
        public  GameInfo game40Info;
         [XmlElement]
        public  GameInfo game41Info;
         [XmlElement]
        public  GameInfo game42Info;
         [XmlElement]
        public  GameInfo game43Info;
         [XmlElement]
        public  GameInfo game44Info;
         [XmlElement]
        public  GameInfo game45Info;
         [XmlElement]
        public  GameInfo game46Info;
         [XmlElement]
        public  GameInfo game47Info;
         [XmlElement]
        public  GameInfo game48Info;
         [XmlElement]
        public  GameInfo game49Info;
         [XmlElement]
        public  GameInfo game50Info;
         [XmlElement]
        public  GameInfo game51Info;
         [XmlElement]
        public  GameInfo game52Info;
         [XmlElement]
        public  GameInfo game53Info;
         [XmlElement]
        public  GameInfo game54Info;
         [XmlElement]
        public  GameInfo game55Info;
         [XmlElement]
        public  GameInfo game56Info;
         [XmlElement]
        public  GameInfo game57Info;
         [XmlElement]
        public  GameInfo game58Info;
         [XmlElement]
        public  GameInfo game59Info;
         [XmlElement]
        public  GameInfo game60Info;
         [XmlElement]
        public  GameInfo game61Info;
         [XmlElement]
        public  GameInfo game62Info;
         [XmlElement]
        public  GameInfo game63Info;
         [XmlElement]
        public  GameInfo game64Info;

        // Función que toma un gameNumber y devuelve un GameInfo // Function that takes a gameNumber and returns a GameInfo //
        public ref GameInfo GetGameInfo(byte[] gameNumber)
        {
             if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x00 }))
                return ref game0Info;  
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x01 }))
                return ref game1Info;
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x02 }))
                return ref game2Info;
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x03 }))
                return ref game3Info;
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x04 }))
                return ref game4Info;
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x05 }))
                return ref game5Info;
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x06 }))
                return ref game6Info;
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x07 }))
                return ref game7Info;
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x08 }))
                return ref game8Info;
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x09 }))
                return ref game9Info;
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x10 }))
                return ref game10Info; 
             else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x11 }))
                return ref game11Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x12 }))
                return ref game12Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x13 }))
                return ref game13Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x14 }))
                return ref game14Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x15 }))
                return ref game15Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x16 }))
                return ref game16Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x17 }))
                return ref game17Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x18 }))
                return ref game18Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x19 }))
                return ref game19Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x20 }))
                return ref game20Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x21 }))
                return ref game21Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x22 }))
                return ref game22Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x23 }))
                return ref game23Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x24 }))
                return ref game24Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x25 }))
                return ref game25Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x26 }))
                return ref game26Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x27 }))
                return ref game27Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x28 }))
                return ref game28Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x29 }))
                return ref game29Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x30 }))
                return ref game30Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x31 }))
                return ref game31Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x32 }))
                return ref game32Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x33 }))
                return ref game33Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x34 }))
                return ref game34Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x35 }))
                return ref game35Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x36 }))
                return ref game36Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x37 }))
                return ref game37Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x38 }))
                return ref game38Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x39 }))
                return ref game39Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x40 }))
                return ref game40Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x41 }))
                return ref game41Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x42 }))
                return ref game42Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x43 }))
                return ref game43Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x44 }))
                return ref game44Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x45 }))
                return ref game45Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x46 }))
                return ref game46Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x47 }))
                return ref game47Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x48 }))
                return ref game48Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x49 }))
                return ref game49Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x50 }))
                return ref game50Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x51 }))
                return ref game51Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x52 }))
                return ref game52Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x53 }))
                return ref game53Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x54 }))
                return ref game54Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x55 }))
                return ref game55Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x56 }))
                return ref game56Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x57 }))
                return ref game57Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x58 }))
                return ref game58Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x59 }))
                return ref game59Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x60 }))
                return ref game60Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x61 }))
                return ref game61Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x62 }))
                return ref game62Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x63 }))
                return ref game63Info;
        	    else if (gameNumber.SequenceEqual(new byte[] { 0x00, 0x64 }))
                return ref game64Info;                                
             else
             {
                 Console.WriteLine($"Warning, game info {BitConverter.ToString(gameNumber)} doesn't exist. Default to a game");
                 return  ref gi;
             }
        }

        // Rutina de inicialización
        public EGMInfo() // Instancia nueva
        {
          game0Info = new GameInfo();
          game1Info = new GameInfo();
          game2Info = new GameInfo();
          game3Info = new GameInfo();
          game4Info = new GameInfo();
          game5Info = new GameInfo();
          game6Info = new GameInfo();
          game7Info = new GameInfo();
          game8Info = new GameInfo();
          game9Info = new GameInfo();
          game10Info = new GameInfo();
          game11Info = new GameInfo();
        	 game12Info = new GameInfo();
        	 game13Info = new GameInfo();
        	 game14Info = new GameInfo();
        	 game15Info = new GameInfo();
        	 game16Info = new GameInfo();
        	 game17Info = new GameInfo();
        	 game18Info = new GameInfo();
        	 game19Info = new GameInfo();
        	 game20Info = new GameInfo();
        	 game21Info = new GameInfo();
        	 game22Info = new GameInfo();
        	 game23Info = new GameInfo();
        	 game24Info = new GameInfo();
        	 game25Info = new GameInfo();
        	 game26Info = new GameInfo();
        	 game27Info = new GameInfo();
        	 game28Info = new GameInfo();
        	 game29Info = new GameInfo();
        	 game30Info = new GameInfo();
        	 game31Info = new GameInfo();
        	 game32Info = new GameInfo();
        	 game33Info = new GameInfo();
        	 game34Info = new GameInfo();
        	 game35Info = new GameInfo();
        	 game36Info = new GameInfo();
        	 game37Info = new GameInfo();
        	 game38Info = new GameInfo();
        	 game39Info = new GameInfo();
        	 game40Info = new GameInfo();
        	 game41Info = new GameInfo();
        	 game42Info = new GameInfo();
        	 game43Info = new GameInfo();
        	 game44Info = new GameInfo();
        	 game45Info = new GameInfo();
        	 game46Info = new GameInfo();
        	 game47Info = new GameInfo();
        	 game48Info = new GameInfo();
        	 game49Info = new GameInfo();
        	 game50Info = new GameInfo();
        	 game51Info = new GameInfo();
        	 game52Info = new GameInfo();
        	 game53Info = new GameInfo();
        	 game54Info = new GameInfo();
        	 game55Info = new GameInfo();
        	 game56Info = new GameInfo();
        	 game57Info = new GameInfo();
        	 game58Info = new GameInfo();
        	 game59Info = new GameInfo();
        	 game60Info = new GameInfo();
        	 game61Info = new GameInfo();
        	 game62Info = new GameInfo();
        	 game63Info = new GameInfo();
        	 game64Info = new GameInfo();
        }


    }
}
