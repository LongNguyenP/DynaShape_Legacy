using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Mesh = Autodesk.Dynamo.MeshToolkit.Mesh;

namespace DynaShape.ZeroTouch
{
    /// <summary>
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Create a planar mesh
        /// </summary>
        /// <param name="cs">The planar mesh will be oriented according to the XY plane of this input coordinate system </param>
        /// <param name="lengthX">The X dimision of the planar mesh</param>
        /// <param name="lengthY">The Y dimision of the planar mesh</param>
        /// <param name="divX">The mesh face division in the X dimension</param>
        /// <param name="divY">The mesh face division in the Y dimension</param>
        /// <param name="alternatingDiagons">Alternating the diagonal direction of the triangular faces</param>
        /// <returns></returns>
        [MultiReturn("mesh", "quadFaceVertexIndices")]
        public static Dictionary<string, object> PlaneMesh(
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

            List<List<int>> quadFaceVertexIndices = new List<List<int>>();

            for (int j = 0; j < divY; j++)
                for (int i = 0; i < divX; i++)
                {
                    int a = i + j * (divX + 1);
                    int b = i + 1 + j * (divX + 1);
                    int c = i + 1 + (j + 1) * (divX + 1);
                    int d = i + (j + 1) * (divX + 1);

                    quadFaceVertexIndices.Add(new List<int> { a, b, c, d });

                    if (alternatingDiagons)
                    {
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
                    {
                        indices.Add(a);
                        indices.Add(b);
                        indices.Add(c);
                        indices.Add(a);
                        indices.Add(c);
                        indices.Add(d);
                    }
                }

            return new Dictionary<string, object>()
            {
                { "mesh", Mesh.ByVerticesAndIndices(vertices, indices) },
                { "quadFaceVertexIndices", quadFaceVertexIndices },
            };
        }


        /// <summary>
        /// Create a rectangular grid of points
        /// </summary>
        /// <param name="cs">A coordinate system upon which the rectangular grid is based </param>
        /// <param name="lengthX">The X dimision of the grid</param>
        /// <param name="lengthY">The Y dimision of the grid</param>
        /// <param name="divX">The grid division in the X dimension</param>
        /// <param name="divY">The grid division in the Y dimension</param>
        /// <returns></returns>
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


        /// <summary>
        /// Create a sequence of lines from a sequence of input points
        /// </summary>
        /// <param name="points">The sequence of points</param>
        /// <param name="loop">If True, the output will also include a line going from the last point back to the first point, creating a loop</param>
        /// <returns></returns>
        public static List<Line> LinesFromPointSequence(List<Point> points, bool loop = false)
        {
            List<Line> lines = new List<Line>();
            for (int i = 0; i < points.Count - 1; i++)
                lines.Add(Line.ByStartPointEndPoint(points[i], points[i + 1]));
            if (loop) lines.Add(Line.ByStartPointEndPoint(points[points.Count - 1], points[0]));
            return lines;
        }

        public static List<List<Point>> GetVerticesOfAllPairOfTriangles(Mesh mesh)
        {
            List<int> faceVertexIndices = mesh.VertexIndicesByTri();
            int vertexCount = (int)mesh.VertexCount;
            Dictionary<int, List<int>> edgeFaceTopology = new Dictionary<int, List<int>>();

            for (int i = 0; i < mesh.TriangleCount; i++)
            {
                int A = faceVertexIndices[i * 3];
                int B = faceVertexIndices[i * 3 + 1];
                int C = faceVertexIndices[i * 3 + 2];

                InsertEdgeFaceTopology(edgeFaceTopology, i, A, B, vertexCount);
                InsertEdgeFaceTopology(edgeFaceTopology, i, B, C, vertexCount);
                InsertEdgeFaceTopology(edgeFaceTopology, i, C, A, vertexCount);
            }

            List<List<Point>> facePairVertices = new List<List<Point>>();

            foreach (List<int> connectedFaces in edgeFaceTopology.Values)
            {
                if (connectedFaces.Count < 2) continue;

                HashSet<int> hashSet = new HashSet<int>();
                hashSet.Add(faceVertexIndices[connectedFaces[0] * 3]);
                hashSet.Add(faceVertexIndices[connectedFaces[0] * 3 + 1]);
                hashSet.Add(faceVertexIndices[connectedFaces[0] * 3 + 2]);
                hashSet.Add(faceVertexIndices[connectedFaces[1] * 3]);
                hashSet.Add(faceVertexIndices[connectedFaces[1] * 3 + 1]);
                hashSet.Add(faceVertexIndices[connectedFaces[1] * 3 + 2]);

                List<Point> vertices = new List<Point>();

                foreach (int i in hashSet)
                    vertices.Add(mesh.Vertices()[i]);

                facePairVertices.Add(vertices);
            }

            return facePairVertices;
        }

        private static void InsertEdgeFaceTopology(Dictionary<int, List<int>> dict, int faceIndex, int start, int end, int vertexCount)
        {
            int i = start < end
                ? start * vertexCount + end
                : end * vertexCount + start;

            if (!dict.ContainsKey(i)) dict.Add(i, new List<int>());

            dict[i].Add(faceIndex);
        }
    }
}
