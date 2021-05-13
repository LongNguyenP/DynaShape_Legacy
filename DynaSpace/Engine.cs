using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using DynaShape;
using DynaShape.GeometryBinders;
using DynaShape.Goals;
using SharpDX;

using Point = Autodesk.DesignScript.Geometry.Point;


namespace DynaSpace
{
    public class Engine : IDisposable
    {
        #region member variables

        internal DynaShape.Solver Solver = new DynaShape.Solver();
        internal List<Goal> Goals = new List<Goal>();
        internal List<GeometryBinder> GeometryBinders = new List<GeometryBinder>();

        internal List<string> DepartmentNames = new List<string>();
        internal List<List<string>> SpaceNames = new List<List<string>>();
        internal List<List<int>> SpaceIds = new List<List<int>>();

        internal ConvexPolygonContainmentGoal ContainmentGoal;
        internal OnPlaneGoal OnPlaneGoal;
        internal List<AnchorGoal> DepartmentAnchorGoals = new List<AnchorGoal>();

        internal SphereCollisionGoal SphereCollisionGoal;

        internal List<LengthGoal> DepartmentCohesionGoals = new List<LengthGoal>();
        internal List<LengthGoal> SpaceAdjacencyGoals = new List<LengthGoal>();
        internal List<LengthGoal> SpaceDepartmentAdjacencyGoals = new List<LengthGoal>();

        internal List<List<CircleBinder>> CircleBinders = new List<List<CircleBinder>>();
        internal List<LineBinder> SpaceAdjacencyLineBinders = new List<LineBinder>();
        internal List<LineBinder> SpaceDepartmentAdjacencyLineBinders = new List<LineBinder>();

        internal List<TextBinder> TextBinders = new List<TextBinder>();
        //internal SpacePlanningBubbleMeshesBinder BubbleMeshesBinder;

        internal List<int> SpaceAdjI = new List<int>();
        internal List<int> SpaceAdjJ = new List<int>();
        internal List<float> SpaceAdjTargets = new List<float>();

        internal List<float> SpaceAdjErrors = new List<float>();
        internal List<float> SpaceAdjErrorRatios = new List<float>();

        #endregion


        [IsVisibleInDynamoLibrary(false)]
        public Engine() { }


        internal void SetUp()
        {
            Goals.Add(ContainmentGoal);
            Goals.Add(OnPlaneGoal);
            Goals.Add(SphereCollisionGoal);
            //Goals.Add(GlobalDirectionGoal);
            //Goals.AddRange(DepartmentAnchorGoals);
            Goals.AddRange(DepartmentCohesionGoals);
            Goals.AddRange(SpaceAdjacencyGoals);
            Goals.AddRange(SpaceDepartmentAdjacencyGoals);

            foreach (var circleBinderList in CircleBinders) GeometryBinders.AddRange(circleBinderList);
            GeometryBinders.AddRange(SpaceAdjacencyLineBinders);
            GeometryBinders.AddRange(SpaceDepartmentAdjacencyLineBinders);
            GeometryBinders.AddRange(TextBinders);
            //GeometryBinders.Add(BubbleMeshesBinder);

            Solver = new DynaShape.Solver();
            Solver.AddGoals(Goals);
            Solver.AddGeometryBinders(GeometryBinders);
        }


        /// <summary>
        /// Create a new DynaSpace engine
        /// </summary>
        /// <param name="data">Space specification data</param>
        /// <returns></returns>
        public static Engine Create(List<object> data)
        {
            return CreateWithInitialPositions(data, null);
        }


        /// <summary>
        /// Create a new DynaSpace engine
        /// </summary>
        /// <param name="data">Space specification data</param>
        /// <param name="initialPositions">The initial position of each space bubble</param>
        /// <returns></returns>
        public static Engine CreateWithInitialPositions(List<object> data, List<Point> initialPositions = null)
        {
            Engine engine = new Engine();

            //===========================================================================
            // Read CSV data
            //===========================================================================

            List<string> texts = new List<string>();
            foreach (object datum in data) texts.Add(datum?.ToString().Trim());
            int stride = 12;


            //===========================================================================
            // Read departments
            //===========================================================================

            engine.DepartmentNames = new List<string>();

            for (int i = stride; i < data.Count; i += stride)
                if (!engine.DepartmentNames.Contains(texts[i + 2]))
                    engine.DepartmentNames.Add(texts[i + 2]);

            for (int i = 0; i < engine.DepartmentNames.Count; i++)
            {
                engine.SpaceIds.Add(new List<int>());
                engine.SpaceNames.Add(new List<string>());
                engine.CircleBinders.Add(new List<CircleBinder>());
            }

            List<Color4> departmentColors = new List<Color4>();

            for (int i = 0; i < engine.DepartmentNames.Count; i++)
                departmentColors.Add(DynaShape.Util.ColorFromHSL((float)i / (float)engine.DepartmentNames.Count, 1.0f, 0.4f));


            //===========================================================================
            // Read spaces
            //===========================================================================

            List<string> spaceNames = new List<string>();
            List<int> departmentIds = new List<int>();
            List<int> quantities = new List<int>();
            List<double> areas = new List<double>();
            List<double> preferences = new List<double>();
            List<List<int>> adjacentSpaceIds = new List<List<int>>();
            List<List<int>> adjacentDepartmentIds = new List<List<int>>();

            for (int i = stride; i < data.Count; i += stride)
            {
                spaceNames.Add(texts[i + 1]);

                int departmentId = -1;
                for (int k = 0; k < engine.DepartmentNames.Count; k++)
                    if (engine.DepartmentNames[k] == texts[i + 2])
                        departmentId = k;
                departmentIds.Add(departmentId);

                quantities.Add(int.Parse(texts[i + 4]));
                areas.Add(double.Parse(texts[i + 7]));
                preferences.Add(texts[i + 9] == null ? 0.0 : double.Parse(texts[i + 9]));

                List<int> adjacentSpaceIds_ = new List<int>();
                if (texts[i + 10] != null)
                {
                    string[] segments = texts[i + 10].Split('.');
                    foreach (string segment in segments) adjacentSpaceIds_.Add(int.Parse(segment));
                }

                adjacentSpaceIds.Add(adjacentSpaceIds_);

                List<int> adjacentDepartmentIds_ = new List<int>();
                if (texts[i + 11] != null)
                {
                    string[] segments = texts[i + 11].Split('.');
                    foreach (string segment in segments) adjacentDepartmentIds_.Add(int.Parse(segment));

                }

                adjacentDepartmentIds.Add(adjacentDepartmentIds_);
            }


            //===================================================================================

            List<Triple> departmentCenters = new List<Triple>();

            for (int i = 0; i < engine.DepartmentNames.Count; i++)
            {
                double alpha = (double)i / engine.DepartmentNames.Count * Math.PI * 2.0;
                departmentCenters.Add(20f * new Triple(Math.Cos(alpha), Math.Sin(alpha), 0.0));
                if (i == 0) engine.DepartmentAnchorGoals.Add(new AnchorGoal(departmentCenters.Last(), Triple.Zero, 0.1f));
            }

            //engine.SpaceAdjacencyLineBinders.Add(new LineBinder(departmentCenters[0], departmentCenters[1], Color.Orange));


            Random random = new Random(0);

            //===================================================================================

            List<List<Triple>> spaceCenters = new List<List<Triple>>();

            List<Triple> spaceCentersFlattened = new List<Triple>();
            List<float> spaceRadiiFlattened = new List<float>();

            for (int i = 0; i < spaceNames.Count; i++)
            {
                engine.SpaceIds[departmentIds[i]].Add(i);
                engine.SpaceNames[departmentIds[i]].Add(spaceNames[i]);

                List<Triple> spaceCenterList = new List<Triple>();
                engine.CircleBinders.Add(new List<CircleBinder>());
                Triple departmentCenter = departmentCenters[departmentIds[i]];
                for (int j = 0; j < 1; j++)
                //for (int j = 0; j < quantities[i]; j++)
                {
                    double a = 10;
                    Triple spaceCenter = initialPositions == null
                        ? departmentCenter + new Triple(random.NextDouble() * 2.0 * a - a, random.NextDouble() * 2.0 * a - a, 0.01 + random.NextDouble() * 1)
                        : initialPositions[i].ToTriple();
                    float spaceRadius = (float)Math.Sqrt(areas[i] / Math.PI);
                    spaceCenterList.Add(spaceCenter);
                    spaceCentersFlattened.Add(spaceCenter);
                    spaceRadiiFlattened.Add(spaceRadius);

                    CircleBinder circleBinder = new CircleBinder(spaceCenter, spaceRadius, Triple.BasisZ, departmentColors[departmentIds[i]]);
                    engine.CircleBinders[departmentIds[i]].Add(circleBinder);
                    engine.TextBinders.Add(new TextBinder(spaceCenter, i.ToString()));
                    engine.DepartmentCohesionGoals.Add(new LengthGoal(spaceCenter, departmentCenter, 0f));
                }

                spaceCenters.Add(spaceCenterList);
            }

            engine.OnPlaneGoal = new OnPlaneGoal(spaceCentersFlattened, new Triple(0f, 0f, 0.001f), Triple.BasisZ, 1.0f);
            engine.SphereCollisionGoal = new SphereCollisionGoal(spaceCentersFlattened, spaceRadiiFlattened, 0.5f);
            engine.ContainmentGoal = new ConvexPolygonContainmentGoal(spaceCentersFlattened, spaceRadiiFlattened, new List<Triple>(), 1f);


            //===================================================================================
            // Space Adjacency
            //===================================================================================

            HashSet<int> adjacencyKeys = new HashSet<int>();

            for (int i = 0; i < spaceNames.Count; i++)
            {
                foreach (int j in adjacentSpaceIds[i])
                {
                    if (i == j || j >= spaceNames.Count)
                        continue; // Safeguard against some non-sense data from the csv file
                    int adjacencyKey = i < j ? i * spaceNames.Count + j : j * spaceNames.Count + i;
                    if (adjacencyKeys.Contains(adjacencyKey)) continue;
                    adjacencyKeys.Add(adjacencyKey);

                    foreach (Triple startPoint in spaceCenters[i])
                        foreach (Triple endPoint in spaceCenters[j])
                        {
                            engine.SpaceAdjacencyGoals.Add(new LengthGoal(startPoint, endPoint,
                                spaceRadiiFlattened[i] + spaceRadiiFlattened[j], 30f));
                            engine.SpaceAdjacencyLineBinders.Add(new LineBinder(startPoint, endPoint));
                            engine.SpaceAdjI.Add(i);
                            engine.SpaceAdjJ.Add(j);
                            engine.SpaceAdjTargets.Add(spaceRadiiFlattened[i] + spaceRadiiFlattened[j]);
                        }
                }
            }
            //===================================================================================

            engine.SetUp();

            return engine;
        }


        public void ComputeScores()
        {
            SpaceAdjErrors.Clear();
            SpaceAdjErrorRatios.Clear();

            for (int i = 0; i < SpaceAdjacencyGoals.Count; i++)
            {
                float currentDistance = (float)(SpaceAdjacencyGoals[i].GetOutput(Solver.Nodes)[0]);
                SpaceAdjErrors.Add(currentDistance - SpaceAdjTargets[i]);
                SpaceAdjErrorRatios.Add(currentDistance / SpaceAdjTargets[i]);
            }
        }


        public List<List<Circle>> GetSpaceCircles()
        {
            List<List<Circle>> circles = new List<List<Circle>>();

            for (int i = 0; i < CircleBinders.Count; i++)
            {
                circles.Add(new List<Circle>());
                foreach (CircleBinder circleBinder in CircleBinders[i])
                    circles[i].Add((Circle)Solver.GetGeometries(circleBinder)[0]);
            }

            return circles;
        }


        public List<Line> GetSpaceAdjLines()
        {
            List<Line> lines = new List<Line>();
            foreach (LineBinder lineBinder in SpaceAdjacencyLineBinders)
                lines.Add((Line)Solver.GetGeometries(lineBinder)[0]);
            return lines;
        }

        /// <summary>
        /// Run the DynaSpace engine
        /// </summary>
        /// <param name="engine">The DynaSpace engine (from Engine.Create node)</param>
        /// <param name="boundaryVertices">The vertices of the boundary polygon</param>
        /// <param name="silentModeSettings">Only used when you want to run the engine in silent mode. To do this, create a silentModeSettings (using SilentModeSettings.Create node) and input it here</param>
        /// <param name="reset">Reset the engine to the initial state</param>
        /// <param name="execute">Run the engine to iteratively solve the space constraints</param>
        /// <param name="enableManipulation">Enable mouse manipulation of the space bubbles</param>
        /// <param name="showSpaceIds">Display the ID of each space bubble</param>
        /// <param name="showSpaceAdjacency">Display the adjacency indicator lines between the space bubbles</param>
        /// <param name="showSpaceDepartmentAdjacency">Display the adjacency indicator lines between the space bubbles</param>
        /// <param name="settings">You can supply customized settings to fine-tune the engine's execution. To do this, you will need the output from the Settings.Create node</param>
        /// <returns name="departmentNames">The name of each department</returns>
        /// <returns name="spaceIds">The ID of each space bubble</returns>
        /// <returns name="spaceNames">The name of each space bubble </returns>
        /// <returns name="spaceCircles">the circle that represents each space bubble</returns>
        /// <returns name="spaceAdjLines">each line represents a space-space adjacency relationships</returns>
        /// <returns name="spaceAdjErrors">The error for the adjacency requirement between two space bubbles of radii R1 and R2 is measured as "D - (R1 + R2)", where D is the distance between the two bubble's centers. If the adjacency requirement is fully satisfied, the two bubbles should touch each other and hence the error will be 0. If the two bubbles are too far apart, the error will become positive. If the bubbles are too close (in which case they overlap), the error will become negative.</returns>
        /// <returns name="spaceAdjErrorRatios">Similar to space adjacency errors, but measured as fractions rather than distance values. It is defined as "D / (R1 + R2"), where R1 and R2 are the bubbles' radii and D is the distance between the two bubbles' centers. We want the error ratio to be as close to 1.0 as possible. Error larger than 1.0 means the bubbles are too far apart and error smaller than 1.0 means the bubbles are too close and overlapping with each other</returns>
        [MultiReturn(
            "departmentNames",
            "spaceIds",
            "spaceNames",
            "spaceCircles",
            "spaceAdjLines",
            "spaceAdjErrors",
            "spaceAdjErrorRatios")]
        public static Dictionary<string, object> Execute(
           Engine engine,
           [DefaultArgument("null")] List<Point> boundaryVertices,
           [DefaultArgument("null")] SilentModeSettings silentModeSettings,
           [DefaultArgument("true")] bool reset,
           [DefaultArgument("true")] bool execute,
           [DefaultArgument("false")] bool enableManipulation,
           [DefaultArgument("true")] bool showSpaceIds,
           [DefaultArgument("true")] bool showSpaceAdjacency,
           [DefaultArgument("true")] bool showSpaceDepartmentAdjacency,
           [DefaultArgument("null")] Settings settings)
        {
            if (settings == null)
                settings = new Settings()
                {
                    DampingFactor = 0f,
                    Iterations = 0,
                    BoundaryStrength = 1000f,
                    PlanarConstraintStrength = 200,
                    SphereCollisionStrength = 100f,
                    DepartmentCohesionStrength = 1,
                    SpaceAdjacencyStrength = 1,
                    SpaceDepartmentAdjacencyStrength = 0f
                };


            if (silentModeSettings != null)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                engine.Solver.StopBackgroundExecution();
                engine.Solver.ClearRender();

                engine.Solver.Reset();
                engine.Solver.EnableMouseInteraction = enableManipulation;
                engine.Solver.IterationCount = settings.Iterations;
                engine.Solver.DampingFactor = settings.DampingFactor;

                foreach (var binder in engine.TextBinders) binder.Show = showSpaceIds;
                foreach (var binder in engine.SpaceAdjacencyLineBinders) binder.Show = showSpaceAdjacency;
                foreach (var binder in engine.SpaceDepartmentAdjacencyLineBinders)
                    binder.Show = showSpaceDepartmentAdjacency;

                engine.ContainmentGoal.PolygonVertices = boundaryVertices.ToTriples();
                engine.ContainmentGoal.Weight = settings.BoundaryStrength;
                engine.OnPlaneGoal.Weight = settings.PlanarConstraintStrength;

                foreach (var goal in engine.DepartmentCohesionGoals) goal.Weight = settings.DepartmentCohesionStrength;
                foreach (var goal in engine.SpaceAdjacencyGoals) goal.Weight = settings.SpaceAdjacencyStrength;
                foreach (var goal in engine.SpaceDepartmentAdjacencyGoals)
                    goal.Weight = settings.SpaceDepartmentAdjacencyStrength;


                while (engine.Solver.CurrentIteration < silentModeSettings.MaxIterationCount)
                {
                    engine.SphereCollisionGoal.Weight = engine.Solver.CurrentIteration < silentModeSettings.SphereCollisionKickin ? 0f : settings.SphereCollisionStrength;
                    engine.OnPlaneGoal.Weight = engine.Solver.CurrentIteration < silentModeSettings.PlanarConstraintKickin ? 0f : settings.PlanarConstraintStrength;
                    engine.ContainmentGoal.Weight = engine.Solver.CurrentIteration < silentModeSettings.BoundaryKickin ? 0f : settings.BoundaryStrength;
                    engine.Solver.Iterate();

                    if (
                        engine.Solver.GetLargestMove() < silentModeSettings.TerminationThreshold &&
                        engine.Solver.CurrentIteration > silentModeSettings.SphereCollisionKickin &&
                        engine.Solver.CurrentIteration > silentModeSettings.PlanarConstraintKickin &&
                        engine.Solver.CurrentIteration > silentModeSettings.BoundaryKickin)

                        break;
                }

                TimeSpan computationTime = stopwatch.Elapsed;
                stopwatch.Restart();

                engine.ComputeScores();

                return new Dictionary<string, object>
                {
                    {"departmentNames", engine.DepartmentNames},
                    {"spaceIds", engine.SpaceIds},
                    {"spaceNames", engine.SpaceNames},
                    {"spaceCircles", engine.GetSpaceCircles()},
                    {"spaceAdjLines", engine.GetSpaceAdjLines()},
                    {"spaceAdjErrors", engine.SpaceAdjErrors},
                    {"spaceAdjErrorRatios", engine.SpaceAdjErrorRatios},
                };
            }


            if (reset)
            {
                engine.Solver.StopBackgroundExecution();
                engine.Solver.Reset();
                engine.Solver.Render();
            }
            else
            {
                engine.Solver.EnableMouseInteraction = enableManipulation;
                engine.Solver.IterationCount = settings.Iterations;
                engine.Solver.DampingFactor = settings.DampingFactor;

                foreach (var binder in engine.TextBinders) binder.Show = showSpaceIds;
                foreach (var binder in engine.SpaceAdjacencyLineBinders) binder.Show = showSpaceAdjacency;
                foreach (var binder in engine.SpaceDepartmentAdjacencyLineBinders)
                    binder.Show = showSpaceDepartmentAdjacency;

                engine.ContainmentGoal.PolygonVertices = boundaryVertices.ToTriples();
                engine.ContainmentGoal.Weight = settings.BoundaryStrength;
                engine.OnPlaneGoal.Weight = settings.PlanarConstraintStrength;

                engine.SphereCollisionGoal.Weight = settings.SphereCollisionStrength;

                foreach (Goal goal in engine.DepartmentCohesionGoals) goal.Weight = settings.DepartmentCohesionStrength;
                foreach (Goal goal in engine.SpaceAdjacencyGoals) goal.Weight = settings.SpaceAdjacencyStrength;
                foreach (Goal goal in engine.SpaceDepartmentAdjacencyGoals)
                    goal.Weight = settings.SpaceDepartmentAdjacencyStrength;

                if (execute) engine.Solver.StartBackgroundExecution();
                else
                {
                    engine.Solver.StopBackgroundExecution();
                    engine.Solver.ClearRender();
                    engine.ComputeScores();
                }
            }

            return execute
                ? new Dictionary<string, object>
                {
                    {"stats", null},
                    {"departmentNames", null},
                    {"spaceIds", null},
                    {"spaceNames", null},
                    {"spaceCircles", null},
                    {"spaceAdjLines", null},
                    {"spaceAdjErrors", null},
                    {"spaceAdjErrorRatios", null},
                }
                : new Dictionary<string, object>
                {
                    {"stats", null},
                    {"departmentNames", engine.DepartmentNames},
                    {"spaceIds", engine.SpaceIds},
                    {"spaceNames", engine.SpaceNames},
                    {"spaceCircles", engine.GetSpaceCircles()},
                    {"spaceAdjLines", engine.GetSpaceAdjLines()},
                    {"spaceAdjErrors", engine.SpaceAdjErrors},
                    {"spaceAdjErrorRatios", engine.SpaceAdjErrorRatios},
                };
        }

        public void Dispose()
        {
            Solver?.Dispose();
        }
    }
}
