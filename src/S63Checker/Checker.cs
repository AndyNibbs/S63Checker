using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace S63Checker
{
    internal class Checker
    {
        private string path;
        private OutputDetail detail;
        private static SHA1CryptoServiceProvider SHA1 { get; } = new SHA1CryptoServiceProvider();
        public DSACryptoServiceProvider SA { get; }

        public Checker(string path, OutputDetail detail)
        {
            var cert = new X509Certificate2("iho.crt");
            var now = DateTime.UtcNow;
            bool inDate = (now > cert.NotBefore && now < cert.NotAfter);

            if (inDate)
                SA = cert.PublicKey.Key as DSACryptoServiceProvider;
            else
                throw new InvalidDataException("IHO.CRT has expired");

            this.path = path;
            this.detail = detail;
        }

        internal bool DoSignatureCheck()
        {
            using (var source = SourceFactory(path))
            {
                return CheckSource(source);
            }
        }

        private bool CheckSource(ISource source)
        {
            var cellFiles = source.Paths.Where(path => FileNaming.IsCellFile(path));
            var fails = new List<string>();

            foreach(string cellPath in cellFiles)
            {
                string cellFileName = Path.GetFileName(cellPath);

                string signaturePath = FileNaming.SignatureFilename(cellPath);

                bool hasSignature = source.Paths.Contains(signaturePath, StringComparer.InvariantCultureIgnoreCase);

                if (hasSignature)
                {
                    if (!CheckSignature(source, cellFileName, cellPath, signaturePath))
                    {
                        WriteVerbose($"FAIL: {cellFileName} Signature check failure");
                        fails.Add(cellFileName);
                    }
                    else
                    {
                        WriteVerbose($"PASS: {cellFileName}");
                    }
                }
                else
                {
                    WriteVerbose($"FAIL: {cellFileName} No signature file at {signaturePath}");
                    fails.Add(cellFileName);
                }
            }

            WriteVerbose(string.Empty);

            if (fails.Any())
            {
                Write("Check failed on following files");
            }
            else
            {
                Write("Check passed");
            }

            foreach (string name in fails)
            {
                Write($"\t{name}");
            }

            return !fails.Any();
        }

        private bool CheckSignature(ISource source, string cellFileName, string cellPath, string signaturePath)
        {
            S63SignatureFile sig = new S63SignatureFile(source.OpenRead(signaturePath));

            var certSig = sig.DataServerCertSignedBySA;

            var publicKeyFromFile = sig.PublicKeyOfDSCert();// the whole second half the of the signature file  --  Encoding.ASCII.GetBytes(LineRange(8, 8));
                       
            var pkHash = SHA1.ComputeHash(publicKeyFromFile);
            if (!SA.VerifySignature(pkHash, certSig))
            {
                WriteVerbose($"Cell certificate not signed {cellFileName}");
                return false;
            }
            
            var cellHash = SHA1.ComputeHash(source.OpenRead(cellPath));
           
            using (var dsaCell = new DSACryptoServiceProvider())
            {
                dsaCell.ImportParameters(new DSAParameters()
                {
                    P = sig.BigP,
                    Q = sig.BigQ,
                    G = sig.BigG,
                    Y = sig.BigY,
                });

                return dsaCell.VerifySignature(cellHash, sig.CellSignature);
            }
        }

        private ISource SourceFactory(string path)
        {
            if (Path.GetExtension(path).Equals(".iso", StringComparison.InvariantCultureIgnoreCase))
            {
                return new IsoSource(path);
            }

            return new FolderSource(path);
        }



        private void Write(string line)
        {
            if (detail != OutputDetail.Silent)
                Console.WriteLine(line);
        }

        private void WriteVerbose(string line)
        {
            if (detail == OutputDetail.Verbose)
                Console.WriteLine(line);
        }
    }
}