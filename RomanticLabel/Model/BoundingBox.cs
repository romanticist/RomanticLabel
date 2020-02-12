using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using RomanticLabel.ViewModel;

namespace RomanticLabel.Model
{
    public class BoundingBox : BaseViewModel
    {
        public BoundingBox(Rect rect, string id) { this._rectangle = rect; this.ID = id;}
        private Rect _rectangle;
        private string _id;
        private string _color = "Yellow";

        #region Databinding
        
        public Rect Rectangle
        {
            get { return _rectangle; }
            set
            {
                _rectangle = value;
                OnPropertyChanged("Rectangle");
            }
        }
        public string ID
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("ID");
            }
        }
        public double X
        {
            get { return _rectangle.X; }
            set
            {
                _rectangle.X = value;
                OnPropertyChanged("Rectangle");
            }
        }
        public double Y
        {
            get { return _rectangle.Y; }
            set
            {
                _rectangle.Y = value;
                OnPropertyChanged("Rectangle");
            }
        }
        public double Width
        {
            get { return _rectangle.Width; }
            set
            {
                _rectangle.Width = value;
                OnPropertyChanged("Rectangle");
            }
        }
        public double Height
        {
            get { return _rectangle.Height; }
            set
            {
                _rectangle.Height = value;
                OnPropertyChanged("Rectangle");
            }
        }
        public string Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged("Color");
            }
        }
        #endregion
    }
}
