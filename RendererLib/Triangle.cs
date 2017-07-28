using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRenderTest
{
    public struct Triangle
    {
        private Vector3 _p0, _p1, _p2;

        public Vector3 P0 => _p0;
        public Vector3 P1 => _p1;
        public Vector3 P2 => _p2;

        public Triangle(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            _p0 = p0;
            _p1 = p1;
            _p2 = p2;
        }

        public Vector3 Normal()
        {
            Vector3 normal = Vector3.Cross(_p1 - _p0, _p2 - _p0);
            normal = Vector3.Normalize(normal);
            return normal;
        }
    }
}
