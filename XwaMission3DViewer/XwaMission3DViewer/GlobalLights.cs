using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace XwaMission3DViewer
{
    public sealed class GlobalLights : ModelVisual3D
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(GlobalLights), new PropertyMetadata(true, IsEnabledChanged));

        public GlobalLights()
        {
            this.OnIsEnabledChanged();
        }

        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        private static void IsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GlobalLights)d).OnIsEnabledChanged();
        }

        private void OnIsEnabledChanged()
        {
            this.Content = null;

            if (!this.IsEnabled)
            {
                return;
            }

            var lightGroup = new Model3DGroup();

            // key light
            lightGroup.Children.Add(new DirectionalLight(Color.FromRgb(180, 180, 180), new Vector3D(-1, -1, -1)));

            // fill light
            lightGroup.Children.Add(new DirectionalLight(Color.FromRgb(120, 120, 120), new Vector3D(1, -1, -0.1)));

            // rim/back light
            lightGroup.Children.Add(new DirectionalLight(Color.FromRgb(60, 60, 60), new Vector3D(0.1, 1, -1)));

            // and a little bit from below
            lightGroup.Children.Add(new DirectionalLight(Color.FromRgb(50, 50, 50), new Vector3D(0.1, 0.1, 1)));

            lightGroup.Children.Add(new AmbientLight(Color.FromRgb(30, 30, 30)));

            this.Content = lightGroup;
        }
    }
}
