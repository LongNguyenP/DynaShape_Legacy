using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Printing;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;


namespace DynaShape
{
    [IsVisibleInDynamoLibrary(false)]
    public static class GeometryRender
    {
        public static Color DefaultPointColor = Color.FromArgb(255, 255, 0, 0);
        public static Color DefaultLineColor = Color.FromArgb(255, 80, 180, 200);
        public  static Color DefaultMeshFaceColor = Color.FromArgb(192, 60, 180, 250);

        public static void DrawPoint(IRenderPackage package, Triple point, Color color)
        {
            package.AddPointVertex(point.X, point.Y, point.Z);
            package.AddPointVertexColor(color.R, color.G, color.B, color.A);
        }


        public static void DrawPoint(IRenderPackage package, Triple point)
           => DrawPoint(package, point, DefaultPointColor);


        public static void DrawPoints(IRenderPackage package, List<Triple> points, Color color)
        {
            for (int i = 0; i < points.Count; i++)
            {
                package.AddPointVertex(points[i].X, points[i].Y, points[i].Z);
                package.AddPointVertexColor(color.R, color.G, color.B, color.A);
            }
        }


        public static void DrawPoints(IRenderPackage package, List<Triple> points)
           => DrawPoints(package, points, DefaultPointColor);


        public static void DrawLine(IRenderPackage package, Triple start, Triple end)
           => DrawLine(package, start, end, DefaultLineColor);


        public static void DrawLine(IRenderPackage package, Triple start, Triple end, Color color)
        {
            package.AddLineStripVertexCount(2);
            package.AddLineStripVertex(start.X, start.Y, start.Z);
            package.AddLineStripVertex(end.X, end.Y, end.Z);
            package.AddLineStripVertexColor(color.R, color.G, color.B, color.A);
            package.AddLineStripVertexColor(color.R, color.G, color.B, color.A);
        }


        public static void DrawPolyline(IRenderPackage package, List<Triple> vertices, Color color)
        {
            package.AddLineStripVertexCount(vertices.Count);
            for (int i = 0; i < vertices.Count; i++)
            {
                package.AddLineStripVertex(vertices[i].X, vertices[i].Y, vertices[i].Z);
                package.AddLineStripVertexColor(color.R, color.G, color.B, color.A);
            }
        }


        public static void DrawPolyline(IRenderPackage package, List<Triple> vertices)
           => DrawPolyline(package, vertices, DefaultLineColor);
    }
}
