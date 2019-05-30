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
        private readonly ImageProcessBack ImageProcessBack;
        #endregion
        public MainWindow()
        {
            ImageProcessBack = new ImageProcessBack();
            InitializeComponent();
            ButtonFormatSame.IsChecked = true;
        }
        private void ButtonFiles(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Images files |*.jpg;*.webp;*.bmp;*.jp2;*.png;*.tif;*.gif|" + "Documents/Archives |*.pdf;*.zip;*.cbz;*.rar;*.cbr;*.7z;*.cb7|" + "All files |*.*",
                Multiselect = true,
                Title = "Choisir les images à traiter"
            };
            DialogResult dr = openFileDialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                //sp.Children.Clear();
                ImageProcessBack.FullNameOfFilesToProcess.Clear();
                TextBoxListFiles.Text = "";
                ImageProcessBack.FullPathOriginFiles = Path.GetDirectoryName(openFileDialog.FileNames[0]);
                foreach (string fileName in openFileDialog.FileNames)
                {
                    //ShowThumbnail(fileName);
                    ImageProcessBack.FullNameOfFilesToProcess.Add(fileName);
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
            }
            else
            {
                ImageProcessBack.DeleteStrip = false;
            }
        }
        private void RadioButtonFormat(object sender, RoutedEventArgs e)
        {
            ImageProcessBack.PdfFusion = false;
            RadioButton buttonFormat = sender as RadioButton;
            string format = buttonFormat.Name;
            switch (format)
            {
                case "ButtonFormatSame":
                    ImageProcessBack.ImageFormatToSave = FileType.Unknow;
                    ImageProcessBack.PathSave = @"save\";
                    break;
                case "ButtonFormatPng":
                    ImageProcessBack.ImageFormatToSave = FileType.Png;
                    ImageProcessBack.PathSave = @"savePng\";
                    break;
                case "ButtonFormatJp2":
                    ImageProcessBack.ImageFormatToSave = FileType.Jp2;
                    ImageProcessBack.PathSave = @"saveJp2\";
                    break;
                case "ButtonFormatJpg":
                    ImageProcessBack.ImageFormatToSave = FileType.Jpg;
                    ImageProcessBack.PathSave = @"saveJpg\";
                    break;
                case "ButtonFormatTiff":
                    ImageProcessBack.ImageFormatToSave = FileType.Tiff;
                    ImageProcessBack.PathSave = @"saveTiff\";
                    break;
                case "ButtonFormatGif":
                    ImageProcessBack.ImageFormatToSave = FileType.Gif;
                    ImageProcessBack.PathSave = @"saveGif\";
                    break;
                case "ButtonFormatPdfFusion":
                    ImageProcessBack.PathSave = @"savePdf\";
                    ImageProcessBack.PdfFusion = true;
                    break;
                case "ButtonFormatPdfSingle":
                    ImageProcessBack.ImageFormatToSave = FileType.Pdf;
                    ImageProcessBack.PathSave = @"savePdf\";
                    break;
                case "ButtonFormatWebp":
                    ImageProcessBack.ImageFormatToSave = FileType.Webp;
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
        private void LevelDeleteStrips(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ImageProcessBack.StripLevel = (int)slDeleteStrips.Value;
        }
        private void ButtonStartProcess(object sender, RoutedEventArgs e)
        {
            string stringReturn = ImageProcessBack.Process();
            MessageBoxButton buttonEnd = MessageBoxButton.OK;
            MessageBoxImage iconEnd = MessageBoxImage.Information;
            string titleEnd = "Traitement";
            string contentEnd = stringReturn;
            MessageBox.Show(contentEnd, titleEnd, buttonEnd, iconEnd);
            ImageProcessBack.FullNameOfFilesToProcess.Clear();
            TextBoxListFiles.Text = "";
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
