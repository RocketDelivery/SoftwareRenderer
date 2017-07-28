using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Numerics;

namespace VoxelRenderTest
{
    public class MeshVoxel1 : Traceable
    {
        private struct Voxel
        {
            //public UInt16 _texU;
            //public UInt16 _texV;
            public Vector3 _normal;
            public bool _occupied;
        }

        private Voxel[] _voxelData;
        private DDATracer _ddaTracer;

        public MeshVoxel1(
            int sizeX, int sizeY, int sizeZ,
            Vector3 boundLow,
            Vector3 boundHigh)
        {
            _voxelData = new Voxel[sizeX * sizeY * sizeZ];
            _ddaTracer = new DDATracer(
                boundLow, boundHigh,
                (uint)sizeX, (uint)sizeY, (uint)sizeZ);
        }

        public int GetIndex(int x, int y, int z)
        {
            return x + y * (int)_ddaTracer.SizeX + z * (int)_ddaTracer.SizeX * (int)_ddaTracer.SizeY;
        }

        public void GetIndex(int index, out int x, out int y, out int z)
        {
            z = index / ((int)_ddaTracer.SizeX * (int)_ddaTracer.SizeY);
            int t1 = index % ((int)_ddaTracer.SizeX * (int)_ddaTracer.SizeY);
            y = t1 / (int)_ddaTracer.SizeX;
            x = t1 % (int)_ddaTracer.SizeX;
        }

        public void GetIndex(int index, out Vector3i coord)
        {
            int x, y, z;
            GetIndex(index, out x, out y, out z);
            coord = new Vector3i(x, y, z);
        }
        
        public void CalculateVoxel(Mesh mesh, int index, ref Vector3 cellSize)
        {
            int x, y, z;
            GetIndex(index, out x, out y, out z);
            _voxelData[index]._occupied = false;
            Vector3 lowBound = new Vector3(
                        (float)x * cellSize.X + _ddaTracer.LowBound.X,
                        (float)y * cellSize.Y + _ddaTracer.LowBound.Y,
                        (float)z * cellSize.Z + _ddaTracer.LowBound.Z);
            Vector3 highBound = lowBound + cellSize;
            Vector3 boxCenter = lowBound + ((highBound - lowBound) / 2);
            float boxRadius = ((highBound - lowBound) / 2).Length();
            List<Vector3> normals = new List<Vector3>();
            foreach(Triangle tri in mesh.Faces())
            {
                if(SATCollisionSolver.Cull(boxRadius, boxCenter, tri.P0, tri.P1, tri.P2))
                {
                    if(SATCollisionSolver.Test(lowBound, highBound, tri.P0, tri.P1, tri.P2))
                    {
                        _voxelData[index]._occupied = true;
                        normals.Add(Vector3.Normalize(Vector3.Cross(tri.P1 - tri.P0, tri.P2 - tri.P0)));
                    }
                }
            }
            if(_voxelData[index]._occupied)
            {
                Vector3 normalAvg = new Vector3();
                foreach(Vector3 normal in normals)
                {
                    normalAvg += normal;
                }
                _voxelData[index]._normal = Vector3.Normalize(normalAvg);
            }
        }

        public void LoadMesh(Mesh mesh, int numThread)
        {
            Vector3 cellSize = _ddaTracer.CellSize;
            int totalWork = (int)(_ddaTracer.SizeX * _ddaTracer.SizeY * _ddaTracer.SizeZ);
            int workPerThread = totalWork / numThread;
            int currentWork = 0;
            Thread[] threads = new Thread[numThread];
            for(int i = 0; i < threads.Length; ++i)
            {
                int startIndex = i * workPerThread;
                threads[i] = new Thread(() =>
                {
                    for(int index = startIndex; index < startIndex + workPerThread; ++index)
                    {
                        CalculateVoxel(mesh, index, ref cellSize);
                        Interlocked.Increment(ref currentWork);
                    }
                });
            }
            foreach(Thread thread in threads)
            {
                thread.Start();
            }
            Console.Write("Voxelizing...[" + (0 / (float)totalWork).ToString("#.0") + "%]");
            while(true)
            {
                Thread.Sleep(100);
                Console.Write("\rVoxelizing...[" + (100 * currentWork / (float)totalWork).ToString("#.0") + "%]");
                if(currentWork >= totalWork)
                {
                    break;
                }
            }
            Console.Write("\rVoxelizing...[100%] Done.\a\n");
            foreach(Thread thread in threads)
            {
                thread.Join();
            }
        }

        public override IEnumerable<TraceResult> Intersect(Vector3 pos)
        {
            return Intersect(pos, new Vector3(1.0f, 0.0f, 0.0f), false);
        }

        public override IEnumerable<TraceResult> Intersect(Vector3 pos, Vector3 dir, bool query)
        {
            Vector3 hitStart, hitEnd;
            List<Vector3i> hitList = _ddaTracer.Trace(pos, dir, out hitStart, out hitEnd, query);
            Vector3 cellSize = _ddaTracer.CellSize;
            foreach(Vector3i coord in hitList)
            {
                int index = GetIndex(coord.X, coord.Y, coord.Z);
                if(_voxelData[index]._occupied)
                {
                    Vector3 hitWorldPos = new Vector3(
                        coord.X * cellSize.X + _ddaTracer.LowBound.X, 
                        coord.Y * cellSize.Y + _ddaTracer.LowBound.Y, 
                        coord.Z * cellSize.Z + _ddaTracer.LowBound.Z);
                    yield return new TraceResult(
                        hitWorldPos,
                        (pos - hitWorldPos).Length(),
                        _voxelData[index]._normal);
                }
            }
        }
    }
}
