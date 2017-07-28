using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using System.Threading;
using System.Diagnostics;

namespace VoxelRenderTest
{
    public partial class RenderScreen : Form
    {
        private Mesh _objMesh;
        private MeshVoxel1 _voxelMesh1;
        private Vector3 _cameraPos = new Vector3(4.0f, 4.0f, 4.0f);
        private Vector3 _cameraTarget = new Vector3(0.0f, 0.0f, 0.0f);
        private Vector3 _lightPos = new Vector3(5f, 2.0f, 1.5f);
        private Vector3 _origin = new Vector3(0.0f, 0.0f, 0.0f);
        private Vector3 _xAxis = new Vector3(1.0f, 0.0f, 0.0f);
        private Vector3 _yAxis = new Vector3(0.0f, 1.0f, 0.0f);
        private Vector3 _zAxis = new Vector3(0.0f, 0.0f, 1.0f);
        private Pen _xPen = new Pen(Color.Red);
        private Pen _yPen = new Pen(Color.Green);
        private Pen _zPen = new Pen(Color.Blue);
        private bool _doRotation = false;
        private bool _doTranslation = false;
        private int _prevMousePosX;
        private int _prevMousePosY;

        private bool _perspective = true;
        private Traceable _traceable;

        public RenderScreen()
        {
            _objMesh = ModelLoader.LoadObj("D:\\Downloads\\gourd.obj");
            _voxelMesh1 = new MeshVoxel1(64, 64, 64, new Vector3(-2f, -2f, -2f), new Vector3(2f, 2f, 2f));
            _voxelMesh1.LoadMesh(_objMesh, 4);
            _xPen.Width = 2;
            _yPen.Width = 2;
            _zPen.Width = 2;
            _traceable = _voxelMesh1;
            
            InitializeComponent();                 
        }

        private void DrawArea_Paint(object sender, PaintEventArgs e)
        {
            bool lighting = true;
            Matrix4x4 transform, transformInv;
            CalculateTransform(_perspective, out transform, out transformInv);

            Matrix4x4 modelMat = Matrix4x4.Identity;//Matrix4x4.CreateRotationY((float)Math.PI);
            Matrix4x4 modelMatInv;
            if(!Matrix4x4.Invert(modelMat, out modelMatInv))
            {
                Console.WriteLine("Could not invert model matrix");
            }
            
            // While changing rotation, do not draw the mesh.
            if(!_doRotation && !_doTranslation)
            {
                DrawMesh(_traceable, e.Graphics, modelMatInv, transformInv, lighting);
            }
            DrawAxis(e.Graphics, transform);
        }

        private void CalculateTransform(bool perspective, out Matrix4x4 transform, out Matrix4x4 transformInv)
        {
            Matrix4x4 viewMat = Matrix4x4.CreateLookAt(_cameraPos, _cameraTarget, new Vector3(0.0f, 1.0f, 0.0f));
            Matrix4x4 viewMatInv;
            if(!Matrix4x4.Invert(viewMat, out viewMatInv))
            {
                Console.WriteLine("Could not invert transform matrix");
            }
            if(perspective)
            {
                Matrix4x4 projMat = Matrix4x4.CreatePerspectiveFieldOfView(1.0472f, 1, 1, 30);
                Matrix4x4 projMatInv;
                if(!Matrix4x4.Invert(projMat, out projMatInv))
                {
                    Console.WriteLine("Could not invert projection matrix");
                }
                transform = viewMat * projMat;
                transformInv = projMatInv * viewMatInv;
            }
            else
            {
                transform = viewMat;
                transformInv = viewMatInv;
            }
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

        private unsafe void DrawMesh(Traceable mesh, Graphics graphics, Matrix4x4 modelMatInv, Matrix4x4 transformInv, bool useLighting)
        {
            Stopwatch stopWatch = Stopwatch.StartNew();
            stopWatch.Start();
            IntPtr hdc = graphics.GetHdc();
            int renderWidth = this.DrawArea.Width;
            int renderHeight = this.DrawArea.Height;
            uint[] screenBuffer = new uint[renderWidth * renderHeight];

            const int workDivision = 8;
            int totalWork = renderHeight * renderWidth;
            int workPerThread = totalWork / workDivision;
            int remainder = totalWork % workDivision;
            int indexStart = 0;
            int remainingWork = workDivision;
            for(int i = 0; i < workDivision; ++i)
            {
                int indexStartCopy = indexStart;
                int workCount = workPerThread;
                if((remainder - i) > 0)
                {
                    ++workCount;
                }
                ThreadPool.QueueUserWorkItem(
                    (object state) =>
                    {
                        for(int index = indexStartCopy; index < indexStartCopy + workCount; ++index)
                        {
                            int y = index / renderHeight;
                            int x = index % renderHeight;
                            Vector3 pos, dir;
                            CalculateViewRay(transformInv * modelMatInv, x, y, out pos, out dir);
                            TraceResult result = mesh.IntersectClosest(pos, dir);

                            if(result != null)
                            {
                                uint color = 0xFF;
                                if(useLighting)
                                {
                                    Vector3 lightDir = new Vector3(
                                        _lightPos.X - result.HitPos.X,
                                        _lightPos.Y - result.HitPos.Y,
                                        _lightPos.Z - result.HitPos.Z);
                                    lightDir = Vector3.Normalize(lightDir);
                                    float light = Vector3.Dot(lightDir, result.Normal);
                                    if(light < 0.0f)
                                    {
                                        light = 0.0f;
                                    }
                                    color = (uint)(0xFF * light) + 20;
                                    if(color > 0xFF)
                                    {
                                        color = 0xFF;
                                    }
                                }
                                screenBuffer[index] = color << 16 | color << 8 | color;
                            }
                        }
                        Interlocked.Decrement(ref remainingWork);
                    });
                indexStart += workCount;
            }
            while(remainingWork > 0)
            {
                Thread.Yield();
            }
            fixed (uint* buffer = screenBuffer)
            {
                IntPtr bitMap = CreateBitmap(renderWidth, renderHeight, 1, 8 * 4, (IntPtr)buffer);
                IntPtr src = CreateCompatibleDC(hdc);
                SelectObject(src, bitMap);
                BitBlt(
                    hdc, 0, 0,
                    renderWidth, renderHeight,
                    src, 0, 0,
                    13369376); //SRCCOPY
                DeleteDC(src);
                DeleteObject(bitMap);
            }

            graphics.ReleaseHdc(hdc);
            Console.WriteLine("Finished rendering in " + stopWatch.ElapsedMilliseconds + " milliseconds.");
        }

        private void DrawAxis(Graphics graphics, Matrix4x4 transform)
        {
            Vector4 origin4 = Vector4.Transform(new Vector4(_origin, 1.0f), transform);
            Vector4 xAxis4 = Vector4.Transform(new Vector4(_xAxis, 1.0f), transform);
            Vector4 yAxis4 = Vector4.Transform(new Vector4(_yAxis, 1.0f), transform);
            Vector4 zAxis4 = Vector4.Transform(new Vector4(_zAxis, 1.0f), transform);
            Vector3 origin = new Vector3(origin4.X / origin4.W, origin4.Y / origin4.W, origin4.Y / origin4.W);
            Vector3 xAxis = new Vector3(xAxis4.X / xAxis4.W, xAxis4.Y / xAxis4.W, xAxis4.Y / xAxis4.W);
            Vector3 yAxis = new Vector3(yAxis4.X / yAxis4.W, yAxis4.Y / yAxis4.W, yAxis4.Y / yAxis4.W);
            Vector3 zAxis = new Vector3(zAxis4.X / zAxis4.W, zAxis4.Y / zAxis4.W, zAxis4.Y / zAxis4.W);
            Point originPoint = WorldToScreenCoord(origin.X, origin.Y);
            graphics.DrawLine(
                _xPen,
                originPoint,
                WorldToScreenCoord(xAxis.X, xAxis.Y));
            graphics.DrawLine(
                _yPen,
                originPoint,
                WorldToScreenCoord(yAxis.X, yAxis.Y));
            graphics.DrawLine(
                _zPen,
                originPoint,
                WorldToScreenCoord(zAxis.X, zAxis.Y));
        }

        private float ScreenToWorldCoordX(int screenX)
        {
            return ((float)screenX / DrawArea.Width - 0.5f) * 2f;
        }

        private int WorldToScreenCoordX(float worldX)
        {
            return (int)(((worldX + 1.0f) / 2.0f) * DrawArea.Width);
        }

        private float ScreenToWorldCoordY(int screenY)
        {
            return -((float)screenY / DrawArea.Height - 0.5f) * 2f;
        }

        private int WorldToScreenCoordY(float worldY)
        {
            return (int)(((-worldY + 1.0f) / 2.0f) * DrawArea.Height);
        }

        private Point WorldToScreenCoord(float worldX, float worldY)
        {
            return new Point(WorldToScreenCoordX(worldX), WorldToScreenCoordY(worldY));
        }

        private void DrawArea_MouseDown(object sender, MouseEventArgs e)
        {
            _prevMousePosX = e.X;
            _prevMousePosY = e.Y;
            
            if(e.Button == MouseButtons.Right)
            {
                if(!_doTranslation)
                {
                    _doRotation = true;
                }
            }
            if(e.Button == MouseButtons.Middle)
            {
                if(!_doRotation)
                {
                    _doTranslation = true;
                }
            }
            if(e.Button == MouseButtons.Left)
            {
                Console.WriteLine("Ray query at screen X: " + e.Location.X + " Y: " + e.Location.Y);
                Matrix4x4 transform, transformInv;
                Vector3 pos, dir;
                CalculateTransform(_perspective, out transform, out transformInv);
                CalculateViewRay(transformInv, e.Location.X, e.Location.Y, out pos, out dir);
                TraceResult result = _traceable.IntersectClosest(pos, dir, true);
                if(result != null)
                {
                    Console.WriteLine("Closest trace at :" + result.HitPos);
                }
            }
        }

        private void DrawArea_MouseUp(object sender, MouseEventArgs e)
        {
            _doRotation = false;
            _doTranslation = false;
            DrawArea.Invalidate();
        }

        private void DrawArea_MouseMove(object sender, MouseEventArgs e)
        {
            int deltaX = e.X - _prevMousePosX;
            int deltaY = e.Y - _prevMousePosY;
            _prevMousePosX = e.X;
            _prevMousePosY = e.Y;

            if(_doRotation)
            {
                _cameraPos = Vector3.Transform(_cameraPos, Matrix4x4.CreateRotationY(deltaX * -0.01f));
                Matrix4x4 viewMat = Matrix4x4.CreateLookAt(_cameraPos, _cameraTarget, new Vector3(0.0f, 1.0f, 0.0f));
                Vector3 rotAxis = new Vector3(viewMat.M11, viewMat.M21, viewMat.M31);
                _cameraPos = Vector3.Transform(_cameraPos, Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(rotAxis, deltaY * -0.01f)));
                DrawArea.Invalidate();
            }
            if(_doTranslation)
            {
                Matrix4x4 viewMat = Matrix4x4.CreateLookAt(_cameraPos, _cameraTarget, new Vector3(0.0f, 1.0f, 0.0f));
                Vector3 xMove = new Vector3(viewMat.M11, viewMat.M21, viewMat.M31);
                Vector3 yMove = new Vector3(viewMat.M12, viewMat.M22, viewMat.M32);
                _cameraPos += (xMove * deltaX + yMove * -deltaY) * -0.01f;
                _cameraTarget += (xMove * deltaX + yMove * -deltaY) * -0.01f;
                DrawArea.Invalidate();
            }
        }

        private void DrawArea_MouseWheel(object sender, MouseEventArgs e)
        {
            if(_doRotation)
            {
                Vector3 cameraDir = _cameraPos - _cameraTarget;
                cameraDir = Vector3.Normalize(cameraDir);
                _cameraPos += cameraDir * (e.Delta / 360f);
                DrawArea.Invalidate();
            }
        }

        private void DrawArea_MouseEnter(object sender, EventArgs e)
        {
            DrawArea.Focus();
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        internal static extern IntPtr CreateBitmap(int nWidth, int nHeight, uint cPlanes, uint cBitsPerPel, IntPtr lpvBits);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        internal static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        internal static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        internal static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        internal static extern bool DeleteDC(IntPtr hdc);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        internal static extern bool DeleteObject(IntPtr hObject);
    }
}
