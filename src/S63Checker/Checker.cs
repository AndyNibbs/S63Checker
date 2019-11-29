using System;

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
            return true;
        }
    }
}