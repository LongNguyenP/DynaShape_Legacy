using System.Collections.Generic;
using DSCore;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using DynaShape.GeometryBinders;
using HelixToolkit.Wpf.SharpDX.Core;
using Point = Autodesk.DesignScript.Geometry.Point;


namespace DynaShape.ZeroTouch
{
    public static class GeometryBinders
    {
        public static CircleBinder CircleBinder(
            Point center,
            float radius,
            [DefaultArgument("Vector.ZAxis()")] Vector planeNormal,
            [DefaultArgument("null")] Color color)
        {
            return new CircleBinder(
                center.ToTriple(),
                radius,
                planeNormal.ToTriple(),
                color?.ToSharpDXColor() ?? DynaShapeDisplay.DefaultLineColor);
        }


        public static LineBinder LineBinder(
            Line line,
            [DefaultArgument("null")] Color color)
        {
            return new LineBinder(
                line.StartPoint.ToTriple(),
                line.EndPoint.ToTriple(),
                color?.ToSharpDXColor() ?? DynaShapeDisplay.DefaultLineColor);
        }


        public static LineBinder LineBinder(
            Point startPoint,
            Point endPoint,
            [DefaultArgument("null")] Color color)
        {
            return new LineBinder(
                startPoint.ToTriple(),
                endPoint.ToTriple(),
                color?.ToSharpDXColor() ?? DynaShapeDisplay.DefaultLineColor);
        }


        public static PolylineBinder PolylineBinder(
            List<Point> vertices,
            [DefaultArgument("null")] Color color,
            [DefaultArgument("false")] bool loop)
        {
            return new PolylineBinder(
                vertices.ToTriples(),
                color?.ToSharpDXColor() ?? DynaShapeDisplay.DefaultLineColor,
                loop);
        }


        public static MeshBinder MeshBinder(
            Autodesk.DesignScript.Geometry.Mesh mesh,
            [DefaultArgument("null")] Color color)
        {
            return new MeshBinder(
                mesh,
                color?.ToSharpDXColor() ?? DynaShapeDisplay.DefaultMeshFaceColor);
        }


        public static MeshBinder MeshBinder(
            Autodesk.Dynamo.MeshToolkit.Mesh toolkitMesh,
            [DefaultArgument("null")] Color color)
        {
            return new MeshBinder(
                toolkitMesh,
                color?.ToSharpDXColor() ?? DynaShapeDisplay.DefaultMeshFaceColor);
        }


        public static TexturedMeshBinder TexturedMeshBinder(
            Autodesk.Dynamo.MeshToolkit.Mesh toolkitMesh,
            [DefaultArgument("null")] Color color,
            string textureFileName,
            TextureCoordinateSet textureCoordinates)
        {
            return new TexturedMeshBinder(
                toolkitMesh,
                color?.ToSharpDXColor() ?? DynaShapeDisplay.DefaultMeshFaceColor,
                textureFileName,
                textureCoordinates.Content);
        }


        public static GeometryBinder ChangeColor(GeometryBinder geometryBinder, Color color)
        {
            geometryBinder.Color = color.ToSharpDXColor();
            return geometryBinder;
        }
    }
}