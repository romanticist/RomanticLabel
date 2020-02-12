using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace RomanticLabel.ViewModel
{
    public interface IMediaService
    {
        void FrameControl(int currentFrame, double fps);
        void Pause();
        void Play();
        void Stop();
        void Capture();
    }
}
