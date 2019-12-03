using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace S63Checker
{
    internal class Checker
    {
        private string path;
        private OutputDetail detail;

        public Checker(string path, OutputDetail detail)
        {
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
                    WriteVerbose($"TODO: {cellFileName} TODO: check sig");
                }
                else
                {
                    WriteVerbose($"FAIL: {cellFileName} No signature file at {signaturePath}");
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