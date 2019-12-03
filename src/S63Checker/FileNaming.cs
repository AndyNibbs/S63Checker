using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S63Checker
{
    static class FileNaming
    {
        public static bool IsCellFile(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);

            if (name.Length != 8)
                return false;

            if (name.StartsWith("GB8", StringComparison.InvariantCultureIgnoreCase)) // special case for AIO
                return true;

            char third = name[2];
            if (third < '1' || third > '6')
                return false;

            string ext = Path.GetExtension(path);

            if (ext.Length != 4)
                return false;

            string num = ext.Substring(1, 3);

            return int.TryParse(num, out int res);
        }

        public static string SignatureFilename(string cellPath)
        {
            var arr = cellPath.ToCharArray();
            arr[arr.Length - 10] += (char)24;
            if (arr[arr.Length - 10] > 'Z')
            {
                return null;
            }
            return new string(arr);
        }
    }
}
