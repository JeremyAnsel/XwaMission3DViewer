using JeremyAnsel.Xwa.Dat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace XwaMission3DViewer
{
    public sealed class BackdropModel
    {
        public BackdropModel()
        {
        }

        public BackdropModel(DatImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            this.Width = image.Width;
            this.Image = image;
        }

        public int Width { get; set; }

        public DatImage Image { get; set; }

        public Material Material { get; set; }

        public void CreateMaterial()
        {
            if (this.Material != null)
            {
                return;
            }

            byte[] data = this.Image.GetImageData();

            if (data != null)
            {
                var bitmap = BitmapSource.Create(this.Image.Width, this.Image.Height, 96, 96, PixelFormats.Bgra32, null, data, this.Image.Width * 4);
                var material = new EmissiveMaterial(new ImageBrush(bitmap));
                material.Freeze();

                this.Material = material;
            }

            this.Image = null;
        }
    }
}
