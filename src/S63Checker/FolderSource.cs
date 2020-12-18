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
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Cannot find directory {path}");
            }

            Root = path;
            Paths = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

            ExchangeSetSanityChecks.ThrowIfNotExchangeSet(path, Paths);
        }

        public Stream OpenRead(string path)
        {
            return File.OpenRead(path);
        }

        public string[] Paths { get; private set; }

        public string Root { get; }

        public void Dispose()
        {
        }
    }
}
