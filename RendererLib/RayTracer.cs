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

        public bool Trace(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 pos, Vector3 dir, out Vector3 intersection, out float dist)
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

        public override IEnumerable<RasterInfo> Rasterize(
            int xLow, int xHigh, int yLow, int yHigh,
            Matrix4x4 modelMat, Matrix4x4 modelMatInv,
            Matrix4x4 viewFrustumMat, Matrix4x4 viewFrustumMatInv)
        {
            Vector3 pos, dir;
            Matrix4x4 mvpMatInv = viewFrustumMatInv * modelMatInv;
            Matrix4x4 mvpMat = modelMat * viewFrustumMat;
            for(int y = yLow; y < yHigh; ++y)
            {
                for(int x = xLow; x < xHigh; ++x)
                {
                    CalculateViewRay(mvpMatInv, x, y, out pos, out dir);
                    TraceResult result = IntersectClosest(pos, dir);
                    
                    if(result != null)
                    {
                        yield return new RasterInfo()
                        {
                            screenX = x,
                            screenY = y,
                            pos = result.HitPos,
                            normal = result.Normal,
                        };
                    }
                }
            }
        }
        
        public IEnumerable<TraceResult> Intersect(Vector3 pos, Vector3 dir)
        {
            foreach(Triangle face in _mesh.Faces())
            {
                Vector3 tempPos;
                float tempDist;
                Vector3 faceNormal = face.Normal();
                if(Trace(face.P0, face.P1, face.P2, pos, dir, out tempPos, out tempDist))
                {
                    yield return new TraceResult(tempPos, tempDist, faceNormal);
                }
            }
        }

        public TraceResult IntersectClosest(Vector3 pos, Vector3 dir)
        {
            TraceResult traceResult = null;
            foreach(TraceResult curResult in Intersect(pos, dir))
            {
                if(traceResult == null)
                {
                    traceResult = curResult;
                }
                else
                {
                    if(curResult.Distance < traceResult.Distance)
                    {
                        traceResult = curResult;
                    }
                }
            }
            return traceResult;
        }

        private void CalculateViewRay(Matrix4x4 transformInv, int screenX, int screenY, out Vector3 pos, out Vector3 dir)
        {
            float worldX = ScreenToWorldCoordX(screenX);
            float worldY = ScreenToWorldCoordY(screenY);
            Vector4 viewRayPosNear = new Vector4(worldX, worldY, 0.0f, 1.0f);
            Vector4 viewRayPosFar = new Vector4(worldX, worldY, 1.0f, 1.0f);
            viewRayPosNear = Vector4.Transform(viewRayPosNear, transformInv);
            viewRayPosFar = Vector4.Transform(viewRayPosFar, transformInv);

            Vector3 viewRayPosNear3 = new Vector3(
                viewRayPosNear.X / viewRayPosNear.W,
                viewRayPosNear.Y / viewRayPosNear.W,
                viewRayPosNear.Z / viewRayPosNear.W);
            Vector3 viewRayPosFar3 = new Vector3(
                viewRayPosFar.X / viewRayPosFar.W,
                viewRayPosFar.Y / viewRayPosFar.W,
                viewRayPosFar.Z / viewRayPosFar.W);
            Vector3 viewRayDir3 = viewRayPosFar3 - viewRayPosNear3;
            viewRayDir3 = Vector3.Normalize(viewRayDir3);
            pos = viewRayPosNear3;
            dir = viewRayDir3;
        }
    }
}
