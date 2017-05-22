using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Dynamo.Controls;
using Dynamo.Wpf.Rendering;
using Dynamo.Wpf.ViewModels.Watch3D;
using DynaShape;
using DynaShape.GeometryBinders;
using DynaShape.Goals;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;


namespace DynaShape.ZeroTouch
{
    public static class Tests
    {
        /// <summary>
        /// This is a quick test setup for shape matching goals. Here we create a 3D grid of cubes, and apply a shape matching goal on each cube to force them maintain their intial unit-cube shape
        /// </summary>
        /// <param name="X">Number of nodes along the X axis, minimum is 2</param>
        /// <param name="Y">Number of nodes along the Y axis, minimum is 2</param>
        /// <param name="Z">Number of nodes along the Z axis, minimum is 2</param>
        /// <returns>The ShapeMatching goals, Anchor goals, and polyline binders</returns>
        [MultiReturn("goals", "geometryBinders", "anchorGoals")]
        public static Dictionary<string, object> WonkyCubes(int X = 21, int Y = 21, int Z = 2)
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

        

        private static int counter = 0;

        [CanUpdatePeriodically(true)]
        public static int MovingPoint(bool reset)
        {
            if (reset) counter = 0;

            return counter++;
        }


        private static void Foo(int i)
        {
            Viewport3DX Viewport = (((DynaShapeViewExtension.DynamoWindow.Content as Grid).Children[2] as Grid).Children[1] as Watch3DView).View;
            List<Model3D> SceneItems = Viewport.ItemsSource as List<Model3D>;

            LineGeometry3D lines = new LineGeometry3D();
            lines.Positions = new Vector3Collection() { new Vector3(0, 0, 0), new Vector3(5, 5, 5) };
            lines.Indices = new IntCollection() { 0, 1 };

            var lineModel = new LineGeometryModel3D()
            {
                Geometry = lines
            };

            lines.Positions[0] = new Vector3(i, 0, 1);
            lineModel.Attach(Viewport.RenderHost);
            SceneItems.Add(lineModel); 
        }
    }
}
