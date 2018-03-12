using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using DynaShape;
using DynaShape.GeometryBinders;
using DynaShape.Goals;

using Mesh = Autodesk.Dynamo.MeshToolkit.Mesh;

namespace DynaShape.ZeroTouch
{
    public static class TestingUtilities
    {
        /// <summary>
        /// This is a quick test setup for shape matching goals. Here we create a 3D grid of cubes, and apply a shape matching goal on each cube to force them maintain their intial unit-cube shape
        /// </summary>
        /// <param name="X">Number of nodes along the X axis, minimum is 2</param>
        /// <param name="Y">Number of nodes along the Y axis, minimum is 2</param>
        /// <param name="Z">Number of nodes along the Z axis, minimum is 2</param>
        /// <returns>The ShapeMatching goals, Anchor goals, and polyline binders</returns>
        [MultiReturn("goals", "geometryBinders", "anchorGoals", "points")]
        public static Dictionary<string, object> WonkyCubes(int X = 21, int Y = 21, int Z = 2)
        {
            Triple[,,] points = new Triple[X, Y, Z];
            List<Point> pointsLinear = new List<Point>();
            for (int i = 0; i < X; i++)
                for (int j = 0; j < Y; j++)
                    for (int k = 0; k < Z; k++)
                    {
                        points[i, j, k] = new Triple(i, j, k + 5f);
                        pointsLinear.Add(Point.ByCoordinates(i, j, k + 5f));
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

            //goals.Add(new FloorGoal(pointsLinear));
            //goals.Add(new ConstantGoal(pointsLinear, Triple.BasisZ * -0.1f));

            return new Dictionary<string, object>
            {
                {"goals", goals},
                {"geometryBinders", geometryBinders},
                {"anchorGoals", anchorGoals},
                {"points", pointsLinear}
            };
        }


        public static Plane BestFitPlane(List<Point> points)
        {
            Util.ComputeBestFitPlane(points.ToTriples(), out Triple origin, out Triple normal);
            return Plane.ByOriginNormal(origin.ToPoint(), normal.ToVector());
        }

        public static Circle BestFitCircle(List<Point> points)
        {
            Util.ComputeBestFitCircle(points.ToTriples(), out Triple center, out Triple normal, out float radius);
            return Circle.ByCenterPointRadiusNormal(center.ToPoint(), radius, normal.ToVector());
        }
    }
}
