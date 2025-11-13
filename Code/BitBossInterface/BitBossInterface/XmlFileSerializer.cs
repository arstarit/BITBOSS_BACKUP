using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace BitbossInterface
{
    /// <summary>
    /// Esta clase permite la serialización Xml de objetos
    /// This class allows Xml serialization of objects
    /// </summary>
    internal class XmlFileSerializer
    {
        /// <summary>
        /// Deserialización a partir de un archivo xml hacia un objeto de tipo T
        /// Deserialization from an xml file to an object of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public T Deserialize<T>(string filepath) where T : class
        {
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(T));
            string input = System.IO.File.ReadAllText(filepath);
            using (StringReader sr = new StringReader(input))
            {
                return (T)ser.Deserialize(sr);
            }
        }

        /// <summary>
        /// Serialización a partir de un objeto de tipo T, y se guarda en un archivo a determinar
        /// Serialization from an object of type T, and it is saved in a file to be determined
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ObjectToSerialize"></param>
        /// <param name="filepath">The file where you want to save the serialization</param>
        public void SaveXml<T>(T ObjectToSerialize, string filepath)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(ObjectToSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, ObjectToSerialize);
                System.IO.File.WriteAllText(filepath, textWriter.ToString());
            }
        }
    }
}
