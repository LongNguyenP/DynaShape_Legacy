using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using SharpDX;

namespace DynaShape.GeometryBinders
{
    [IsVisibleInDynamoLibrary(false)]
    public class SphereBinder : GeometryBinder
    {
        #region Static

        private static readonly int segmentCount = 32;
        private static readonly float[] cosValues = new float[segmentCount];
        private static readonly float[] sinValues = new float[segmentCount];

        static SphereBinder()
        {
            for (int i = 0; i < segmentCount; i++)
            {
                double angle = 2.0 * Math.PI * i / segmentCount;
                cosValues[i] = (float)Math.Cos(angle);
                sinValues[i] = (float)Math.Sin(angle);
            }
        }

        #endregion

        public float Radius;


        public SphereBinder(Triple center, float radius, Color4 color)
        {
            StartingPositions = new[] { center };
            Radius = radius;
            Color = color;
        }

#if CLI == false
        public SphereBinder(Triple center, float radius, Triple planeNormal)
            : this(center, radius, DynaShapeDisplay.DefaultMeshFaceColor)
        {
        }
#endif

        public override List<object> CreateGeometryObjects(List<Node> allNodes)
        {
            return new List<object>
            {
                Sphere.ByCenterPointRadius(allNodes[NodeIndices[0]].Position.ToPoint(), Radius)
            };
        }

#if CLI == false
        public override void CreateDisplayedGeometries(DynaShapeDisplay display, List<Node> allNodes)
        {
            Triple center = allNodes[NodeIndices[0]].Position;
        }
#endif
    }
}
