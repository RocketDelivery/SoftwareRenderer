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
        //private MeshVoxel1 _voxelMesh1;
        private Renderer _renderer;
        private FragmentShaderSingleLight _fragShader;
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
        
        public RenderScreen()
        {
            _objMesh = ModelLoader.LoadObj("D:\\Downloads\\box.obj");
            //_voxelMesh1 = new MeshVoxel1(64, 64, 64, new Vector3(-2f, -2f, -2f), new Vector3(2f, 2f, 2f));
            //_voxelMesh1.LoadMesh(_objMesh, 4);
            _xPen.Width = 2;
            _yPen.Width = 2;
            _zPen.Width = 2;
                        
            InitializeComponent();

            _renderer = new RayTracer();
            if(!_renderer.Load(_objMesh))
            {
                Console.WriteLine("Could not load mesh!");
            }
            _fragShader = new FragmentShaderSingleLight();
        }

        private void DrawArea_Paint(object sender, PaintEventArgs e)
        {
            bool lighting = true;
            Matrix4x4 viewMat, invViewMat, projMat, invProjMat;
            CalculateTransform(_perspective, out viewMat, out invViewMat, out projMat, out invProjMat);

            Matrix4x4 modelMat = Matrix4x4.Identity;//Matrix4x4.CreateRotationY((float)Math.PI);
            Matrix4x4 invModelMat;
            if(!Matrix4x4.Invert(modelMat, out invModelMat))
            {
                Console.WriteLine("Could not invert model matrix");
            }

            // While changing rotation, do not draw the mesh.
            if(!_doRotation && !_doTranslation)
            {
                _renderer.ScreenSizeX = DrawArea.Size.Width;
                _renderer.ScreenSizeY = DrawArea.Size.Height;
                _fragShader.SetUniformData(
                    ref modelMat,
                    ref viewMat,
                    ref projMat,
                    ref invModelMat,
                    ref invViewMat,
                    ref invProjMat,
                    _renderer.ScreenSizeX,
                    _renderer.ScreenSizeY);
                _fragShader.SetLightPosition(_lightPos);
                DrawMesh(_renderer, _fragShader, e.Graphics, modelMat, invModelMat, viewMat, invViewMat, projMat, invProjMat, lighting);
            }
            DrawAxis(e.Graphics, viewMat * projMat);
        }

        private void CalculateTransform(
            bool perspective, 
            out Matrix4x4 viewMat, 
            out Matrix4x4 invViewMat, 
            out Matrix4x4 projMat, 
            out Matrix4x4 invProjMat)
        {
            viewMat = Matrix4x4.CreateLookAt(_cameraPos, _cameraTarget, new Vector3(0.0f, 1.0f, 0.0f));
            if(!Matrix4x4.Invert(viewMat, out invViewMat))
            {
                Console.WriteLine("Could not invert transform matrix");
            }
            if(perspective)
            {
                projMat = Matrix4x4.CreatePerspectiveFieldOfView(1.0472f, 1, 1, 30);
                if(!Matrix4x4.Invert(projMat, out invProjMat))
                {
                    Console.WriteLine("Could not invert projection matrix");
                }
            }
            else
            {
                projMat = Matrix4x4.Identity;
                invProjMat = projMat;
            }
        }

        private unsafe void DrawMesh(
            Renderer renderer,
            FragmentShader fragShader,
            Graphics graphics, 
            Matrix4x4 modelMat, 
            Matrix4x4 invModelMat, 
            Matrix4x4 viewMat, 
            Matrix4x4 invViewMat,
            Matrix4x4 projMat,
            Matrix4x4 invProjMat,
            bool useLighting)
        {
            Stopwatch stopWatch = Stopwatch.StartNew();
            stopWatch.Start();
            IntPtr hdc = graphics.GetHdc();
            int renderWidth = this.DrawArea.Width;
            int renderHeight = this.DrawArea.Height;
            uint[] screenBuffer = new uint[renderWidth * renderHeight];

            const int workDivision = 8;
            int totalWork = renderWidth;
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
                        Fragment fragOut;
                        foreach(RasterInfo raster in renderer.Rasterize(
                            indexStartCopy, indexStartCopy + workCount, 0, renderHeight,
                            modelMat, invModelMat, viewMat * projMat, invProjMat * invViewMat))
                        {
                            fragShader.Main(raster, out fragOut);
                            uint red, green, blue;
                            ClampColorVector(ref fragOut.fragColor);
                            red = (uint)(fragOut.fragColor.X * 0xFF);
                            green = (uint)(fragOut.fragColor.Y * 0xFF);
                            blue = (uint)(fragOut.fragColor.Z * 0xFF);
                            screenBuffer[raster.screenX + raster.screenY * renderWidth] = (uint)red << 16 | (uint)green << 8 | (uint)blue;
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

        private void ClampColorVector(ref Vector3 color)
        {
            if(color.X > 1.0f)
            {
                color.X = 1.0f;
            }
            if(color.X < 0.0f)
            {
                color.X = 0.0f;
            }
            if(color.Y > 1.0f)
            {
                color.Y = 1.0f;
            }
            if(color.Y < 0.0f)
            {
                color.Y = 0.0f;
            }
            if(color.Z > 1.0f)
            {
                color.Z = 1.0f;
            }
            if(color.Z < 0.0f)
            {
                color.Z = 0.0f;
            }
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
            Point originPoint = _renderer.WorldToScreenCoord(origin.X, origin.Y);
            graphics.DrawLine(
                _xPen,
                originPoint,
                _renderer.WorldToScreenCoord(xAxis.X, xAxis.Y));
            graphics.DrawLine(
                _yPen,
                originPoint,
                _renderer.WorldToScreenCoord(yAxis.X, yAxis.Y));
            graphics.DrawLine(
                _zPen,
                originPoint,
                _renderer.WorldToScreenCoord(zAxis.X, zAxis.Y));
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
            /*if(e.Button == MouseButtons.Left)
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
            }*/
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
