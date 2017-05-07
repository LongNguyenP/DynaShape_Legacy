using System.Collections.Generic;
using System.Drawing;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace DynaShape.GeometryBinders
{
    [IsVisibleInDynamoLibrary(false)]
    public class LineBinder : GeometryBinder
    {
        public LineBinder(Triple startVertex, Triple endVertex)
        {
            StartingPositions = new[] { startVertex, endVertex };
            Color = GeometryRender.DefaultLineColor;
        }


        public LineBinder(Triple startVertex, Triple endVertex, Color color)
        {
            StartingPositions = new[] { startVertex, endVertex };
            Color = color;
        }


        public override List<Geometry> GetGeometries(List<Node> allNodes) =>
           new List<Geometry>
           {
            Line.ByStartPointEndPoint(
               allNodes[NodeIndices[0]].Position.ToPoint(),
               allNodes[NodeIndices[1]].Position.ToPoint())
           };


        public override void DrawGraphics(IRenderPackage package, TessellationParameters parameters, List<Node> allNodes)
           => GeometryRender.DrawLine(
                 package,
                 allNodes[NodeIndices[0]].Position,
                 allNodes[NodeIndices[1]].Position,
                 Color);
    }
}
