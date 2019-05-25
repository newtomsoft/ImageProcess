using System.Collections.Generic;
using System.IO;
using Path = System.IO.Path;
using iTextSharp.text.pdf;
using PdfName = iTextSharp.text.pdf.PdfName;
using PdfObject = iTextSharp.text.pdf.PdfObject;
using iTextSharp.text.pdf.parser;

public class PdfImgExtraction
{
    static public List<string> ExtractImage(string fileName)
    {
        List<string> returnFilesNames = new List<string>();
        // existing pdf path
        PdfReader reader = new PdfReader(fileName);
        PRStream pst;
        PdfImageObject pio;
        PdfObject po;
        // number of objects in pdf document
        int n = reader.XrefSize;
        FileStream fs;
        // set image file location
        for (int i = 0; i < n; i++)
        {
            // get the object at the index i in the objects collection
            po = reader.GetPdfObject(i);
            // object not found so continue
            if (po == null || !po.IsStream())
                continue; // TODO clean code
            //cast object to stream
            pst = (PRStream)po;
            //get the object type
            PdfObject type = pst.Get(PdfName.SUBTYPE);
            //check if the object is the image type object
            if (type != null && type.ToString().Equals(PdfName.IMAGE.ToString()))
            {
                //get the image
                pio = new PdfImageObject(pst);
                string fullName = Path.Combine(Path.GetTempPath(), "image" + i + ".jpg"); //todo clean code
                fs = new FileStream(fullName, FileMode.Create);
                //read bytes of image in to an array
                byte[] imgdata = pio.GetImageAsBytes();
                //write the bytes array to file
                fs.Write(imgdata, 0, imgdata.Length);
                fs.Flush();
                fs.Close();
                returnFilesNames.Add(fullName);
            }
        }
        return returnFilesNames;
    }
}