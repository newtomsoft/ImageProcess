using SharpCompress.Archives.Zip;
using SharpCompress.Readers;
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
            using (Stream stream = File.OpenRead(fileZip))
            using (var reader = ReaderFactory.Open(stream))
            {
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        using (var entryStream = reader.OpenEntryStream())
                        {
                            string fileName = reader.Entry.ToString();
                            string fullName = Path.Combine(Path.GetTempPath(), fileName);
                            using (FileStream fileStream = new FileStream(fullName, FileMode.Create, FileAccess.Write))
                            {
                                entryStream.CopyTo(fileStream);
                            }
                            fullNamesOfFiles.Add(fullName);
                        }
                    }
                }
            }
            return fullNamesOfFiles;
        }
    }
}

