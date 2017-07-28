using System.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRenderTest
{
    public class SATCollisionSolver
    {
        static Vector3[] boxTriangleTest;

        static SATCollisionSolver()
        {
            boxTriangleTest = new Vector3[13];
            boxTriangleTest[0] = new Vector3(1.0f, 0.0f, 0.0f);
            boxTriangleTest[1] = new Vector3(0.0f, 1.0f, 0.0f);
            boxTriangleTest[2] = new Vector3(0.0f, 0.0f, 1.0f);
        }

        static public bool Cull(
            float boxRadius, Vector3 boxPos,
            Vector3 v0, Vector3 v1, Vector3 v2)
        {
            Vector3 normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
            if(boxRadius < Math.Abs(Vector3.Dot((boxPos - v0), normal)))
            {
                return false;
            }
            return true;
        }

        static public bool Test(
            Vector3 boxLow, Vector3 boxHigh,
            Vector3 v0, Vector3 v1, Vector3 v2)
        {
            Vector3 origin = (boxHigh - boxLow) / 2 + boxLow;
            v0 -= origin;
            v1 -= origin;
            v2 -= origin;
            boxLow -= origin;
            boxHigh -= origin;
            boxTriangleTest[3] = Vector3.Cross(v1 - v0, v2 - v0);
            Vector3 edge0 = v1 - v0;
            Vector3 edge1 = v2 - v1;
            Vector3 edge2 = v0 - v2;
            boxTriangleTest[4]  = Vector3.Cross(edge0, boxTriangleTest[0]);
            boxTriangleTest[5]  = Vector3.Cross(edge0, boxTriangleTest[1]);
            boxTriangleTest[6]  = Vector3.Cross(edge0, boxTriangleTest[2]);
            boxTriangleTest[7]  = Vector3.Cross(edge1, boxTriangleTest[0]);
            boxTriangleTest[8]  = Vector3.Cross(edge1, boxTriangleTest[1]);
            boxTriangleTest[9]  = Vector3.Cross(edge1, boxTriangleTest[2]);
            boxTriangleTest[10] = Vector3.Cross(edge2, boxTriangleTest[0]);
            boxTriangleTest[11] = Vector3.Cross(edge2, boxTriangleTest[1]);
            boxTriangleTest[12] = Vector3.Cross(edge2, boxTriangleTest[2]);
            Vector3 halfWidth = (boxHigh - boxLow) / 2.0f;
            foreach(Vector3 axis in boxTriangleTest)
            {
                float radius =
                    Math.Abs(Vector3.Dot(halfWidth.X * boxTriangleTest[0], axis)) +
                    Math.Abs(Vector3.Dot(halfWidth.Y * boxTriangleTest[1], axis)) +
                    Math.Abs(Vector3.Dot(halfWidth.Z * boxTriangleTest[2], axis));
                float p0 = Vector3.Dot(axis, v0);
                float p1 = Vector3.Dot(axis, v1);
                float p2 = Vector3.Dot(axis, v2);
                float pMin = Math.Min(Math.Min(p0, p1), p2);
                float pMax = Math.Max(Math.Max(p0, p1), p2);
                if(pMin > radius || pMax < -radius)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
