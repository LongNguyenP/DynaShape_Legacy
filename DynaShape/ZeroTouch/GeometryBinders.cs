using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using DynaShape.GeometryBinders;
using HelixToolkit.Wpf.SharpDX;
using Color = DSCore.Color;
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
                color?.ToColor4() ?? DynaShapeDisplay.DefaultLineColor);
        }


        public static LineBinder LineBinder(
            Line line,
            [DefaultArgument("null")] Color color)
        {
            return new LineBinder(
                line.StartPoint.ToTriple(),
                line.EndPoint.ToTriple(),
                color?.ToColor4() ?? DynaShapeDisplay.DefaultLineColor);
        }


        public static LineBinder LineBinder(
            Point startPoint,
            Point endPoint,
            [DefaultArgument("null")] Color color)
        {
            return new LineBinder(
                startPoint.ToTriple(),
                endPoint.ToTriple(),
                color?.ToColor4() ?? DynaShapeDisplay.DefaultLineColor);
        }


        public static PolylineBinder PolylineBinder(
            List<Point> vertices,
            [DefaultArgument("null")] Color color,
            [DefaultArgument("false")] bool loop)
        {
            return new PolylineBinder(
                vertices.ToTriples(),
                color?.ToColor4() ?? DynaShapeDisplay.DefaultLineColor,
                loop);
        }


        public static MeshBinder MeshBinder(
            Mesh mesh,
            [DefaultArgument("null")] Color color)
        {
            return new MeshBinder(
                mesh,
                color?.ToColor4() ?? DynaShapeDisplay.DefaultMeshFaceColor);
        }


        public static MeshBinder MeshBinder(
            Autodesk.Dynamo.MeshToolkit.Mesh toolkitMesh,
            [DefaultArgument("null")] Color color)
        {
            return new MeshBinder(
                toolkitMesh,
                color?.ToColor4() ?? DynaShapeDisplay.DefaultMeshFaceColor);
        }


        public static TexturedMeshBinder TexturedMeshBinder(
            Autodesk.Dynamo.MeshToolkit.Mesh toolkitMesh,
            [DefaultArgument("null")] Color color,
            string textureFileName,
            Vector2Collection textureCoordinates)
        {
            return new TexturedMeshBinder(
                toolkitMesh,
                color?.ToColor4() ?? DynaShapeDisplay.DefaultMeshFaceColor,
                textureFileName,
                textureCoordinates);
        }

        public static TextBinder TextBinder(
            Point position,
            string text,
            [DefaultArgument("null")] Color color)
        {
            return new TextBinder(position.ToTriple(), text, color?.ToColor4() ?? DynaShapeDisplay.DefaultLineColor);
        }

        public static GeometryBinder ChangeColor(GeometryBinder geometryBinder, Color color)
        {
            geometryBinder.Color = color.ToColor4();
            return geometryBinder;
        }

        public static GeometryBinder Show(GeometryBinder geometryBinder, bool show)
        {
            geometryBinder.Show = show;
            return geometryBinder;
        }
    }
}