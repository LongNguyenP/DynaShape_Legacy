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
        public LineBinder(Triple startPoint, Triple endPoint, Color color)
        {
            StartingPositions = new[] { startPoint, endPoint };
            Color = color;
        }


        public LineBinder(Triple startPoint, Triple endPoint)
            : this(startPoint, endPoint, GeometryRender.DefaultLineColor)
        {
        }


        public override List<DesignScriptEntity> GetGeometries(List<Node> allNodes)
        {
            return new List<DesignScriptEntity>
            {
                Line.ByStartPointEndPoint(
                    allNodes[NodeIndices[0]].Position.ToPoint(),
                    allNodes[NodeIndices[1]].Position.ToPoint())
            };
        }


        public override void DrawGraphics(IRenderPackage package, TessellationParameters parameters, List<Node> allNodes)
        {
            GeometryRender.DrawLine(
                package,
                allNodes[NodeIndices[0]].Position,
                allNodes[NodeIndices[1]].Position,
                Color);
        }
    }
}
