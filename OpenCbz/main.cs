using SharpCompress.Archives;
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
            OpenCompressedFileToFiles("test.cbz");

        }
        static List<string> OpenCompressedFileToFiles(string compressedFile)
        {
            List<string> fullNamesOfFiles = new List<string>();
            IArchive archive = ArchiveFactory.Open(compressedFile);
            foreach (IArchiveEntry entrie in archive.Entries)
            {
                if(!entrie.IsDirectory)
                {
                    var fileName = entrie.ToString();
                    string fullName = Path.Combine(Path.GetTempPath(), fileName);
                    string directoryName = Path.GetDirectoryName(fullName);
                    Directory.CreateDirectory(directoryName);
                    using (FileStream fileStream = new FileStream(fullName, FileMode.Create, FileAccess.Write))
                    {
                        entrie.WriteTo(fileStream);
                    }
                    fullNamesOfFiles.Add(fullName);
                }
            }
            return fullNamesOfFiles;
        }
    }
}

