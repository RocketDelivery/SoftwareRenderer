using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRenderTest
{
    public abstract class FragmentShader
    {
        private Matrix4x4 _modelMat;
        private Matrix4x4 _viewMat;
        private Matrix4x4 _projMat;
        private Matrix4x4 _invModelMat;
        private Matrix4x4 _invViewMat;
        private Matrix4x4 _invProjMat;
        private int _screenWidth, _screenHeight;
        
        //"Uniform or const buffer" data
        public Matrix4x4 ModelMat => _modelMat;
        public Matrix4x4 ViewMat => _viewMat;
        public Matrix4x4 ProjMat => _projMat;
        public int Width => _screenWidth;
        public int Height => _screenHeight;

        public void SetUniformData(
            ref Matrix4x4 modelMat, 
            ref Matrix4x4 viewMat,
            ref Matrix4x4 projMat,
            ref Matrix4x4 invModelMat,
            ref Matrix4x4 invViewMat,
            ref Matrix4x4 invProjMat,
            int screenWidth,
            int screenHeight)
        {
            _modelMat = modelMat;
            _viewMat = viewMat;
            _projMat = projMat;
            _invModelMat = invModelMat;
            _invViewMat = invViewMat;
            _invProjMat = invProjMat;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
        }

        public abstract void Main(
            //"Per-Fragment" data
            RasterInfo raster,
            out Fragment fragOut);
    }
}
