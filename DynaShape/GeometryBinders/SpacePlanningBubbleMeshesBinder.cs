using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Dynamo.MeshToolkit;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using Mesh = Autodesk.DesignScript.Geometry.Mesh;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DynaShape.GeometryBinders
{
//    [IsVisibleInDynamoLibrary(false)]
//    public class SpacePlanningBubbleMeshesBinder : GeometryBinder
//    {
//        #region Static

//        private static readonly int segmentCount = 32;
//        private static readonly float[] cosValues = new float[segmentCount];
//        private static readonly float[] sinValues = new float[segmentCount];

//        static SpacePlanningBubbleMeshesBinder()
//        {
//            for (int i = 0; i < segmentCount; i++)
//            {
//                double angle = 2.0 * Math.PI * i / segmentCount;
//                cosValues[i] = (float)Math.Cos(angle);
//                sinValues[i] = (float)Math.Sin(angle);
//            }
//        }

//        #endregion

//        private IndexGroup[] faces;
//        private List<int> faceIndices;
//        private IntCollection meshIndices;
//        private List<float> radii;
//        private List<Color> colors;
//        private MeshGeometry3D meshGeometry;

//        public SpacePlanningBubbleMeshesBinder(List<Triple> centers, List<float> radii, List<Color> colors)
//        {
//            StartingPositions = centers.ToArray();
//            this.radii = radii;
//            this.colors = colors;

            
//        }


//        public override List<object> CreateGeometryObjects(List<Node> allNodes)
//        {
//            List<object> circles = new List<object>(NodeCount);
//            for (int i = 0; i < NodeCount; i++)
//                circles.Add(Circle.ByCenterPointRadius(allNodes[NodeIndices[i]].Position.ToPoint(), radii[i]));

//            return circles;
//        }

//#if CLI == false
//        public override void CreateDisplayedGeometries(DynaShapeDisplay display, List<Node> allNodes)
//        {
//            MeshGeometry3D meshGeometry = new MeshGeometry3D()
//            {
//                Positions = new Vector3Collection(),
//                Normals = new Vector3Collection(),
//                Indices = new IntCollection(),
//                //Colors = new Color4Collection()
//            };

//            for (int i = 0; i < NodeCount; i++)
//            {
//                Triple center = allNodes[NodeIndices[i]].Position;
//                float r = radii[i];
//                meshGeometry.Positions.Add(center.ToVector3());
//                meshGeometry.Normals.Add(new Vector3(0f, 1f, 0f));
//                //meshGeometry.Colors.Add(new Color4(1f, 0f, 0f, 0.5f));

//                for (int j = 0; j < segmentCount; j++)
//                {
//                    meshGeometry.Positions.Add(new Vector3(center.X + r * cosValues[j], center.Z, -center.Y + r * sinValues[j]));
//                    meshGeometry.Normals.Add(new Vector3(0f, 1f, 0f));
//                    //meshGeometry.Colors.Add(new Color4(1f, 0f, 0f, 0.5f));
//                }

//                int baseIndex = i * (1 + segmentCount);
//                for (int j = 0; j < segmentCount - 1; j++)
//                {
//                    meshGeometry.Indices.Add(baseIndex);
//                    meshGeometry.Indices.Add(baseIndex + j + 1);
//                    meshGeometry.Indices.Add(baseIndex + j + 2);
                    
//                }

//                meshGeometry.Indices.Add(baseIndex);
//                meshGeometry.Indices.Add(baseIndex + segmentCount);
//                meshGeometry.Indices.Add(baseIndex + 1);

//                display.AddMeshModel(
//                    new MeshGeometryModel3D
//                    {
//                        Geometry = meshGeometry,
//                        Material = new PhongMaterial {DiffuseColor = new Color(1f, 1f, 1f, 0.8f)},
//                    });
//            }
//        }
//#endif
//    }
}
