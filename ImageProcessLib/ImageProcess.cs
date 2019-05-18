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
        private int GetNumberOfSimilarColumnsAtLeft(int stripLevel)
        {
            int i = 0;
            bool similarColors = IsColumnHaveSimilarColors(0, stripLevel);
            while (similarColors)
            {
                similarColors = IsColumnHaveSimilarColors(++i, stripLevel);
            }
            return i;
        }
        private int GetNumberOfSimilarColumnsAtRight(int stripLevel)
        {
            int i = Width - 1;
            while (IsColumnHaveSimilarColors(i--, stripLevel)) ;
            return Width - i - 2;
        }
        private int GetNumberOfSimilarLinesAtTop(int stripLevel)
        {
            int i = Height - 1;
            bool similarColors = IsLineHaveSimilarColors(Height - 1, stripLevel);
            while (similarColors)
            {
                similarColors = IsLineHaveSimilarColors(--i, stripLevel);
            }
            return Height - i - 1;
        }
        private int GetNumberOfSimilarLinesAtBottom(int stripLevel)
        {
            int i = 0;
            bool similarColors = IsLineHaveSimilarColors(0, stripLevel);
            while (similarColors)
            {
                similarColors = IsLineHaveSimilarColors(++i, stripLevel);
            }
            return i;
        }
        private bool IsColumnHaveSimilarColors(int indexCol, int level)
        {
            double minimumStdDeviation = level;
            double step;
            int nbCount;
            int count;
            if (Height > MaxNumberOfPointsForEstimateSimilarColors)
            {
                step = (double)Height / MaxNumberOfPointsForEstimateSimilarColors;
                nbCount = MaxNumberOfPointsForEstimateSimilarColors;
            }
            else
            {
                step = 1;
                nbCount = Height;
            }

            Color colorPixel;
            if (indexCol < 0 || indexCol >= Width)
            {
                return false;
            }
            ulong sumColorR = 0, sumColorG = 0, sumColorB = 0;

            double jDouble = 0;
            int j;
            for (count = 0; count < nbCount; count++)
            {
                j = (int)Math.Round(jDouble);
                colorPixel = Bitmap.GetPixel(indexCol, j);
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
            for (count = 0; count < nbCount; count++)
            {
                j = (int)Math.Round(jDouble);
                colorPixel = Bitmap.GetPixel(indexCol, j);
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
        private bool IsLineHaveSimilarColors(int indexLine, int level)
        {
            double minimumStdDeviation = level;
            double step;
            int nbCount;
            int count;
            if (Width > MaxNumberOfPointsForEstimateSimilarColors)
            {
                step = (double)Width / MaxNumberOfPointsForEstimateSimilarColors;
                nbCount = MaxNumberOfPointsForEstimateSimilarColors;
            }
            else
            {
                step = 1;
                nbCount = Width;
            }

            Color colorPixel;
            if (indexLine < 0 || indexLine >= Height)
            {
                return false;
            }
            ulong sumColorR = 0, sumColorG = 0, sumColorB = 0;
            double iDouble = 0;
            int i;
            for (count = 0; count < nbCount; count++)
            {
                i = (int)Math.Round(iDouble);
                colorPixel = Bitmap.GetPixel(i, indexLine);
                sumColorR += colorPixel.R;
                sumColorG += colorPixel.G;
                sumColorB += colorPixel.B;
                iDouble += step;
            }
            double averageR = sumColorR / (double)nbCount;
            double averageG = sumColorG / (double)nbCount;
            double averageB = sumColorB / (double)nbCount;

            double R2 = 0, G2 = 0, B2 = 0;
            iDouble = 0;
            for (count = 0; count < nbCount; count++)
            {
                i = (int)Math.Round(iDouble);
                colorPixel = Bitmap.GetPixel(i, indexLine);
                R2 += Math.Pow(colorPixel.R - averageR, 2);
                G2 += Math.Pow(colorPixel.G - averageG, 2);
                B2 += Math.Pow(colorPixel.B - averageB, 2);
                iDouble += step;
            }
            double stdDeviationR = Math.Sqrt(R2 / nbCount), stdDeviationG = Math.Sqrt(G2 / nbCount), stdDeviationB = Math.Sqrt(B2 / Width);
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
                int left, top, right, bottom;
                bool toDelete = true;
                while (toDelete)
                {
                    left = GetNumberOfSimilarColumnsAtLeft(stripLevel);
                    top = GetNumberOfSimilarLinesAtTop(stripLevel);
                    right = GetNumberOfSimilarColumnsAtRight(stripLevel);
                    bottom = GetNumberOfSimilarLinesAtBottom(stripLevel);

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
                    string fullNameToSave = Path.Combine(NameOfDirectory, pathImageSave, NameOfFile+fileExtension);
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
                        thePdfDocument.Save(Path.Combine(NameOfDirectory, pathImageSave, NameOfFile+".pdf"));
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
