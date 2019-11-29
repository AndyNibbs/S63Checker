using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S63Checker
{
    class FolderSource : ISource
    {
        public FolderSource(string path)
        {
            
        }

 
        public Stream OpenRead(string path)
        {
            return File.OpenRead(path);
        }

        public string[] Paths { get; private set; }
    }
}
