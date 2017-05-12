using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace DynaShape.GeometryBinders
{
    [IsVisibleInDynamoLibrary(false)]
    public class MeshBinder : GeometryBinder
    {
        private Mesh mesh = null;

        public MeshBinder(Mesh mesh)
        {
            StartingPositions = mesh.VertexPositions.ToTriples().ToArray();
            this.mesh = mesh;
        }

        public override List<Geometry> GetGeometries(List<Node> allNodes)
        {
            throw new NotImplementedException("Mesh Binider has not been implemented");
        }

        public override void DrawGraphics(IRenderPackage package, TessellationParameters parameters, List<Node> allNodes)
        {
            throw new NotImplementedException("Mesh Binider has not been implemented");
        }
    }
}
