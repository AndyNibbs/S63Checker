using System.IO;
using System.IO.Compression;
using System.Linq;

namespace S63Checker
{
    internal class ZipSource : ISource
    {
        private ZipArchive _archive;

        public ZipSource(string path)
        {
            _archive = ZipFile.OpenRead(path);
            Paths = _archive.Entries.Select(entry => entry.FullName).ToArray();
        }

        public string Root => string.Empty;
        public string[] Paths { get; }

        public void Dispose()
        {
            _archive.Dispose();
        }

        public Stream OpenRead(string path)
        {
            var entry = _archive.GetEntry(path);
            return entry.Open();
        }
    }
}