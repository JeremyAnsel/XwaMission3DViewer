﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JeremyAnsel.Xwa.Statistics
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Les identificateurs doivent avoir un suffixe correct", Justification = "Justified.")]
    public sealed class ExeObjectStatistics : Collection<ExeObjectEntry>
    {
        private const int BaseOffset = 0x1F9E40;

        private const int Length = 557;

        public static ExeObjectStatistics FromFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(null, path);
            }

            ExeVersionString.Match(path);

            ExeObjectStatistics obj = new ExeObjectStatistics();

            using (BinaryReader file = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read), Encoding.ASCII))
            {
                for (int i = 0; i < ExeObjectStatistics.Length; i++)
                {
                    file.BaseStream.Seek(ExeObjectStatistics.BaseOffset + i * ExeObjectEntry.Length, SeekOrigin.Begin);

                    ExeObjectEntry entry = ExeObjectEntry.FromByteArray(file.ReadBytes(ExeObjectEntry.Length));

                    obj.Add(entry);
                }
            }

            return obj;
        }
    }
}
