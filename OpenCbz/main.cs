using SharpCompress.Archives.Zip;
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
        static void Main()
        {
            OpenZipToTempFiles("test.zip");

        }
        static List<string> OpenZipToTempFiles(string fileZip)
        {
            List<string> fullNamesOfFiles = new List<string>();
            ZipArchive zip;
            try
            {
                zip = ZipArchive.Open(fileZip);
                var entries = zip.Entries;
                List<ZipArchiveEntry> listFiles = new List<ZipArchiveEntry>();
                foreach (var entrie in entries)
                {
                    string fileName = entrie.ToString();
                    Stream stream = entrie.OpenEntryStream();
                    string fullName = Path.Combine(Path.GetTempPath(), fileName);
                    if (Path.GetFileName(fullName) == "") continue;
                    fullNamesOfFiles.Add(fullName);
                    Directory.CreateDirectory(Path.GetDirectoryName(fullName));
                    using (FileStream fileStream = new FileStream(fullName, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
                zip.Dispose();
            }
            catch
            {
                throw;
            }
            return fullNamesOfFiles;
        }


    }

}

