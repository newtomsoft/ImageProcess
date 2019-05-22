using FreeImageAPI;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Drawing;
using System.IO;

namespace ImageProcessLib
{
    public class ImageProcess : IDisposable
    {
        #region members
        public FreeImageBitmap Bitmap { get; private set; }
        public string FullNameOfFile { get; }
        public string NameOfFile { get; }
        public FREE_IMAGE_FORMAT FormatImage { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        private const int MaxNumberOfPointsForEstimateSimilarColors = 400;
        #endregion
        public ImageProcess(string fullNameOfFile)
        {
            FullNameOfFile = fullNameOfFile;
            NameOfFile = Path.GetFileName(FullNameOfFile);
            try
            {
                Bitmap = new FreeImageBitmap(FullNameOfFile);
                FormatImage = Bitmap.ImageFormat;
                Bitmap.ConvertColorDepth(FREE_IMAGE_COLOR_DEPTH.FICD_24_BPP);
                Width = Bitmap.Width;
                Height = Bitmap.Height;
            }
            catch (Exception e)
            {

                FormatImage = FREE_IMAGE_FORMAT.FIF_UNKNOWN;
                Width = 0;
                Height = 0;
                throw e;
            }
        }
        public ImageProcess(MemoryStream theStream, string fullNameOfImage)
        {
            FullNameOfFile = fullNameOfImage;
            NameOfFile = Path.GetFileName(FullNameOfFile);
            Bitmap = new FreeImageBitmap(theStream, FREE_IMAGE_FORMAT.FIF_JPEG);
            //Bitmap = new FreeImageBitmap(theStream);
            Bitmap.ConvertColorDepth(FREE_IMAGE_COLOR_DEPTH.FICD_24_BPP);
            FormatImage = Bitmap.ImageFormat;
            Width = Bitmap.Width;
            Height = Bitmap.Height;
        }

        void IDisposable.Dispose() => ((IDisposable)Bitmap).Dispose();
        private bool IsEdgeHaveSimilarColors(int thickness, ImageEdge edge, int level)
        {
            double minimumStdDeviation = level;
            double step;
            int nbCount;
            int count;
            int heightOrWidth = Height;
            int moveStrip = 0;
            int beginIndexStrip = 0;
            switch (edge)
            {
                case ImageEdge.left:
                    heightOrWidth = Height;
                    moveStrip = 1;
                    beginIndexStrip = 0;
                    break;
                case ImageEdge.right:
                    heightOrWidth = Height;
                    moveStrip = -1;
                    beginIndexStrip = Width - 1;
                    break;
                case ImageEdge.top:
                    heightOrWidth = Width;
                    moveStrip = -1;
                    beginIndexStrip = Height - 1;
                    break;
                case ImageEdge.bottom:
                    heightOrWidth = Width;
                    moveStrip = 1;
                    beginIndexStrip = 0;
                    break;
            }
            if (heightOrWidth * thickness > MaxNumberOfPointsForEstimateSimilarColors)
            {
                step = (double)heightOrWidth * thickness / MaxNumberOfPointsForEstimateSimilarColors;
                nbCount = MaxNumberOfPointsForEstimateSimilarColors;
            }
            else
            {
                step = 1;
                nbCount = heightOrWidth * thickness;
            }

            Color colorPixel;
            ulong sumColorR = 0, sumColorG = 0, sumColorB = 0;
            double indexStepWithDot = 0;
            int indexStepInt;
            int indexStrip = beginIndexStrip;
            for (count = 0; count < nbCount; count++)
            {
                indexStepInt = (int)Math.Round(indexStepWithDot);
                if (indexStepInt >= heightOrWidth)
                {
                    indexStrip += moveStrip;
                    indexStepInt -= heightOrWidth;
                    indexStepWithDot -= heightOrWidth;
                }
                if(edge == ImageEdge.bottom || edge == ImageEdge.top)
                {
                    colorPixel = Bitmap.GetPixel(indexStepInt, indexStrip);
                }
                else
                {
                    colorPixel = Bitmap.GetPixel(indexStrip, indexStepInt);
                }
                sumColorR += colorPixel.R;
                sumColorG += colorPixel.G;
                sumColorB += colorPixel.B;
                indexStepWithDot += step;
            }
            double averageR = sumColorR / (double)nbCount;
            double averageG = sumColorG / (double)nbCount;
            double averageB = sumColorB / (double)nbCount;

            double R2 = 0, G2 = 0, B2 = 0;
            indexStepWithDot = 0;
            indexStrip = beginIndexStrip;
            for (count = 0; count < nbCount; count++)
            {
                indexStepInt = (int)Math.Round(indexStepWithDot);
                if (indexStepInt >= heightOrWidth)
                {
                    indexStrip += moveStrip;
                    indexStepInt -= heightOrWidth;
                    indexStepWithDot -= heightOrWidth;
                }
                if (edge == ImageEdge.bottom || edge == ImageEdge.top)
                {
                    colorPixel = Bitmap.GetPixel(indexStepInt, indexStrip);
                }
                else
                {
                    colorPixel = Bitmap.GetPixel(indexStrip, indexStepInt);
                }
                R2 += Math.Pow(colorPixel.R - averageR, 2);
                G2 += Math.Pow(colorPixel.G - averageG, 2);
                B2 += Math.Pow(colorPixel.B - averageB, 2);
                indexStepWithDot += step;
            }
            double stdDeviationR = Math.Sqrt(R2 / nbCount), stdDeviationG = Math.Sqrt(G2 / nbCount), stdDeviationB = Math.Sqrt(B2 / nbCount);
            if (stdDeviationR <= minimumStdDeviation && stdDeviationG <= minimumStdDeviation && stdDeviationB <= minimumStdDeviation)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void DeleteStrips(int stripLevel)
        {
            const int initialThickness = 256;
            int currentThickness;
            bool toDelete = true, toDeleteEdge;
            int edgeValue = 0, left, top, right, bottom;
            int[] ltrb = new int[4];
            bool boolEdge;
            if (FormatImage != FREE_IMAGE_FORMAT.FIF_UNKNOWN)
            {
                while (toDelete)
                {
                    foreach (ImageEdge currentEdge in Enum.GetValues(typeof(ImageEdge)))
                    {
                        toDeleteEdge = true;
                        currentThickness = initialThickness;
                        while (toDeleteEdge)
                        {
                            boolEdge = IsEdgeHaveSimilarColors(currentThickness, currentEdge, stripLevel);
                            if (boolEdge)
                            {
                                toDeleteEdge = false;
                            }
                            else
                            {
                                currentThickness /= 2;
                                toDeleteEdge = currentThickness==0?false:true;
                            }
                            edgeValue = currentThickness;
                        }
                        ltrb[(int)currentEdge] = edgeValue;
                    }
                    left = ltrb[(int)ImageEdge.left];
                    top = ltrb[(int)ImageEdge.top];
                    right = ltrb[(int)ImageEdge.right];
                    bottom = ltrb[(int)ImageEdge.bottom];
                    if (left + right < Width && bottom + top < Height)
                    {
                        Bitmap.EnlargeCanvas<bool>(-left, -top, -right, -bottom, null);
                        if (Width != Bitmap.Width || Height != Bitmap.Height)
                        {
                            Width = Bitmap.Width;
                            Height = Bitmap.Height;
                            toDelete = true;
                        }
                        else
                        {
                            toDelete = false;
                        }
                    }
                    else
                    {
                        throw new Exception("All pixels are similar");
                    }
                }
            }
        }
        public void SaveToWebpFree(string pathImageSave = @"Save_webp\")
        {
            if (FormatImage != FREE_IMAGE_FORMAT.FIF_UNKNOWN)
            {
                string fileExtension = ".webp";
                //Directory.CreateDirectory(Path.Combine(NameOfDirectory, pathImageSave));
                //Bitmap.Save(NameOfDirectory + pathImageSave + NameOfFile + "free" + fileExtension, FREE_IMAGE_FORMAT.FIF_UNKNOWN); //TODO
            }
        }
        public void SaveToWebp(string fullNameToSave)
        {
            if (FormatImage != FREE_IMAGE_FORMAT.FIF_UNKNOWN)
            {
                Bitmap bitmap;
                WebPFormat imageWebp = new WebPFormat();
                try
                {
                    bitmap = new Bitmap(FullNameOfFile);
                }
                catch
                {
                    MemoryStream memoryStream = new MemoryStream();
                    Bitmap.Save(memoryStream, FREE_IMAGE_FORMAT.FIF_BMP);
                    bitmap = new Bitmap(memoryStream);
                }
                imageWebp.Save(fullNameToSave, bitmap, 24);
                bitmap.Dispose();
            }
        }
        public void SaveToWebp(MemoryStream MemoryStreamToSave)
        {
            if (FormatImage != FREE_IMAGE_FORMAT.FIF_UNKNOWN)
            {
                Bitmap bitmap;
                WebPFormat imageWebp = new WebPFormat();
                try
                {
                    bitmap = new Bitmap(FullNameOfFile);
                }
                catch
                {
                    MemoryStream memoryStream = new MemoryStream();
                    Bitmap.Save(memoryStream, FREE_IMAGE_FORMAT.FIF_BMP);
                    bitmap = new Bitmap(memoryStream);
                }
                imageWebp.Save(MemoryStreamToSave, bitmap, 24);
                bitmap.Dispose();
            }
        }
        public void Save(MemoryStream memoryStream)
        {
            if (FormatImage != FREE_IMAGE_FORMAT.FIF_UNKNOWN)
            {
                try
                {
                    Bitmap.Save(memoryStream, FREE_IMAGE_FORMAT.FIF_JPEG);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        public void SaveTo(FileFormat outputFileFormat, string fullPathImageSave)
        {
            FREE_IMAGE_FORMAT outputFormat;
            if (FormatImage != FREE_IMAGE_FORMAT.FIF_UNKNOWN)
            {
                string fileExtension;
                switch (outputFileFormat)
                {
                    case FileFormat.Jp2:
                        outputFormat = FREE_IMAGE_FORMAT.FIF_JP2;
                        fileExtension = ".jp2";
                        break;
                    case FileFormat.Jpg:
                        outputFormat = FREE_IMAGE_FORMAT.FIF_JPEG;
                        fileExtension = ".jpg";
                        break;
                    case FileFormat.Png:
                        outputFormat = FREE_IMAGE_FORMAT.FIF_PNG;
                        fileExtension = ".png";
                        break;
                    case FileFormat.Tiff:
                        outputFormat = FREE_IMAGE_FORMAT.FIF_TIFF;
                        fileExtension = ".tif";
                        break;
                    case FileFormat.Gif:
                        outputFormat = FREE_IMAGE_FORMAT.FIF_GIF;
                        fileExtension = ".gif";
                        break;
                    case FileFormat.Bmp:
                        outputFormat = FREE_IMAGE_FORMAT.FIF_BMP;
                        fileExtension = ".bmp";
                        break;
                    case FileFormat.Webp:
                        outputFormat = FREE_IMAGE_FORMAT.FIF_UNKNOWN;
                        fileExtension = ".webp";
                        break;
                    case FileFormat.Pdf:
                        outputFormat = FREE_IMAGE_FORMAT.FIF_JPEG;
                        fileExtension = ".jpg";
                        break;
                    default:
                        fileExtension = "";
                        outputFormat = FormatImage;
                        break;
                }
                Directory.CreateDirectory(fullPathImageSave);
                try
                {
                    string fullNameToSave = Path.Combine(fullPathImageSave, NameOfFile + fileExtension);
                    if (outputFormat != FREE_IMAGE_FORMAT.FIF_UNKNOWN)
                    {
                        Bitmap.Save(fullNameToSave, outputFormat);
                    }
                    else
                    {
                        SaveToWebp(fullNameToSave);
                    }
                    if (outputFileFormat == FileFormat.Pdf)
                    {
                        PdfDocument thePdfDocument = new PdfDocument();
                        FileStream filestream = new FileStream(fullNameToSave, FileMode.Open);
                        XImage img = XImage.FromStream(filestream);
                        XGraphics xgr = XGraphics.FromPdfPage(thePdfDocument.AddPage(new PdfPage { Width = img.PointWidth, Height = img.PointHeight }));
                        xgr.DrawImage(img, 0, 0);
                        xgr.Dispose();
                        filestream.Dispose();
                        File.Delete(fullNameToSave);
                        thePdfDocument.Save(Path.Combine(fullPathImageSave, NameOfFile + ".pdf"));
                        thePdfDocument.Close();
                        thePdfDocument.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        public void Resize(int width, int height)
        {
            Size size = new Size(width, height);
            Bitmap.Rescale(size, FREE_IMAGE_FILTER.FILTER_LANCZOS3);
        }
    }
}
