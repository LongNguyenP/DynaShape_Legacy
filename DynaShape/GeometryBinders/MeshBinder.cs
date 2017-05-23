using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using SharpDX;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DynaShape.GeometryBinders
{
    [IsVisibleInDynamoLibrary(false)]
    public class MeshBinder : GeometryBinder
    {
        private IndexGroup[] faces = null;


        public MeshBinder(Mesh mesh, Color4 color)
        {
            StartingPositions = mesh.VertexPositions.ToTriples().ToArray();
            Color = color;
            faces = mesh.FaceIndices;
        }


        public MeshBinder(Mesh mesh) 
            : this(mesh, DynaShapeDisplay.DefaultMeshFaceColor)
        {}
 

        public override List<DesignScriptEntity> GetGeometries(List<Node> allNodes)
        {
            List<Point> vertices = new List<Point>(NodeCount);
            for (int i = 0; i < NodeCount; i++)
                vertices.Add(allNodes[NodeIndices[i]].Position.ToPoint());
            
            return new List<DesignScriptEntity> { Mesh.ByPointsFaceIndices(vertices, faces) };
        }


        public override void DrawGraphics(DynaShapeDisplay display, List<Node> allNodes)
        {
            //======================================================================
            // Compute vertex normals by averaging normals of surrounding faces
            //======================================================================

            Triple[] vertexNormals = new Triple[NodeCount];

            foreach (IndexGroup face in faces)
            {
                Triple A = allNodes[NodeIndices[face.A]].Position;
                Triple B = allNodes[NodeIndices[face.B]].Position;
                Triple C = allNodes[NodeIndices[face.C]].Position;

                Triple n = (B - A).Cross(C - A).Normalise();

                vertexNormals[face.A] += n;
                vertexNormals[face.B] += n;
                vertexNormals[face.C] += n;

                if (face.D == uint.MaxValue) continue;

                Triple D = allNodes[NodeIndices[face.D]].Position;

                n = (C - A).Cross(D - A).Normalise();

                vertexNormals[face.A] += n;
                vertexNormals[face.C] += n;
                vertexNormals[face.D] += n;
            }

            for (int i = 0; i < NodeCount; i++) vertexNormals[i] = vertexNormals[i].Normalise();


            //===============================================================
            // Render mesh triangles, using vertex normals computed above
            //===============================================================

            foreach (IndexGroup face in faces)
            {
                Triple A = allNodes[NodeIndices[face.A]].Position;
                Triple B = allNodes[NodeIndices[face.B]].Position;
                Triple C = allNodes[NodeIndices[face.C]].Position;

                Triple nA = vertexNormals[face.A];
                Triple nB = vertexNormals[face.B];
                Triple nC = vertexNormals[face.C];

                //package.AddTriangleVertex(A.X, A.Y, A.Z);
                //package.AddTriangleVertex(B.X, B.Y, B.Z);
                //package.AddTriangleVertex(C.X, C.Y, C.Z);

                //package.AddTriangleVertexNormal(nA.X, nA.Y, nA.Z);
                //package.AddTriangleVertexNormal(nB.X, nB.Y, nB.Z);
                //package.AddTriangleVertexNormal(nC.X, nC.Y, nC.Z);              

                //package.AddTriangleVertexColor(Color.R, Color.G, Color.B, Color.A);
                //package.AddTriangleVertexColor(Color.R, Color.G, Color.B, Color.A);
                //package.AddTriangleVertexColor(Color.R, Color.G, Color.B, Color.A);

                //package.AddTriangleVertexUV(0.0, 0.0);
                //package.AddTriangleVertexUV(0.0, 0.0);
                //package.AddTriangleVertexUV(0.0, 0.0);

                if (face.D == uint.MaxValue) continue;

                Triple D = allNodes[NodeIndices[face.D]].Position;
                Triple nD = vertexNormals[face.D];

                //package.AddTriangleVertex(A.X, A.Y, A.Z);               
                //package.AddTriangleVertex(C.X, C.Y, C.Z);
                //package.AddTriangleVertex(D.X, D.Y, D.Z);

                //package.AddTriangleVertexNormal(nA.X, nA.Y, nA.Z);
                //package.AddTriangleVertexNormal(nC.X, nC.Y, nC.Z);
                //package.AddTriangleVertexNormal(nD.X, nD.Y, nD.Z);
                
                //package.AddTriangleVertexColor(Color.R, Color.G, Color.B, Color.A);
                //package.AddTriangleVertexColor(Color.R, Color.G, Color.B, Color.A);
                //package.AddTriangleVertexColor(Color.R, Color.G, Color.B, Color.A);

                //package.AddTriangleVertexUV(0.0, 0.0);
                //package.AddTriangleVertexUV(0.0, 0.0);
                //package.AddTriangleVertexUV(0.0, 0.0);

            }
        }
    }
}
