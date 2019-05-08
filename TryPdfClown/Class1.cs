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

                //if (content is Text)
                //{
                //    ContentScanner.TextWrapper text = (ContentScanner.TextWrapper)level.CurrentWrapper;
                //    //ContentScanner.GraphicsState test = level.getState();
                //    foreach (ContentScanner.TextStringWrapper textString in text.TextStrings)
                //    {
                //        org.pdfclown.objects.Rectangle rf = (RectangleF)textString.Box;
                //        /*txtOutput.Text = txtOutput.Text + Environment.NewLine + "Text [font size: " + textString.Style.FontSize + " ],[font Name: " +
                //            textString.Style.Font.Name + " ]: " + textString.Text + "[position = left :" + rf.Left.ToString() + " & Top: " + rf.Top.ToString() + "X:" + rf.X.ToString() + "Y:" + rf.Y.ToString();*/

                //        txtOutput.Text = txtOutput.Text + Environment.NewLine + textString.Text;

                //    }

                //}

                //else if (content is ShowText)
                //{
                //    it.stefanochizzolini.clown.documents.contents.fonts.Font font = level.State.Font;
                //    txtOutput.Text = txtOutput.Text + (font.Decode(((ShowText)content).Text));

                //}

                if (content is ContainerObject)
                {
                    // Scan the inner level!
                    Extract(level.ChildLevel, page);
                }
                else if (content is InlineImage)
                {
                    ContentScanner.InlineImageWrapper img = (ContentScanner.InlineImageWrapper)level.CurrentWrapper;
                   // ExportImage(img.InlineImage.Body.Value, System.IO.Path.DirectorySeparatorChar + "ImageExtractionSample_" + (index++) + ".jpg");
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
                            ;
                            PdfDictionary header = ((PdfStream)dataObject).Header;
                            if (header.ContainsKey(PdfName.Type) && header[PdfName.Type].Equals(PdfName.XObject) && header[PdfName.Subtype].Equals(PdfName.Image))
                            {
                                if (header[PdfName.Filter].Equals(PdfName.Image)) // JPEG image.
                                {
                                    // Get the image data (keeping it encoded)!
                                    IBuffer body1 = ((PdfStream)dataObject).GetBody(false);
                                    //object h1 = PdfName.ColorSpace;



                                    // Export the image!

                                    ExportImage( body1, System.IO.Path.DirectorySeparatorChar + "Image_" + (index++) + ".png"
                                      );
                                }
                            }



                        }


                        //else if (content is it.stefanochizzolini.clown.documents.interaction.annotations.Link)
                        //{
                        //    Dictionary<RectangleF?, List<ITextString>> textStrings = null;
                        //    PageAnnotations annotations = page.Annotations;
                        //    TextExtractor extractor = new TextExtractor();

                        //    if (annotations == null)
                        //    {
                        //        Console.WriteLine("No annotations here.");
                        //        continue;
                        //    }

                        //    foreach (it.stefanochizzolini.clown.documents.interaction.annotations.Annotation annotation in annotations)
                        //    {
                        //        if (annotation is it.stefanochizzolini.clown.documents.interaction.annotations.Link)
                        //        {

                        //            if (textStrings == null)
                        //            { textStrings = extractor.Extract(page); }

                        //            it.stefanochizzolini.clown.documents.interaction.annotations.Link link = (it.stefanochizzolini.clown.documents.interaction.annotations.Link)annotation;
                        //            RectangleF linkBox = link.Box;
                        //            StringBuilder linkTextBuilder = new StringBuilder();
                        //            foreach (ITextString linkTextString in extractor.Filter(textStrings, linkBox))
                        //            { linkTextBuilder.Append(linkTextString.Text); }
                        //            string bb = linkTextBuilder.ToString();
                        //            txtOutput.Text = txtOutput.Text + "Link '" + linkTextBuilder.ToString();
                        //            txtOutput.Text = txtOutput.Text + "    Position: "
                        //                + "x:" + Math.Round(linkBox.X) + ","
                        //                + "y:" + Math.Round(linkBox.Y) + ","
                        //                + "w:" + Math.Round(linkBox.Width) + ","
                        //                + "h:" + Math.Round(linkBox.Height);

                        //        }

                        //    }
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

                    /*=============================================================================*/
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

            Console.WriteLine("Output: " + outputPath);
        }


        //private byte[] BmpToBytes_Unsafe(Bitmap bmp)
        //{

        //    BitmapData bData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, IMAGE_WIDTH, IMAGE_HEIGHT),
        //        ImageLockMode.ReadOnly,
        //        PixelFormat.Format24bppRgb);

        //    int lineSize = bData.Width * 3;
        //    int byteCount = lineSize * bData.Height;
        //    byte[] bmpBytes = new byte[byteCount];
        //    byte[] tempLine = new byte[lineSize];
        //    int bmpIndex = 0;

        //    IntPtr scan = new IntPtr(bData.Scan0.ToInt32() + (lineSize * (bData.Height - 1)));

        //    for (int i = 0; i < bData.Height; i++)
        //    {
        //        Marshal.Copy(scan, tempLine, 0, lineSize);
        //        scan = new IntPtr(scan.ToInt32() - bData.Stride);
        //        tempLine.CopyTo(bmpBytes, bmpIndex);
        //        bmpIndex += lineSize;
        //    }

        //    bmp.UnlockBits(bData);

        //    return bmpBytes;
        //}
    }


}