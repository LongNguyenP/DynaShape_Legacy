using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using SharpDX;
using Point = Autodesk.DesignScript.Geometry.Point;


namespace DynaShape.GeometryBinders
{
    [IsVisibleInDynamoLibrary(false)]
    public class PolylineBinder : GeometryBinder
    {
        public bool Loop;

        public PolylineBinder(List<Triple> vertices, Color4 color, bool loop = false)
        {
            StartingPositions = vertices.ToArray();
            Color = color;
            Loop = loop;
        }


        public PolylineBinder(List<Triple> vertices, bool loop = false)
            : this(vertices, DynaShapeDisplay.DefaultLineColor, loop)
        {
        }


        public override List<object> CreateGeometryObjects(List<Node> allNodes)
        {
            List<Point> points = new List<Point>();
            for (int i = 0; i < NodeCount; i++) points.Add(allNodes[NodeIndices[i]].Position.ToPoint());
            return new List<object> { PolyCurve.ByPoints(points, Loop) };
        }


        public override void CreateDisplayedGeometries(DynaShapeDisplay display, List<Node> allNodes)
        {
            List<Triple> vertices = new List<Triple>();
            for (int i = 0; i < NodeCount; i++) vertices.Add(allNodes[NodeIndices[i]].Position);
            display.DrawPolyline(vertices, Color, Loop);
        }
    }
}