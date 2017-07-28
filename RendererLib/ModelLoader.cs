using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRenderTest
{
    public class ModelLoader
    {
        public static Mesh LoadObj(String objFile)
        {
            StreamReader streamReader = null;
            try
            {
                streamReader = new StreamReader(objFile);
            }
            catch(IOException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            String line = null;
            int lineNumber = 0;
            int vertexCount = 0;
            List<Vector3> vertices = new List<Vector3>(1024);
            List<Tuple<int, int, int>> faces = new List<Tuple<int, int, int>>(1024);
            char[] delimiters = new char[] { ' ', '\t' };
            char[] faceDelimiter = new char[] { '/' };
            while((line = streamReader.ReadLine()) != null)
            {
                ++lineNumber;
                line = line.Trim().ToLower();
                if(String.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                else if(line.StartsWith("#"))
                {
                    continue;
                }
                if(line.StartsWith("vt"))
                {
                    // Not implemented yet.
                    continue;
                }
                else if(line.StartsWith("vn"))
                {
                    // Not implemented yet.
                    continue;
                }
                else if(line.StartsWith("v"))
                {

                    String[] vertexDef = line.Split(
                        delimiters,
                        System.StringSplitOptions.RemoveEmptyEntries);
                    if(vertexDef.Length != 4)
                    {
                        Console.WriteLine("Invalid formatting at line " + lineNumber + ": " + line);
                        continue;
                    }
                    Vector3 vertex = new Vector3();
                    vertex.X = float.Parse(vertexDef[1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    vertex.Y = float.Parse(vertexDef[2], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    vertex.Z = float.Parse(vertexDef[3], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    vertices.Add(vertex);
                    ++vertexCount;
                }
                else if(line.StartsWith("f"))
                {
                    String[] faceDef = line.Split(
                        delimiters,
                        System.StringSplitOptions.RemoveEmptyEntries);
                    if(faceDef.Length != 4)
                    {
                        Console.WriteLine("Invalid formatting at line " + lineNumber + ": " + line);
                        continue;
                    }
                    Tuple<int, int, int> face = new Tuple<int, int, int>(
                        GetFaceVertexIndex(faceDef[1], vertexCount),
                        GetFaceVertexIndex(faceDef[2], vertexCount),
                        GetFaceVertexIndex(faceDef[3], vertexCount));
                    faces.Add(face);
                }
                else
                {
                    Console.WriteLine("Unknown token encountered at line " + lineNumber + ": " + line);
                }
            }
            Console.WriteLine("Obj file line count: " + lineNumber);
            Console.WriteLine("Number of vertices: " + vertices.Count);
            Console.WriteLine("Number of faces: " + faces.Count);
            streamReader.Close();

            return new Mesh(vertices, faces);
        }

        private static int GetFaceVertexIndex(String faceDef, int vertexCount)
        {
            String[] faceVertexDef = faceDef.Split('/');
            int rawIndex = Int32.Parse(faceVertexDef[0]);
            if(rawIndex < 0)
            {
                rawIndex = vertexCount + rawIndex + 1;
            }
            // Vertex index starts from 1 in obj file.
            rawIndex -= 1;
            return rawIndex;
        }
    }
}
