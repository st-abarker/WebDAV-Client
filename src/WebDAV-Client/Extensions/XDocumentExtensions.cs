using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace WebDav
{
    internal static class XDocumentExtensions
    {
        public static XDocument TryParse(string text)
        {
            try
            {
	            using var ms = new MemoryStream();
	            using var writer = new StreamWriter(ms);
                writer.Write(text);
                writer.Flush();
                ms.Position = 0;
	            using var read = new XmlTextReader(ms);
                return XDocument.Load(read);
            }
            catch
            {
                return null;
            }
        }
    }
}
