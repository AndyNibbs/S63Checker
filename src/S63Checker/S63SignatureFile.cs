using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S63Checker
{
    class S63SignatureFile
    {
        /*
            // Signature part R:
            6660 E559 7ADC ED81 260F A487 236D CA1B 2201 BE87.
            // Signature part S:
            61CA 33C9 1839 49A4 7823 0977 9422 DF62 B384 0136.
            // Signature part R:
            7AAF 45AF D759 7558 0D3F B52E AEDC 7C9F 7E77 BF4F.
            // Signature part S:
            18A9 D232 DF9D B01B 51D5 91D8 F71A A967 3D7A 9863.
            // BIG p
            FCA6 82CE 8E12 CABA 26EF CCF7 110E 526D B078 B05E DECB CD1E B4A2 08F3 AE16 17AE 01F3 5B91 A47E 6DF6 3413 C5E1 2ED0 899B CD13 2ACD 50D9 9151 BDC4 3EE7 3759 2E17.
            // BIG q
            962E DDCC 369C BA8E BB26 0EE6 B6A1 26D9 346E 38C5.
            // BIG g
            6784 71B2 7A9C F44E E91A 49C5 147D B1A9 AAF2 44F0 5A43 4D64 8693 1D2D 1427 1B9E 3503 0B71 FD73 DA17 9069 B32E 2935 630E 1C20 6235 4D0D A20A 6C41 6E50 BE79 4CA4.
            // BIG y
            4645 6F86 5627 2ECE 4121 5354 D4EA AD75 1C62 71AA E80D 92DF EBB2 3212 3AAF 07AE E04E D252 58FF 3BCE 15E1 CDAA C7FC 7623 E9A6 5058 678C 8BB7 0419 265A 08D5 4786.       
         */

        public S63SignatureFile(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.ASCII))
            {
                for (int n = 0; n < 8; ++n)
                {
                    string heading = reader.ReadLine();
                    if (!heading.Equals(PartHeadings[n], StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new FormatException($"Expected {PartHeadings[n]} in signature file");
                    }

                    string line = reader.ReadLine();

                    _values.Add((SignaturePart)n, StringToByteArray(line));

                    Content[n * 2] = heading;
                    Content[(n * 2) + 1] = line;
                }
            }
        }

        public string[] Content = new string[16];

        public static byte[] StringToByteArray(String hex)
        {
            string sanitised = hex.Trim('.').Replace(" ", "");

            int NumberChars = sanitised.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(sanitised.Substring(i, 2), 16);
            return bytes;
        }

        private Dictionary<SignaturePart, byte[]> _values = new Dictionary<SignaturePart, byte[]>();

        internal enum SignaturePart
        {
            PartR1 = 0, // Parts R1 and S1 are the DS' signature of the ENC data file:
            PartS1,
            PartR2, // Parts R2 and S2 are the SA's signature of the DS certificate (P/Q/G/Y):
            PartS2,
            BigP, // The ENC cert
            BigQ,
            BigG,
            BigY
        }

        public byte[] PublicKeyOfDSCert()
        {
            StringBuilder sb = new StringBuilder();

            for (int n = 8; n < 16; ++n)
            {
                sb.Append(Content[n]);
                sb.Append("\r\n");
            }
            
            return Encoding.ASCII.GetBytes(sb.ToString());
        }
        
        public byte[] CellSignature => _values[SignaturePart.PartR1].Concat(_values[SignaturePart.PartS1]).ToArray();
        public byte[] DataServerCertSignedBySA => _values[SignaturePart.PartR2].Concat(_values[SignaturePart.PartS2]).ToArray();

        public byte[] BigP => _values[SignaturePart.BigP];
        public byte[] BigQ => _values[SignaturePart.BigQ];
        public byte[] BigG => _values[SignaturePart.BigG];
        public byte[] BigY => _values[SignaturePart.BigY];

        private string[] PartHeadings =
        {
            "// Signature part R:", // DS sig R
            "// Signature part S:", // DS sig S
            "// Signature part R:", // SA sig R
            "// Signature part S:", // SA sig S
            "// BIG p", // ENC cert p,q,g,y
            "// BIG q",
            "// BIG g",
            "// BIG y",
        };
    }
}
