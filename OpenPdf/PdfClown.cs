using System;
using System.Collections.Generic;
using System.Drawing;
using org.pdfclown.documents;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.contents.objects;
using org.pdfclown.tools;
using System.IO;
using org.pdfclown.bytes;
using org.pdfclown.objects;
using org.pdfclown.documents.contents.xObjects;
using Path = System.IO.Path;

public class PdfClown
{
    public int indexImage=0;

    public List<string> GetImages(string FileName)
    {
        List<string> filesNames = new List<string>();
        org.pdfclown.files.File file;
        Document document;
        try
        {
            file = new org.pdfclown.files.File(FileName);
            document = file.Document;
        }
        catch
        {
            throw;
        }
        PageStamper stamper = new PageStamper();
        foreach (Page page in document.Pages)
        {
            stamper.Page = page;
            filesNames.AddRange(Extract(new ContentScanner(page), page));
            stamper.Flush();

        }
        return filesNames;
    }

    private List<string> Extract(ContentScanner level, Page page)
    {
        List<string> filesNames = new List<string>();
        if (level == null)
            return null;

        while (level.MoveNext())
        {
            ContentObject content = level.Current;
            if (content is ContainerObject)
            {
                filesNames.AddRange(Extract(level.ChildLevel, page));
            }
            else if (content is GraphicsObject)
            {
                ContentScanner.GraphicsObjectWrapper objectWrapper = level.CurrentWrapper;
                if (objectWrapper == null)
                {
                    continue;
                }
                if (objectWrapper is ContentScanner.XObjectWrapper xObjectWrapper)
                {
                    var xobject = xObjectWrapper.XObject;
                    if (xobject is ImageXObject)
                    {
                        PdfDataObject dataObject = xobject.BaseDataObject;
                        PdfDictionary header = ((PdfStream)dataObject).Header;
                        if (header.ContainsKey(PdfName.Type) && header[PdfName.Type].Equals(PdfName.XObject) && header[PdfName.Subtype].Equals(PdfName.Image))
                        {
                            IBuffer body1 = ((PdfStream)dataObject).GetBody(false);
                            filesNames.Add(SaveTempImage(body1, indexImage++.ToString()));
                        }
                    }
                }
            }
        }
        return filesNames;
    }
    private string SaveTempImage(IBuffer data, string fileName)
    {
        string fullName = Path.Combine(Path.GetTempPath(), fileName);
        try
        {
            FileStream fileStream = new FileStream(fullName, FileMode.Create, FileAccess.Write);
            BinaryWriter memoryWriter = new BinaryWriter(fileStream);
            memoryWriter.Write(data.ToByteArray());
            fileStream.Dispose();
            return fullName;
        }
        catch (Exception e)
        {
            throw new Exception(" file writing has failed.", e);
        }
    }
}
