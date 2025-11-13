using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace BitbossInterface
{
    [XmlType]
    public class ExtendedAccountingMeter
    {
        // Name of the accounting meter.
        [XmlAttribute]
        public string Name;

        // Code for the accounting meter represented as a byte array.
        [XmlAttribute]
        public byte[] MeterCode;

        // The cycle value for the accounting meter.
        [XmlAttribute]
        public int CycleValue;

        // The current value of the accounting meter.
        [XmlAttribute]
        public int Value;

        // Constructor for the ExtendedAccountingMeter class.
        public ExtendedAccountingMeter()
        {
            // Initializing the MeterCode with a byte array of size 2.
            MeterCode = new byte[2];
        }

        // Method to get the current value of the accounting meter.
        public int GetValue()
        {
            return Value;
        }

        // Method to increment the value of the accounting meter by a specified amount.
        public void increment(int amount)
        {
            // Add the specified amount to the current value of the meter.
            // For example, Value += amount;
        }
    }

}
