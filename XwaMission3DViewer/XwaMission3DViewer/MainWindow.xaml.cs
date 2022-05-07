using HelixToolkit.Wpf;
using JeremyAnsel.Xwa.Dat;
using JeremyAnsel.Xwa.HooksConfig;
using JeremyAnsel.Xwa.Mission;
using JeremyAnsel.Xwa.WpfOpt;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace XwaMission3DViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SortedDictionary<string, OptModel> _optModels = new SortedDictionary<string, OptModel>(StringComparer.OrdinalIgnoreCase);

        private readonly SortedDictionary<string, string> _missionObjects = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly SortedDictionary<string, BackdropModel> _backdropModels = new SortedDictionary<string, BackdropModel>(StringComparer.OrdinalIgnoreCase);

        private IList<int> _selectedCrafts;

        public MainWindow()
        {
            InitializeComponent();
        }

        public TieFile MissionFile { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Qualité du code", "IDE0051:Supprimer les membres privés non utilisés", Justification = "Justified.")]
        private void RunBusyAction(Action action)
        {
            this.RunBusyAction(dispatcher => action());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Ne pas intercepter les types d'exception générale", Justification = "Justified.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2008:Ne pas créer de tâches sans passer TaskScheduler", Justification = "<En attente>")]
        private void RunBusyAction(Action<Action<Action>> action)
        {
            Action<Action> dispatcherAction = a =>
            {
                this.Dispatcher.Invoke(a);
            };

            Task.Factory.StartNew(state =>
            {
                var disp = (Action<Action>)state;
                disp(() => { this.BusyIndicator.IsBusy = true; });

                try
                {
                    action(disp);
                }
                catch (Exception ex)
                {
                    disp(() => MessageBox.Show(this, ex.ToString(), this.Title, MessageBoxButton.OK, MessageBoxImage.Error));
                }

                disp(() => { this.BusyIndicator.IsBusy = false; });
            }, dispatcherAction);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Ne pas intercepter les types d'exception générale", Justification = "Justified.")]
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetWorkingDirectory();

            bool error = false;

            if (System.IO.Directory.Exists(AppSettings.WorkingDirectory))
            {
                try
                {
                    AppSettings.SetData();
                    this.workingDirectoryText.Text = AppSettings.WorkingDirectory;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    error = true;
                }
            }
            else
            {
                error = true;
            }

            if (error)
            {
                this.Close();
                return;
            }

            this.OpenButton_Click(null, null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Supprimer les objets avant la mise hors de portée", Justification = "Justified.")]
        private void SetWorkingDirectory()
        {
            var dlg = new FolderBrowserForWPF.Dialog
            {
                Title = "Choose a working directory containing " + AppSettings.XwaExeFileName + " or a child directory"
            };

            if (dlg.ShowDialog() == true)
            {
                string fileName = dlg.FileName;

                if (!System.IO.File.Exists(System.IO.Path.Combine(fileName, "XWingAlliance.exe")))
                {
                    fileName = System.IO.Path.GetDirectoryName(fileName);

                    if (!System.IO.File.Exists(System.IO.Path.Combine(fileName, "XWingAlliance.exe")))
                    {
                        return;
                    }
                }

                AppSettings.WorkingDirectory = fileName + System.IO.Path.DirectorySeparatorChar;
            }
        }

        private void Viewport3D_CameraChanged(object sender, RoutedEventArgs e)
        {
            const double nearDistance = 100 * 3;
            const double farDistance = 40000000 * 3;

            var viewport = (HelixViewport3D)sender;

            if (viewport.Camera is PerspectiveCamera)
            {
                viewport.Camera.NearPlaneDistance = nearDistance;
                viewport.Camera.FarPlaneDistance = farDistance;
            }
            else
            {
                viewport.Camera.NearPlaneDistance = -farDistance;
                viewport.Camera.FarPlaneDistance = farDistance;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Ne pas intercepter les types d'exception générale", Justification = "Justified.")]
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".tie",
                CheckFileExists = true,
                Filter = "Mission files (*.tie)|*.tie",
                InitialDirectory = AppSettings.WorkingDirectory + "MISSIONS"
            };

            string fileName;

            if (dialog.ShowDialog(this) == true)
            {
                fileName = dialog.FileName;
            }
            else
            {
                return;
            }

            this.RunBusyAction(disp =>
            {
                try
                {
                    this.LoadResdataPlanets(fileName);
                }
                catch (Exception ex)
                {
                    disp(() => MessageBox.Show(this, ex.ToString(), this.Title, MessageBoxButton.OK, MessageBoxImage.Error));
                }

                disp(() =>
                {
                    this._optModels.Clear();
                    this.regionControl.Value = 1;
                    this.viewport3D.ResetCamera();
                    this.UpdateMap(fileName);
                    this.viewport3D.ZoomExtents();
                    this.missionFileText.Text = fileName;
                });
            });
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.MissionFile?.FileName == null)
            {
                return;
            }

            this.UpdateMap(this.MissionFile.FileName);
        }

        private void ShowLightsButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.MissionFile?.FileName == null)
            {
                return;
            }

            this.viewport3D.ResetCamera();
            this.UpdateMap(this.MissionFile.FileName);
            this.viewport3D.ZoomExtents();
        }

        private void RegionControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.MissionFile?.FileName == null)
            {
                return;
            }

            this.viewport3D.ResetCamera();
            this.UpdateMap(this.MissionFile.FileName);
            this.viewport3D.ZoomExtents();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Ne pas intercepter les types d'exception générale", Justification = "Justified.")]
        private void BackdropsDistanceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.MissionFile?.FileName == null)
            {
                return;
            }

            this.SetSelectedCrafts();
            this.ClearMap();

            try
            {
                this.LoadMap(this.regionControl.Value ?? 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private OptModel GetOptModel(string name)
        {
            if (!this._optModels.TryGetValue(name, out OptModel model))
            {
                string fileName = AppSettings.WorkingDirectory + "FLIGHTMODELS\\" + name + ".opt";

                if (!System.IO.File.Exists(fileName))
                {
                    return null;
                }

                model = new OptModel(fileName);

                this._optModels.Add(name, model);
            }

            return model;
        }

        private Visual3D CreateModel3D(bool isSelected, CraftModel craft)
        {
            int positionX = craft.PositionX * 256;
            int positionY = -craft.PositionY * 256;
            int positionZ = craft.PositionZ * 256;

            if (string.IsNullOrEmpty(craft.CraftName))
            {
                return null;
            }

            var model = this.GetOptModel(craft.CraftName);

            if (model == null)
            {
                return null;
            }

            var visual = new OptVisual3D
            {
                Cache = model.Cache,
                SortingFrequency = 0.2,
                Version = craft.Markings,
                IsWireframe = isSelected
            };

            var transformGroup = new Transform3DGroup();

            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), -craft.Roll * 256 * 360.0 / 65536)));

            if (craft.UseStartWaypoint)
            {
                transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), craft.HeadingZ)));
                transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), -craft.HeadingXY)));
            }
            else
            {
                transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), (craft.Pitch - 64) * 256 * 360.0 / 65536)));
                transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), -craft.Yaw * 256 * 360.0 / 65536)));
            }

            transformGroup.Children.Add(new TranslateTransform3D(positionY, -positionX, positionZ));
            transformGroup.Freeze();

            visual.Transform = transformGroup;

            return visual;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Ne pas intercepter les types d'exception générale", Justification = "Justified.")]
        private void UpdateMap(string fileName)
        {
            this._missionObjects.Clear();
            this.craftList.Items.Clear();
            this.ClearMap();

            try
            {
                this.MissionFile = TieFile.FromFile(fileName);

                string missionFileNameBase = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(fileName), System.IO.Path.GetFileNameWithoutExtension(fileName));
                this.LoadMissionObjects(missionFileNameBase);

                this.LoadMap(this.regionControl.Value ?? 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, fileName + "\n" + ex.ToString(), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMissionObjects(string basePath)
        {
            var lines = XwaHooksConfig.GetFileLines(basePath + "_Objects.txt");

            if (lines.Count == 0)
            {
                lines = XwaHooksConfig.GetFileLines(basePath + ".ini", "Objects");
            }

            foreach (string line in lines)
            {
                int splitIndex = line.IndexOf('=');

                if (splitIndex == -1)
                {
                    continue;
                }

                string key = System.IO.Path.GetFileNameWithoutExtension(line.Substring(0, splitIndex).Trim());
                string value = System.IO.Path.GetFileNameWithoutExtension(line.Substring(splitIndex + 1).Trim());
                this._missionObjects.Add(key, value);
            }
        }

        private void ClearMap()
        {
            for (int i = this.viewport3D.Children.Count - 1; i >= 0; i--)
            {
                Visual3D child = this.viewport3D.Children[i];

                if (child is OptVisual3D)
                {
                    this.viewport3D.Children.RemoveAt(i);
                }
                else if (child is QuadVisual3D)
                {
                    this.viewport3D.Children.RemoveAt(i);
                }
                else if (child is BackdropLight)
                {
                    this.viewport3D.Children.RemoveAt(i);
                }
            }
        }

        private void LoadMap(int region)
        {
            if (region <= 0 || region > 4)
            {
                return;
            }

            this.AddMapModels(region);
            this.AddMapBackdrops(region);
        }

        private void AddMapModels(int region)
        {
            bool loadCraftList = this.craftList.Items.Count == 0;

            for (int flightGroupIndex = 0; flightGroupIndex < this.MissionFile.FlightGroups.Count; flightGroupIndex++)
            {
                var flightGroup = this.MissionFile.FlightGroups[flightGroupIndex];
                var model = new CraftModel(flightGroup);

                if (this._missionObjects.TryGetValue(model.CraftName, out string value))
                {
                    model.CraftName = value;
                }

                if (loadCraftList)
                {
                    this.craftList.Items.Add(model);
                }

                if (model.Region != region)
                {
                    continue;
                }

                int craftsCount = Math.Max(flightGroup.CraftsCount, (byte)1);
                int formationType = flightGroup.FormationType;
                int formationSpacing = flightGroup.FormationSpacing + 1;

                for (int wingmanIndex = 0; wingmanIndex < craftsCount; wingmanIndex++)
                {
                    var visual = this.CreateModel3D(
                        !loadCraftList && this._selectedCrafts.Contains(flightGroupIndex),
                        model);

                    if (visual != null)
                    {
                        this.SetModel3DWingman(visual, model.CraftName, wingmanIndex, formationType, formationSpacing);
                        this.viewport3D.Children.Add(visual);
                    }
                }
            }
        }

        private void SetModel3DWingman(Visual3D visual, string name, int wingmanIndex, int formationType, int formationSpacing)
        {
            if (AppSettings.FormationOffsetsX == null)
            {
                return;
            }

            if (formationType < 0 || formationType >= AppSettings.FormationOffsetsX.GetLength(0))
            {
                return;
            }

            if (wingmanIndex < 0 || wingmanIndex >= AppSettings.FormationOffsetsX.GetLength(1))
            {
                return;
            }

            var model = this.GetOptModel(name);

            if (model == null)
            {
                return;
            }

            short formationX = AppSettings.FormationOffsetsX[formationType, wingmanIndex];
            short formationY = AppSettings.FormationOffsetsY[formationType, wingmanIndex];
            short formationZ = AppSettings.FormationOffsetsZ[formationType, wingmanIndex];

            float x = model.SpanSize.X * formationX * formationSpacing;
            float y = model.SpanSize.Y * formationY * formationSpacing;
            float z = model.SpanSize.Z * formationZ * formationSpacing;

            if (formationSpacing == 1)
            {
                x += model.SpanSize.X / 2 * formationX;
                y += model.SpanSize.Y / 2 * formationY;
                z += model.SpanSize.Z / 2 * formationZ;
            }

            var transformGroup = new Transform3DGroup();

            transformGroup.Children.Add(new TranslateTransform3D(y, -x, z));

            foreach (Transform3D transform in ((Transform3DGroup)visual.Transform).Children)
            {
                transformGroup.Children.Add(transform);
            }

            transformGroup.Freeze();
            visual.Transform = transformGroup;
        }

        private void AddMapBackdrops(int region)
        {
            if (AppSettings.ExePlanets == null)
            {
                return;
            }

            bool showLights = this.showLightsButton.IsChecked ?? false;

            for (int flightGroupIndex = 0; flightGroupIndex < this.MissionFile.FlightGroups.Count; flightGroupIndex++)
            {
                var flightGroup = this.MissionFile.FlightGroups[flightGroupIndex];

                int startRegion = flightGroup.StartPointRegions[0] + 1;
                int craftId = flightGroup.CraftId;
                string name = flightGroup.Name;
                int positionX = flightGroup.StartPoints[0].PositionX * 256;
                int positionY = -flightGroup.StartPoints[0].PositionY * 256;
                int positionZ = flightGroup.StartPoints[0].PositionZ * 256;
                string cargo = flightGroup.Cargo;
                string specialCargo = flightGroup.SpecialCargo;
                int planetId = flightGroup.PlanetId;

                if (startRegion != region)
                {
                    continue;
                }

                if (planetId == 0)
                {
                    continue;
                }

                if (craftId != 183)
                {
                    continue;
                }

                if (planetId < 0 || planetId >= AppSettings.ExePlanets.Length)
                {
                    continue;
                }

                var planet = AppSettings.ExePlanets[planetId];

                if (planet.ModelIndex == 0)
                {
                    continue;
                }

                byte imageNumber;

                if (planet.DataIndex2 != 0)
                {
                    imageNumber = 0;
                }
                else if (planet.ModelIndex == 487)
                {
                    imageNumber = 0;
                }
                else
                {
                    if ((planet.Flags & 1) != 0)
                    {
                        imageNumber = flightGroup.GlobalCargoIndex;
                    }
                    else
                    {
                        imageNumber = 0;
                    }
                }

                float size = 1.0f;
                if (!string.IsNullOrEmpty(specialCargo))
                {
                    if (float.TryParse(specialCargo, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                    {
                        size = value;
                    }
                }

                size *= AppSettings.BackdropsScale * 256 * 3;

                float colorI = 0.0f;
                float colorR = 0.0f;
                float colorG = 0.0f;
                float colorB = 0.0f;

                if (!string.IsNullOrEmpty(cargo))
                {
                    string[] parts = cargo.Split();
                    if (parts.Length == 1)
                    {
                        float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out colorI);
                    }
                }

                if (!string.IsNullOrEmpty(name))
                {
                    string[] parts = name.Split();
                    if (parts.Length == 3)
                    {
                        float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out colorR);
                        float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out colorG);
                        float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out colorB);
                    }
                }

                colorR *= colorI * 255;
                colorG *= colorI * 255;
                colorB *= colorI * 255;

                Utils.ComputeHeadingAngles(positionX, positionY, positionZ, out double headingXY, out double headingZ);

                string key = planet.DataIndex1 + ", " + (planet.DataIndex2 != 0 ? planet.DataIndex2 : imageNumber);
                Material material;

                if (this._backdropModels.TryGetValue(key, out BackdropModel image))
                {
                    image.CreateMaterial();

                    material = image.Material;
                    size = size * image.Width / 256;
                }
                else
                {
                    material = null;
                }

                var quad = new QuadVisual3D();
                quad.BeginEdit();

                quad.Point1 = new Point3D(0, 0.5, 0.5);
                quad.Point2 = new Point3D(0, -0.5, 0.5);
                quad.Point3 = new Point3D(0, -0.5, -0.5);
                quad.Point4 = new Point3D(0, 0.5, -0.5);

                quad.Material = material;
                quad.BackMaterial = null;

                //const double scale = 100000 * 3;
                double scale = this.backdropsDistanceSlider.Value * 160 * 256;

                var transforms = new Transform3DGroup();
                transforms.Children.Add(new ScaleTransform3D(size, size, size));

                transforms.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), headingZ)));
                transforms.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), -headingXY)));

                Vector3D position = new Vector3D(positionX, positionY, positionZ);

                if (position.LengthSquared != 0)
                {
                    position.Normalize();
                }

                transforms.Children.Add(new TranslateTransform3D(position.Y * scale, -position.X * scale, position.Z * scale));

                transforms.Freeze();
                quad.Transform = transforms;

                quad.EndEdit();
                this.viewport3D.Children.Add(quad);

                if (colorI != 0.0)
                {
                    var light = new BackdropLight(Color.FromRgb((byte)colorR, (byte)colorG, (byte)colorB), new Vector3D(-position.Y, position.X, -position.Z));

                    if (showLights)
                    {
                        light.ShowLights = true;
                        light.Transform = new ScaleTransform3D(100000, 100000, 100000);

                        var elements = (light.Children[0] as ModelVisual3D).Children.Cast<MeshElement3D>();

                        foreach (MeshElement3D element in elements)
                        {
                            element.BeginEdit();
                            element.Material = new EmissiveMaterial(element.Fill);
                            element.BackMaterial = null;
                            element.EndEdit();
                        }
                    }

                    this.viewport3D.Children.Add(light);
                }
            }
        }

        private void SetSelectedCrafts()
        {
            var selected = new List<int>();

            for (int i = 0; i < this.craftList.Items.Count; i++)
            {
                if (this.craftList.SelectedItems.Contains(this.craftList.Items[i]))
                {
                    selected.Add(i);
                }
            }

            this._selectedCrafts = selected;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Ne pas intercepter les types d'exception générale", Justification = "Justified.")]
        private void CraftList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.craftList.Items.Count == 0)
            {
                return;
            }

            this.SetSelectedCrafts();
            this.ClearMap();

            try
            {
                this.LoadMap(this.regionControl.Value ?? 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadResdataPlanets(string fileName)
        {
            this._backdropModels.Clear();

            string basePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(fileName), System.IO.Path.GetFileNameWithoutExtension(fileName));

            var lines = XwaHooksConfig.GetFileLines(basePath + "_Resdata.txt");

            if (lines.Count == 0)
            {
                lines = XwaHooksConfig.GetFileLines(basePath + ".ini", "Resdata");
            }

            foreach (string line in XwaHooksConfig.GetFileLines(AppSettings.WorkingDirectory + "Resdata.txt"))
            {
                lines.Add(line);
            }

            var planetGroupIds = AppSettings.ExePlanets.Select(t => t.DataIndex1).ToList();

            foreach (string line in lines)
            {
                DatFile dat = DatFile.FromFile(AppSettings.WorkingDirectory + line);

                foreach (DatImage image in dat.Images)
                {
                    if (!planetGroupIds.Contains(image.GroupId))
                    {
                        continue;
                    }

                    string key = image.GroupId + ", " + image.ImageId;

                    if (this._backdropModels.ContainsKey(key))
                    {
                        continue;
                    }

                    this._backdropModels.Add(key, new BackdropModel(image));
                }
            }
        }
    }
}
