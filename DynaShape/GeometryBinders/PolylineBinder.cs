using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Dynamo.Wpf.Rendering;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DynaShape.GeometryBinders
{
    [IsVisibleInDynamoLibrary(false)]
    public class PolylineBinder : GeometryBinder
    {
        public PolylineBinder(List<Triple> vertices, Color color)
        {
            StartingPositions = vertices.ToArray();
            Color = color;
        }


        public PolylineBinder(List<Triple> vertices)
            :this(vertices, GeometryRender.DefaultLineColor)
        {
        }


        public override List<DesignScriptEntity> GetGeometries(List<Node> allNodes)
        {
            List<Point> points = new List<Point>();
            for (int i = 0; i < NodeCount; i++) points.Add(allNodes[NodeIndices[i]].Position.ToPoint());
            return new List<DesignScriptEntity> { PolyCurve.ByPoints(points) };
        }


        public override void DrawGraphics(IRenderPackage package, TessellationParameters parameters, List<Node> allNodes)
        {
            List<Triple> vertices = new List<Triple>();

            for (int i = 0; i < NodeCount; i++) vertices.Add(allNodes[NodeIndices[i]].Position);
            GeometryRender.DrawPolyline(package, vertices, Color);
        }
    }
}