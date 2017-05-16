using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Configuration;
using Autodesk.DesignScript.Runtime;
using Dynamo.Wpf.ViewModels.Watch3D;
using DynaShape;
using DynaShape.Goals;
using DynaShape.GeometryBinders;


namespace DynaShape.ZeroTouch
{
    public static class Solver
    {
        public static DynaShape.Solver Create()
           => new DynaShape.Solver();

        [CanUpdatePeriodically(true)]
        [MultiReturn(
           "nodePositions",
           "goalOutputs",
           "geometries",
           "time",
           "display")]
        public static Dictionary<string, object> Execute(
           DynaShape.Solver solver,
           List<Goal> goals,
           [DefaultArgument("null")] List<GeometryBinder> geometryBinders,
           [DefaultArgument("10")] int iterations,
           [DefaultArgument("true")] bool reset,
           [DefaultArgument("true")] bool momentum,
           [DefaultArgument("true")] bool fastDisplay,
           [DefaultArgument("false")] bool mouseInteract)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (reset)
            {
                solver.Clear();
                solver.AddGoals(goals);
                if (geometryBinders != null)
                    solver.AddGeometryBinders(geometryBinders);
            }
            else
            {
                solver.AllowMouseInteraction = mouseInteract;
                solver.Step(iterations, momentum);
            }

            double time = stopwatch.Elapsed.TotalMilliseconds;

            return fastDisplay
               ? new Dictionary<string, object> {
                   { "time", time},
                   { "display", new DynaShapeDisplay(solver) } }
               : new Dictionary<string, object> {
                   { "nodePositions", solver.GetNodePositionsAsPoints()},
                   { "geometries", solver.GetGeometries()},
                   { "time", time } };
        }
    }
}

