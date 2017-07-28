using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRenderTest
{
    public class TraceResult
    {
        private Vector3 _hitPos;
        private float _distance;
        private Vector3 _normal;

        public Vector3 HitPos => _hitPos;
        public float Distance => _distance;
        public Vector3 Normal => _normal;

        public TraceResult(Vector3 hitPos, float distance, Vector3 normal)
        {
            _hitPos = hitPos;
            _distance = distance;
            _normal = normal;
        }
    }
}
