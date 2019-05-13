﻿using System;
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

public class PdfClown
{
    public int index;

    public List<MemoryStream> InitiateProcess(string FileName)
    {
        List<MemoryStream> memoryStreams = new List<MemoryStream>();
        org.pdfclown.files.File file;
        Document document;
        try
        {
            file = new org.pdfclown.files.File(FileName);
            document = file.Document;
        }
        catch
        {
            return memoryStreams;
        }
        PageStamper stamper = new PageStamper();
        foreach (Page page in document.Pages)
        {
            stamper.Page = page;
            memoryStreams.AddRange(Extract(new ContentScanner(page), page));
            stamper.Flush();

        }
        return memoryStreams;
    }

    private List<MemoryStream> Extract(ContentScanner level, Page page)
    {
        List<MemoryStream> memoryStreams = new List<MemoryStream>();
        if (level == null)
            return memoryStreams;

        while (level.MoveNext())
        {
            ContentObject content = level.Current;
            if (content is ContainerObject)
            {
                memoryStreams.AddRange(Extract(level.ChildLevel, page));
            }
            else if (content is GraphicsObject)
            {
                ContentScanner.GraphicsObjectWrapper objectWrapper = level.CurrentWrapper;
                if (objectWrapper == null)
                {
                    continue;
                }
                SizeF? imageSize = null;
                if (objectWrapper is ContentScanner.XObjectWrapper)
                {
                    ContentScanner.XObjectWrapper xObjectWrapper = (ContentScanner.XObjectWrapper)objectWrapper;
                    var xobject = xObjectWrapper.XObject;
                    if (xobject is ImageXObject)
                    {
                        imageSize = xobject.Size;
                        PdfDataObject dataObject = xobject.BaseDataObject;
                        PdfDictionary header = ((PdfStream)dataObject).Header;
                        if (header.ContainsKey(PdfName.Type) && header[PdfName.Type].Equals(PdfName.XObject) && header[PdfName.Subtype].Equals(PdfName.Image))
                        {
                            IBuffer body1 = ((PdfStream)dataObject).GetBody(false);
                            MemoryStream memoryStream = ExportImage(body1, @"D:\" + "Image_" + (index++) + ".png");
                            memoryStreams.Add(memoryStream);
                        }
                    }
                }
                else if (objectWrapper is ContentScanner.InlineImageWrapper)
                {
                    InlineImage inlineImage = ((ContentScanner.InlineImageWrapper)objectWrapper).InlineImage;
                    imageSize = inlineImage.Size; // Image native size.
                }
                if (imageSize.HasValue)
                {
                    RectangleF box = objectWrapper.Box.Value; // Image position (location and size) on the page.
                }
            }
        }
        return memoryStreams;
    }
    private MemoryStream ExportImage(IBuffer data, string outputPath)
    {
        try
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter memoryWriter = new BinaryWriter(memoryStream);
            memoryWriter.Write(data.ToByteArray());
            return memoryStream;
        }
        catch (Exception e)
        {
            throw new Exception(outputPath + " file writing has failed.", e);
        }
    }
}