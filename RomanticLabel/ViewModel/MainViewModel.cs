using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.Collections;
using System.Reflection;
using System.Windows;
using Prism.Regions;
using Prism.Commands;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using RomanticLabel.Model;
using System.Windows.Input;
using System.IO;
using Alturos.Yolo;

namespace RomanticLabel.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public IMediaService MediaService { get; private set; }
        public Dictionary<int, ObservableCollection<BoundingBox> > BBox;
        public KeyValuePair<int, object> start_interpolation;
        public KeyValuePair<int, object> end_interpolation;
        public Point mouseDownLocation;
        public double fps;
        
        public MainViewModel()
        {
            //initialized
            this.IsPlay = false;
            this.Interpolation = "Interpolation Start";
            BBox = new Dictionary< int, ObservableCollection<BoundingBox> >();

            //view binding
            this.GaugeView = new GaugeViewModel(this);

            //commands
            FindPath = new RelayCommand(ExFindPath);
            CloseWindow = new RelayCommand(ExCloseWindow);
            NextFrameCommand = new RelayCommand(ExNextFrameCommand);
            PrevFrameCommand = new RelayCommand(ExPrevFrameCommand);
            DrawGhostRect = new RelayCommand(ExDrawGhostRect);
            SaveBBox = new RelayCommand(ExSaveBBox);
            CancelInterpolation = new RelayCommand(ExCancelInterpolation);
            ObjectDetect = new RelayCommand(ExObjectDetect);
    }

        #region DataBindings

        private GaugeViewModel _gaugeView;
        private string _videoPath;
        private double _panelX;
        private double _panelY;
        private int _crossHairThickness;
        private bool _isPlay;
        private ObservableCollection<BoundingBox> _boundingBoxes = new ObservableCollection<BoundingBox>();
        private string _interpolation;
        private Rect _ghostRect;
        private bool _isLeftMouseDown;
        
        public GaugeViewModel GaugeView
        {
            get { return _gaugeView; }
            set
            {
                _gaugeView = value;
                OnPropertyChanged("GaugeView");
            }
        }
        public string VideoPath
        {
            get { return _videoPath; }
            set
            {
                _videoPath = value;
                OnPropertyChanged("VideoPath");
                CrossHairThickness = 2;
                this.IsPlay = true;
                this.MediaService.Play();
                this.MediaService.Pause();
                fps = Convert.ToDouble(GetVideoFPS(VideoPath));
                this.GaugeView.MaxFrame = (int)(GetVideoDuration(VideoPath).TotalMilliseconds/1000.0 * fps);
                LoadBBox();
            }
        }
        public double PanelX
        {
            get { return _panelX; }
            set
            {
                if (value.Equals(_panelX)) return;
                if (IsLeftMouseDown)
                    GhostRect = new Rect(mouseDownLocation, new Point(PanelX,PanelY));
                _panelX = value;
                OnPropertyChanged("PanelX");                

            }
        }
        public double PanelY
        {
            get { return _panelY; }
            set
            {
                if (value.Equals(_panelY)) return;
                if (IsLeftMouseDown)
                    GhostRect = new Rect(mouseDownLocation, new Point(PanelX, PanelY));
                _panelY = value;
                OnPropertyChanged("PanelY");
            }
        }
        public int CrossHairThickness
        {
            get { return _crossHairThickness; }
            set
            {
                _crossHairThickness = value;
                OnPropertyChanged("CrossHairThickness");
            }
        }
        public bool IsPlay
        {
            get { return _isPlay; }
            set
            {
                _isPlay = value;
                OnPropertyChanged("IsPlay");
            }
        }
        public ObservableCollection<BoundingBox> BoundingBoxes
        {
            get { return _boundingBoxes; }
            set
            {
                _boundingBoxes = value;
                OnPropertyChanged("BoundingBoxes");
            }
        }
        public string Interpolation
        {
            get { return _interpolation; }
            set
            {
                _interpolation = value;
                OnPropertyChanged("Interpolation");
            }
        }
        public Rect GhostRect
        {
            get { return _ghostRect; }
            set
            {
                _ghostRect = value;
                OnPropertyChanged("GhostRect");
            }
        }
        public bool IsLeftMouseDown
        {
            get { return _isLeftMouseDown; }
            set
            {
                _isLeftMouseDown = value;
                OnPropertyChanged("IsLeftMouseDown");
            }
        }
        #endregion

        #region Commands
        public RelayCommand FindPath { get; private set; }
        public RelayCommand CloseWindow { get; private set; }
        public RelayCommand NextFrameCommand { get; private set; }
        public RelayCommand PrevFrameCommand { get; private set; }
        public RelayCommand DrawGhostRect { get; private set; }
        public RelayCommand SaveBBox { get; private set; }
        public RelayCommand CancelInterpolation { get; private set; }
        public RelayCommand ObjectDetect { get; private set; }

        private DelegateCommand<IMediaService> loadedCommand;

        private void ExFindPath()
        {
            try
            {
                var dlg = new CommonOpenFileDialog();
                dlg.Filters.Add(new CommonFileDialogFilter("mp4", "mp4"));
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    VideoPath = dlg.FileName;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred from {MethodBase.GetCurrentMethod().Name}");
                Console.WriteLine(ex.ToString());
            }
        }
        private void ExCloseWindow()
        {
            System.Windows.Application.Current.Shutdown();
        }
        private void ExNextFrameCommand()
        {
            if (this.GaugeView.Value < GaugeView.MaxFrame && IsPlay)
                this.GaugeView.Value++;            
        }
        private void ExPrevFrameCommand()
        {
            if (this.GaugeView.Value > 0 && IsPlay)
                this.GaugeView.Value--;
        }
        private void ExDrawGhostRect()
        {
            if (IsPlay && IsLeftMouseDown)
            {
                IsLeftMouseDown = false;
                GhostRect = new Rect();

                var rect = new Rect(mouseDownLocation, new Point(PanelX,PanelY));
                BoundingBoxes.Add(new BoundingBox(rect, "New"));
            }
            else if (IsPlay && !IsLeftMouseDown)
            {
                IsLeftMouseDown = true;
                mouseDownLocation = new Point(PanelX, PanelY);
            }
        }

        private void ExSaveBBox()
        {
            var path = VideoPath.Substring(0, VideoPath.Length - 4) + ".txt";
            var data = "";
            for (int i = 0; i <= this.GaugeView.MaxFrame; i++)
            {
                
                var frameNum = i;
                var countObj = BBox[i].Count;
                if (countObj == 0) continue;
                data += $"{frameNum}{Environment.NewLine}{countObj}{Environment.NewLine}";
                
                for (int j = 0; j < BBox[i].Count; j++)
                {
                    var classid = BBox[i][j].ID;
                    var scaledX = BBox[i][j].X;
                    var scaledY = BBox[i][j].Y;
                    var scaledWidth = BBox[i][j].Width;
                    var scaledHeight = BBox[i][j].Height;
                    data += $"\"{classid}\" {scaledX} {scaledY} {scaledWidth} {scaledHeight}{Environment.NewLine}";
                }
            }
            File.WriteAllText(path, data);
            MessageBox.Show("Saved\r\n");
        }
        private void ExCancelInterpolation()
        {
            if(GaugeView.Value == start_interpolation.Key)
            {
                ((BoundingBox)start_interpolation.Value).Color = "Yellow";
            }
            else
            {
                BBox[start_interpolation.Key][BBox[start_interpolation.Key].IndexOf((BoundingBox)start_interpolation.Value)].Color = "Yellow";
            }
            
            Interpolation = "Interpolation Start";
        }

        private void ExObjectDetect()
        {
            this.MediaService.Capture();
            var configurationDetector = new ConfigurationDetector();
            var config = configurationDetector.Detect();
            using (var yoloWrapper = new YoloWrapper(config))
            {
                var items = yoloWrapper.Detect(@"C:\Users\csy18\OneDrive\바탕 화면\RomanticLabel\RomanticLabel\cap.png");
                foreach(var item in items)
                {
                    var rect = new Rect(item.X,item.Y,item.Width,item.Height);
                    BoundingBoxes.Add(new BoundingBox(rect, item.Type));
                }                
            }
        }

        public DelegateCommand<IMediaService> LoadedCommand
        {
            get
            {
                if(this.loadedCommand == null)
                {
                    this.loadedCommand = new DelegateCommand<IMediaService>((mediaService) =>
                    {
                        this.MediaService = mediaService;
                    });
                }
                return loadedCommand;
            }
        }

        public ICommand DeleteBoundingBoxCommand => new Command(p =>
        {
            BoundingBoxes.Remove((BoundingBox)p);
        });

        public ICommand InterpolationCommand => new Command(p =>
        {
            if (Interpolation == "Interpolation Start")
            {
                ((BoundingBox)p).Color = "Red";
                start_interpolation = new KeyValuePair<int, object>(GaugeView.Value, p);

                Interpolation = "InterPolation End";
            }
            else if (Interpolation == "InterPolation End")
            {
                if (start_interpolation.Key == GaugeView.Value)
                {
                    MessageBox.Show("Same Frame Error\r\n");
                    return;
                }
                ((BoundingBox)p).Color = "Red";
                ((BoundingBox)p).ID = ((BoundingBox)start_interpolation.Value).ID;
                end_interpolation = new KeyValuePair<int, object>(GaugeView.Value, p);

                //interpolation logic
                if (start_interpolation.Key > end_interpolation.Key)
                {
                    KeyValuePair<int, object> tmp = start_interpolation;
                    start_interpolation = end_interpolation;
                    end_interpolation = tmp;
                }
                int total = end_interpolation.Key - start_interpolation.Key;
                double x = (((BoundingBox)end_interpolation.Value).X - ((BoundingBox)start_interpolation.Value).X) / total;
                double y = (((BoundingBox)end_interpolation.Value).Y - ((BoundingBox)start_interpolation.Value).Y) / total;
                double w = (((BoundingBox)end_interpolation.Value).Width - ((BoundingBox)start_interpolation.Value).Width) / total;
                double h = (((BoundingBox)end_interpolation.Value).Height - ((BoundingBox)start_interpolation.Value).Height) / total;
                double acc_x = 0, acc_y = 0, acc_w = 0, acc_h = 0;
                
                for (int i = start_interpolation.Key + 1; i < end_interpolation.Key; i++)
                {
                    acc_x += x; acc_y += y; acc_w += w; acc_h += h;
                    BoundingBox bb = (BoundingBox)start_interpolation.Value;
                    Rect interRect = new Rect(bb.X+acc_x,bb.Y+acc_y,bb.Width+acc_w,bb.Height+acc_h);
                    bb = new BoundingBox(interRect, ((BoundingBox)start_interpolation.Value).ID);
                    bb.Color = "Red";
                    BBox[i].Add(bb);
                }

                Interpolation = "Interpolation Start";
            }
            
        });

        #endregion

        #region useful function

        private static TimeSpan GetVideoDuration(string filePath)
        {
            using (var shell = ShellObject.FromParsingName(filePath))
            {
                IShellProperty prop = shell.Properties.System.Media.Duration;
                var t = (ulong)prop.ValueAsObject;
                return TimeSpan.FromTicks((long)t);
            }
        }

        private static string GetVideoFPS(string filePath)
        {
            ShellFile shellFile = ShellFile.FromFilePath(filePath);
            return (shellFile.Properties.System.Video.FrameRate.Value/1000.0).ToString();
        }

        private void LoadBBox()
        {            
            for (int i = 0; i <= this.GaugeView.MaxFrame; i++)
                BBox[i] = new ObservableCollection<BoundingBox>();

            string path = VideoPath.Substring(0, VideoPath.Length - 4) + ".txt";
            if (File.Exists(path))
            {
                var boundingBoxes = File.ReadAllLines(path);
                for(int i = 0; i < boundingBoxes.Length;)
                {
                    var split1 = boundingBoxes[i].Split('\n');
                    var a = int.Parse(split1[0]); i++;
                    var split2 = boundingBoxes[i].Split('\n');
                    var b = int.Parse(split2[0]); i++;
                    for (int j = 0; j < b; j++, i++)
                    {
                        var split = boundingBoxes[i].Split(' ');
                        var id = split[0].Substring(1, split[0].Length - 2);
                        var x = double.Parse(split[1]);
                        var y = double.Parse(split[2]);
                        var w = double.Parse(split[3]);
                        var h = double.Parse(split[4]);
                        var rect = new Rect(x, y, w, h);
                        BBox[a].Add(new BoundingBox(rect, id));
                    }
                }  
            }
        }

        #endregion
    }
}
