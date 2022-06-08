using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace biometria_8
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Bitmap? sourceImage = null;
        Bitmap? imageTemplate = null;
        Bitmap? imageFinger = null;

        Dictionary<(int, int), (int, int)> templateValues = new Dictionary<(int, int), (int, int)>();
        Dictionary<(int, int), (int, int)> fingerValues = new Dictionary<(int, int), (int, int)>();

        public MainWindow()
        {
            InitializeComponent();
        }
        private void OpenFileTemplate(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg;*.png)|*.jpg;*.png|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName;
                imageTemplate = this.sourceImage = new Bitmap($"{fileName}");
                ImgTemplate.Source = ImageSourceFromBitmap(this.sourceImage);

            }
            if (sourceImage == null)
            {
                MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Bitmap bitmap = new Bitmap(this.sourceImage.Width, this.sourceImage.Height);
            bitmap = (Bitmap)this.imageTemplate.Clone();
            (Bitmap bmp, templateValues) = Algorithm.GetMinutiae(bitmap);
            ImgCalcTemplate.Source = ImageSourceFromBitmap(bmp);
        }
        [return: MarshalAs(UnmanagedType.Bool)]

        private void OpenFileFinger(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg;*.png)|*.jpg;*.png|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName;
                imageFinger = new Bitmap($"{fileName}");
                ImgFinger.Source = ImageSourceFromBitmap(imageFinger);
            }
        }
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]

        public static extern bool DeleteObject([In] IntPtr hObject);

        public ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        private void CheckFinger(object sender, RoutedEventArgs e)
        {
            if (ImgFinger == null)
            {
                MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Bitmap bitmap = new Bitmap(this.imageFinger.Width, this.imageFinger.Height);
            bitmap = (Bitmap)this.imageFinger.Clone();
            (Bitmap bmp, fingerValues) = Algorithm.GetMinutiae(bitmap);
            ImgCalcFinger.Source = ImageSourceFromBitmap(bmp);


            if (Algorithm.Submit(templateValues, fingerValues))
                SubmitBox.Text = "Is Gucci!";
            else
                SubmitBox.Text = "Is Bad!";



        }

        private void GenerateImage_Click(object sender, RoutedEventArgs e)
        {
            if (imageTemplate == null)
            {
                MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            imageFinger = Algorithm.GenerateImage(imageTemplate.Width, imageTemplate.Height, templateValues);
            ImgFinger.Source = ImageSourceFromBitmap(imageFinger);

        }
    }
}
