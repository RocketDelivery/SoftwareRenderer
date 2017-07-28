using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRenderTest
{
    using System.Numerics;
    using Face = Tuple<int, int, int>;

    public class Mesh
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
    }
}
