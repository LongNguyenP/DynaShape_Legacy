using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
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
        private IntCollection meshFaceIndices;
        private BitmapImage diffuseMap;
        private Vector2Collection textureCoordinates;

        public TexturedMeshBinder(Autodesk.Dynamo.MeshToolkit.Mesh mesh, Color4 color, string textureFileName, Vector2Collection textureCoordinates)
        {
            StartingPositions = mesh.Vertices().ToTriples().ToArray();
            Color = color;

            try { diffuseMap = new BitmapImage(new Uri(textureFileName)); }
            catch (FileNotFoundException) { throw new Exception("Could not locate the texture file"); }

            this.textureCoordinates = textureCoordinates;

            faceIndices = mesh.VertexIndicesByTri();
            int faceCount = faceIndices.Count / 3;

            faces = new IndexGroup[faceCount];

            for (int i = 0; i < faceCount; i++)
                faces[i] = IndexGroup.ByIndices(
                    (uint)faceIndices[i * 3],
                    (uint)faceIndices[i * 3 + 1],
                    (uint)faceIndices[i * 3 + 2]);

            meshFaceIndices = new IntCollection();

            foreach (IndexGroup face in faces)
            {
                meshFaceIndices.Add((int)face.A);
                meshFaceIndices.Add((int)face.B);
                meshFaceIndices.Add((int)face.C);
            }
        }

#if CLI == false
        public TexturedMeshBinder(Autodesk.Dynamo.MeshToolkit.Mesh mesh, string textureFileName, Vector2Collection textureCoordinates)
            : this(mesh, DynaShapeDisplay.DefaultMeshFaceColor, textureFileName, textureCoordinates)
        {
        }
#endif

        public override List<object> CreateGeometryObjects(List<Node> allNodes)
        {
            List<Point> vertices = new List<Point>(NodeCount);
            for (int i = 0; i < NodeCount; i++)
                vertices.Add(allNodes[NodeIndices[i]].Position.ToPoint());

            return new List<object> { Autodesk.Dynamo.MeshToolkit.Mesh.ByVerticesAndIndices(vertices, faceIndices) };
        }

#if CLI == false
        public override void CreateDisplayedGeometries(DynaShapeDisplay display, List<Node> allNodes)
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


            ////===============================================================
            //// Render mesh triangles, using vertex normals computed above
            ////===============================================================

            MeshGeometry3D meshGeometry = new MeshGeometry3D()
            {
                Positions = new Vector3Collection(),
                Normals = new Vector3Collection(),
                Indices = meshFaceIndices,
                TextureCoordinates = textureCoordinates,
            };

            for (int i = 0; i < NodeCount; i++)
            {
                meshGeometry.Positions.Add(allNodes[NodeIndices[i]].Position.ToVector3());
                meshGeometry.Normals.Add(vertexNormals[i].ToVector3());
            }


            display.AddMeshModel(
                new MeshGeometryModel3D
                {
                    Geometry = meshGeometry,
                    Material = new PhongMaterial
                    {
                        DiffuseColor = new Color(1.0f, 1.0f, 1f, 1.0f),
                        DiffuseMap = this.diffuseMap.StreamSource
                    }
                });
        }
#endif
    }
}
