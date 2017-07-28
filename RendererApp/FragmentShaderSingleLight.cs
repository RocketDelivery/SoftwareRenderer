using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRenderTest
{
    class FragmentShaderSingleLight : FragmentShader
    {
        private Vector3 _lightPos;
        public Vector3 LightPos => _lightPos;
        
        public void SetLightPosition(Vector3 lightPos)
        {
            _lightPos = lightPos;
        }
        
        public override void Main(RasterInfo raster, out Fragment fragOut)
        {
            Vector3 lightDir = LightPos - raster.pos;
            lightDir = Vector3.Normalize(lightDir);
            float light = Vector3.Dot(lightDir, raster.normal);
            if(light < 0.2f)
            {
                light = 0.2f;
            }
            if(light > 1.0f)
            {
                light = 1.0f;
            }
            Vector4 fragPos = Vector4.Transform(new Vector4(raster.pos, 1.0f), ModelMat * ViewMat * ProjMat);
            fragOut.fragColor = new Vector3(light, light, light);
            fragOut.depth = fragPos.Z / fragPos.W;
        }
    }
}
