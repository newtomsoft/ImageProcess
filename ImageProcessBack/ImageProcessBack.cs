using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageProcessLib;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

public class ImageProcessBack
{
    #region members
    public bool PdfFusion;
    public bool DeleteOrigin;
    public bool DeleteStrip;
    public int StripLevel;
    public string PathSave;
    public List<string> FullNameOfImagesToProcess;
    public PdfDocument ThePdfDocument;
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
            try
            {
                PdfClown pdfFile = new PdfClown();
                List<MemoryStream> memorystreams = pdfFile.InitiateProcess(fullNameOfImage);
                if (memorystreams.Count() != 0)
                {
                    int i = 0; // TODO cleancode
                    foreach (MemoryStream memorystream in memorystreams)
                    {
                        i++;
                        using (ImageProcess imageToProcess = new ImageProcess(memorystream, fullNameOfImage + i.ToString()))
                        {
                            if (DeleteStrip)
                            {
                                imageToProcess.DeleteStrips(StripLevel);
                            }

                            if (PdfFusion)
                            {
                                MemoryStream memoryStream = new MemoryStream();
                                imageToProcess.SaveTo(memoryStream);
                                AddPageToPdfDocument(memoryStream);
                            }
                            else
                            {
                                imageToProcess.SaveTo(ImageFormatToSave, PathSave);
                            }
                        }
                    }
                }
                else
                {
                    using (ImageProcess imageToProcess = new ImageProcess(fullNameOfImage))
                    {
                        if (DeleteStrip)
                        {
                            imageToProcess.DeleteStrips(StripLevel);
                        }

                        if (PdfFusion)
                        {
                            MemoryStream memoryStream = new MemoryStream();
                            imageToProcess.SaveTo(memoryStream);
                            AddPageToPdfDocument(memoryStream);
                        }
                        else
                        {
                            imageToProcess.SaveTo(ImageFormatToSave, PathSave);
                        }

                    }
                    if (DeleteOrigin)
                    {
                        File.Delete(fullNameOfImage);
                    }
                }
            }
            catch (Exception ex)
            {
                listErrors += "Erreur : " + ex.Message + "sur image " + fullNameOfImage + "\n";
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
    public void InitPdfDocument()
    {
        ThePdfDocument = new PdfDocument();
    }
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

