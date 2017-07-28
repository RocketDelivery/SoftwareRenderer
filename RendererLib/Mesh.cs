using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRenderTest
{
    using System.Numerics;
    using Face = Tuple<int, int, int>;

    public class Mesh : Traceable
    {
        private List<Vector3> _vertices;
        private List<Face> _faces;

        public Mesh(List<Vector3> vertices, List<Face> faces)
        {
            _vertices = vertices;
            _faces = faces;
        }

        public IEnumerable<Triangle> Faces()
        {
            foreach(Face face in _faces)
            {
                yield return new Triangle(
                    _vertices[face.Item1],
                    _vertices[face.Item2],
                    _vertices[face.Item3]);
            }
        }

        public override IEnumerable<TraceResult> Intersect(Vector3 pos)
        {
            foreach(Face face in _faces)
            {
                Vector3 tempPos;
                float tempDist;
                Vector3 p0 = _vertices[face.Item1];
                Vector3 p1 = _vertices[face.Item2];
                Vector3 p2 = _vertices[face.Item3];
                Vector3 faceNormal = GetFaceNormal(face);
                if(RayTracer.Test(p0, p1, p2, pos, -faceNormal, out tempPos, out tempDist))
                {
                    yield return new TraceResult(tempPos, tempDist, faceNormal);
                }
            }
        }

        public override IEnumerable<TraceResult> Intersect(Vector3 pos, Vector3 dir, bool query)
        {
            foreach(Face face in _faces)
            {
                Vector3 tempPos;
                float tempDist;
                Vector3 faceNormal = GetFaceNormal(face);
                Vector3 p0 = _vertices[face.Item1];
                Vector3 p1 = _vertices[face.Item2];
                Vector3 p2 = _vertices[face.Item3];
                if(RayTracer.Test(p0, p1, p2, pos, dir, out tempPos, out tempDist))
                {
                    yield return new TraceResult(tempPos, tempDist, faceNormal);
                }         
            }
        }

        private Vector3 GetFaceNormal(Face face)
        {
            Vector3 normal = Vector3.Cross(
                _vertices[face.Item2] - _vertices[face.Item1],
                _vertices[face.Item3] - _vertices[face.Item1]);
            normal = Vector3.Normalize(normal);
            return normal;
        }
    }
}
