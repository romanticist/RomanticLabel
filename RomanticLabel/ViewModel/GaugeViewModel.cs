using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanticLabel.ViewModel
{
    public class GaugeViewModel : BaseViewModel
    {
        MainViewModel mainViewModel;
        public GaugeViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            MaxFrame = 170;
            Angle = -85;
            Value = 0;
        }
        
        #region Data Binding

        private int _angle;
        private int _value;
        private int _maxFrame;
        public int Angle
        {
            get
            { return _angle; }
            private set
            {
                _angle = value;
                OnPropertyChanged("Angle");
            }
        }
        public int Value
        {
            get
            { return _value; }
            set
            {
                mainViewModel.BBox[_value] = mainViewModel.BoundingBoxes;
                mainViewModel.BoundingBoxes = mainViewModel.BBox[value];
                if (mainViewModel.IsPlay)  mainViewModel.MediaService.FrameControl(value, mainViewModel.fps);
                _value = value;
                Angle = (int)((value / (double)MaxFrame) * 170) - 85;
                OnPropertyChanged("Value");
            }
        }
        public int MaxFrame
        {
            get
            { return _maxFrame; }
            set
            {
                _maxFrame = value;
                OnPropertyChanged("MaxFrame");
            }
        }
        #endregion
    }
}