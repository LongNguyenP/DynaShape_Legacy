using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Configuration;
using System.Threading;
using System.Windows.Controls;
using Autodesk.DesignScript.Runtime;
using Dynamo.Controls;
using Dynamo.Wpf.ViewModels.Watch3D;
using DynaShape;
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
        public static DynaShape.Solver Create()
           => new DynaShape.Solver();


        /// <summary>
        /// Execute the solver to iteratively solve the input goals/constraints
        /// </summary>
        /// <param name="solver">The solver, which can be obtained from the Solver.Create node</param>
        /// <param name="goals">The goals/constraints that the solver will solve</param>
        /// <param name="geometryBinders">The geometry binders</param>
        /// <param name="iterations">[Not available in the current version] the solver will display/output the intermediate results for every n iterations being executed in the background. If set to 0, the solver will attempt to display/output the intermediate result approximately every 25 miliseconds, which is sufficient for real-time visual feedback.</param>
        /// <param name="reset">Reset the solver to the initial condition. You should set this to True at the beginning of a scenario, then set it to False. If you add, remove goals, you will need to reset for the changes to take effect</param>
        /// <param name="execute">Execute or stop executing the solver</param>
        /// <param name="enableMomentum">Apply momentum effect to the movement of the nodes. For simulation of physical motion, this results in more realistic motion. For constraint-based optimization, it often helps the solver to reach the final solution in fewer iteration (i.e. faster), but can sometimes lead to unstable and counter-intuitive solution. In such case, try setting momnentum to False </param>
        /// <param name="enableFastDisplay"></param>
        /// <param name="enableManipulation">Enable manipulation of the nodes in the background view with mouse</param>
        /// <returns></returns>
        [MultiReturn("nodePositions", "goalOutputs", "geometries")]
        [CanUpdatePeriodically(true)]
        public static Dictionary<string, object> Execute(
           DynaShape.Solver solver,
           List<Goal> goals,
           [DefaultArgument("null")] List<GeometryBinder> geometryBinders,
           [DefaultArgument("0")] int iterations,
           [DefaultArgument("true")] bool reset,
           [DefaultArgument("true")] bool execute,
           [DefaultArgument("true")] bool enableMomentum,
           [DefaultArgument("true")] bool enableFastDisplay,
           [DefaultArgument("false")] bool enableManipulation)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
       
            if (reset)
            {
                solver.StopBackgroundExecution();
                solver.Clear();
                solver.AddGoals(goals);
                if (geometryBinders != null) solver.AddGeometryBinders(geometryBinders);
                solver.Display.Render();
            }
            else
            {
                solver.EnableMouseInteraction = enableManipulation;
                solver.EnableMomentum = enableMomentum;
                solver.EnableFastDisplay = enableFastDisplay;
                solver.IterationCount = iterations;
                
                if (execute) solver.StartBackgroundExecution();
                else
                {
                    solver.StopBackgroundExecution();
                    if (!enableFastDisplay) solver.Iterate();
                }              
            }

            return enableFastDisplay
               ? null
               : new Dictionary<string, object> {
                   { "nodePositions", solver.GetStructuredNodePositionsAsPoints() },
                   { "goalOutputs", solver.GetGoalOutputs() },
                   { "geometries", solver.GetGeometries()} };
        }
    }
}

