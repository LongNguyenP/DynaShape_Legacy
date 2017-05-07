using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using DynaShape;
using DynaShape.GeometryBinders;
using DynaShape.Goals;

namespace DynaShape.ZeroTouch
{
    public static class Tests
    {
        [MultiReturn("goals", "geometryBinders")]
        public static Dictionary<string, object> CyclicPolygonAndEqualLengths(int n, double a, int s)
        {
            Random random = s > 0 ? new Random(s) : new Random();
            Triple[,] triples = new Triple[n, n];
            for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                triples[i, j] = new Triple(i, j, 0.0 + a * (2.0 * random.NextDouble() - 1.0));


            List<Goal> goals = new List<Goal>
            {
                new AnchorGoal(triples[0, 0], new Triple(0.0, 0.0, 0.0)),
                new AnchorGoal(triples[n - 1, 0], new Triple(n - 1, 0.0, 0.0)),
                new AnchorGoal(triples[n - 1, n - 1], new Triple(n - 1, n - 1, 0.0)),
                new AnchorGoal(triples[0, n - 1], new Triple(0.0, n - 1, 0.0))
            };


            List<GeometryBinder> geometryBinders = new List<GeometryBinder>();

            //================================================================
            // Cyclcic polygon goals
            //================================================================

            for (int i = 0; i < n; i++)
            {
                List<Triple> tripleRow = new List<Triple>();
                for (int j = 0; j < n; j++) tripleRow.Add(triples[i, j]);
                goals.Add(new CoCircularGoal(tripleRow));
                geometryBinders.Add(new PolylineBinder(tripleRow));
            }

            for (int i = 0; i < n; i++)
            {
                List<Triple> tripleRow = new List<Triple>();
                for (int j = 0; j < n; j++) tripleRow.Add(triples[j, i]);
                goals.Add(new CoCircularGoal(tripleRow));
                geometryBinders.Add(new PolylineBinder(tripleRow));
            }


            //================================================================
            // Equal lengths goal
            //================================================================

            for (int i = 0; i < n; i++)
            {
                List<Triple> t1 = new List<Triple>();
                List<Triple> t2 = new List<Triple>();
                for (int j = 0; j < n - 1; j++)
                {
                    t1.Add(triples[i, j]);
                    t1.Add(triples[i, j + 1]);
                    t2.Add(triples[j, i]);
                    t2.Add(triples[j + 1, i]);
                }
                goals.Add(new EqualLengthsGoal(t1));
                goals.Add(new EqualLengthsGoal(t2));
            }


            return new Dictionary<string, object>
            {
                {"goals", goals},
                {"geometryBinders", geometryBinders},
            };
        }



        [MultiReturn("goals", "geometryBinders", "anchorGoals")]
        public static Dictionary<string, object> WonkyVoxels(int X = 21, int Y = 21, int Z = 2, double g = -0.01)
        {
            Triple[,,] points = new Triple[X, Y, Z];
            for (int i = 0; i < X; i++)
            for (int j = 0; j < Y; j++)
            for (int k = 0; k < Z; k++)
            {
                points[i, j, k] = new Triple((float) i, (float) j, (float) k);
            }


            List<Goal> anchorGoals = new List<Goal>();

            anchorGoals.Add(new AnchorGoal(points[0, 0, Z - 1]));
            anchorGoals.Add(new AnchorGoal(points[X - 1, 0, Z - 1]));
            anchorGoals.Add(new AnchorGoal(points[X - 1, Y - 1, Z - 1]));
            anchorGoals.Add(new AnchorGoal(points[0, Y - 1, Z - 1]));

            List<Goal> goals = new List<Goal>();
            goals.AddRange(anchorGoals);


            List<GeometryBinder> geometryBinders = new List<GeometryBinder>();

            List<Triple> vertices = new List<Triple>();

            vertices.Clear();
            for (int i = 0; i < X; i++)
            for (int j = 0; j < Y; j++)
                vertices.Add(points[i, i % 2 == 0 ? j : Y - 1 - j, 0]);
            geometryBinders.Add(new PolylineBinder(vertices));

            vertices.Clear();
            for (int j = 0; j < X; j++)
            for (int i = 0; i < Y; i++)
                vertices.Add(points[j % 2 == 0 ? i : X - 1 - i, j, 0]);
            geometryBinders.Add(new PolylineBinder(vertices));

            vertices.Clear();
            for (int i = 0; i < X; i++)
            for (int j = 0; j < Y; j++)
                vertices.Add(points[i, i % 2 == 0 ? j : Y - 1 - j, Z - 1]);
            geometryBinders.Add(new PolylineBinder(vertices));

            vertices.Clear();
            for (int j = 0; j < X; j++)
            for (int i = 0; i < Y; i++)
                vertices.Add(points[j % 2 == 0 ? i : X - 1 - i, j, Z - 1]);
            geometryBinders.Add(new PolylineBinder(vertices));



            vertices.Clear();
            for (int j = 0; j < Y; j++)
            for (int k = 0; k < Z; k++)
                vertices.Add(points[0, j, j % 2 == 0 ? k : Z - 1 - k]);
            geometryBinders.Add(new PolylineBinder(vertices));

            vertices.Clear();
            for (int k = 0; k < Z; k++)
            for (int j = 0; j < Y; j++)
                vertices.Add(points[0, k % 2 == 0 ? j : Y - 1 - j, k]);
            geometryBinders.Add(new PolylineBinder(vertices));

            vertices.Clear();
            for (int j = 0; j < Y; j++)
            for (int k = 0; k < Z; k++)
                vertices.Add(points[X - 1, j, j % 2 == 0 ? k : Z - 1 - k]);
            geometryBinders.Add(new PolylineBinder(vertices));

            vertices.Clear();
            for (int k = 0; k < Z; k++)
            for (int j = 0; j < Y; j++)
                vertices.Add(points[X - 1, k % 2 == 0 ? j : Y - 1 - j, k]);
            geometryBinders.Add(new PolylineBinder(vertices));



            vertices.Clear();
            for (int k = 0; k < Z; k++)
            for (int i = 0; i < Y; i++)
                vertices.Add(points[k % 2 == 0 ? i : X - 1 - i, 0, k]);
            geometryBinders.Add(new PolylineBinder(vertices));

            vertices.Clear();
            for (int i = 0; i < Y; i++)
            for (int k = 0; k < Z; k++)
                vertices.Add(points[i, 0, i % 2 == 0 ? k : Z - 1 - k]);
            geometryBinders.Add(new PolylineBinder(vertices));

            vertices.Clear();
            for (int k = 0; k < Z; k++)
            for (int i = 0; i < Y; i++)
                vertices.Add(points[k % 2 == 0 ? i : X - 1 - i, Y - 1, k]);
            geometryBinders.Add(new PolylineBinder(vertices));

            vertices.Clear();
            for (int i = 0; i < Y; i++)
            for (int k = 0; k < Z; k++)
                vertices.Add(points[i, Y - 1, i % 2 == 0 ? k : Z - 1 - k]);
            geometryBinders.Add(new PolylineBinder(vertices));


            for (int i = 0; i < X - 1; i++)
            for (int j = 0; j < Y - 1; j++)
            for (int k = 0; k < Z - 1; k++)
            {
                var voxelPoints = new List<Triple>()
                {
                    points[i + 0, j + 0, k + 0],
                    points[i + 1, j + 0, k + 0],
                    points[i + 1, j + 1, k + 0],
                    points[i + 0, j + 1, k + 0],
                    points[i + 0, j + 0, k + 1],
                    points[i + 1, j + 0, k + 1],
                    points[i + 1, j + 1, k + 1],
                    points[i + 0, j + 1, k + 1],
                };


                goals.Add(new ShapeMatchingGoal(voxelPoints, voxelPoints));
            }


            return new Dictionary<string, object>
            {
                {"goals", goals},
                {"geometryBinders", geometryBinders},
                {"anchorGoals", anchorGoals}
            };
        }
    }
}
