using System;
using System.IO;

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
                return false;
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