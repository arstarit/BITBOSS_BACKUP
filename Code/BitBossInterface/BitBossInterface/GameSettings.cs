using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace BitbossInterface
{

    /// <summary>
    /// La clase GameSettings consta de 4 atributos
    /// The class GameSettings consists of 4 atributes
    /// </summary>
    [XmlType]
    public class GameSettings
    {
        [XmlAttribute]
        public  byte[] maxBet; // La apuesta máxima -- The max bet
        [XmlAttribute]
        public  byte progressiveGroup; // El grupo progresivo -- The progressive group
        [XmlAttribute]
        public  byte[] progressiveLevels; // Los niveles progresivos -- The progressive levels
        [XmlAttribute]
        public  byte[] CashoutLimit; // Los límites de cashout -- The cashout limits

        /// <summary>
        /// Al iniciarlizar, cada atributo se setea en 0 -- When initialize, each attribute is set to zero
        /// </summary>
        public GameSettings()
        {
           maxBet = new byte[] {0x00, 0x00};
           progressiveGroup = 0x00;
           progressiveLevels = new byte[] { 0x00, 0x00 , 0x00, 0x00};
           CashoutLimit = new byte[] {0x00, 0x00};
        }


    }
}
