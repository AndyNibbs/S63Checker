using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace S63Checker
{
    class Program
    {
        static int Main(string[] args)
        {
            var detail = OutputDetail.Basic;
            try
            {
                if (args.Length == 0 || (args.Length==1 && args[0].Equals("-?")))
                {
                    return Usage();
                }

                bool silent = HasFlag("silent", args);
                bool verbose = HasFlag("verbose", args);
                detail = ChooseOutputDetail(silent, verbose);

                string path = args.First();
                ThrowIfInvalidPath(path);

                var checker = new Checker(path, detail);

                bool checkPassed = checker.DoSignatureCheck();

                return checkPassed ? 0 : 1;
            }
            catch (Exception x)
            {
                switch (detail)
                {
                    case OutputDetail.Silent:
                        break;
                    case OutputDetail.Verbose:
                        Console.WriteLine(x.ToString());
                        break;
                    default:
                        Console.WriteLine(x.Message);
                        break;
                }

                return 2;
            }
        }

        private static void ThrowIfInvalidPath(string path)
        {
            if (Path.GetExtension(path).Equals(".iso", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException("Could not find .iso file", path);
                }
            }
            else if (Path.GetExtension(path).Equals(".zip", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException("Could not find .zip file", path);
                }
            }
            else
            {
                if (!Directory.Exists(path))
                {
                    throw new DirectoryNotFoundException($"Could not find exchange set directory {path}");
                }
            }
        }

        private static OutputDetail ChooseOutputDetail(bool silent, bool verbose)
        {
            if (silent)
            {
                return OutputDetail.Silent;
            }
            if (verbose)
            {
                return OutputDetail.Verbose;
            }
            else
            {
                return OutputDetail.Basic;
            }
        }

        static bool HasFlag(string flag, string[] args)
        {
            string decorated = (flag[0] == '-') ? flag : "-" + flag;
            decorated = decorated.ToLower();
            return args.Skip(1).Where(a => a.ToLower().Contains(decorated)).Any();
        }

        static int Usage()
        {
            Console.WriteLine();
            Console.WriteLine("S63Checker <path to exchange set folder | path to .iso> [-verbose] [-silent] [-?]");
            Console.WriteLine();
            Console.WriteLine("Don't use verbose and silent together -- if you do it'll be silent");
            Console.WriteLine();
            Console.WriteLine("Return values:");
            Console.WriteLine("0 means signatures are good");
            Console.WriteLine("1 signature check failed");
            Console.WriteLine("2 error occurred");
            Console.WriteLine("3 this was displayed");
            Console.WriteLine();
            Console.WriteLine($"Version {Assembly.GetExecutingAssembly().GetName().Version}");
            return 3;
        }
    }
}
