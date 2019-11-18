using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using DynaShape;
using DynaShape.GeometryBinders;
using DynaShape.Goals;
using SharpDX;
using Mesh = Autodesk.Dynamo.MeshToolkit.Mesh;
using Point = Autodesk.DesignScript.Geometry.Point;


namespace DynaShape.ZeroTouch
{
    public static class KinetiX
    {
        private static List<ShapeMatchingGoal> shapeMatchingGoals;
        private static List<Point> vertices;
        private static List<int> indices;
        private static List<PolylineBinder> polylineBinders;


        [MultiReturn("shapeMatchingGoals", "meshBinders", "polylineBinders")]
        public static Dictionary<string, object> Shearing(int xCount = 5, int yCount = 5, double k = 0.2, double thickness = 0.5)
        {
            shapeMatchingGoals = new List<ShapeMatchingGoal>();
            vertices = new List<Point>();
            indices = new List<int>();
            polylineBinders = new List<PolylineBinder>();

            for (int i = 0; i < xCount; i++)
            for (int j = 0; j < yCount; j++)
            {
                if ((i + j) % 2 == 0)
                {
                    CreateUnit(new List<Triple>()
                    {
                        new Triple(i, j + k, 0f),
                        new Triple(i, j, 0f),
                        new Triple(i + 1f - k, j, 0f),
                        new Triple(i, j + k, thickness),
                        new Triple(i, j, thickness),
                        new Triple(i + 1f - k, j, thickness),
                    });

                    CreateUnit(new List<Triple>()
                    {
                        new Triple(i + 1f - k, j, 0f),
                        new Triple(i + 1f, j, 0f),
                        new Triple(i + 1f, j + 1f - k, 0f),
                        new Triple(i + 1f - k, j, thickness),
                        new Triple(i + 1f, j, thickness),
                        new Triple(i + 1f, j + 1f - k, thickness),
                    });

                    CreateUnit(new List<Triple>()
                    {
                        new Triple(i + 1f, j + 1f - k, 0f),
                        new Triple(i + 1f, j + 1f, 0f),
                        new Triple(i + k, j + 1f, 0f),
                        new Triple(i + 1f, j + 1f - k, thickness),
                        new Triple(i + 1f, j + 1f, thickness),
                        new Triple(i + k, j + 1f, thickness),
                    });

                    CreateUnit(new List<Triple>()
                    {
                        new Triple(i + k, j + 1f, 0f),
                        new Triple(i, j + 1f, 0f),
                        new Triple(i, j + k, 0f),
                        new Triple(i + k, j + 1f, thickness),
                        new Triple(i, j + 1f, thickness),
                        new Triple(i, j + k, thickness),
                    });
                }
                else
                {
                    if (i == 0 || i == xCount - 1 || j == 0 || j == yCount - 1)
                    {
                        CreateUnit(new List<Triple>()
                        {
                            new Triple(i, j + 1f - k, 0f),
                            new Triple(i, j, 0f),
                            new Triple(i + k, j, 0f),
                            new Triple(i, j + 1f - k, thickness),
                            new Triple(i, j, thickness),
                            new Triple(i + k, j, thickness),
                        });

                        CreateUnit(new List<Triple>()
                        {
                            new Triple(i + k, j, 0f),
                            new Triple(i + 1f, j, 0f),
                            new Triple(i + 1f, j + k, 0f),
                            new Triple(i + k, j, thickness),
                            new Triple(i + 1f, j, thickness),
                            new Triple(i + 1f, j + k, thickness),
                        });

                        CreateUnit(new List<Triple>()
                        {
                            new Triple(i + 1f, j + k, 0f),
                            new Triple(i + 1f, j + 1f, 0f),
                            new Triple(i + 1f - k, j + 1f, 0f),
                            new Triple(i + 1f, j + k, thickness),
                            new Triple(i + 1f, j + 1f, thickness),
                            new Triple(i + 1f - k, j + 1f, thickness),
                        });

                        CreateUnit(new List<Triple>()
                        {
                            new Triple(i + 1f - k, j + 1f, 0f),
                            new Triple(i, j + 1f, 0f),
                            new Triple(i, j + 1f - k, 0f),
                            new Triple(i + 1f - k, j + 1f, thickness),
                            new Triple(i, j + 1f, thickness),
                            new Triple(i, j + 1f - k, thickness),
                        });
                    }
                }
            }




            return new Dictionary<string, object>
            {
                {"shapeMatchingGoals", shapeMatchingGoals},
                {"meshBinders", new MeshBinder(Mesh.ByVerticesAndIndices(vertices, indices), new Color(0f, 0.7f, 1f, 0.9f))},
                {"polylineBinders", KinetiX.polylineBinders},
            };
        }

        private static void CreateUnit(List<Triple> triples)
        {
            List<Triple> t = triples;

            shapeMatchingGoals.Add(new ShapeMatchingGoal(t));

            int n;

            vertices.Add(t[0].ToPoint());
            vertices.Add(t[1].ToPoint());
            vertices.Add(t[4].ToPoint());
            vertices.Add(t[3].ToPoint());
            n = vertices.Count - 4;
            indices.AddRange(new[] {n, n + 1, n + 2, n, n + 2, n + 3});

            vertices.Add(t[1].ToPoint());
            vertices.Add(t[2].ToPoint());
            vertices.Add(t[5].ToPoint());
            vertices.Add(t[4].ToPoint());
            n = vertices.Count - 4;
            indices.AddRange(new[] {n, n + 1, n + 2, n, n + 2, n + 3});

            polylineBinders.Add(new PolylineBinder(new List<Triple>()
                {
                    t[0], t[1], t[2], t[5], t[4], t[3], t[0], t[1], t[4]
                },
                new Color(0.4f, 0.7f, 1f, 1f)));
        }
    }
}
