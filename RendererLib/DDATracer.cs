using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VoxelRenderTest
{
    public class DDATracer : Renderer
    {
        private readonly uint _sizeX, _sizeY, _sizeZ;
        private readonly Vector3 _boundLow, _boundHigh;

        private struct Voxel
        {
            //public UInt16 _texU;
            //public UInt16 _texV;
            public Vector3 _normal;
            public bool _occupied;
        }

        private Voxel[] _voxelData;
        
        public uint SizeX => _sizeX;
        public uint SizeY => _sizeY;
        public uint SizeZ => _sizeZ;
        public Vector3 LowBound => _boundLow;
        public Vector3 CellSize
        {
            get
            {
                return new Vector3(
                    (_boundHigh.X - _boundLow.X) / _sizeX,
                    (_boundHigh.Y - _boundLow.Y) / _sizeY,
                    (_boundHigh.Z - _boundLow.Z) / _sizeZ);
            }
        }

        public DDATracer(Vector3 boundLow, Vector3 boundHigh, uint xSize, uint ySize, uint zSize)
        {
            _voxelData = new Voxel[xSize * ySize * zSize];
            _boundLow = boundLow;
            _boundHigh = boundHigh;
            _sizeX = xSize;
            _sizeY = ySize;
            _sizeZ = zSize;
        }

        public override IEnumerable<RasterInfo> Rasterize(
            int xLow, 
            int xHigh, 
            int yLow, 
            int yHigh, 
            Matrix4x4 modelMat, 
            Matrix4x4 modelMatInv, 
            Matrix4x4 viewFrustumMat,
            Matrix4x4 viewFrustumMatInv)
        {
            Vector3 cellSize = CellSize;
            Vector3 pos, dir;
            Vector3i coord;
            Vector3 hitStart, hitEnd;
            Matrix4x4 mvpMatInv = viewFrustumMatInv * modelMatInv;
            Matrix4x4 mvpMat = modelMat * viewFrustumMat;
            for(int y = yLow; y < yHigh; ++y)
            {
                for(int x = xLow; x < xHigh; ++x)
                {
                    CalculateViewRay(mvpMatInv, x, y, out pos, out dir);
                    if(TraceClosest(pos, dir, out coord, out hitStart, out hitEnd))
                    {
                        int index = GetIndex(coord.X, coord.Y, coord.Z);
                        yield return new RasterInfo()
                        {
                            screenX = x,
                            screenY = y,
                            pos = new Vector3(
                                coord.X * cellSize.X + LowBound.X,
                                coord.Y * cellSize.Y + LowBound.Y,
                                coord.Z * cellSize.Z + LowBound.Z),
                            normal = _voxelData[index]._normal,
                        };
                    }
                }
            }
        }

        public bool TraceClosest(Vector3 pos, Vector3 dir, out Vector3i coord, out Vector3 hitStart, out Vector3 hitEnd)
        {
            List<Vector3i> hitList = Trace(pos, dir, out hitStart, out hitEnd);
            if(hitList.Any())
            {
                coord = hitList.First();
                return true;
            }
            else
            {
                coord = new Vector3i(0, 0, 0);
                return false;
            }
        }

        public List<Vector3i> Trace(Vector3 pos, Vector3 dir, out Vector3 hitStart, out Vector3 hitEnd)
        {
            List<Vector3i> hitList = new List<Vector3i>();
            if(!IntersectBound(pos, dir, out hitStart, out hitEnd))
            {
                return hitList;
            }
            Vector3 cellSize = CellSize;
            Vector3 posRel = (hitStart - _boundLow);
            Vector3 posRelCell = new Vector3(
                posRel.X / cellSize.X,
                posRel.Y / cellSize.Y,
                posRel.Z / cellSize.Z);

            float dtx, dty, dtz;
            float tx, ty, tz;
            if(dir.X >= 0.0f)
            {
                tx = (((float)Math.Floor(posRelCell.X) + 1.0f) * cellSize.X - posRel.X) / dir.X;
            }
            else
            {
                tx = ((float)Math.Floor(posRelCell.X) * cellSize.X - posRel.X) / dir.X;
            }
            if (dir.Y >= 0.0f)
            {
                ty = (((float)Math.Floor(posRelCell.Y) + 1.0f) * cellSize.Y - posRel.Y) / dir.Y;
            }
            else
            {
                ty = ((float)Math.Floor(posRelCell.Y) * cellSize.Y - posRel.Y) / dir.Y;
            }
            if (dir.Z >= 0.0f)
            {
                tz = (((float)Math.Floor(posRelCell.Z) + 1.0f) * cellSize.Z - posRel.Z) / dir.Z;
            }
            else
            {
                tz = ((float)Math.Floor(posRelCell.Z) * cellSize.Z - posRel.Z) / dir.Z;
            }

            dtx = cellSize.X / dir.X;
            if (dir.X < 0.0f)
            {
                dtx *= -1;
            }
            dty = cellSize.Y / dir.Y;
            if (dir.Y < 0.0f)
            {
                dty *= -1;
            }
            dtz = cellSize.Z / dir.Z;
            if (dir.Z < 0.0f)
            {
                dtz *= -1;
            }

            Vector3i coord = new Vector3i((int)posRelCell.X, (int)posRelCell.Y, (int)posRelCell.Z);
            while(IsCurrentlyInside(coord))
            {
                int index = GetIndex(coord.X, coord.Y, coord.Z);
                if(_voxelData[index]._occupied)
                {
                    hitList.Add(coord);
                }
                if(tx < ty)
                {
                    if(tx < tz)
                    {
                        tx += dtx;
                        if(dir.X >= 0)
                        {
                            ++coord.X;
                        }
                        else
                        {
                            --coord.X;
                        }
                    }
                    else
                    {
                        tz += dtz;
                        if(dir.Z >= 0)
                        {
                            ++coord.Z;
                        }
                        else
                        {
                            --coord.Z;
                        }
                    }
                }
                else
                {
                    if(ty < tz)
                    {
                        ty += dty;
                        if(dir.Y >= 0)
                        {
                            ++coord.Y;
                        }
                        else
                        {
                            --coord.Y;
                        }
                    }
                    else
                    {
                        tz += dtz;
                        if(dir.Z >= 0)
                        {
                            ++coord.Z;
                        }
                        else
                        {
                            --coord.Z;
                        }
                    }
                }
            }
            return hitList;
        }
        
        private bool IntersectBound(Vector3 pos, Vector3 dir, out Vector3 startPos, out Vector3 endPos)
        {
            float tmin = float.NegativeInfinity, tmax = float.PositiveInfinity;
            Vector3 adjBoundLow = _boundLow + new Vector3(0.01f, 0.01f, 0.01f);
            Vector3 adjBoundHigh = _boundHigh - new Vector3(0.01f, 0.01f, 0.01f);
            if(dir.X != 0.0f)
            {
                float tx1 = (adjBoundLow.X - pos.X) / dir.X;
                float tx2 = (adjBoundHigh.X - pos.X) / dir.X;

                tmin = (float)Math.Max(tmin, Math.Min(tx1, tx2));
                tmax = (float)Math.Min(tmax, Math.Max(tx1, tx2));
            }

            if(dir.Y != 0.0)
            {
                float ty1 = (adjBoundLow.Y - pos.Y) / dir.Y;
                float ty2 = (adjBoundHigh.Y - pos.Y) / dir.Y;

                tmin = (float)Math.Max(tmin, Math.Min(ty1, ty2));
                tmax = (float)Math.Min(tmax, Math.Max(ty1, ty2));
            }

            if(dir.Z != 0.0)
            {
                float tz1 = (adjBoundLow.Z - pos.Z) / dir.Z;
                float tz2 = (adjBoundHigh.Z - pos.Z) / dir.Z;

                tmin = (float)Math.Max(tmin, Math.Min(tz1, tz2));
                tmax = (float)Math.Min(tmax, Math.Max(tz1, tz2));
            }

            if(tmax >= tmin)
            {
                startPos = pos + dir * tmin;
                endPos = pos + dir * tmax;
                return true;
            }
            startPos = new Vector3(0.0f, 0.0f, 0.0f);
            endPos = startPos;
            return false;
        }

        public bool IsCurrentlyInside(Vector3i coord)
        {
            if(coord.X < 0 || coord.Y < 0 || coord.Z < 0 || coord.X >= _sizeX || coord.Y >= _sizeY || coord.Z >= _sizeZ)
            {
                return false;
            }
            return true;
        }

        public override bool Load(Mesh mesh)
        {
            Vector3 cellSize = CellSize;
            int numThread = 4;
            int totalWork = (int)(SizeX * SizeY * SizeZ);
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
            return true;
        }

        private void CalculateVoxel(Mesh mesh, int index, ref Vector3 cellSize)
        {
            int x, y, z;
            GetIndex(index, out x, out y, out z);
            _voxelData[index]._occupied = false;
            Vector3 lowBound = new Vector3(
                        (float)x * cellSize.X + LowBound.X,
                        (float)y * cellSize.Y + LowBound.Y,
                        (float)z * cellSize.Z + LowBound.Z);
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
        
        public int GetIndex(int x, int y, int z)
        {
            return x + y * (int)SizeX + z * (int)SizeX * (int)SizeY;
        }

        public void GetIndex(int index, out int x, out int y, out int z)
        {
            z = index / ((int)SizeX * (int)SizeY);
            int t1 = index % ((int)SizeX * (int)SizeY);
            y = t1 / (int)SizeX;
            x = t1 % (int)SizeX;
        }

        public void GetIndex(int index, out Vector3i coord)
        {
            int x, y, z;
            GetIndex(index, out x, out y, out z);
            coord = new Vector3i(x, y, z);
        }
    }
}
