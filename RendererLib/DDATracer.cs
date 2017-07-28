using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRenderTest
{
    public class DDATracer
    {
        private readonly uint _sizeX, _sizeY, _sizeZ;
        private readonly Vector3 _boundLow, _boundHigh;

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
            _boundLow = boundLow;
            _boundHigh = boundHigh;
            _sizeX = xSize;
            _sizeY = ySize;
            _sizeZ = zSize;
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

        public List<Vector3i> Trace(Vector3 pos, Vector3 dir, out Vector3 hitStart, out Vector3 hitEnd, bool query = false)
        {
            Vector3 startPos, endPos;
            List<Vector3i> hitList = new List<Vector3i>();
            bool hit = IntersectBound(pos, dir, out startPos, out endPos);
            hitStart = startPos;
            hitEnd = endPos;
            if(!hit)
            {
                return hitList;
            }
            Vector3 cellSize = CellSize;
            Vector3 posRel = (startPos - _boundLow);
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
                hitList.Add(coord);

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

        private void PrintQuery(String message, bool query)
        {
            if(query)
            {
                Console.WriteLine(message);
            }
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
    }
}
