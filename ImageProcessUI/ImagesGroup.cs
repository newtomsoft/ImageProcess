using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using FreeImageAPI;

namespace ImageLib
{
    class ImagesGroup
    {
        public List<string> AllFilesNames { get; private set; }
        public string NameOfDirectory { get; private set; }
        public ImagesGroup(string nameOfDirectory)
        {
            NameOfDirectory = nameOfDirectory;
            if (NameOfDirectory.Last() != '\\')
            {
                NameOfDirectory += '\\';
            }
            IEnumerable<string> allFullFilesNames = Directory.EnumerateFiles(nameOfDirectory, "*.*").OrderBy(filename => filename);
            AllFilesNames = new List<string>();
            foreach (string file in allFullFilesNames)
            {
                AllFilesNames.Add(file.Substring(NameOfDirectory.Length));
            }
        }
        public void DeleteStrips()
        {
            List<string> newFilesNames = new List<string>(); 
            foreach (string fileName in AllFilesNames)
            {
                ImageProcess image = new ImageProcess(NameOfDirectory+fileName);
                image.DeleteStrips();
                image.SaveTo(FREE_IMAGE_FORMAT.FIF_BMP, @"");
                newFilesNames.Add(fileName+".tmp");
            }
            AllFilesNames = newFilesNames;
            NameOfDirectory += @"temp\";
        }

        public void SaveToPdf(string relativePathSave = @"Save_pdf\")
        {
            PdfDocument thePdfDoc = new PdfDocument();
            foreach (string nameOfAfile in AllFilesNames)
            {
                try
                {
                    XImage img = XImage.FromStream(new FileStream(NameOfDirectory + nameOfAfile, FileMode.Open));
                    XGraphics xgr = XGraphics.FromPdfPage(thePdfDoc.AddPage(new PdfPage { Width = img.PointWidth, Height = img.PointHeight }));
                    xgr.DrawImage(img, 0, 0);
                    xgr.Dispose();
                }
                catch
                {
                    Console.WriteLine("Image not supported by tool. Please convert before in jpg/gif/png/tiff");
                }
            }
            Directory.CreateDirectory(NameOfDirectory + relativePathSave);
            //System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            thePdfDoc.Save(NameOfDirectory + relativePathSave + "DocumentCreatedFromImages.pdf");
            thePdfDoc.Close();
            thePdfDoc.Dispose();
        }

    }
}
