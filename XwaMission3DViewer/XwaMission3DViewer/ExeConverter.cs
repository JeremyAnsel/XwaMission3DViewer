using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XwaMission3DViewer
{
    public static class ExeConverter
    {
        public static string ModelIndexToName(int modelIndex)
        {
            var obj = AppSettings.Objects?.ElementAtOrDefault(modelIndex);

            if (obj == null)
            {
                return string.Empty;
            }

            if (obj.DataIndex1 == -1)
            {
                return string.Empty;
            }

            if (AppSettings.SpaceCraft != null && AppSettings.Equipment != null)
            {
                switch (obj.DataIndex1)
                {
                    case 0:
                        return AppSettings.SpaceCraft.ElementAtOrDefault(obj.DataIndex2) ?? string.Empty;

                    case 1:
                        return AppSettings.Equipment.ElementAtOrDefault(obj.DataIndex2) ?? string.Empty;
                }
            }

            return obj.DataIndex1 + ", " + obj.DataIndex2;
        }

        public static string CraftIdToName(int craftId)
        {
            if (AppSettings.ExeSpecies == null || craftId < 0 || craftId >= AppSettings.ExeSpecies.Length)
            {
                return string.Empty;
            }

            ushort modelIndex = AppSettings.ExeSpecies[craftId];
            return ExeConverter.ModelIndexToName(modelIndex);
        }
    }
}
