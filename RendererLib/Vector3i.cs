using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRenderTest
{
    public struct Vector3i
    {
        private int _x;
        private int _y;
        private int _z;

        #region properties
        public int X
        {
            get
            {
                return _x;
            }
            
            set
            {
                _x = value;
            }
        }

        public int Y
        {
            get
            {
                return _y;
            }

            set
            {
                _y = value;
            }
        }

        public int Z
        {
            get
            {
                return _z;
            }

            set
            {
                _z = value;
            }
        }
        #endregion

        public Vector3i(int x, int y, int z)
        {
            _x = x;
            _y = y;
            _z = z;
        }
        
        public static Vector3i operator +(Vector3i vec1, Vector3i vec2)
        {
            return new Vector3i(vec1.X + vec2.X, vec1.Y + vec2.Y, vec1.Z + vec2.Z);
        }

        public static Vector3i operator -(Vector3i vec1, Vector3i vec2)
        {
            return new Vector3i(vec1.X - vec2.X, vec1.Y - vec2.Y, vec1.Z - vec2.Z);
        }

        public static bool operator ==(Vector3i vec1, Vector3i vec2)
        {
            return vec1.X == vec2.X & vec1.Y == vec2.Y & vec1.Z == vec2.Z;
        }

        public static bool operator !=(Vector3i vec1, Vector3i vec2)
        {
            return vec1.X != vec2.X | vec1.Y != vec2.Y | vec1.Z != vec2.Z;
        }

        public override string ToString()
        {
            return _x + " " + _y + " " + _z;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is Vector3i))
                return false;

            Vector3i other = (Vector3i)obj;
            return this == other;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
