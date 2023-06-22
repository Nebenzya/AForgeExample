using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WebCamCapture;

public partial class MainWindow : Window
{
    FilterInfoCollection? videoDevices;
    VideoCaptureDevice? videoSource;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count == 0)
                throw new Exception();

            foreach (FilterInfo device in videoDevices)
            {
                cbWebCams.Items.Add(device.Name);
            }

            lbInfo.Content = ($"Initialization {videoDevices.Count} devices");
        }
        catch
        {
            lbInfo.Content = "Error: No devices detected";
            btStart.IsEnabled = false;
            videoDevices = null;
        }
    }

    private void MainWindow_Closed(object? sender, EventArgs e) => Reset();

    private void btStart_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            videoSource = new VideoCaptureDevice(videoDevices[cbWebCams.SelectedIndex].MonikerString);
            videoSource.NewFrame += NewFrame;
            videoSource.Start();
            lbInfo.Content = "Camera ON";
        }
        catch
        {
            MessageBox.Show("Failed!");
            Reset();
        }
    }

    private void btStop_Click(object sender, RoutedEventArgs e) => Reset();

    private void NewFrame(object sender, NewFrameEventArgs eventArgs)
    {

        try
        {
            var bitmap = (Bitmap)eventArgs.Frame.Clone();

            MemoryStream memory = new MemoryStream();
            bitmap.Save(memory, ImageFormat.Bmp);
            memory.Seek(0, SeekOrigin.Begin);

            var bitmapimage = new BitmapImage()
            { 
                CacheOption = BitmapCacheOption.None 
            };
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = memory;
            bitmapimage.EndInit();
            bitmapimage.Freeze();


            Dispatcher.Invoke(() =>
            {
                img1.Source = bitmapimage;
            });

        }
        catch (Exception ex)
        {
            lbInfo.Content = ex.Message;
        }
    }

    private void btScreen_Click(object sender, RoutedEventArgs e)
    {
        img2.Source = img1.Source?.Clone();
    }

    private void btSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string fileName = "noname";
            string filePath = $"{Directory.GetCurrentDirectory()}/{fileName}.jpg";
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)img2.Source));
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
                encoder.Save(stream);

            lbInfo.Content = $"The screen is saved to {filePath}";
        }
        catch (Exception ex)
        {
            lbInfo.Content = ex.Message;
        }
    }

    private void Reset()
    {
        if (videoSource != null && videoSource.IsRunning)
        {
            videoSource.SignalToStop();
            videoSource = null;
            lbInfo.Content = "Camera OFF";
        }

        img1.Source = null;
    }
}

