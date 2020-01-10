using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace XwaMission3DViewer
{
    /// <summary>
    /// A visual element that shows a view cube.
    /// </summary>
    public class CustomViewCubeVisual3D : ViewCubeVisual3D
    {
        /// <summary>
        /// The normal vectors.
        /// </summary>
        private readonly Dictionary<object, Vector3D> faceNormals = new Dictionary<object, Vector3D>();

        /// <summary>
        /// The up vectors.
        /// </summary>
        private readonly Dictionary<object, Vector3D> faceUpVectors = new Dictionary<object, Vector3D>();

        private readonly IList<ModelUIElement3D> CubeFaceModels = new List<ModelUIElement3D>(6);
        private readonly IList<ModelUIElement3D> EdgeModels = new List<ModelUIElement3D>(4 * 3);
        private readonly IList<ModelUIElement3D> CornerModels = new List<ModelUIElement3D>(8);
        private static readonly Point3D[] xAligned = { new Point3D(0, -1, -1), new Point3D(0, 1, -1), new Point3D(0, -1, 1), new Point3D(0, 1, 1) }; //x
        private static readonly Point3D[] yAligned = { new Point3D(-1, 0, -1), new Point3D(1, 0, -1), new Point3D(-1, 0, 1), new Point3D(1, 0, 1) };//y
        private static readonly Point3D[] zAligned = { new Point3D(-1, -1, 0), new Point3D(-1, 1, 0), new Point3D(1, -1, 0), new Point3D(1, 1, 0) };//z

        private static readonly Point3D[] cornerPoints =   {
                new Point3D(-1,-1,-1 ), new Point3D(1, -1, -1), new Point3D(1, 1, -1), new Point3D(-1, 1, -1),
                new Point3D(-1,-1,1 ),new Point3D(1,-1,1 ),new Point3D(1,1,1 ),new Point3D(-1,1,1 )};

        private readonly PieSliceVisual3D circle = new PieSliceVisual3D();

        private readonly Brush CornerBrush = Brushes.Gold;
        private readonly Brush EdgeBrush = Brushes.Silver;

        /// <summary>
        ///   Initializes a new instance of the <see cref = "CustomViewCubeVisual3D" /> class.
        /// </summary>
        public CustomViewCubeVisual3D()
        {
            this.InitialModels();
        }

        private void InitialModels()
        {
            this.Children.Clear();

            for (int i = 0; i < 6; ++i)
            {
                var element = new ModelUIElement3D();
                CubeFaceModels.Add(element);
                Children.Add(CubeFaceModels[i]);
                element.MouseLeftButtonDown += this.FaceMouseLeftButtonDown;
            }
            this.Children.Add(circle);

            for (int i = 0; i < xAligned.Length + yAligned.Length + zAligned.Length; ++i)
            {
                var element = new ModelUIElement3D();
                EdgeModels.Add(element);
                element.MouseLeftButtonDown += FaceMouseLeftButtonDown;
                element.MouseEnter += EdggesMouseEnters;
                element.MouseLeave += EdgesMouseLeaves;
            }

            for (int i = 0; i < cornerPoints.Length; ++i)
            {
                var element = new ModelUIElement3D();
                CornerModels.Add(element);
                element.MouseLeftButtonDown += FaceMouseLeftButtonDown;
                element.MouseEnter += EdggesMouseEnters;
                element.MouseLeave += EdgesMouseLeaves;
            }

            UpdateVisuals();
        }

        /// <summary>
        /// Updates the visuals.
        /// </summary>
        private void UpdateVisuals()
        {
            var vecUp = this.ModelUpDirection;
            // create left vector 90° from up
            var vecLeft = new Vector3D(vecUp.Y, vecUp.Z, vecUp.X);

            var vecFront = Vector3D.CrossProduct(vecLeft, vecUp);

            faceNormals.Clear();
            faceUpVectors.Clear();
            AddCubeFace(CubeFaceModels[0], vecFront, vecUp, GetCubefaceColor(0), this.FrontText);
            AddCubeFace(CubeFaceModels[1], -vecFront, vecUp, GetCubefaceColor(1), this.BackText);
            AddCubeFace(CubeFaceModels[2], vecLeft, vecUp, GetCubefaceColor(2), this.LeftText);
            AddCubeFace(CubeFaceModels[3], -vecLeft, vecUp, GetCubefaceColor(3), this.RightText);
            AddCubeFace(CubeFaceModels[4], vecUp, vecFront, GetCubefaceColor(4), this.TopText);
            AddCubeFace(CubeFaceModels[5], -vecUp, -vecFront, GetCubefaceColor(5), this.BottomText);

            //var circle = new PieSliceVisual3D();
            circle.BeginEdit();
            circle.Center = (this.ModelUpDirection * (-this.Size / 2)).ToPoint3D();
            circle.Normal = this.ModelUpDirection;
            circle.UpVector = vecLeft; // rotate 90° so that it's at the bottom plane of the cube.
            circle.InnerRadius = this.Size;
            circle.OuterRadius = this.Size * 1.3;
            circle.StartAngle = 0;
            circle.EndAngle = 360;
            circle.Fill = Brushes.Gray;
            circle.EndEdit();

            AddCorners();
            AddEdges();
            EnableDisableEdgeClicks();
        }

        private Brush GetCubefaceColor(int index)
        {
            switch (index)
            {
                case 0:
                case 1:
                    return Brushes.Red;
                case 2:
                case 3:
                    if (ModelUpDirection.Z < 1)
                    {
                        return Brushes.Blue;
                    }
                    else
                    {
                        return Brushes.Green;
                    }
                case 4:
                case 5:
                    if (ModelUpDirection.Z < 1)
                    {
                        return Brushes.Green;
                    }
                    else
                    {
                        return Brushes.Blue;
                    }
                default:
                    return Brushes.White;
            }
        }

        private void EnableDisableEdgeClicks()
        {
            foreach (var item in EdgeModels)
            {
                Children.Remove(item);
            }
            foreach (var item in CornerModels)
            {
                Children.Remove(item);
            }
            if (EnableEdgeClicks)
            {
                foreach (var item in EdgeModels)
                {
                    (item.Model as GeometryModel3D).Material = MaterialHelper.CreateMaterial(EdgeBrush);
                    Children.Add(item);
                }
                foreach (var item in CornerModels)
                {
                    (item.Model as GeometryModel3D).Material = MaterialHelper.CreateMaterial(CornerBrush);
                    Children.Add(item);
                }
            }
        }

        private void AddEdges()
        {
            var halfSize = Size / 2;
            var sideLength = halfSize / 2;

            int counter = 0;
            foreach (var p in xAligned)
            {
                Point3D center = p.Multiply(halfSize);
                AddEdge(EdgeModels[counter++], center, 1.5 * halfSize, sideLength, sideLength, p.ToVector3D());
            }


            foreach (var p in yAligned)
            {
                Point3D center = p.Multiply(halfSize);
                AddEdge(EdgeModels[counter++], center, sideLength, 1.5 * halfSize, sideLength, p.ToVector3D());
            }


            foreach (var p in zAligned)
            {
                Point3D center = p.Multiply(halfSize);
                AddEdge(EdgeModels[counter++], center, sideLength, sideLength, 1.5 * halfSize, p.ToVector3D());
            }
        }

        private void AddEdge(ModelUIElement3D element, Point3D center, double x, double y, double z, Vector3D faceNormal)
        {
            var builder = new MeshBuilder(false, true);

            builder.AddBox(center, x, y, z);

            var geometry = builder.ToMesh();
            geometry.Freeze();

            var model = new GeometryModel3D { Geometry = geometry, Material = MaterialHelper.CreateMaterial(EdgeBrush) };
            element.Model = model;

            faceNormals.Add(element, faceNormal);
            faceUpVectors.Add(element, ModelUpDirection);
        }

        private void AddCorners()
        {
            var a = Size / 2;
            var sideLength = a / 2;
            int counter = 0;
            foreach (var p in cornerPoints)
            {
                var builder = new MeshBuilder(false, true);

                Point3D center = p.Multiply(a);
                builder.AddBox(center, sideLength, sideLength, sideLength);
                var geometry = builder.ToMesh();
                geometry.Freeze();

                var model = new GeometryModel3D { Geometry = geometry, Material = MaterialHelper.CreateMaterial(CornerBrush) };
                var element = CornerModels[counter++];
                element.Model = model;
                faceNormals.Add(element, p.ToVector3D());
                faceUpVectors.Add(element, ModelUpDirection);
            }
        }

        private void EdgesMouseLeaves(object sender, MouseEventArgs e)
        {
            ModelUIElement3D s = sender as ModelUIElement3D;
            (s.Model as GeometryModel3D).Material = MaterialHelper.CreateMaterial(Colors.Silver);
        }

        private void EdggesMouseEnters(object sender, MouseEventArgs e)
        {
            ModelUIElement3D s = sender as ModelUIElement3D;
            (s.Model as GeometryModel3D).Material = MaterialHelper.CreateMaterial(Colors.Goldenrod);
        }

        private void CornersMouseLeave(object sender, MouseEventArgs e)
        {
            ModelUIElement3D s = sender as ModelUIElement3D;
            (s.Model as GeometryModel3D).Material = MaterialHelper.CreateMaterial(Colors.Gold);
        }

        private void CornersMouseEnters(object sender, MouseEventArgs e)
        {
            ModelUIElement3D s = sender as ModelUIElement3D;
            (s.Model as GeometryModel3D).Material = MaterialHelper.CreateMaterial(Colors.Goldenrod);
        }

        /// <summary>
        /// Adds a cube face.
        /// </summary>
        /// <param name="element">
        /// </param>
        /// <param name="normal">
        /// The normal.
        /// </param>
        /// <param name="up">
        /// The up vector.
        /// </param>
        /// <param name="b">
        /// The brush.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        private void AddCubeFace(ModelUIElement3D element, Vector3D normal, Vector3D up, Brush b, string text)
        {
            var material = CreateTextMaterial(b, text);

            double a = this.Size;

            var builder = new MeshBuilder(false, true);
            builder.AddCubeFace(this.Center, normal, up, a, a, a);
            var geometry = builder.ToMesh();
            geometry.Freeze();

            var model = new GeometryModel3D { Geometry = geometry, Material = material };

            element.Model = model;

            this.faceNormals.Add(element, normal);
            this.faceUpVectors.Add(element, up);
        }

        private static Material CreateTextMaterial(Brush b, string text)
        {
            var grid = new Grid { Width = 20, Height = 20, Background = b };
            grid.Children.Add(
                new TextBlock
                {
                    Text = text,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = 15,
                    Foreground = Brushes.White
                });
            grid.Arrange(new Rect(new Point(0, 0), new Size(20, 20)));

            var bmp = new RenderTargetBitmap((int)grid.Width, (int)grid.Height, 96, 96, PixelFormats.Default);
            bmp.Render(grid);
            bmp.Freeze();
            var m = MaterialHelper.CreateMaterial(new ImageBrush(bmp));
            m.Freeze();
            return m;
        }

        /// <summary>
        /// Handles left clicks on the view cube.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private void FaceMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            var faceNormal = this.faceNormals[sender];
            var faceUp = this.faceUpVectors[sender];

            var lookDirection = -faceNormal;
            var upDirection = faceUp;
            lookDirection.Normalize();
            upDirection.Normalize();

            // Double-click reverses the look direction
            if (e.ClickCount == 2)
            {
                lookDirection *= -1;
                if (upDirection != this.ModelUpDirection)
                {
                    upDirection *= -1;
                }
            }

            if (this.Viewport != null)
            {
                if (this.Viewport.Camera is ProjectionCamera camera)
                {
                    var target = camera.Position + camera.LookDirection;
                    double distance = camera.LookDirection.Length;
                    lookDirection *= distance;
                    var newPosition = target - lookDirection;
                    CameraHelper.AnimateTo(camera, newPosition, lookDirection, upDirection, 500);
                }
            }

            e.Handled = true;
            this.OnClicked(lookDirection, upDirection);
        }
    }
}
