﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace RomanticLabel.ViewModel
{
    class MouseBehaviors : Behavior<Panel>
    {
        public static readonly DependencyProperty MouseXProperty = DependencyProperty.Register(
         "MouseX", typeof(double), typeof(MouseBehaviors), new PropertyMetadata(default(double)));

        public static readonly DependencyProperty MouseYProperty = DependencyProperty.Register(
         "MouseY", typeof(double), typeof(MouseBehaviors), new PropertyMetadata(default(double)));
        public double MouseY
        {
            get { return (double)GetValue(MouseYProperty); }
            set { SetValue(MouseYProperty, value); }
        }

        public double MouseX
        {
            get { return (double)GetValue(MouseXProperty); }
            set { SetValue(MouseXProperty, value); }
        }

        protected override void OnAttached()
        {
            AssociatedObject.MouseMove += AssociatedObjectOnMouseMove;
        }

        private void AssociatedObjectOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            var pos = mouseEventArgs.GetPosition(AssociatedObject);
            MouseX = pos.X;
            MouseY = pos.Y;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseMove -= AssociatedObjectOnMouseMove;
        }
    }
}
