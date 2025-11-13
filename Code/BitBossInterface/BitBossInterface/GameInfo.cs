using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace BitbossInterface
{

    /// <summary>
    /// La clase WagerCategory consta de 3 atributos
    /// The class WagerCategory consists of 3 atributes
    /// </summary>
    [XmlType]
    public class WagerCategory
    {
        [XmlAttribute]
        public  byte[] category; // The wager categories
        [XmlAttribute]
        public  byte[] paybackPercentage; 
        [XmlAttribute]
        public  byte[] coinInMeterValue;  
        /// <summary>
        /// Al inicializar, se setea todo en cero (en string y en byte)
        /// When initialize, set all in zero (in string and byte)
        /// </summary>
         public WagerCategory()
         {
             coinInMeterValue =  new byte[] {0x00, 0x00};
             paybackPercentage = new byte[] {0x00, 0x00, 0x00, 0x00};
             category = new byte[] {0x00, 0x00};
         }
    }

    /// <summary>
    /// La clase GameInfo consta de 4 atributos
    /// The class GameInfo consists of 4 atributes
    /// </summary>
    [XmlType]
    public class GameInfo
    {

         [XmlAttribute]
        public  string gameName;     // El nombre del game -- The game name
         [XmlAttribute]
        public  string paytableName; // el name de la tabla de pago -- the name of pay table
        [XmlElement]
        public  List<WagerCategory> wagerCategoriesList; // Las categorías -- The wager categories
         [XmlAttribute]
        public  byte[] wagerCategories; // Las categorías -- The wager categories
        
        /// <summary>
        /// Al inicializar, se setea todo en cero (en string y en byte)
        /// When initialize, set all in zero (in string and byte)
        /// </summary>
         public GameInfo()
         {
             gameName = "";
             paytableName = "";
             wagerCategoriesList = new List<WagerCategory>();
             wagerCategories = new byte[] {0x00, 0x00};
         }


    }
}
