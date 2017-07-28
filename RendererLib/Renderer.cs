using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRenderTest
{
    public abstract class Renderer
    {
        public abstract bool Load(Mesh mesh);
        public abstract uint DrawAt(
            int sizeX, int sizeY, 
            int screenX, int screenY, 
            ref Matrix4x4 modelMat, ref Matrix4x4 modelMatInv,
            ref Matrix4x4 viewFrustumMat, ref Matrix4x4 viewFrustumMatInv);

        protected uint GetColor(int red, int green, int blue)
        {
            return (uint)blue << 16 | (uint)green << 8 | (uint)red;
        }
    }
}
