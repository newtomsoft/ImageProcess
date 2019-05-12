using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using ImageProcessLib;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using RadioButton = System.Windows.Controls.RadioButton;
using CheckBox = System.Windows.Controls.CheckBox;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace ImageProcessUI
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region members
        private ImageProcessBack ImageProcessBack;
        #endregion
        public MainWindow()
        {
            ImageProcessBack = new ImageProcessBack();
            InitializeComponent();
            same.IsChecked = true;
        }
        private void ButtonFiles(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Images files |*.jpg;*.webp;*.bmp;*.jp2;*.png;*.tif;*.gif;*.pdf|" + "All files |*.*";
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "Choisir les images à traiter";
            DialogResult dr = openFileDialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                //sp.Children.Clear();
                ImageProcessBack.FullNameOfImagesToProcess.Clear();
                TextBoxListFiles.Text = "";
                foreach (String fileName in openFileDialog.FileNames)
                {
                    //ShowThumbnail(fileName);
                    ImageProcessBack.FullNameOfImagesToProcess.Add(fileName);
                    TextBoxListFiles.Text += fileName + "\n";
                }
            }
        }
        private void CheckBoxStrips(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            if (checkbox.IsChecked == true)
            {
                ImageProcessBack.DeleteStrip = true;
                low.IsChecked = true;
            }
            else
            {
                ImageProcessBack.DeleteStrip = false;
                low.IsChecked = false;
                medium.IsChecked = false;
                high.IsChecked = false;
            }
        }
        private void RadioButtonStrip(object sender, RoutedEventArgs e)
        {
            RadioButton buttonStrip = sender as RadioButton;
            string strip = buttonStrip.Name;
            switch (strip)
            {
                case "low":
                    ImageProcessBack.StripLevel = 6;
                    break;
                case "medium":
                    ImageProcessBack.StripLevel = 50;
                    break;
                case "high":
                    ImageProcessBack.StripLevel = 100;
                    break;
            }
        }
        private void RadioButtonFormat(object sender, RoutedEventArgs e)
        {
            ImageProcessBack.PdfFusion = false;
            RadioButton buttonFormat = sender as RadioButton;
            string format = buttonFormat.Name;
            switch (format)
            {
                case "same":
                    ImageProcessBack.ImageFormatToSave = FileFormat.Unknow;
                    ImageProcessBack.PathSave = @"save\";
                    break;
                case "png":
                    ImageProcessBack.ImageFormatToSave = FileFormat.Png;
                    ImageProcessBack.PathSave = @"savePng\";
                    break;
                case "jp2":
                    ImageProcessBack.ImageFormatToSave = FileFormat.Jp2;
                    ImageProcessBack.PathSave = @"saveJp2\";
                    break;
                case "jpg":
                    ImageProcessBack.ImageFormatToSave = FileFormat.Jpg;
                    ImageProcessBack.PathSave = @"saveJpg\";
                    break;
                case "tiff":
                    ImageProcessBack.ImageFormatToSave = FileFormat.Tiff;
                    ImageProcessBack.PathSave = @"saveTiff\";
                    break;
                case "gif":
                    ImageProcessBack.ImageFormatToSave = FileFormat.Gif;
                    ImageProcessBack.PathSave = @"saveGif\";
                    break;
                case "pdfFusion":
                    ImageProcessBack.PathSave = "";
                    ImageProcessBack.PdfFusion = true;
                    break;
                case "pdfSingle":
                    ImageProcessBack.ImageFormatToSave = FileFormat.Pdf;
                    ImageProcessBack.PathSave = @"savePdf\";
                    break;
                case "webp":
                    ImageProcessBack.ImageFormatToSave = FileFormat.Webp;
                    ImageProcessBack.PathSave = @"saveWebp\";
                    break;
            }
        }
        private void CheckBoxDeleteOrigin(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            if (checkbox.IsChecked == true)
            {
                ImageProcessBack.DeleteOrigin = true;
            }
            else
            {
                ImageProcessBack.DeleteOrigin = false;
            }
        }
        private void ButtonStartProcess(object sender, RoutedEventArgs e)
        {
            string stringReturn = ImageProcessBack.Process();
            MessageBoxButton buttonEnd = MessageBoxButton.OK;
            MessageBoxImage iconEnd = MessageBoxImage.Information;
            string titleEnd = "Traitement";
            string contentEnd = stringReturn;
            MessageBox.Show(contentEnd, titleEnd, buttonEnd, iconEnd);
            ImageProcessBack.FullNameOfImagesToProcess.Clear();
            TextBoxListFiles.Text = "";
        }
        private void InitPdfDocument()
        {
            ImageProcessBack.ThePdfDocument = new PdfDocument();
        }
        private void AddPageToPdfDocument(MemoryStream memoryStream)
        {
            try
            {
                XImage img = XImage.FromStream(memoryStream);
                XGraphics xgr = XGraphics.FromPdfPage(ImageProcessBack.ThePdfDocument.AddPage(new PdfPage { Width = img.PointWidth, Height = img.PointHeight }));
                xgr.DrawImage(img, 0, 0);
                xgr.Dispose();
            }
            catch
            {
                Console.WriteLine("Image not supported by tool. Please convert before in jpg/gif/png/tiff");
            }
        }
        private void SavePdfDocument()
        {
            string fullNameOfOneImage = ImageProcessBack.FullNameOfImagesToProcess[0];
            string pathToSave = Path.GetDirectoryName(fullNameOfOneImage) + @"\SavePdf\";
            Directory.CreateDirectory(pathToSave);
            ImageProcessBack.ThePdfDocument.Save(pathToSave + "SavefromImages.pdf");
            ImageProcessBack.ThePdfDocument.Close();
            ImageProcessBack.ThePdfDocument.Dispose();
        }
        private void ShowThumbnail(string filename)
        {
            //TODO
            using (ImageProcess thumbnail = new ImageProcess(filename))
            {
                thumbnail.Resize(50, 50);
            }
            Image i = new Image();
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(filename);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            i.Source = src;
            i.Stretch = Stretch.Uniform;
            //sp.Children.Add(i);
        }
    }
}
