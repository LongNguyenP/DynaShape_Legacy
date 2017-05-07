using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace DynaShape.GeometryBinders
{
    // Each geometry binder allows us to attach a geometry (eg. lines, polylines, meshes) to a subset of nodes in the system
    [IsVisibleInDynamoLibrary(false)]
    public abstract class GeometryBinder
    {
        public Color Color;

        public Triple[] StartingPositions;
        public int[] NodeIndices;

        public int NodeCount => StartingPositions.Length;
        public virtual List<Geometry> GetGeometries(List<Node> allNodes) => null;

        public virtual void DrawGraphics(IRenderPackage package, TessellationParameters parameters, List<Node> allNodes)
        {
        }
    }
}
