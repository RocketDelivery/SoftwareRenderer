using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Numerics;

namespace VoxelRenderTest
{
    public class VoxelGrid1
    {
        private DDATracer _ddaTracer;

        public VoxelGrid1(
            int sizeX, int sizeY, int sizeZ,
            Vector3 boundLow,
            Vector3 boundHigh)
        {
            
            _ddaTracer = new DDATracer(
                boundLow, boundHigh,
                (uint)sizeX, (uint)sizeY, (uint)sizeZ);
        }
        
        public void LoadMesh(Mesh mesh, int numThread)
        {
            
        }


        
    }
}
