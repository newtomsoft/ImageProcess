using FreeImageAPI;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Drawing;
using System.IO;

namespace ImageProcessLib
{
    public class ImageProcess : IDisposable
    {
        public FreeImageBitmap Bitmap { get; private set; }
        public string FullNameOfFile { get; }
        public string NameOfFile { get; }
        public string NameOfDirectory { get; }
        public FREE_IMAGE_FORMAT FormatImage { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public ImageProcess(string fullNameOfFile)
        {
            FullNameOfFile = fullNameOfFile;
            var indexSlash = FullNameOfFile.LastIndexOf('\\');
            NameOfDirectory = FullNameOfFile.Substring(0, indexSlash + 1);
            NameOfFile = FullNameOfFile.Substring(indexSlash + 1);
            try
            {
                Bitmap = new FreeImageBitmap(FullNameOfFile);
                FormatImage = Bitmap.ImageFormat;
                FreeImageBitmap bitmapTemp = new FreeImageBitmap(Bitmap.GetColorConvertedInstance(FREE_IMAGE_COLOR_DEPTH.FICD_24_BPP));
                Bitmap.Dispose();
                Bitmap = bitmapTemp;
                Width = Bitmap.Width;
                Height = Bitmap.Height;
            }
            catch (Exception)
            {
                FormatImage = FREE_IMAGE_FORMAT.FIF_UNKNOWN;
                Width = 0;
                Height = 0;
            }
        }
        public ImageProcess(string nameOfFile, string nameOfDirectory) : this(nameOfDirectory + nameOfFile)
        {
        }
        public ImageProcess(FreeImageBitmap theBitmap, string fullNameOfFile)
        {
            FullNameOfFile = fullNameOfFile;
            NameOfDirectory = @"./";
            NameOfFile = FullNameOfFile.Substring(NameOfDirectory.Length);
            Bitmap = theBitmap;
            FormatImage = Bitmap.ImageFormat;
            Width = Bitmap.Width;
            Height = Bitmap.Height;
        }
        void IDisposable.Dispose() => ((IDisposable)Bitmap).Dispose();
        private int GetNumberOfSimilarColumnsAtLeft()
        {
            int i = 0;
            bool similarColors = IsColumnHaveSimilarColors(0);
            while (similarColors)
            {
                similarColors = IsColumnHaveSimilarColors(++i);
            }
            return i;
        }
        private int GetNumberOfSimilarColumnsAtRight()
        {
            int i = Width - 1;
            bool similarColors = IsColumnHaveSimilarColors(Width - 1);
            while (similarColors)
            {
                similarColors = IsColumnHaveSimilarColors(--i);
            }
            return Width - i - 1;
        }
        private int GetNumberOfSimilarLinesAtBottom()
        {
            int i = Height - 1;
            bool similarColors = IsLineHaveSimilarColors(Height - 1);
            while (similarColors)
            {
                similarColors = IsLineHaveSimilarColors(--i);
            }
            return Height - i - 1;
        }
        private int GetNumberOfSimilarLinesAtTop()
        {
            int i = 0;
            bool similarColors = IsLineHaveSimilarColors(0);
            while (similarColors)
            {
                similarColors = IsLineHaveSimilarColors(++i);
            }
            return i;
        }
        public void DeleteStrips()
        {
            if (FormatImage != FREE_IMAGE_FORMAT.FIF_UNKNOWN)
            {
                int left = GetNumberOfSimilarColumnsAtLeft();
                int right = GetNumberOfSimilarColumnsAtRight();
                Bitmap = Bitmap.Copy(left, Height, Width - right, 0);
                Width = Bitmap.Width;
                Height = Bitmap.Height;

                int bottom = GetNumberOfSimilarLinesAtBottom();
                int top = GetNumberOfSimilarLinesAtTop();
                Bitmap = Bitmap.Copy(0, Height - top, Width, bottom);
                Width = Bitmap.Width;
                Height = Bitmap.Height;

                left = GetNumberOfSimilarColumnsAtLeft();
                right = GetNumberOfSimilarColumnsAtRight();
                Bitmap = Bitmap.Copy(left, Height, Width - right, 0);
                Width = Bitmap.Width;
                Height = Bitmap.Height;
            }
        }
        private bool IsColumnHaveSimilarColors(int indexCol)
        {
            double step;
            int nbCount;
            int count;
            if (Height>400)
            {
                step = (double)Height / 400;
                nbCount = 400;
            }
            else
            {
                step = 1;
                nbCount = Height;
            }

            const double minimumStdDeviation = 23;
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
            if (stdDeviationR < minimumStdDeviation && stdDeviationG < minimumStdDeviation && stdDeviationB < minimumStdDeviation)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool IsLineHaveSimilarColors(int indexLine)
        {
            double step;
            int nbCount;
            int count;
            if (Width > 400)
            {
                step = (double)Width / 400;
                nbCount = 400;
            }
            else
            {
                step = 1;
                nbCount = Height;
            }

            const double minimumStdDeviation = 23;
            Color colorPixel;
            if (indexLine < 0 || indexLine >= Height)
            {
                return false;
            }
            ulong sumColorA = 0, sumColorR = 0, sumColorG = 0, sumColorB = 0;
            double iDouble = 0;
            int i;
            for (count = 0; count < nbCount; count++)
            {
                i = (int)Math.Round(iDouble);
                colorPixel = Bitmap.GetPixel(i, indexLine);
                sumColorA += colorPixel.A;
                sumColorR += colorPixel.R;
                sumColorG += colorPixel.G;
                sumColorB += colorPixel.B;
                iDouble += step;
            }
            double averageA = sumColorA / (double)nbCount;
            double averageR = sumColorR / (double)nbCount;
            double averageG = sumColorG / (double)nbCount;
            double averageB = sumColorB / (double)nbCount;

            double A2 = 0, R2 = 0, G2 = 0, B2 = 0;
            iDouble = 0;
            for (count = 0; count < nbCount; count++)
            {
                i = (int)Math.Round(iDouble);
                colorPixel = Bitmap.GetPixel(i, indexLine);
                A2 += Math.Pow(colorPixel.A - averageA, 2);
                R2 += Math.Pow(colorPixel.R - averageR, 2);
                G2 += Math.Pow(colorPixel.G - averageG, 2);
                B2 += Math.Pow(colorPixel.B - averageB, 2);
                iDouble += step;
            }
            double stdDeviationA = Math.Sqrt(A2 / nbCount), stdDeviationR = Math.Sqrt(R2 / nbCount), stdDeviationG = Math.Sqrt(G2 / nbCount), stdDeviationB = Math.Sqrt(B2 / Width);
            if (stdDeviationA < minimumStdDeviation && stdDeviationR < minimumStdDeviation && stdDeviationG < minimumStdDeviation && stdDeviationB < minimumStdDeviation)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public void SaveToWebpFree(string pathImageSave = @"Save_webp\")
        {
            if (FormatImage != FREE_IMAGE_FORMAT.FIF_UNKNOWN)
            {
                string fileExtension = ".webp";
                Directory.CreateDirectory(NameOfDirectory + pathImageSave);
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
                    Bitmap.Save(fullNameToSave + ".temp.bmp", FREE_IMAGE_FORMAT.FIF_BMP);
                    bitmap = new Bitmap(fullNameToSave + ".temp.bmp");
                }
                imageWebp.Save(fullNameToSave, bitmap, 24);
                bitmap.Dispose();
                File.Delete(fullNameToSave + ".temp.bmp");
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
                Directory.CreateDirectory(NameOfDirectory + pathImageSave);
                try
                {
                    string fullNameToSave = NameOfDirectory + pathImageSave + NameOfFile + fileExtension;
                    if(outputFormat!= FREE_IMAGE_FORMAT.FIF_UNKNOWN)
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
                        thePdfDocument.Save(NameOfDirectory + pathImageSave + NameOfFile +".pdf");
                        thePdfDocument.Close();
                        thePdfDocument.Dispose();
                    }
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }
        public void Resize(int width, int height)
        {
            Size size = new Size(width, height);
            Bitmap.Rescale(size,FREE_IMAGE_FILTER.FILTER_LANCZOS3);
        }
    }
}
