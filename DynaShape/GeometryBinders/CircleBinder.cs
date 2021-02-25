using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using SharpDX;

namespace DynaShape.GeometryBinders
{
    [IsVisibleInDynamoLibrary(false)]
    public class CircleBinder : GeometryBinder
    {
        #region Static

        private static readonly int segmentCount = 32;
        private static readonly float[] cosValues = new float[segmentCount];
        private static readonly float[] sinValues = new float[segmentCount];

        static CircleBinder()
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

        public Triple PlaneNormal
        {
            get => zAxis;
            set
            {
                zAxis = value;
                xAxis = zAxis.GeneratePerpendicular();
                yAxis = zAxis.Cross(xAxis);
            }
        }

        private Triple xAxis, yAxis, zAxis;


        public CircleBinder(Triple center, float radius, Triple planeNormal, Color4 color)
        {
            StartingPositions = new[] { center };
            Radius = radius;
            PlaneNormal = planeNormal;
            Color = color;
        }


        public CircleBinder(Triple center, float radius, Triple planeNormal)
            : this(center, radius, planeNormal, DynaShapeDisplay.DefaultLineColor)
        {
        }


        public override List<object> CreateGeometryObjects(List<Node> allNodes)
        {
            return new List<object>
            {
                Circle.ByCenterPointRadiusNormal(allNodes[NodeIndices[0]].Position.ToPoint(), Radius, zAxis.ToVector())
            };
        }


        public override void CreateDisplayedGeometries(DynaShapeDisplay display, List<Node> allNodes)
        {
            Triple center = allNodes[NodeIndices[0]].Position;

            List<Triple> vertices = new List<Triple>(segmentCount);

            for (int i = 0; i < segmentCount; i++)
                vertices.Add(center + xAxis * Radius * cosValues[i] + yAxis * Radius * sinValues[i]);

            display.DrawPolyline(vertices, Color, true);
        }
    }
}
