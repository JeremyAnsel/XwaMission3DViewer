using JeremyAnsel.Xwa.Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XwaMission3DViewer
{
    public static class AppSettings
    {
        public const string XwaExeFileName = "XWingAlliance.exe";

        public static string WorkingDirectory { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Les propriétés ne doivent pas retourner de tableaux", Justification = "Justified.")]
        public static ushort[] ExeSpecies { get; private set; }

        public static ExeObjectStatistics Objects { get; private set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Les propriétés ne doivent pas retourner de tableaux", Justification = "Justified.")]
        public static string[] SpaceCraft { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Les propriétés ne doivent pas retourner de tableaux", Justification = "Justified.")]
        public static string[] Equipment { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Les propriétés ne doivent pas retourner de tableaux", Justification = "Justified.")]
        public static PlanetEntry[] ExePlanets { get; private set; }

        public static float BackdropsScale { get; private set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Les propriétés ne doivent pas retourner de tableaux", Justification = "Justified.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1814:Préférer les tableaux en escalier aux tableaux multidimensionnels", Justification = "Justified.")]
        public static short[,] FormationOffsetsX { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Les propriétés ne doivent pas retourner de tableaux", Justification = "Justified.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1814:Préférer les tableaux en escalier aux tableaux multidimensionnels", Justification = "Justified.")]
        public static short[,] FormationOffsetsY { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Les propriétés ne doivent pas retourner de tableaux", Justification = "Justified.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1814:Préférer les tableaux en escalier aux tableaux multidimensionnels", Justification = "Justified.")]
        public static short[,] FormationOffsetsZ { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Les propriétés ne doivent pas retourner de tableaux", Justification = "Justified.")]
        public static short[] FormationSpacings { get; private set; }

        public static void SetData()
        {
            if (AppSettings.ExeSpecies == null)
            {
                if (Directory.Exists(AppSettings.WorkingDirectory))
                {
                    AppSettings.ExeSpecies = AppSettings.ReadExeSpecies(AppSettings.WorkingDirectory + AppSettings.XwaExeFileName);
                }
            }

            if (AppSettings.Objects == null)
            {
                if (Directory.Exists(AppSettings.WorkingDirectory))
                {
                    AppSettings.Objects = ExeObjectStatistics.FromFile(AppSettings.WorkingDirectory + AppSettings.XwaExeFileName);
                }
            }

            if (AppSettings.SpaceCraft == null)
            {
                if (Directory.Exists(AppSettings.WorkingDirectory))
                {
                    AppSettings.SpaceCraft = File.ReadAllLines(AppSettings.WorkingDirectory + @"FLIGHTMODELS\SPACECRAFT0.LST", Encoding.ASCII);

                    for (int i = 0; i < AppSettings.SpaceCraft.Length; i++)
                    {
                        AppSettings.SpaceCraft[i] = Path.GetFileNameWithoutExtension(AppSettings.SpaceCraft[i]);
                    }
                }
            }

            if (AppSettings.Equipment == null)
            {
                if (Directory.Exists(AppSettings.WorkingDirectory))
                {
                    AppSettings.Equipment = File.ReadAllLines(AppSettings.WorkingDirectory + @"FLIGHTMODELS\EQUIPMENT0.LST", Encoding.ASCII);

                    for (int i = 0; i < AppSettings.Equipment.Length; i++)
                    {
                        AppSettings.Equipment[i] = Path.GetFileNameWithoutExtension(AppSettings.Equipment[i]);
                    }
                }
            }

            if (AppSettings.ExePlanets == null)
            {
                if (Directory.Exists(AppSettings.WorkingDirectory))
                {
                    AppSettings.ExePlanets = AppSettings.ReadExePlanets(AppSettings.WorkingDirectory + AppSettings.XwaExeFileName);
                }
            }

            if (AppSettings.BackdropsScale == 0.0f)
            {
                if (Directory.Exists(AppSettings.WorkingDirectory))
                {
                    string path = AppSettings.WorkingDirectory + AppSettings.XwaExeFileName;

                    if (!File.Exists(path))
                    {
                        throw new FileNotFoundException(null, path);
                    }

                    ExeVersionString.Match(path);

                    using (BinaryReader file = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read), Encoding.ASCII))
                    {
                        file.BaseStream.Seek(0x1A83AC, SeekOrigin.Begin);
                        AppSettings.BackdropsScale = file.ReadSingle();
                    }
                }
            }

            if (AppSettings.FormationOffsetsX == null)
            {
                if (Directory.Exists(AppSettings.WorkingDirectory))
                {
                    AppSettings.FormationOffsetsX = AppSettings.ReadFormationOffsetsX(AppSettings.WorkingDirectory + AppSettings.XwaExeFileName);
                    AppSettings.FormationOffsetsY = AppSettings.ReadFormationOffsetsY(AppSettings.WorkingDirectory + AppSettings.XwaExeFileName);
                    AppSettings.FormationOffsetsZ = AppSettings.ReadFormationOffsetsZ(AppSettings.WorkingDirectory + AppSettings.XwaExeFileName);
                    AppSettings.FormationSpacings = AppSettings.ReadFormationSpacings(AppSettings.WorkingDirectory + AppSettings.XwaExeFileName);
                }
            }
        }

        private static ushort[] ReadExeSpecies(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(null, path);
            }

            ExeVersionString.Match(path);

            //var species = new ushort[232];
            var species = new ushort[233];

            using (BinaryReader file = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read), Encoding.ASCII))
            {
                file.BaseStream.Seek(0x1AFB70, SeekOrigin.Begin);

                for (int i = 0; i < species.Length; i++)
                {
                    species[i] = file.ReadUInt16();
                }
            }

            return species;
        }

        private static PlanetEntry[] ReadExePlanets(string path)
        {
            if (AppSettings.Objects == null)
            {
                return null;
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(null, path);
            }

            ExeVersionString.Match(path);

            var planets = new PlanetEntry[104];

            using (BinaryReader file = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read), Encoding.ASCII))
            {
                file.BaseStream.Seek(0x1AFD40, SeekOrigin.Begin);

                for (int i = 0; i < planets.Length; i++)
                {
                    var entry = new PlanetEntry
                    {
                        ModelIndex = file.ReadUInt16(),
                        Flags = file.ReadByte()
                    };

                    var obj = AppSettings.Objects?.ElementAtOrDefault(entry.ModelIndex);

                    if (obj != null)
                    {
                        entry.DataIndex1 = obj.DataIndex1;
                        entry.DataIndex2 = obj.DataIndex2;
                    }

                    planets[i] = entry;
                }
            }

            return planets;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1814:Préférer les tableaux en escalier aux tableaux multidimensionnels", Justification = "Justified.")]
        private static short[,] ReadFormationOffsetsX(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(null, path);
            }

            ExeVersionString.Match(path);

            var formation = new short[34, 6];

            using (BinaryReader file = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read), Encoding.ASCII))
            {
                file.BaseStream.Seek(0x1B5C80, SeekOrigin.Begin);

                for (int i = 0; i < formation.GetLength(0); i++)
                {
                    for (int j = 0; j < formation.GetLength(1); j++)
                    {
                        formation[i, j] = file.ReadInt16();
                    }
                }
            }

            return formation;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1814:Préférer les tableaux en escalier aux tableaux multidimensionnels", Justification = "Justified.")]
        private static short[,] ReadFormationOffsetsY(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(null, path);
            }

            ExeVersionString.Match(path);

            var formation = new short[34, 6];

            using (BinaryReader file = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read), Encoding.ASCII))
            {
                file.BaseStream.Seek(0x1B5E18, SeekOrigin.Begin);

                for (int i = 0; i < formation.GetLength(0); i++)
                {
                    for (int j = 0; j < formation.GetLength(1); j++)
                    {
                        formation[i, j] = file.ReadInt16();
                    }
                }
            }

            return formation;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1814:Préférer les tableaux en escalier aux tableaux multidimensionnels", Justification = "Justified.")]
        private static short[,] ReadFormationOffsetsZ(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(null, path);
            }

            ExeVersionString.Match(path);

            var formation = new short[34, 6];

            using (BinaryReader file = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read), Encoding.ASCII))
            {
                file.BaseStream.Seek(0x1B5FB0, SeekOrigin.Begin);

                for (int i = 0; i < formation.GetLength(0); i++)
                {
                    for (int j = 0; j < formation.GetLength(1); j++)
                    {
                        formation[i, j] = file.ReadInt16();
                    }
                }
            }

            return formation;
        }

        private static short[] ReadFormationSpacings(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(null, path);
            }

            ExeVersionString.Match(path);

            var spacings = new short[34];

            using (BinaryReader file = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read), Encoding.ASCII))
            {
                file.BaseStream.Seek(0x1B6148, SeekOrigin.Begin);

                for (int i = 0; i < spacings.Length; i++)
                {
                    spacings[i] = file.ReadInt16();
                }
            }

            return spacings;
        }
    }
}
