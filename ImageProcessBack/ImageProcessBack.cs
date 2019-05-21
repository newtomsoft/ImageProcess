using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using ImageProcessLib;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

/// <summary>
/// back of the solution
/// manage image processing and call libraries that decode / encode images
/// </summary>
public class ImageProcessBack
{
    #region members
    /// <summary>
    /// true if we want to generate un pdf file containing all images 
    /// </summary>
    public bool PdfFusion;
    /// <summary>
    /// true if we want to delete orginales files
    /// </summary>
    public bool DeleteOrigin;
    /// <summary>
    /// true if we want to delete strip around the image
    /// </summary>
    public bool DeleteStrip;
    /// <summary>
    /// Tolerance level for strips removal
    /// </summary>
    public int StripLevel;
    /// <summary>
    /// relative directory path where save image(s)
    /// </summary>
    public string PathSave;
    /// <summary>
    /// full name of all images we want to process
    /// </summary>
    public List<string> FullNameOfImagesToProcess;
    /// <summary>
    /// The pdf document if we convert image into this format
    /// </summary>
    public PdfDocument ThePdfDocument;
    /// <summary>
    /// format of image or document we want to save (jpg, png, pdf)
    /// </summary>
    public FileFormat ImageFormatToSave;
    #endregion
    public ImageProcessBack()
    {
        PdfFusion = false;
        DeleteOrigin = false;
        DeleteStrip = false;
        FullNameOfImagesToProcess = new List<string>();
        ImageFormatToSave = FileFormat.Unknow;
    }
    /// <summary>
    /// process all images or files containing images in <c>FullNameOfImagesToProcess</c>
    /// </summary>
    /// <returns>string with all warning and errors for show in a MessageBox or similar to alert user</returns>
    public string Process()
    {
        string listErrors = "";
        if (FullNameOfImagesToProcess.Count == 0)
        {
            listErrors = "Erreur\nMerci de choisir des images";
            return listErrors;
        }
        if (PdfFusion)
        {
            InitPdfDocument();
        }
        foreach (string fullNameOfImage in FullNameOfImagesToProcess)
        {
            string mimeType = MimeType.getFromFile(fullNameOfImage);
            List<MemoryStream> memorystreams = new List<MemoryStream>();
            List<string> imagesFullNames = new List<string>();
            FileFormat fileToReadType = FileFormat.Unknow;

            // TODO use temp file for pdf
            switch (mimeType)
            {
                case "application/pdf":
                    PdfClown pdfFile = new PdfClown();
                    memorystreams = pdfFile.GetImages(fullNameOfImage);
                    fileToReadType = FileFormat.Pdf;
                    break;
                case var someVal when new Regex(@"application/x-zip.*").IsMatch(someVal):
                    imagesFullNames = OpenZipToTempFiles(fullNameOfImage);
                    fileToReadType = FileFormat.Zip;
                    break;
                case var someVal when new Regex(@"image/.*").IsMatch(someVal):
                    fileToReadType = FileFormat.Image;
                    imagesFullNames.Add(fullNameOfImage);
                    break;
                default:
                    break;
            }
            switch (fileToReadType)
            {
                case FileFormat.Pdf:
                    int i = 0; // TODO cleancode
                    foreach (MemoryStream memorystream in memorystreams)
                    {
                        i++;
                        using (ImageProcess imageToProcess = new ImageProcess(memorystream, fullNameOfImage + i.ToString()))
                        {
                            if (DeleteStrip)
                            {
                                try
                                {
                                    imageToProcess.DeleteStrips(StripLevel);
                                }
                                catch (Exception ex)
                                {
                                    listErrors += "Erreur : " + ex.Message + " sur " + fullNameOfImage + "(image " + i + ") => bordures inchangées\n";
                                }
                            }

                            if (PdfFusion)
                            {
                                MemoryStream memoryStream = new MemoryStream();
                                imageToProcess.Save(memoryStream);
                                AddPageToPdfDocument(memoryStream);
                            }
                            else
                            {
                                imageToProcess.SaveTo(ImageFormatToSave, PathSave);
                            }
                        }
                    }
                    break;
                case FileFormat.Zip:
                case FileFormat.Image:
                    foreach (string imageFullName in imagesFullNames)
                    {
                        using (ImageProcess imageToProcess = new ImageProcess(imageFullName))
                        {
                            if (DeleteStrip)
                            {
                                try
                                {
                                    imageToProcess.DeleteStrips(StripLevel);
                                }
                                catch (Exception ex)
                                {
                                    listErrors += "Erreur : " + ex.Message + " sur image " + imageFullName + " => bordures inchangées\n";
                                }
                            }

                            if (PdfFusion)
                            {
                                MemoryStream memoryStream = new MemoryStream();
                                imageToProcess.Save(memoryStream);
                                AddPageToPdfDocument(memoryStream);
                            }
                            else
                            {
                                imageToProcess.SaveTo(ImageFormatToSave, PathSave);
                            }

                        }
                        if (DeleteOrigin || fileToReadType == FileFormat.Zip)
                        {
                            File.Delete(imageFullName);
                        }
                    }
                    break;
            }
        }

        if (PdfFusion)
        {
            SavePdfDocument();
        }
        string contentEnd = "Fin de traitement\n";
        FullNameOfImagesToProcess.Clear();
        //TextBoxListFiles.Text = "";
        return contentEnd + listErrors;
    }
    private List<string> OpenZipToTempFiles(string fileZip)
    {
        List<string> fullNamesOfFiles = new List<string>();
        ZipArchive zip;
        try
        {
            zip = ZipFile.Open(fileZip, ZipArchiveMode.Read);
            var entries = zip.Entries;
            List<ZipArchiveEntry> listFiles = new List<ZipArchiveEntry>();
            foreach (var entrie in entries)
            {
                string fileName = entrie.FullName;
                string fullName = Path.Combine(Path.GetTempPath(), fileName);
                if (Path.GetFileName(fullName) == "") continue;
                fullNamesOfFiles.Add(fullName);
                Stream stream = entrie.Open();
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

    /// <summary>
    /// create new pdf document to put image into it after that
    /// </summary>
    public void InitPdfDocument()
    {
        ThePdfDocument = new PdfDocument();
    }
    /// <summary>
    /// add a page into the pdf document witch is into the <c>memoryStream</c>
    /// </summary>
    /// <param name="memoryStream"></param>
    public void AddPageToPdfDocument(MemoryStream memoryStream)
    {
        try
        {
            XImage img = XImage.FromStream(memoryStream);
            XGraphics xgr = XGraphics.FromPdfPage(ThePdfDocument.AddPage(new PdfPage { Width = img.PointWidth, Height = img.PointHeight }));
            xgr.DrawImage(img, 0, 0);
            xgr.Dispose();
        }
        catch
        {
            Console.WriteLine("Image not supported by tool. Please convert before in jpg/gif/png/tiff");
        }
    }
    /// <summary>
    /// save the pdf document after adding all images into
    /// </summary>
    public void SavePdfDocument()
    {
        string fullNameOfOneImage = FullNameOfImagesToProcess[0];
        string pathToSave = Path.GetDirectoryName(fullNameOfOneImage) + @"\SavePdf\";
        Directory.CreateDirectory(pathToSave);
        ThePdfDocument.Save(pathToSave + "SavefromImages.pdf");
        ThePdfDocument.Close();
        ThePdfDocument.Dispose();
    }
}

