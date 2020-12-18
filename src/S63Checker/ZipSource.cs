using System.IO;
using System.IO.Compression;
using System.Linq;

namespace S63Checker
{
    internal class ZipSource : ISource
    {
        private ZipArchive _archive;
        private ILookup<string, ZipArchiveEntry> _lookup;

        public ZipSource(string path)
        {
            _archive = ZipFile.OpenRead(path);

            _lookup = _archive.Entries.ToLookup(e => e.FullName.Replace(@"/", @"\"));
            Paths = _lookup.Select(g => g.Key).ToArray();
        }

        public string Root => string.Empty;
        public string[] Paths { get; }
        public void Dispose() => _archive.Dispose();
        public Stream OpenRead(string path)
        {
            var e = _lookup[path.Replace(@"/", @"\")].FirstOrDefault();

            if (e is null)
                throw new FileNotFoundException(path);

            return e.Open();
        }
    }
}