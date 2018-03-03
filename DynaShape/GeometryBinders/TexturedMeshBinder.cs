using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Dynamo.MeshToolkit;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DynaShape.GeometryBinders
{
    [IsVisibleInDynamoLibrary(false)]
    public class TexturedMeshBinder : GeometryBinder
    {
        private IndexGroup[] faces;
        private List<int> faceIndices;
        private bool dynamoMesh = true;


        public TexturedMeshBinder(Autodesk.Dynamo.MeshToolkit.Mesh mesh, Color color)
        {
            dynamoMesh = false;
            StartingPositions = mesh.Vertices().ToTriples().ToArray();
            Color = color;

            faceIndices = mesh.VertexIndicesByTri();
            int faceCount = faceIndices.Count / 3;

            faces = new IndexGroup[faceCount];

            for (int i = 0; i < faceCount; i++)
                faces[i] = IndexGroup.ByIndices(
                    (uint)faceIndices[i * 3],
                    (uint)faceIndices[i * 3 + 1],
                    (uint)faceIndices[i * 3 + 2]);
        }


        public TexturedMeshBinder(Autodesk.Dynamo.MeshToolkit.Mesh mesh)
            : this(mesh, DynaShapeDisplay.DefaultMeshFaceColor)
        {
        }


        public override List<object> CreateGeometryObjects(List<Node> allNodes)
        {
            List<Point> vertices = new List<Point>(NodeCount);
            for (int i = 0; i < NodeCount; i++)
                vertices.Add(allNodes[NodeIndices[i]].Position.ToPoint());

            return dynamoMesh
                ? new List<object> { Autodesk.DesignScript.Geometry.Mesh.ByPointsFaceIndices(vertices, faces) }
                : new List<object> { Autodesk.Dynamo.MeshToolkit.Mesh.ByVerticesAndIndices(vertices, faceIndices) };
        }


        public override void CreateDisplayedGeometries(DynaShapeDisplay display, List<Node> allNodes)
        {
            ////======================================================================
            //// Compute vertex normals by averaging normals of surrounding faces
            ////======================================================================

            //Triple[] vertexNormals = new Triple[NodeCount];

            //foreach (IndexGroup face in faces)
            //{
            //    Triple A = allNodes[NodeIndices[face.A]].Position;
            //    Triple B = allNodes[NodeIndices[face.B]].Position;
            //    Triple C = allNodes[NodeIndices[face.C]].Position;

            //    Triple n = (B - A).Cross(C - A).Normalise();

            //    vertexNormals[face.A] += n;
            //    vertexNormals[face.B] += n;
            //    vertexNormals[face.C] += n;

            //    if (face.D == uint.MaxValue) continue;

            //    Triple D = allNodes[NodeIndices[face.D]].Position;

            //    n = (C - A).Cross(D - A).Normalise();

            //    vertexNormals[face.A] += n;
            //    vertexNormals[face.C] += n;
            //    vertexNormals[face.D] += n;
            //}

            //for (int i = 0; i < NodeCount; i++) vertexNormals[i] = vertexNormals[i].Normalise();



            ////===============================================================
            //// Render mesh triangles, using vertex normals computed above
            ////===============================================================

            MeshGeometry3D meshGeometry = new MeshGeometry3D()
            {
                Positions = new Vector3Collection(),
                Normals = new Vector3Collection(),
                Indices = new IntCollection(),
                TextureCoordinates = new Vector2Collection(),
            };

            //for (int i = 0; i < NodeCount; i++)
            //{
            //    meshGeometry.Positions.Add(allNodes[NodeIndices[i]].Position.ToVector3());
            //    meshGeometry.Normals.Add(vertexNormals[i].ToVector3());
            //}

            //foreach (IndexGroup face in faces)
            //{
            //    meshGeometry.Indices.Add((int)face.A);
            //    meshGeometry.Indices.Add((int)face.B);
            //    meshGeometry.Indices.Add((int)face.C);

            //    if (face.D == uint.MaxValue) continue;

            //    meshGeometry.Indices.Add((int)face.A);
            //    meshGeometry.Indices.Add((int)face.C);
            //    meshGeometry.Indices.Add((int)face.D);
            //}


            meshGeometry.Positions.Add(new Vector3(0f, 0f, 0f));
            meshGeometry.Positions.Add(new Vector3(10f, 0f, 0f));
            meshGeometry.Positions.Add(new Vector3(10f, 0f, -10f));
            meshGeometry.Positions.Add(new Vector3(0f, 0f, -10f));

            meshGeometry.Normals.Add(Vector3.UnitY);
            meshGeometry.Normals.Add(Vector3.UnitY);
            meshGeometry.Normals.Add(Vector3.UnitY);
            meshGeometry.Normals.Add(Vector3.UnitY);

            meshGeometry.Indices.Add(0);
            meshGeometry.Indices.Add(1);
            meshGeometry.Indices.Add(2);
            meshGeometry.Indices.Add(0);
            meshGeometry.Indices.Add(2);
            meshGeometry.Indices.Add(3);

            meshGeometry.TextureCoordinates.Add(new Vector2(0f, 0f));
            meshGeometry.TextureCoordinates.Add(new Vector2(1f, 0f));
            meshGeometry.TextureCoordinates.Add(new Vector2(1f, 1f));
            meshGeometry.TextureCoordinates.Add(new Vector2(0f, 1f));

            PhongMaterial material = new PhongMaterial();
            material.DiffuseColor = new Color(1.0f, 1.0f, 1f, 1.0f);
            material.DiffuseMap = new BitmapImage(new Uri(@"D:\Desktop\texture.jpg"));

            display.AddMeshModel(
                new MeshGeometryModel3D
                {
                    Geometry = meshGeometry,
                    Material = material
                });
        }
    }
}
