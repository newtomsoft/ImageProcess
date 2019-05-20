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
        public string NameOfDirectory { get; }
        public FREE_IMAGE_FORMAT FormatImage { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        private const int MaxNumberOfPointsForEstimateSimilarColors = 400;
        #endregion
        public ImageProcess(string fullNameOfFile)
        {
            FullNameOfFile = fullNameOfFile;
            NameOfDirectory = Path.GetDirectoryName(FullNameOfFile);
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
            NameOfDirectory = Path.GetDirectoryName(FullNameOfFile) + "\\";
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
            double jDouble = 0;
            int j;
            int indexStrip = beginIndexStrip;
            for (count = 0; count < nbCount; count++)
            {
                j = (int)Math.Round(jDouble);
                if (j >= heightOrWidth)
                {
                    indexStrip += moveStrip;
                    j -= heightOrWidth;
                    jDouble -= heightOrWidth;
                }
                if(edge == ImageEdge.bottom || edge == ImageEdge.top)
                {
                    colorPixel = Bitmap.GetPixel(j, indexStrip);
                }
                else
                {
                    colorPixel = Bitmap.GetPixel(indexStrip, j);
                }
                sumColorR += colorPixel.R;
                sumColorG += colorPixel.G;
                sumColorB += colorPixel.B;
                jDouble += step;
            }
            double averageR = sumColorR / (double)nbCount;
            double averageG = sumColorG / (double)nbCount;
            double averageB = sumColorB / (double)nbCount;

            double R2 = 0, G2 = 0, B2 = 0;
            jDouble = 0;
            indexStrip = beginIndexStrip;
            for (count = 0; count < nbCount; count++)
            {
                j = (int)Math.Round(jDouble);
                if (j >= heightOrWidth)
                {
                    indexStrip += moveStrip;
                    j -= heightOrWidth;
                    jDouble -= heightOrWidth;
                }
                if (edge == ImageEdge.bottom || edge == ImageEdge.top)
                {
                    colorPixel = Bitmap.GetPixel(j, indexStrip);
                }
                else
                {
                    colorPixel = Bitmap.GetPixel(indexStrip, j);
                }
                R2 += Math.Pow(colorPixel.R - averageR, 2);
                G2 += Math.Pow(colorPixel.G - averageG, 2);
                B2 += Math.Pow(colorPixel.B - averageB, 2);
                jDouble += step;
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
            if (FormatImage != FREE_IMAGE_FORMAT.FIF_UNKNOWN)
            {
                const int initialThickness = 256;
                int thicknessRight = initialThickness, thicknessLeft = initialThickness, thicknessTop = initialThickness, thicknessBottom = initialThickness;
                bool toDelete = true, toDeleteLeft = true, toDeleteRight = true, toDeleteTop = true, toDeleteBottom = true;
                int left = 0, top = 0, right = 0, bottom = 0;
                bool boolLeft, boolRight, boolTop, boolBottom;
                while (toDelete)
                {
                    while (toDeleteLeft)
                    {
                        boolLeft = IsEdgeHaveSimilarColors(thicknessLeft, ImageEdge.left, stripLevel);
                        if (!boolLeft)
                        {
                            thicknessLeft /= 2;
                            left = thicknessLeft;
                        }
                        if (thicknessLeft == 0)
                        {
                            toDeleteLeft = false;
                        }
                        if (boolLeft)
                        {
                            left = thicknessLeft;
                            toDeleteLeft = false;
                            thicknessLeft = initialThickness;
                        }
                    }
                    while (toDeleteRight)
                    {
                        boolRight = IsEdgeHaveSimilarColors(thicknessRight, ImageEdge.right, stripLevel);
                        if (!boolRight)
                        {
                            thicknessRight /= 2;
                            right = thicknessRight;
                        }
                        if (thicknessRight == 0)
                        {
                            toDeleteRight = false;
                        }
                        if (boolRight)
                        {
                            right = thicknessRight;
                            toDeleteRight = false;
                            thicknessRight = initialThickness;
                        }
                    }
                    while (toDeleteTop)
                    {
                        boolTop = IsEdgeHaveSimilarColors(thicknessTop, ImageEdge.top, stripLevel);
                        if (!boolTop)
                        {
                            thicknessTop /= 2;
                            top = thicknessTop;
                        }
                        if (thicknessTop == 0)
                        {
                            toDeleteTop = false;
                        }
                        if (boolTop)
                        {
                            top = thicknessTop;
                            toDeleteTop = false;
                            thicknessTop = initialThickness;
                        }
                    }
                    while (toDeleteBottom)
                    {
                        boolBottom = IsEdgeHaveSimilarColors(thicknessBottom, ImageEdge.bottom, stripLevel);
                        if (!boolBottom)
                        {
                            thicknessBottom /= 2;
                            bottom = thicknessBottom;
                        }
                        if (thicknessBottom == 0)
                        {
                            toDeleteBottom = false;
                        }
                        if (boolBottom)
                        {
                            bottom = thicknessBottom;
                            toDeleteBottom = false;
                            thicknessBottom = initialThickness;
                        }
                    }
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
                    toDeleteLeft = true;
                    toDeleteRight = true;
                    toDeleteTop = true;
                    toDeleteBottom = true;
                }
            }
        }
        public void SaveToWebpFree(string pathImageSave = @"Save_webp\")
        {
            if (FormatImage != FREE_IMAGE_FORMAT.FIF_UNKNOWN)
            {
                string fileExtension = ".webp";
                Directory.CreateDirectory(Path.Combine(NameOfDirectory, pathImageSave));
                Bitmap.Save(NameOfDirectory + pathImageSave + NameOfFile + "free" + fileExtension, FREE_IMAGE_FORMAT.FIF_UNKNOWN); //TODO
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
        public void SaveTo(FileFormat outputFileFormat, string pathImageSave = @"Save\")
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
                Directory.CreateDirectory(Path.Combine(NameOfDirectory, pathImageSave));
                try
                {
                    string fullNameToSave = Path.Combine(NameOfDirectory, pathImageSave, NameOfFile + fileExtension);
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
                        thePdfDocument.Save(Path.Combine(NameOfDirectory, pathImageSave, NameOfFile + ".pdf"));
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
