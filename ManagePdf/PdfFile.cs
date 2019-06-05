using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using PdfSharp.Drawing;
using System;
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
    public void AddImage(ref MemoryStream memoryStream)
    {
        try
        {
            XImage img = XImage.FromStream(memoryStream);
            AddImage(ref img);
            img.Dispose();
        }
        catch
        {
            throw new Exception("Image non supportée. Utiliser fichier BMP, PNG, GIF, JPEG, TIFF, ou PDF");
        }
    }
    /// <summary>
    /// add a page into the pdf document witch is the <c>fullFileName</c>
    /// </summary>
    /// <param name="fullFileName"></param>
    public void AddImage(string fullFileName)
    {
        try
        {
            XImage img = XImage.FromFile(fullFileName);
            AddImage(ref img);
            img.Dispose();
        }
        catch
        {
            throw new Exception("Image non supportée. Utiliser fichier BMP, PNG, GIF, JPEG, TIFF, ou PDF");
        }
    }
    /// <summary>
    /// add a page into the pdf document witch is the <c>ximage</c>
    /// </summary>
    /// <param name="ximage"></param>
    private void AddImage(ref XImage ximage)
    {
        try
        {
            XGraphics xgr = XGraphics.FromPdfPage(ThePdfDocument.AddPage(new PdfSharp.Pdf.PdfPage { Width = ximage.PointWidth, Height = ximage.PointHeight }));
            xgr.DrawImage(ximage, 0, 0);
            xgr.Dispose();
        }
        catch
        {
            throw;
        }
    }
    /// <summary>
    /// save the pdf document
    /// </summary>
    public void Save(string fullPathSave, string fileName)
    {
        Directory.CreateDirectory(fullPathSave);
        try
        {
            ThePdfDocument.Save(Path.Combine(fullPathSave, fileName));
        }
        catch
        {
            ThePdfDocument.Close();
            ThePdfDocument.Dispose();
            throw new Exception("Sauvegarde du pdf impossible");
        }
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
                        string subdirectory = Path.GetFileName(fileName).Replace('.', '_');
                        string TempDirectory = Path.Combine(Path.GetTempPath(), subdirectory);
                        Directory.CreateDirectory(TempDirectory);
                        string imageFullName = Path.Combine(TempDirectory, imageName);
                        FileStream fs = new FileStream(imageFullName, FileMode.Create);
                        byte[] imgdata = pdfImageObject.GetImageAsBytes();
                        fs.Write(imgdata, 0, imgdata.Length);
                        fs.Flush();
                        fs.Close();
                        filesNames.Add(imageFullName);
                    }
                    catch
                    {
                        // ignore object
                    }
                }
            }
        }
        pdfDoc.Dispose();
        return filesNames;
    }
}