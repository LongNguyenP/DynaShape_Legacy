using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using DynaShape.GeometryBinders;
using DynaShape.Goals;
using SharpDX;

using Point = Autodesk.DesignScript.Geometry.Point;

namespace DynaShape.ZeroTouch
{
    public static class SpacePlanning
    {
        [IsVisibleInDynamoLibrary(false)]
        public class Engine
        {
            internal DynaShape.Solver Solver = new DynaShape.Solver();
            internal List<Goal> Goals = new List<Goal>();
            internal List<GeometryBinder> GeometryBinders = new List<GeometryBinder>();

            internal ConvexPolygonContainmentGoal ContainmentGoal;
            internal OnPlaneGoal OnPlaneGoal;
            internal DirectionGoal GlobalDirectionGoal;
            internal List<AnchorGoal> DepartmentAnchorGoals = new List<AnchorGoal>();
            
            internal SphereCollisionGoal SphereCollisionGoal;

            internal List<LengthGoal> DepartmentCohesionGoals = new List<LengthGoal>();
            internal List<LengthGoal> SpaceAdjacencyGoals = new List<LengthGoal>();
            internal List<LengthGoal> SpaceDepartmentAdjacencyGoals = new List<LengthGoal>();

            internal List<CircleBinder> CircleBinders = new List<CircleBinder>();
            internal List<LineBinder> SpaceAdjacencyLineBinders = new List<LineBinder>();
            internal List<LineBinder> SpaceDepartmentAdjacencyLineBinders = new List<LineBinder>();
            internal List<TextBinder> TextBinders = new List<TextBinder>();

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

                GeometryBinders.AddRange(CircleBinders);
                GeometryBinders.AddRange(SpaceAdjacencyLineBinders);
                GeometryBinders.AddRange(SpaceDepartmentAdjacencyLineBinders);
                GeometryBinders.AddRange(TextBinders);

                Solver = new DynaShape.Solver();
                Solver.AddGoals(Goals);
                Solver.AddGeometryBinders(GeometryBinders);
            }


            public void Reset()
            {
                Solver.StopBackgroundExecution();
                Solver.Clear();
                Solver.AddGoals(Goals);
                Solver.AddGeometryBinders(GeometryBinders);
                Solver.Display.Render();
            }
        }

        public static Engine Create(List<object> data)
        {
            Engine engine = new Engine();

            //===========================================================================
            // Read Depart from spreadsheet
            //===========================================================================

            List<string> departments = new List<string>
            {
                "PSYCHIATRIC INPATIENT",
                "EXAM ROOM",
                "KPU PSY INPATIENT",
                "WORKSTATION",
            };

            //===========================================================================
            // Read spaces from spreadsheet
            //===========================================================================

            List<string> texts = new List<string>();
            foreach (object datum in data) texts.Add(datum?.ToString().Trim());

            List<string> spaceNames = new List<string>();
            List<int> departmentIds = new List<int>();
            List<int> quantities = new List<int>();
            List<double> areas = new List<double>();
            List<double> preferences = new List<double>();
            List<string> programTypes = new List<string>();
            List<List<int>> adjacentSpaceIds = new List<List<int>>();
            List<List<int>> adjacentDepartmentIds = new List<List<int>>();

            int stride = 12;
            for (int i = stride; i < data.Count; i += stride)
            {
                spaceNames.Add(texts[i + 1]);

                int departmentId = -1;
                for (int k = 0; k < departments.Count; k++)
                    if (departments[k] == texts[i + 2])
                        departmentId = k;
                departmentIds.Add(departmentId);

                quantities.Add(int.Parse(texts[i + 4]));
                areas.Add(double.Parse(texts[i + 5]));
                preferences.Add(double.Parse(texts[i + 7]));
                programTypes.Add(texts[i + 8]);

                List<int> adjacentSpaceIds_ = new List<int>();
                if (texts[i + 9] != null)
                {
                    string[] segments = texts[i + 9].Split('.');
                    foreach (string segment in segments) adjacentSpaceIds_.Add(int.Parse(segment));
                }

                adjacentSpaceIds.Add(adjacentSpaceIds_);

                List<int> adjacentDepartmentIds_ = new List<int>();
                if (texts[i + 10] != null)
                {
                    string[] segments = texts[i + 10].Split('.');
                    foreach (string segment in segments) adjacentDepartmentIds_.Add(int.Parse(segment));

                }

                adjacentDepartmentIds.Add(adjacentDepartmentIds_);
            }


            //===================================================================================

            List<Triple> departmentCenters = new List<Triple>();

            for (int i = 0; i < departments.Count; i++)
            {
                double alpha = (double) i / departments.Count * Math.PI * 2.0;
                departmentCenters.Add(20f * new Triple(Math.Cos(alpha), Math.Sin(alpha), 0.0));
                if (i == 0) engine.DepartmentAnchorGoals.Add(new AnchorGoal(departmentCenters.Last(), Triple.Zero, 0.1f));
            }

            engine.SpaceAdjacencyLineBinders.Add(new LineBinder(departmentCenters[0], departmentCenters[1], Color.Orange));

            List<Color> departmentColors = new List<Color>()
            {
                new Color(255, 64, 64),
                new Color(64, 255, 64),
                new Color(200, 200, 0),
                new Color(64, 64, 255),
            };

            Random random = new Random(0);

            //===================================================================================

            List<List<Triple>> spaceCentersStructured = new List<List<Triple>>();

            List<Triple> spaceCenters = new List<Triple>();
            List<float> spaceRadii = new List<float>();

            for (int i = 0; i < spaceNames.Count; i++)
            {
                List<Triple> spaceCenterList = new List<Triple>();

                Triple departmentCenter = departmentCenters[departmentIds[i]];
                for (int j = 0; j < 1; j++)
                    //for (int j = 0; j < quantities[i]; j++)
                {
                    double a =10;
                    Triple spaceCenter = departmentCenter + new Triple(random.NextDouble() * 2.0 * a - a, random.NextDouble() * 2.0 * a - a, 0.0);
                    float spaceRadius = (float) Math.Sqrt(areas[i] / Math.PI);
                    spaceCenterList.Add(spaceCenter);
                    spaceCenters.Add(spaceCenter);
                    spaceRadii.Add(spaceRadius);

                    engine.CircleBinders.Add(new CircleBinder(spaceCenter, spaceRadius, Triple.BasisZ, departmentColors[departmentIds[i]]));
                    engine.TextBinders.Add(new TextBinder(spaceCenter, i.ToString()));
                    engine.DepartmentCohesionGoals.Add(new LengthGoal(spaceCenter, departmentCenter, 0f));
                }

                spaceCentersStructured.Add(spaceCenterList);
            }

            engine.OnPlaneGoal = new OnPlaneGoal(spaceCenters, new Triple(0f, 0f, 0.0001f), Triple.BasisZ, 1.0f);
            engine.GlobalDirectionGoal = new DirectionGoal(departmentCenters[0], departmentCenters[1], Triple.BasisX, 1.0f);
            engine.SphereCollisionGoal = new SphereCollisionGoal(spaceCenters, spaceRadii, 0.5f);
            engine.ContainmentGoal = new ConvexPolygonContainmentGoal(spaceCenters, spaceRadii, new List<Triple>(), 1f);

            //===================================================================================
            // Space Adjacency
            //===================================================================================

            HashSet<int> adjacencyKeys = new HashSet<int>();

            for (int i = 0; i < spaceNames.Count; i++)
            {
                foreach (int j in adjacentSpaceIds[i])
                {
                    if (i == j || j >= spaceNames.Count) continue; // Safeguard against some non-sense data from the csv file
                    int adjacencyKey = i < j ? i * spaceNames.Count + j : j * spaceNames.Count + i;
                    if (adjacencyKeys.Contains(adjacencyKey)) continue;
                    adjacencyKeys.Add(adjacencyKey);

                    foreach (Triple startPoint in spaceCentersStructured[i])
                    foreach (Triple endPoint in spaceCentersStructured[j])
                    {
                        engine.SpaceAdjacencyGoals.Add(new LengthGoal(startPoint, endPoint, spaceRadii[i] + spaceRadii[j], 30f));
                        engine.SpaceAdjacencyLineBinders.Add(new LineBinder(startPoint, endPoint));
                    }
                }
            }
            //===================================================================================

            engine.SetUp();

            return engine;
        }


        [MultiReturn("info", "nodePositions", "geometries")]

        public static Dictionary<string, object> Execute(
            Engine engine,
            [DefaultArgument("0")] int iterations,
            [DefaultArgument("true")] bool reset,
            [DefaultArgument("true")] bool execute,
            [DefaultArgument("true")] bool enableMomentum,
            [DefaultArgument("true")] bool enableFastDisplay,
            [DefaultArgument("false")] bool enableManipulation,
            [DefaultArgument("0.95")] float dampingFactor,
            List<Point> BoundaryVertices,
            [DefaultArgument("1.0")] float BoundaryStrength,
            [DefaultArgument("1.0")] float globalPositioningStrength,
            [DefaultArgument("30.0")] float sphereCollisionStrength,
            [DefaultArgument("0.02")] float departmentCohesionStrength,
            [DefaultArgument("0.1")] float spaceAdjacencyStrength,
            [DefaultArgument("0.1")] float spaceDepartmentAdjacencyStrength,
            [DefaultArgument("true")] bool showSpaceNames,
            [DefaultArgument("true")] bool showSpaceAdjacency,
            [DefaultArgument("true")] bool showSpaceDepartmentAdjacency)
        {
#if CLI
            if (!execute)
                new Dictionary<string, object> {
                    { "info", null },
                    { "nodePositions", null },
                    { "geometries", null } };


#else
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (reset)
            {
                engine.Reset();
            }
            else
            {
                if (engine.Solver.Goals.Count == 0) engine.Solver.Reset();

                engine.Solver.EnableMouseInteraction = enableManipulation;
                engine.Solver.EnableMomentum = enableMomentum;
                engine.Solver.EnableFastDisplay = enableFastDisplay;
                engine.Solver.IterationCount = iterations;
                engine.Solver.DampingFactor = dampingFactor;
                
                foreach (var binder in engine.TextBinders) binder.Show = showSpaceNames;
                foreach (var binder in engine.SpaceAdjacencyLineBinders) binder.Show = showSpaceAdjacency;
                foreach (var binder in engine.SpaceDepartmentAdjacencyLineBinders) binder.Show = showSpaceDepartmentAdjacency;

                engine.ContainmentGoal.PolygonVertices = BoundaryVertices.ToTriples();
                engine.ContainmentGoal.Weight = BoundaryStrength;
                engine.OnPlaneGoal.Weight = globalPositioningStrength;
                engine.GlobalDirectionGoal.Weight = globalPositioningStrength * 50f;
                foreach (Goal goal in engine.DepartmentAnchorGoals) goal.Weight = globalPositioningStrength;

                engine.SphereCollisionGoal.Weight = sphereCollisionStrength;

                foreach (Goal goal in engine.DepartmentCohesionGoals) goal.Weight = departmentCohesionStrength;
                foreach (Goal goal in engine.SpaceAdjacencyGoals) goal.Weight = spaceAdjacencyStrength;
                foreach (Goal goal in engine.SpaceDepartmentAdjacencyGoals) goal.Weight = spaceDepartmentAdjacencyStrength;

                if (execute) engine.Solver.StartBackgroundExecution();
                else
                {
                    engine.Solver.StopBackgroundExecution();
                    if (!enableFastDisplay) engine.Solver.Iterate();
                }
            }

            return enableFastDisplay
                ? new Dictionary<string, object> {
                    { "info", null },
                    { "nodePositions", null },
                    { "geometries", null } }
                : new Dictionary<string, object> {
                    { "info", null },
                    { "nodePositions", engine.Solver.GetNodePositionsAsPoints() },
                    { "geometries", engine.Solver.GetGeometries() } };
#endif
        }
    }
}
