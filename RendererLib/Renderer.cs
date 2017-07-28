using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRenderTest
{
    public abstract class Renderer
    {
        private int _sizeX;
        private int _sizeY;

        public int ScreenSizeX
        {
            get
            {
                return _sizeX;
            }

            set
            {
                _sizeX = value;
            }
        }

        public int ScreenSizeY
        {
            get
            {
                return _sizeY;
            }

            set
            {
                _sizeY = value;
            }
        }

        public Point ScreenSize
        {
            get
            {
                return new Point(_sizeX, _sizeY);
            }

            set
            {
                _sizeX = value.X;
                _sizeY = value.Y;
            }
        }

        public abstract bool Load(Mesh mesh);
        public abstract IEnumerable<RasterInfo> Rasterize(
            int xLow, int xHigh, int yLow, int yHigh,
            Matrix4x4 modelMat, Matrix4x4 modelMatInv,
            Matrix4x4 viewFrustumMat, Matrix4x4 viewFrustumMatInv);
        
        public float ScreenToWorldCoordX(int screenX)
        {
            return ((float)screenX / _sizeX - 0.5f) * 2f;
        }

        public int WorldToScreenCoordX(float worldX)
        {
            return (int)(((worldX + 1.0f) / 2.0f) * _sizeX);
        }

        public float ScreenToWorldCoordY(int screenY)
        {
            return -((float)screenY / _sizeY - 0.5f) * 2f;
        }

        public int WorldToScreenCoordY(float worldY)
        {
            return (int)(((-worldY + 1.0f) / 2.0f) * _sizeY);
        }

        public Point WorldToScreenCoord(float worldX, float worldY)
        {
            return new Point(WorldToScreenCoordX(worldX), WorldToScreenCoordY(worldY));
        }

        public void CalculateViewRay(Matrix4x4 transformInv, int screenX, int screenY, out Vector3 pos, out Vector3 dir)
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
