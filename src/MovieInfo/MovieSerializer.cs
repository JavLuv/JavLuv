using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace MovieInfo
{
    public class MovieSerializer<T>
    {
        #region Public Functions

        static public void Save(string filename, T data)
        {
            // This will remove the xsi/xsd namespaces from serialization
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            // Create a serializer for movie data
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            // Save to a temp filename
            string tempFileName = filename + "~";

            // Serialize XML data to disk
            using (TextWriter textWriter = new StreamWriter(tempFileName, false, Encoding.UTF8))
            {
                serializer.Serialize(textWriter, data, ns);
            }

            // Copy temp filename to filename, then delete temp name
            File.Copy(tempFileName, filename, true);
            File.Delete(tempFileName);
        }

        static public T Load(string filename, Action<XDocument> filter = null)
        {
            // Create an XDocument from the filename
            XDocument xdoc = XDocument.Load(filename);

            // Filter document if required
            if (filter != null)
                filter(xdoc);

            // Save the XDocument to a text memory stream and return it
            // for deserialization into data structures.
            using (MemoryStream stream = new MemoryStream())
            {
                using (TextWriter textWriter = new StreamWriter(stream))
                {
                    xdoc.Save(textWriter);
                    stream.Seek(0, SeekOrigin.Begin);
                    XmlSerializer deserializer = new XmlSerializer(typeof(T));
                    return (T)deserializer.Deserialize(stream);
                }
            }          
        }

        #endregion
    }
}
