using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using PdfSharp.Drawing;
using System.Collections.Generic;
using System.IO;
using Path = System.IO.Path;

public class PdfFile
{
    PdfSharp.Pdf.PdfDocument ThePdfDocument;
    /// <summary>
    /// create new pdf document to put image into it after that
    /// </summary>
    public PdfFile()
    {
        ThePdfDocument = new PdfSharp.Pdf.PdfDocument();
    }
    /// <summary>
    /// add a page into the pdf document witch is into the <c>memoryStream</c>
    /// </summary>
    /// <param name="memoryStream"></param>
    public void AddImage(MemoryStream memoryStream)
    {
        try
        {
            XImage img = XImage.FromStream(memoryStream);
            XGraphics xgr = XGraphics.FromPdfPage(ThePdfDocument.AddPage(new PdfSharp.Pdf.PdfPage { Width = img.PointWidth, Height = img.PointHeight }));
            xgr.DrawImage(img, 0, 0);
            xgr.Dispose();
        }
        catch
        {
            //Console.WriteLine("Image not supported by tool. Please convert before in jpg/gif/png/tiff");
        }
    }
    /// <summary>
    /// save the pdf document
    /// </summary>
    public void Save(string fullPathSave, string fileName)
    {
        Directory.CreateDirectory(fullPathSave);
        ThePdfDocument.Save(Path.Combine(fullPathSave, fileName));
        ThePdfDocument.Close();
        ThePdfDocument.Dispose();
    }
    public static List<string> ExtractImagesToTempPath(string fileName)
    {
        List<string> filesNames = new List<string>();
        PdfReader pdfDoc = new PdfReader(fileName);
        int objectsNumber = pdfDoc.XrefSize;
        int imageFoundIndex = 0;
        for (int i = 0; i < objectsNumber; i++)
        {
            PdfObject currentpdfObject = pdfDoc.GetPdfObject(i);
            if (currentpdfObject != null && currentpdfObject.IsStream())
            {
                PRStream currentPdfReaderStream = (PRStream)currentpdfObject;
                PdfObject type = currentPdfReaderStream.Get(PdfName.SUBTYPE);
                if (type != null && type.ToString().Equals(PdfName.IMAGE.ToString()))
                {
                    try
                    {
                        PdfImageObject pdfImageObject = new PdfImageObject(currentPdfReaderStream);
                        string imageName = "image" + ++imageFoundIndex;
                        string imageFullName = Path.Combine(Path.GetTempPath(), imageName);
                        FileStream fs = new FileStream(imageFullName, FileMode.Create);
                        byte[] imgdata = pdfImageObject.GetImageAsBytes();
                        fs.Write(imgdata, 0, imgdata.Length);
                        fs.Flush();
                        fs.Close();
                        filesNames.Add(imageFullName);
                    }
                    catch
                    {
                    }
                }
            }
        }
        pdfDoc.Dispose();
        return filesNames;
    }
}