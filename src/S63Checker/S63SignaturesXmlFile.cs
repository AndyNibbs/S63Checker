using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace S63Checker
{

    internal class S63SignaturesXmlFile
    {
        public static XNamespace Namespace { get; } = XNamespace.Get(@"http://www.iho.int/s63/1.1.1");

        public IEnumerable<XmlDataServer> DataServers { get; }
        public IEnumerable<XmlSignature> Signatures { get; }

        public S63SignaturesXmlFile(Stream stream)
        {
            var ns = Namespace;

            XDocument doc = XDocument.Load(stream);

            if (doc.Root.Name != ns + "digitalSignatures")
                throw new InvalidDataException("Bad XML signatures file - root is not digitalSignatures");

            DataServers = doc.Root.Element(ns + "dataServers").Elements(ns + "dataServer").Select(el => new XmlDataServer(el)).ToArray();
            Signatures = doc.Root.Elements(ns + "fileSignatures").Elements(ns + "fileSignature").Select(el => new XmlSignature(el)).ToArray();
        }
    }

    internal class XmlDataServer
    {
        public XmlDataServer(XElement dataServerElement)
        {
            var ns = S63SignaturesXmlFile.Namespace;

            ID = (string)dataServerElement.Attribute("dataServerID");

            string parameterP = (string)dataServerElement.Element(ns + "Parameters").Element(ns + "P");
            string parameterQ = (string)dataServerElement.Element(ns + "Parameters").Element(ns + "Q");
            string parameterG = (string)dataServerElement.Element(ns + "Parameters").Element(ns + "G");
            string publicKeyY = (string)dataServerElement.Element(ns + "PublicKey").Element(ns + "Y");

            string certR = (string)dataServerElement.Element(ns + "dataserverCertificate").Element(ns + "R");
            string certS = (string)dataServerElement.Element(ns + "dataserverCertificate").Element(ns + "S");

            BigP = S63SignatureFile.StringToByteArray(parameterP);
            BigQ = S63SignatureFile.StringToByteArray(parameterQ);
            BigG = S63SignatureFile.StringToByteArray(parameterG);
            BigY = S63SignatureFile.StringToByteArray(publicKeyY);
            CertR = S63SignatureFile.StringToByteArray(certR);
            CertS = S63SignatureFile.StringToByteArray(certS);

            // I found this bit fairly troubling to program. Formatting something in a very specific way and then taking the bytes!
            StringBuilder sb = new StringBuilder();

            sb.Append("// BIG p");
            sb.Append("\r\n");
            sb.Append(parameterP);
            sb.Append(".\r\n");

            sb.Append("// BIG q");
            sb.Append("\r\n");
            sb.Append(parameterQ);
            sb.Append(".\r\n");

            sb.Append("// BIG g");
            sb.Append("\r\n");
            sb.Append(parameterG);
            sb.Append(".\r\n");

            sb.Append("// BIG y");
            sb.Append("\r\n");
            sb.Append(publicKeyY);
            sb.Append(".\r\n");

            PublicKeyOfDSCert = Encoding.ASCII.GetBytes(sb.ToString());
        }

 

        public string ID { get; private set; }
        public byte[] BigP { get; set; }
        public byte[] BigQ { get; set; }
        public byte[] BigG { get; set; }
        public byte[] BigY { get; set; }
        public byte[] CertR { get; private set; }
        public byte[] CertS { get; private set; }
        public byte[] PublicKeyOfDSCert { get; private set; }
    }

    internal class XmlSignature
    {
        public XmlSignature(XElement fileSignature)
        {
            var ns = S63SignaturesXmlFile.Namespace;
            DataServerID = (string)fileSignature.Attribute("dataServerID");
            FileLocation = (string)fileSignature.Element(ns + "fileLocation");
            FileName = (string)fileSignature.Element(ns + "fileName");
            string r = (string)fileSignature.Element(ns + "Signature").Element(ns + "R");
            string s = (string)fileSignature.Element(ns + "Signature").Element(ns + "S");
            SignatureR = S63SignatureFile.StringToByteArray(r);
            SignatureS = S63SignatureFile.StringToByteArray(s);
        }

        public string DataServerID { get; }
        public string FileLocation { get; }
        public string FileName { get; }
        public byte[] SignatureR { get; }
        public byte[] SignatureS { get; }
    }
}
