using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace RomanticLabel.ViewModel
{
    public class BoundingThumb : Thumb
    {

        public BoundingThumb()
        {
            DragDelta += new DragDeltaEventHandler(OnDragDelta);
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var item = DataContext as ContentPresenter;
            if (item != null)
            {
                double left = Canvas.GetLeft(item);
                double top = Canvas.GetTop(item);
                Canvas.SetLeft(item, left + e.HorizontalChange);
                Canvas.SetTop(item, top + e.VerticalChange);
            }
        }

    }
}
