using DiscUtils.Iso9660;
using System.IO;

namespace S63Checker
{
    /// <summary>
    /// Uses Quamotion Diskutils library to explore an ISO path
    /// </summary>
    internal class IsoSource : ISource
    {
        public IsoSource(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Cannot find file {path}");
            }

            _isoStream = File.OpenRead(path);
            _cd = new CDReader(_isoStream, joliet: true, hideVersions: true);

            Paths = _cd.GetFiles("\\", "*.*", SearchOption.AllDirectories);
        }

        public string[] Paths { get; private set; }
            
        public Stream OpenRead(string path) => _cd.OpenFile(path, FileMode.Open);

        private bool disposedValue = false; // To detect redundant calls
        private FileStream _isoStream;
        private CDReader _cd;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_cd is object) 
                        _cd.Dispose();

                    if (_isoStream is object)
                        _isoStream.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}