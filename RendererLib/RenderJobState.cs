using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRenderTest
{
    public class RenderJobState
    {
        public int startIndex;
        public int workCount;
        public int screenHeight;
        public uint[] buffer;
        public Matrix4x4 transformInv;
        public Matrix4x4 modelMatInv;
    }
}
