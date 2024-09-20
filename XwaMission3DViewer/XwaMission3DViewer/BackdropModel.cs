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
        private string _datFileName;

        private short _datGroupId;

        private short _datImageId;

        public BackdropModel(string datFileName, DatImage image)
        {
            if (string.IsNullOrEmpty(datFileName))
            {
                throw new ArgumentNullException(nameof(datFileName));
            }

            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            this._datFileName = datFileName;
            this._datGroupId = image.GroupId;
            this._datImageId = image.ImageId;

            this.Width = image.Width;
            this.Height = image.Height;
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public Material Material { get; private set; }

        public void CreateMaterial()
        {
            if (this.Material != null)
            {
                return;
            }

            DatImage image = DatFile.GetImageDataById(this._datFileName, this._datGroupId, this._datImageId);

            byte[] data = image.GetImageData();

            if (data != null)
            {
                var bitmap = BitmapSource.Create(image.Width, image.Height, 96, 96, PixelFormats.Bgra32, null, data, image.Width * 4);
                var material = new EmissiveMaterial(new ImageBrush(bitmap));
                material.Freeze();

                this.Material = material;
            }
        }
    }
}
