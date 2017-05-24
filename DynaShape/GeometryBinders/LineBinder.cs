using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using SharpDX;

namespace DynaShape.GeometryBinders
{
    [IsVisibleInDynamoLibrary(false)]
    public class LineBinder : GeometryBinder
    {
        public LineBinder(Triple startPoint, Triple endPoint, Color4 color)
        {
            StartingPositions = new[] { startPoint, endPoint };
            Color = color;
        }


        public LineBinder(Triple startPoint, Triple endPoint)
            : this(startPoint, endPoint, DynaShapeDisplay.DefaultLineColor)
        {
        }


        public override List<DesignScriptEntity> CreateGeometryObjects(List<Node> allNodes)
        {
            return new List<DesignScriptEntity>
            {
                Line.ByStartPointEndPoint(
                    allNodes[NodeIndices[0]].Position.ToPoint(),
                    allNodes[NodeIndices[1]].Position.ToPoint())
            };
        }

        public override void CreateDisplayedGeometries(DynaShapeDisplay display, List<Node> allNodes)
        {
            display.DrawLine(
                allNodes[NodeIndices[0]].Position,
                allNodes[NodeIndices[1]].Position,
                Color);
        }
    }
}
