﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.DesignScript.Runtime;
using DynaShape.Goals;
using DynaShape.GeometryBinders;


namespace DynaShape.ZeroTouch
{
    public static class Solver
    {
        /// <summary>
        /// Create a DynaShape solver, which will be input into the Solver.Execute node
        /// </summary>
        /// <returns></returns>
        public static DynaShape.Solver Create() => new DynaShape.Solver();


        /// <summary>
        /// Execute the solver to iteratively solve the input goals/constraints
        /// </summary>
        /// <param name="solver">The solver, which can be obtained from the Solver.Create node</param>
        /// <param name="goals">The goals/constraints that the solver will solve</param>
        /// <param name="geometryBinders">The geometry binders</param>
        /// <param name="nodeMergeThreshold">Before the solver starts to run, nodes that have identical positions (within this threshold) will be merged into one node</param>
        /// <param name="iterations">The number of iterations that the solver will execute in the background before display the intermediate result. If set to 0 (the default value), the solver will attempt run as many iterations as possible within approximately 25 milliseconds, which is sufficient for real-time visual feedback. Using a small value (e.g. 1) will make the solver appears to run more slowly and display more intermediate result, allowing us to better observe and understand how the nodes and goals behave</param>
        /// <param name="reset">Reset the solver to the initial condition. You should set this to True at the beginning of a scenario, then set it to False. If you add, remove goals, you will need to reset for the changes to take effect</param>
        /// <param name="execute">Execute or stop executing the solver</param>
        /// <param name="enableMomentum">Apply momentum effect to the movement of the nodes. For simulation of physical motion, this results in more realistic motion. For constraint-based optimization, it often helps the solver to reach the final solution in fewer iteration (i.e. faster), but can sometimes lead to unstable and counter-intuitive solution. In such case, try setting momentum to False </param>
        /// <param name="enableFastDisplay"></param>
        /// <param name="enableManipulation">Enable manipulation of the nodes in the background view with mouse</param>
        /// <param name="dampingFactor">When momentum mode is enabled, this value will determine how much the node's velocity is damped at each iteration</param>
        /// <returns></returns>
        [MultiReturn("nodePositions", "goalOutputs", "geometries")]
        [CanUpdatePeriodically(true)]
        public static Dictionary<string, object> Execute(
           DynaShape.Solver solver,
           List<Goal> goals,
           [DefaultArgument("null")] List<GeometryBinder> geometryBinders,
           [DefaultArgument("0.001")] float nodeMergeThreshold,
           [DefaultArgument("0")] int iterations,
           [DefaultArgument("true")] bool reset,
           [DefaultArgument("true")] bool execute,
           [DefaultArgument("true")] bool enableMomentum,
           [DefaultArgument("true")] bool enableFastDisplay,
           [DefaultArgument("false")] bool enableManipulation,
           [DefaultArgument("0.98")] float dampingFactor)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (reset)
            {
                solver.StopBackgroundExecution();
                solver.Clear();
                solver.AddGoals(goals);
                if (geometryBinders != null)
                    solver.AddGeometryBinders(geometryBinders, nodeMergeThreshold);
                solver.Render();
            }
            else
            {

                solver.EnableMouseInteraction = enableManipulation;
                solver.EnableMomentum = enableMomentum;
                solver.EnableFastDisplay = enableFastDisplay;
                solver.IterationCount = iterations;
                solver.DampingFactor = dampingFactor;

                if (execute) solver.StartBackgroundExecution();
                else
                {
                    solver.StopBackgroundExecution();
                    if (!enableFastDisplay) solver.ClearRender();
                    if (!enableFastDisplay) solver.Iterate();
                }
            }

            return execute || enableFastDisplay
               ? new Dictionary<string, object> {
                   { "nodePositions", null },
                   { "goalOutputs", null },
                   { "geometries", null } }
               : new Dictionary<string, object> {
                   { "nodePositions", solver.GetNodePositionsAsPoints() },
                   { "goalOutputs", solver.GetGoalOutputs() },
                   { "geometries", solver.GetGeometries() } };
        }


        /// <summary>
        /// Execute the solver silently and only display the final result.
        /// </summary>
        /// <param name="goals">The goals/constraints that the solver will solve</param>
        /// <param name="geometryBinders">The geometry binders</param>
        /// <param name="nodeMergeThreshold">Before the solver starts to run, nodes that have identical positions (within this threshold) will be merged into one node</param>
        /// <param name="iterations">The maximum number of iterations that will be executed</param>
        /// <param name="terminationThreshold">if the nodes' movement is below this threshold, the solver will stop executing and output the result</param>
        /// <param name="execute">This allows us to temporarily disable the solver while setting up/changing the parameters the parameters</param>
        /// <param name="enableMomentum">Apply momentum effect to the movement of the nodes. For simulation of physical motion, this results in more realistic motion. For constraint-based optimization, it often helps the solver to reach the final solution in fewer iteration (i.e. faster), but can sometimes lead to unstable and counter-intuitive solution. In such case, try setting momentum to False </param>
        /// <param name="dampingFactor"></param>
        /// <returns></returns>
        [MultiReturn("nodePositions", "goalOutputs", "geometries", "stats")]
        [CanUpdatePeriodically(true)]
        public static Dictionary<string, object> ExecuteSilently(
            List<Goal> goals,
            [DefaultArgument("null")] List<GeometryBinder> geometryBinders,
            [DefaultArgument("0.001")] float nodeMergeThreshold,
            [DefaultArgument("10000")] int iterations,
            [DefaultArgument("0.001")] float terminationThreshold,
            [DefaultArgument("true")] bool execute,
            [DefaultArgument("true")] bool enableMomentum,
            [DefaultArgument("0.98")] float dampingFactor)
        {
            if (!execute)
                return new Dictionary<string, object> {
                    { "nodePositions", null },
                    { "goalOutputs", null },
                    { "geometries", null },
                    { "stats", null}};

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DynaShape.Solver solver = new DynaShape.Solver();
            solver.AddGoals(goals, nodeMergeThreshold);
            if (geometryBinders != null) solver.AddGeometryBinders(geometryBinders, nodeMergeThreshold);
            solver.EnableMomentum = enableMomentum;
            solver.DampingFactor = dampingFactor;
            solver.Execute(iterations, terminationThreshold);

            TimeSpan computationTime = stopwatch.Elapsed;
            stopwatch.Restart();

            return new Dictionary<string, object> {
                { "nodePositions", solver.GetNodePositionsAsPoints() },
                { "goalOutputs", solver.GetGoalOutputs() },
                { "geometries", solver.GetGeometries() },
                { "stats", String.Concat(
                    "Computation Time         : " + computationTime,
                    "\nData Output Time         : " + stopwatch.Elapsed,
                    "\nNo. of Iterations Spent  : " + solver.CurrentIteration,
                    "\nLargest Movement Sqr.    : " + solver.GetKineticEnergy())
                }
            };
        }
    }
}

