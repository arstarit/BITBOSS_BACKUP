using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MainController
{
    public class XmlFileSerializer
    {
        // This method deserializes an XML file located at the provided filepath into an object of type T.
        // The 'T' type parameter represents the type of the object to be deserialized.
        static public T Deserialize<T>(string filepath) where T : class
        {
            // Create a new instance of XmlSerializer for the specified type 'T'.
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(T));

            // Read all the text from the XML file at the specified filepath.
            string input = System.IO.File.ReadAllText(filepath);

            // Create a StringReader instance with the input string.
            using (StringReader sr = new StringReader(input))
            {
                // Deserialize the XML content from the StringReader and cast it to type 'T'.
                return (T)ser.Deserialize(sr);
            }
        }

        // This method serializes the provided ObjectToSerialize and saves it as an XML file at the specified filepath.
        // The 'T' type parameter represents the type of the object being serialized.
        static public void SaveXml<T>(T ObjectToSerialize, string filepath)
        {
            // Create a new instance of XmlSerializer for the type of the ObjectToSerialize.
            XmlSerializer xmlSerializer = new XmlSerializer(ObjectToSerialize.GetType());

            // Create a StringWriter instance to hold the XML content.
            using (StringWriter textWriter = new StringWriter())
            {
                // Serialize the ObjectToSerialize into the StringWriter.
                xmlSerializer.Serialize(textWriter, ObjectToSerialize);

                // Write the content of the StringWriter to an XML file at the specified filepath.
                System.IO.File.WriteAllText(filepath, textWriter.ToString());
            }
        }
    }


}
