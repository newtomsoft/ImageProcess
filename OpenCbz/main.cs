using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCbz
{
    class Program
    {
        static void Main(string[] args)
        {
            ZipArchive zip;
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                zip = ZipFile.Open("test.cbr", ZipArchiveMode.Read);
                foreach (var entrie in zip.Entries)
                {
                    Console.WriteLine(entrie.FullName);
                    Stream stream = entrie.Open();
                    stream.CopyTo(memoryStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }





        }
    }
}
