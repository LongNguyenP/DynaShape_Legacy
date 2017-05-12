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
        private Mesh mesh = null;

        public MeshBinder(Mesh mesh)
        {
            StartingPositions = mesh.VertexPositions.ToTriples().ToArray();
            this.mesh = mesh;
        }

        public override List<Geometry> GetGeometries(List<Node> allNodes)
        {
            // Build Dynamo Mesh
            return null;
        }

        public override void DrawGraphics(IRenderPackage package, TessellationParameters parameters, List<Node> allNodes)
        {

            foreach (IndexGroup face in mesh.FaceIndices)
            {
                Triple A = allNodes[NodeIndices[face.A]].Position;
                Triple B = allNodes[NodeIndices[face.B]].Position;
                Triple C = allNodes[NodeIndices[face.C]].Position;

                Triple n = (B - A).Cross(C - A).Normalise();

                package.AddTriangleVertex(A.X, A.Y, A.Z);
                package.AddTriangleVertex(B.X, B.Y, B.Z);
                package.AddTriangleVertex(C.X, C.Y, C.Z);

                package.AddTriangleVertexNormal(n.X, n.Y, n.Z);
                package.AddTriangleVertexNormal(n.X, n.Y, n.Z);
                package.AddTriangleVertexNormal(n.X, n.Y, n.Z);

                package.AddTriangleVertexColor(GeometryRender.DefaultMeshFaceColor.A, GeometryRender.DefaultMeshFaceColor.R, GeometryRender.DefaultMeshFaceColor.G, GeometryRender.DefaultMeshFaceColor.B);
                package.AddTriangleVertexColor(GeometryRender.DefaultMeshFaceColor.A, GeometryRender.DefaultMeshFaceColor.R, GeometryRender.DefaultMeshFaceColor.G, GeometryRender.DefaultMeshFaceColor.B);
                package.AddTriangleVertexColor(GeometryRender.DefaultMeshFaceColor.A, GeometryRender.DefaultMeshFaceColor.R, GeometryRender.DefaultMeshFaceColor.G, GeometryRender.DefaultMeshFaceColor.B);

                package.AddTriangleVertexUV(0.0, 0.0);
                package.AddTriangleVertexUV(0.0, 0.0);
                package.AddTriangleVertexUV(0.0, 0.0);
            }

        }
    }
}
