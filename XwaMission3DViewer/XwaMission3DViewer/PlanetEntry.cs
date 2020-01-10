using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XwaMission3DViewer
{
    public sealed class PlanetEntry
    {
        public ushort ModelIndex { get; set; }

        public byte Flags { get; set; }

        public short DataIndex1 { get; set; }

        public short DataIndex2 { get; set; }
    }
}
