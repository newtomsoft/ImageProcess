using SharpCompress.Archives;
using System.Collections.Generic;
using System.IO;

namespace ManageCompressedFile
{
    public class CompressedFile
    {
        public static List<string> ExtractFilesToTempPath(string compressedFile)
        {
            List<string> fullNamesOfFiles = new List<string>();
            IArchive archive = ArchiveFactory.Open(compressedFile);
            foreach (IArchiveEntry entrie in archive.Entries)
            {
                if (!entrie.IsDirectory)
                {
                    var fileName = entrie.Key;
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
