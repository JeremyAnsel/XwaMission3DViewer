using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace XwaMission3DViewer
{
    public sealed class BackdropLight : LightSetup
    {
        private readonly Light light;

        public BackdropLight(Color color, Vector3D direction)
        {
            this.light = new DirectionalLight(color, direction);

            this.OnSetupChanged();
        }

        protected override void AddLights(Model3DGroup lightGroup)
        {
            if (lightGroup == null)
            {
                return;
            }

            if (this.light != null)
            {
                lightGroup.Children.Add(this.light);
            }
        }
    }
}
