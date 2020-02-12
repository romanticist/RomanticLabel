using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using RomanticLabel.ViewModel;


namespace RomanticLabel.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMediaService
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        void IMediaService.FrameControl(int currentFrame, double fps)
        {
            this.MediaPlayer.Position = TimeSpan.FromSeconds(currentFrame / fps);
        }
        void IMediaService.Pause()
        {
            this.MediaPlayer.Pause();
        }
        void IMediaService.Play()
        {
            this.MediaPlayer.Play();
        }
        void IMediaService.Stop()
        {
            this.MediaPlayer.Stop();
        }
        void IMediaService.Capture()
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(800, 450, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(this.MediaPlayer);
            PngBitmapEncoder pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            
            using (Stream fileStream = File.Create(@"C:\Users\csy18\OneDrive\바탕 화면\RomanticLabel\RomanticLabel\cap.png")) 
            {
                pngImage.Save(fileStream);
            }
        }
    }
}
