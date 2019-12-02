using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S63Checker
{
    static class ExchangeSetSanityChecks
    {
        public static void ThrowIfNotExchangeSet(string root, string[] paths)
        {
            // Aim is to allow checker to be pointed at an AVCS or PRIMAR DVD or update CD,
            // but also an adhoc exchange set - as typically built for a specific vessel.

            // For the purposes here it has to have one or more SERIAL.ENC or one SERIAL.AIO
            // should have an equal number of CATALOG.031 files in an an ENC_ROOT subfolder.

            var cats = paths.Where(p => Path.GetFileName(p).Equals("CATALOG.031", StringComparison.InvariantCultureIgnoreCase));
            var serialAio = paths.Where(p => Path.GetFileName(p).Equals("SERIAL.AIO", StringComparison.InvariantCultureIgnoreCase));
            var serialEnc = paths.Where(p => Path.GetFileName(p).Equals("SERIAL.ENC", StringComparison.InvariantCultureIgnoreCase));

            if (serialAio.Any())
            {
                if (serialAio.Count() != 1)
                    throw new InvalidDataException("Can only check one AIO exchange set");

                if (serialEnc.Any())
                    throw new InvalidDataException("Cannot check a mixture of AIO and ENC exchange sets");

                if (cats.Count() != 1)
                    throw new InvalidDataException("Too many CATALOG.031 for an AIO exchange set");

                foreach (string catPath in cats)
                    ThrowIfEncRootDoesNotPrecede(catPath);
            }
            else if (serialEnc.Any())
            {
                if (serialEnc.Count() != cats.Count())
                    throw new InvalidDataException("Number of CATALOG.031 does not match number of SERIAL.ENC");

                foreach (string catPath in cats)
                    ThrowIfEncRootDoesNotPrecede(catPath);
            }
            else
            {
                throw new InvalidDataException("Not an S-63 exchange set");
            }
        }

        private static void ThrowIfEncRootDoesNotPrecede(string catPath)
        {
            string parent = Directory.GetParent(catPath).Name;
            if (!parent.Equals("ENC_ROOT", StringComparison.InvariantCultureIgnoreCase))
                throw new InvalidDataException($"CATALOG.031 is not not in folder ENC_ROOT, {catPath}");
        }
    }
}
