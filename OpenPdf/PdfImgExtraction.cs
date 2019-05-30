using System.Collections.Generic;
using System.IO;
using Path = System.IO.Path;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

public class PdfImgExtraction
{
    public static List<string> ExtractImage(string fileName)
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