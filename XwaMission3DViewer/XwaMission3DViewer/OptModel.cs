using JeremyAnsel.Xwa.Opt;
using JeremyAnsel.Xwa.WpfOpt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XwaMission3DViewer
{
    public sealed class OptModel
    {
        public OptModel(string fileName)
        {
            this.File = OptFile.FromFile(fileName);
            this.Cache = new OptCache(this.File);
            this.SpanSize = this.File.SpanSize;
        }

        public OptFile File { get; private set; }

        public OptCache Cache { get; private set; }

        public Vector SpanSize { get; private set; }
    }
}
