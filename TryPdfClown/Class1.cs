using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using org.pdfclown.documents;
using org.pdfclown.files;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.contents.objects;
using org.pdfclown.tools;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.fonts;
using System.IO;
using org.pdfclown.bytes;
using org.pdfclown.objects;
using System.Drawing;
using System.IO;
using org.pdfclown.documents.interaction.annotations;
using org.pdfclown.documents.contents.xObjects;
using System.Drawing.Imaging;

namespace Test1
{

    public class PdfClown
    {
        public int index;

        public void InitiateProcess(string FileName)
        {
            org.pdfclown.files.File file;
            Document document;
            try
            {
                file = new org.pdfclown.files.File(FileName);
                document = file.Document;
            }
            catch
            {
                return;
            }

            PageStamper stamper = new PageStamper();
            foreach (Page page in document.Pages)
            {
                stamper.Page = page;
                Extract(new ContentScanner(page), page);
                stamper.Flush();
            }
        }

        private void Extract(ContentScanner level, Page page)
        {
            string ctype = string.Empty;
            if (level == null)
                return;

            while (level.MoveNext())
            {
                ContentObject content = level.Current;

                string aa = content.GetType().ToString();

                if (content is ContainerObject)
                {
                    Extract(level.ChildLevel, page);
                }
                else if (content is GraphicsObject)
                {

                    ContentScanner.GraphicsObjectWrapper objectWrapper = level.CurrentWrapper;
                    if (objectWrapper == null)
                    {
                        continue;
                    }
                    SizeF? imageSize = null; // Image native size.
                    if (objectWrapper is ContentScanner.XObjectWrapper)
                    {
                        ContentScanner.XObjectWrapper xObjectWrapper = (ContentScanner.XObjectWrapper)objectWrapper;
                        var xobject = xObjectWrapper.XObject;
                        // Is the external object an image?
                        if (xobject is ImageXObject)
                        {
                            imageSize = xobject.Size; // Image native size.
                            PdfDataObject dataObject = xobject.BaseDataObject;
                            PdfDictionary header = ((PdfStream)dataObject).Header;
                            if (header.ContainsKey(PdfName.Type) && header[PdfName.Type].Equals(PdfName.XObject) && header[PdfName.Subtype].Equals(PdfName.Image))
                            {
                                IBuffer body1 = ((PdfStream)dataObject).GetBody(false);
                                ExportImage(body1, @"D:\" + "Image_" + (index++) + ".png");
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
        }
        private void ExportImage(IBuffer data, string outputPath)
        {
            FileStream outputStream;
            try
            {
                outputStream = new FileStream(outputPath, FileMode.CreateNew);
            }
            catch (Exception e)
            {
                throw new Exception(outputPath + " file couldn't be created.", e);
            }
            try
            {
                BinaryWriter writer = new BinaryWriter(outputStream);
                writer.Write(data.ToByteArray());
                writer.Close();
                outputStream.Close();
            }
            catch (Exception e)
            {
                throw new Exception(outputPath + " file writing has failed.", e);
            }
        }
    }
}