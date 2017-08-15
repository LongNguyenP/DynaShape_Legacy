using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Mesh = Autodesk.Dynamo.MeshToolkit.Mesh;

namespace DynaShape.ZeroTouch
{
    public static class Utilities
    {
        public static Mesh PlaneMesh(
            [DefaultArgument("CoordinateSystem.ByOriginVectors(Point.Origin(), Vector.XAxis(), Vector.YAxis())")] CoordinateSystem cs,
            [DefaultArgument("20.0")]double lengthX,
            [DefaultArgument("20.0")]double lengthY,
            [DefaultArgument("20")]int divX,
            [DefaultArgument("20")]int divY,
            [DefaultArgument("true")]bool alternatingDiagons)
        {
            List<Point> vertices = new List<Point>((divX + 1) * (divY + 1));

            for (int j = 0; j <= divY; j++)
                for (int i = 0; i <= divX; i++)
                    vertices.Add(
                        Point.ByCartesianCoordinates(
                            cs,
                            ((double)i / divX - 0.5f) * lengthX,
                            ((double)j / divY - 0.5f) * lengthY));


            List<int> indices = new List<int>();

            if (alternatingDiagons)
                for (int j = 0; j < divY; j++)
                    for (int i = 0; i < divX; i++)
                    {
                        int a = i + j * (divX + 1);
                        int b = i + 1 + j * (divX + 1);
                        int c = i + 1 + (j + 1) * (divX + 1);
                        int d = i + (j + 1) * (divX + 1);

                        if ((i + j) % 2 == 0)
                        {
                            indices.Add(a);
                            indices.Add(b);
                            indices.Add(c);
                            indices.Add(a);
                            indices.Add(c);
                            indices.Add(d);
                        }
                        else
                        {
                            indices.Add(a);
                            indices.Add(b);
                            indices.Add(d);
                            indices.Add(b);
                            indices.Add(c);
                            indices.Add(d);
                        }
                    }
            else
                for (int j = 0; j < divY; j++)
                    for (int i = 0; i < divX; i++)
                    {
                        int a = i + j * (divX + 1);
                        int b = i + 1 + j * (divX + 1);
                        int c = i + 1 + (j + 1) * (divX + 1);
                        int d = i + (j + 1) * (divX + 1);

                        indices.Add(a);
                        indices.Add(b);
                        indices.Add(c);
                        indices.Add(a);
                        indices.Add(c);
                        indices.Add(d);
                    }

            return Mesh.ByVerticesAndIndices(vertices, indices);
        }


        [MultiReturn("all", "xRows", "yRows", "borders", "corners", "quads")]
        public static Dictionary<string, object> RectangularGrid(
            [DefaultArgument("CoordinateSystem.ByOriginVectors(Point.Origin(), Vector.XAxis(), Vector.YAxis())")] CoordinateSystem cs,
            [DefaultArgument("20.0")] double lengthX,
            [DefaultArgument("20.0")] double lengthY,
            [DefaultArgument("20")] int divX,
            [DefaultArgument("20")] int divY)
        {
            List<Point> all = new List<Point>((divX + 1) * (divY + 1));
            for (int j = 0; j <= divY; j++)
                for (int i = 0; i <= divX; i++)
                    all.Add(
                        Point.ByCartesianCoordinates(
                            cs,
                            ((double)i / divX - 0.5f) * lengthX,
                            ((double)j / divY - 0.5f) * lengthY));

            List<List<Point>> xRows = new List<List<Point>>();
            for (int j = 0; j <= divY; j++)
            {
                List<Point> xRow = new List<Point>();
                for (int i = 0; i <= divX; i++)
                    xRow.Add(all[i + j * (divX + 1)]);
                xRows.Add(xRow);
            }

            List<List<Point>> yRows = new List<List<Point>>();
            for (int i = 0; i <= divX; i++)
            {
                List<Point> yRow = new List<Point>();
                for (int j = 0; j <= divY; j++)
                    yRow.Add(all[i + j * (divX + 1)]);
                yRows.Add(yRow);
            }

            List<List<Point>> quads = new List<List<Point>>();
            for (int j = 0; j < divY; j++)
                for (int i = 0; i < divX; i++)
                    quads.Add(
                        new List<Point> {
                        all[i     + j       * (divX + 1)],
                        all[i + 1 + j       * (divX + 1)],
                        all[i + 1 + (j + 1) * (divX + 1)],
                        all[i     + (j + 1) * (divX + 1)]});

            return new Dictionary<string, object> {
                { "all", all },
                { "xRows", xRows },
                { "yRows", yRows },
                { "borders", new List<List<Point>> { xRows[0], yRows[divX], xRows[divY], yRows[0]} },
                { "corners", new List<Point> { all[0], all[divX], all[(divX + 1) * (divY + 1) - 1], all[(divX + 1) * divY] } },
                { "quads", quads }};
        }


        public static List<Line> LinesFromPointSequence(List<Point> points, bool loop = false)
        {
            List<Line> lines = new List<Line>();
            for (int i = 0; i < points.Count - 1; i++)
                lines.Add(Line.ByStartPointEndPoint(points[i], points[i + 1]));
            if (loop) lines.Add(Line.ByStartPointEndPoint(points[points.Count - 1], points[0]));
            return lines;
        }

        //public static Mesh DiagridMesh(
        //    [DefaultArgument("CoordinateSystem.ByOriginVectors(Point.Origin(), Vector.XAxis(), Vector.YAxis())")] CoordinateSystem cs,
        //    [DefaultArgument("10.0")]double radius,
        //    [DefaultArgument("10.0")]double lengthY,
        //    [DefaultArgument("10")]int div)
        //{

        //}
    }
}
