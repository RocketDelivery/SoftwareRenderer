using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRenderTest
{
    public class RayTracer : Renderer
    {
        private Mesh _mesh;

        public static bool Test(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 pos, Vector3 dir, out Vector3 intersection, out float dist)
        {
            Vector3 normal = Vector3.Normalize(Vector3.Cross(p1 - p0, p2 - p0));
            // http://www.cs.cornell.edu/courses/cs465/2003fa/homeworks/raytri.pdf
            float dirCos = Vector3.Dot(normal, dir);
            if(Math.Abs(dirCos) < 0.001)
            {
                dist = float.NaN;
                intersection = new Vector3(float.NaN, float.NaN, float.NaN);
                return false;
            }
            float t = -Vector3.Dot((pos - p0), normal) / Vector3.Dot(dir, normal);
            dist = t;
            Vector3 P = pos + dir * t;
            intersection = P;
            if(t < 0.0f)
            {
                return false;
            }

            if(Vector3.Dot(Vector3.Cross((p1 - p0), (P - p0)), normal) >= 0 &&
                Vector3.Dot(Vector3.Cross((p2 - p1), (P - p1)), normal) >= 0 &&
                Vector3.Dot(Vector3.Cross((p0 - p2), (P - p2)), normal) >= 0)
            {
                return true;
            }
            return false;
        }

        public override bool Load(Mesh mesh)
        {
            _mesh = mesh;
            return true;
        }

        public override uint DrawAt(
            int sizeX, int sizeY,
            int screenX, int screenY,
            ref Matrix4x4 modelMat, ref Matrix4x4 modelMatInv,
            ref Matrix4x4 viewFrustumMat, ref Matrix4x4 viewFrustumMatInv)
        {
            return 0;
        }
    }
}
