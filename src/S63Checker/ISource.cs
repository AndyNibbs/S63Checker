using System.IO;

namespace S63Checker
{
    /// <summary>
    /// Abstracts away the source being an ISO or a folder or something else
    /// </summary>
    internal interface ISource
    {
        string[] Paths { get; }

        Stream OpenRead(string path);
    }
}