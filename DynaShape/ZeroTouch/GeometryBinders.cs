using System.Collections.Generic;
using DSCore;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using DynaShape;
using DynaShape.GeometryBinders;
using DynaShape.Goals;
using Point = Autodesk.DesignScript.Geometry.Point;


namespace DynaShape.ZeroTouch
{
    public static class GeometryBinders
    {
        public static LineBinder LineBinder(
           Line line,
           [DefaultArgument("null")] Color color)
           => new LineBinder(line.StartPoint.ToTriple(), line.EndPoint.ToTriple(), color?.ToSystemColor() ?? GeometryRender.DefaultLineColor);


        public static LineBinder LineBinder(
           Point startPoint,
           Point endPoint,
           [DefaultArgument("null")] Color color)
           => new LineBinder(startPoint.ToTriple(), endPoint.ToTriple(), color?.ToSystemColor() ?? GeometryRender.DefaultLineColor);


        public static PolylineBinder PolylineBinder(
           List<Point> vertices,
           [DefaultArgument("null")] Color color)
           => new PolylineBinder(vertices.ToTriples(), color?.ToSystemColor() ?? GeometryRender.DefaultLineColor);


        public static GeometryBinder ChangeColor(GeometryBinder geometryBinder, Color color)
        {
            geometryBinder.Color = color.ToSystemColor();
            return geometryBinder;
        }
    }
}