﻿using System;
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
using ImageLib;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using RadioButton = System.Windows.Controls.RadioButton;
using CheckBox = System.Windows.Controls.CheckBox;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace IhmWpf
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool PdfFusion = false;
        private bool DeleteOrigin = false;
        private bool DeleteStrip = false;
        private string PathSave;
        private List<string> FullNameOfImagesToProcess;
        private PdfDocument ThePdfDocument;
        FileFormat ImageFormatToSave = FileFormat.Unknow;
        public MainWindow()
        {
            InitializeComponent();
            FullNameOfImagesToProcess = new List<string>();
        }
        private void ButtonFiles(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Images (*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|" + "All files (*.*)|*.*";
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "Choisir les images à traiter";
            DialogResult dr = openFileDialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                //sp.Children.Clear();
                FullNameOfImagesToProcess.Clear();
                TextBoxListFiles.Text = "";
                foreach (String fileName in openFileDialog.FileNames)
                {
                    //ShowThumbnail(fileName);
                    FullNameOfImagesToProcess.Add(fileName);
                    TextBoxListFiles.Text += fileName+"\n";
                }
            }
        }
        private void CheckBoxStrips(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            if (checkbox.IsChecked == true)
            {
                DeleteStrip = true;
            }
            else
            {
                DeleteStrip = false;
            }
        }
        private void RadioButtonFormat(object sender, RoutedEventArgs e)
        {
            RadioButton buttonFormat = sender as RadioButton;
            string format = buttonFormat.Name;
            switch(format)
            {
                case "same":
                    ImageFormatToSave = FileFormat.Unknow;
                    PathSave = @"save\";
                    break;
                case "png":
                    ImageFormatToSave = FileFormat.Png;
                    PathSave = @"savePng\";
                    break;
                case "jp2":
                    ImageFormatToSave = FileFormat.Jp2;
                    PathSave = @"saveJp2\";
                    break;
                case "jpg":
                    ImageFormatToSave = FileFormat.Jpg;
                    PathSave = @"saveJpg\";
                    break;
                case "tiff":
                    ImageFormatToSave = FileFormat.Tiff;
                    PathSave = @"saveTiff\";
                    break;
                case "gif":
                    ImageFormatToSave = FileFormat.Gif;
                    PathSave = @"saveGif\";
                    break;
                case "pdfFusion":
                    ImageFormatToSave = FileFormat.Jpg; 
                    PathSave = "";
                    PdfFusion = true;
                    break;
                case "pdfSingle":
                    ImageFormatToSave = FileFormat.Pdf; 
                    PathSave = @"savePdf\";
                    break;
                case "webp":
                    ImageFormatToSave = FileFormat.Webp;
                    PathSave = @"saveWebp\";
                    break;
            }
        }
        private void CheckBoxDeleteOrigin(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            if(checkbox.IsChecked==true)
            {
                DeleteOrigin = true;
            }
            else
            {
                DeleteOrigin = false;
            }
        }
        private void ButtonStartProcess(object sender, RoutedEventArgs e)
        {
            if (PdfFusion)
            {
                InitPdfDocument();
            }
            foreach (string fullNameOfImage in FullNameOfImagesToProcess)
            {
                try
                {
                    using (ImageProcess imageToProcess = new ImageProcess(fullNameOfImage))
                    {
                        if (DeleteStrip)
                        {
                            imageToProcess.DeleteStrips();
                        }

                        imageToProcess.SaveTo(ImageFormatToSave, PathSave);

                        if (PdfFusion)
                        {
                            AddPageToPdfDocument(fullNameOfImage);
                        }
                    }
                    if (DeleteOrigin)
                    {
                        File.Delete(fullNameOfImage);
                    }
                }
                catch (Exception ex)
                {
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Error;
                    string title = "Erreur";
                    string content ="Erreur : " + ex.Message + "sur image " + fullNameOfImage;
                    MessageBox.Show(content, title, button, icon);
                }
            }
            if (FullNameOfImagesToProcess.Count == 0)
            {
                string title = "Erreur";
                string content = "Merci de choisir des images";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBox.Show(content, title, button, icon);
            }
            else
            {
                if (PdfFusion)
                {
                    SavePdfDocument();
                }
                MessageBoxButton buttonEnd = MessageBoxButton.OK;
                MessageBoxImage iconEnd = MessageBoxImage.Information;
                string titleEnd = "Traitement ok";
                string contentEnd = "Fin de traitement";
                MessageBox.Show(contentEnd, titleEnd, buttonEnd, iconEnd);
            }
            FullNameOfImagesToProcess.Clear();
        }
        private void InitPdfDocument()
        {
            ThePdfDocument = new PdfDocument();
        }
        private void AddPageToPdfDocument(string fullNameOfImage)
        {
            try
            {
                FileStream filestream = new FileStream(fullNameOfImage + ".jpg", FileMode.Open);
                XImage img = XImage.FromStream(filestream);
                XGraphics xgr = XGraphics.FromPdfPage(ThePdfDocument.AddPage(new PdfPage { Width = img.PointWidth, Height = img.PointHeight }));
                xgr.DrawImage(img, 0, 0);
                xgr.Dispose();
                filestream.Dispose();
                File.Delete(fullNameOfImage + ".jpg");
            }
            catch
            {
                Console.WriteLine("Image not supported by tool. Please convert before in jpg/gif/png/tiff");
            }
        }
        private void SavePdfDocument()
        {
            string fullNameOfOneImage = FullNameOfImagesToProcess[0];
            string pathToSave = Path.GetDirectoryName(fullNameOfOneImage) + @"\SavePdf\";
            Directory.CreateDirectory(pathToSave);
            ThePdfDocument.Save(pathToSave + "SavefromImages.pdf");
            ThePdfDocument.Close();
            ThePdfDocument.Dispose();
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