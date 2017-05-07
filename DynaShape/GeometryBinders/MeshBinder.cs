using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace DynaShape.GeometryBinders
{
    [IsVisibleInDynamoLibrary(false)]
    public class MeshBinder : GeometryBinder
    {
        private Mesh mesh;

        public MeshBinder(Mesh mesh)
        {
            StartingPositions = mesh.VertexPositions.ToTriples().ToArray();
            this.mesh = mesh;
        }

        public override List<Geometry> GetGeometries(List<Node> allNodes)
        {
            return null;
        }

        public override void DrawGraphics(IRenderPackage package, TessellationParameters parameters, List<Node> allNodes)
        {
            //this.mesh.Tessellate(package, parameters);
            //return;

            //List<Point> vertices = new List<Point>();
            //foreach (int i in NodeIndices)
            //   vertices.Add(allNodes[NodeIndices[i]].Position.ToPoint());

            //Mesh mesh = Mesh.ByPointsFaceIndices(vertices, new [] { IndexGroup.ByIndices(0, 1, 2, 3) });
            //mesh.Tessellate(package, parameters);

            //return;


            foreach (IndexGroup indexGroup in mesh.FaceIndices)
            {
                Triple vertex = allNodes[NodeIndices[indexGroup.A]].Position;
                package.AddTriangleVertex(vertex.X, vertex.Y, vertex.Z);
                package.AddTriangleVertexColor(128, 128, 128, 64);
                package.AddTriangleVertexNormal(0.0, 0.0, 1.0);
                package.AddTriangleVertexUV(0.0, 0.0);

                vertex = allNodes[NodeIndices[indexGroup.B]].Position;
                package.AddTriangleVertex(vertex.X, vertex.Y, vertex.Z);
                package.AddTriangleVertexColor(128, 128, 128, 64);
                package.AddTriangleVertexNormal(0.0, 0.0, 1.0);
                package.AddTriangleVertexUV(0.0, 0.0);

                vertex = allNodes[NodeIndices[indexGroup.C]].Position;
                package.AddTriangleVertex(vertex.X, vertex.Y, vertex.Z);
                package.AddTriangleVertexColor(128, 128, 128, 64);
                package.AddTriangleVertexNormal(0.0, 0.0, 1.0);
                package.AddTriangleVertexUV(0.0, 0.0);
            }


        }
    }
}
