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

        public List<string> PathsChecked { get; } = new List<string>();

        public Checker(string path, OutputDetail detail)
        {
            if (!File.Exists("iho.crt"))
                throw new FileNotFoundException("IHO.CRT not found alongside checker");

            var cert = new X509Certificate2("iho.crt");
            var now = DateTime.UtcNow;
            bool inDate = (now > cert.NotBefore && now < cert.NotAfter);

            if (inDate)
            {
                SA = cert.PublicKey.Key as DSACryptoServiceProvider;
            }
            else
            {
                throw new InvalidDataException("IHO.CRT has expired");
            }

            this.path = path;
            this.detail = detail;
        }

        internal bool DoSignatureCheck()
        {
            using (var source = SourceFactory(path))
            {
                ExchangeSetSanityChecks.ThrowIfNotExchangeSet(source.Root, source.Paths);

                return CheckSource(source);
            }
        }

        private bool CheckSource(ISource source)
        {
            var cellFiles = source.Paths.Where(path => FileNaming.IsCellFile(path));
            var fails = new List<string>();

            // This check the S63 1.1 signing of cell files
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

                    PathsChecked.Add(signaturePath);
                    PathsChecked.Add(cellPath);
                }
                else
                {
                    WriteVerbose($"FAIL: {cellFileName} No signature file at {signaturePath}");
                    fails.Add(cellFileName);
                }
            }

            // This is checking of the proposed extensions to the S63 standard which allows
            // signing of auxiliary and metadata files
            var signaturesPath = source.Paths.Where(path => path.Contains(@"INFO\S63_SIGNATURES.XML"));
            bool hasExtendingSignatureFile = signaturesPath.Count() == 1;
            if (hasExtendingSignatureFile)
            {
                WriteVerbose("Checking using signatures XML...");
                CheckSignaturesXml(signaturesPath.First(), source, fails);

                var uncheckedFiles = source.Paths.Except(PathsChecked);
                foreach (string ucf in uncheckedFiles)
                {
                    if (IsNoSignatureExpected(ucf))
                    {
                        WriteVerbose($"Unchecked {ucf} (expected/normal)");
                    }
                    else
                    {
                        Write($"No signature for {ucf}");
                        fails.Add(ucf);
                    }
                }

                WriteVerbose(string.Empty);
            }
            else
            {
                Write(@"WARNING Does not have proposed extra signature file (INFO\S63_SIGNATURES.XML)");
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

        private bool IsNoSignatureExpected(string ucf)
        {
            if (Path.GetFileName(ucf).Equals("IHO.CRT", StringComparison.InvariantCultureIgnoreCase))
                return true;

            if (Path.GetFileName(ucf).Equals("S63_SIGNATURES.XML", StringComparison.InvariantCultureIgnoreCase))
                return true;

            return false;
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

        private void CheckSignaturesXml(string sigXmlPath, ISource source, List<string> fails)
        {
            var signatures = new S63SignaturesXmlFile(source.OpenRead(sigXmlPath));
            if (!XmlDataServersAreSignedBySA(signatures))
            {
                fails.Add(sigXmlPath);
            }
            
            foreach(var sig in signatures.Signatures)
            {
                string filePath = Path.Combine(source.Root, sig.FileLocation, sig.FileName);

                byte[] fileHash = SHA1.ComputeHash(source.OpenRead(filePath));

                var dataServer = signatures.DataServers.FirstOrDefault(ds => ds.ID.Equals(sig.DataServerID));

                if (dataServer is null)
                {
                    Write($"Data server not found {sig.DataServerID}");
                    fails.Add(filePath);
                    continue;
                }

                using (var dsaCell = new DSACryptoServiceProvider())
                {
                    dsaCell.ImportParameters(new DSAParameters()
                    {
                        P = dataServer.BigP,
                        Q = dataServer.BigQ,
                        G = dataServer.BigG,
                        Y = dataServer.BigY
                    });

                    bool isgood = dsaCell.VerifySignature(fileHash, sig.SignatureR.Concat(sig.SignatureS).ToArray());
                    
                    if (isgood)
                    {
                        WriteVerbose($"PASS: {Path.Combine(sig.FileLocation, sig.FileName)}");
                    }
                    else
                    {
                        WriteVerbose($"FAIL: {Path.Combine(sig.FileLocation, sig.FileName)}\tSignature check failure");
                        fails.Add(filePath);
                    }
                }

                PathsChecked.Add(filePath.Replace(@"/", @"\"));
            }
        }

        private bool XmlDataServersAreSignedBySA(S63SignaturesXmlFile sf)
        {
            bool success = true;

            foreach(var ds in sf.DataServers)
            {
                if (XmlDataServersSignedBySA(ds))
                {
                    WriteVerbose($"PASS: XML data server {ds.ID} is signed by SA");
                }
                else
                {
                    WriteVerbose($"FAIL: XML data server {ds.ID} NOT signed by SA");
                    success = false;
                }
            }

            return success;
        }

        private bool XmlDataServersSignedBySA(XmlDataServer ds)
        {
            byte[] sig = ds.CertR.Concat(ds.CertS).ToArray();
            byte[] pubkey = ds.PublicKeyOfDSCert;
            
            byte[] pkHash = SHA1.ComputeHash(pubkey);
            
            bool isGood = SA.VerifySignature(pkHash, sig);
            
            return isGood;
        }

        private ISource SourceFactory(string path)
        {
            if (Path.GetExtension(path).Equals(".iso", StringComparison.InvariantCultureIgnoreCase))
            {
                return new IsoSource(path);
            }

            if (Path.GetExtension(path).Equals(".zip", StringComparison.InvariantCultureIgnoreCase))
            {
                return new ZipSource(path);
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