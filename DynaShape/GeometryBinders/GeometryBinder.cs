using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using SharpDX;

namespace DynaShape.GeometryBinders
{
    // Each geometry binder allows us to attach a geometry (eg. lines, polylines, meshes) to a subset of nodes in the system
    [IsVisibleInDynamoLibrary(false)]
    public abstract class GeometryBinder
    {
        public Color4 Color;

        public Triple[] StartingPositions;
        public int[] NodeIndices;

        public int NodeCount => StartingPositions.Length;

        public virtual List<DesignScriptEntity> GetGeometries(List<Node> allNodes) => null;

        public virtual void DrawGraphics(DynaShapeDisplay display, List<Node> allNodes)
        { }
        
    }
}
