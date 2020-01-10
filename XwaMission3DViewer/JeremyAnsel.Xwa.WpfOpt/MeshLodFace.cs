using JeremyAnsel.Xwa.Opt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JeremyAnsel.Xwa.WpfOpt
{
    public sealed class MeshLodFace
    {
        internal MeshLodFace(Mesh mesh, MeshLod lod, FaceGroup face)
        {
            this.Mesh = mesh;
            this.Lod = lod;
            this.Face = face;
        }

        public Mesh Mesh { get; private set; }

        public MeshLod Lod { get; private set; }

        public FaceGroup Face { get; private set; }
    }
}
