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
            MemoryStream memoryStream;
            try
            {
                zip = ZipFile.Open("test.cbr", ZipArchiveMode.Read);
                foreach (var entrie in zip.Entries)
                {
                    Console.WriteLine(entrie.FullName);
                    memoryStream = (MemoryStream)entrie.Open();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }





        }
    }
}
