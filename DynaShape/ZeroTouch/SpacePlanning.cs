using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.DesignScript.Geometry;
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
        public class SilentModeSettings
        {
            public int MaxIterationCount;
            public float TerminationThreshold;
            public int SphereCollisionKickin;
            public int PlanarConstraintKickin;
        }

        public static SilentModeSettings CreateSilentModeSettings(int maxIterationCount, float terminationThreshold, int sphereCollisionKickin = 2500, int planarConstraintKickin = 5000)
        {
            return new SilentModeSettings()
            {
                MaxIterationCount =  maxIterationCount,
                TerminationThreshold = terminationThreshold,
                SphereCollisionKickin = sphereCollisionKickin,
                PlanarConstraintKickin = planarConstraintKickin
            };
        }


        [MultiReturn(
          "SpaceName",
          "Department",
          "DepartmentId",
          "Quantity",
          "Width",
          "Height",
          "Area",
          "Preference",
          "AdjacentSpaces",
          "AdjacentDepartments")]
        public static Dictionary<string, object> ParseData(List<object> data)
        {
            List<string> spaceNames = new List<string>();
            List<string> departments = new List<string>();
            List<int> departmentIds = new List<int>();
            List<int> quantities = new List<int>();
            List<double> widths = new List<double>();
            List<double> heights = new List<double>();
            List<double> areas = new List<double>();
            List<int> preferences = new List<int>();
            List<List<int>> adjacentSpaces = new List<List<int>>();
            List<List<int>> adjacentDepartments = new List<List<int>>();

            int stride = 12;
            int sheetHeight = data.Count / stride - 1;

            Dictionary<string, int> cols = new Dictionary<string, int>();

            List<string> columnNames = new List<string>
            {
                "SPACE ID",
                "SPACE NAME",
                "DEPARTMENT",
                "DEPARTMENT ID",
                "QUANTITY",
                "WIDTH",
                "HEIGHT",
                "AREA",
                "PREFERENCE",
                "ADJACENT SPACES",
                "ADJACENT DEPARTMENTS"
            };

            for (int i = 0; i < stride; i++)
                foreach (string columnName in columnNames)
                    if (data[i].ToString() == columnName)
                        cols.Add(columnName, i);
       

            for (int j = stride; j < data.Count; j += stride)
            {
                spaceNames.Add(data[j + cols["SPACE NAME"]].ToString());
                departments.Add(data[j + cols["DEPARTMENT"]].ToString());
                departmentIds.Add(int.Parse(data[j + cols["DEPARTMENT ID"]].ToString()));
                quantities.Add(int.Parse(data[j + cols["QUANTITY"]].ToString()));
                if (data[j + cols["WIDTH"]] != null) widths.Add(double.Parse(data[j + cols["WIDTH"]].ToString()));
                if (data[j + cols["HEIGHT"]] != null) heights.Add(double.Parse(data[j + cols["HEIGHT"]].ToString()));
                if (data[j + cols["AREA"]] != null) areas.Add(double.Parse(data[j + cols["AREA"]].ToString()));
                if (data[j + cols["PREFERENCE"]] != null) areas.Add(double.Parse(data[j + cols["PREFERENCE"]].ToString()));

                adjacentSpaces.Add(new List<int>());
                object raw = data[j + cols["ADJACENT SPACES"]];
                if (raw != null)
                {
                    string[] segments = raw.ToString().Split('.', ';');
                    foreach (string segment in segments)
                        adjacentSpaces.Last().Add(int.Parse(segment));
                }

                adjacentDepartments.Add(new List<int>());
                raw = data[j + cols["ADJACENT DEPARTMENTS"]];
                if (raw != null)
                {
                    string[] segments = raw.ToString().Split('.', ';');
                    foreach (string segment in segments)
                        adjacentDepartments.Last().Add(int.Parse(segment));
                }

                // Clean up inconsitencies and redundancies in ADJACENT SPACES

                for (int i = 0; i < )
            }

            return new Dictionary<string, object>
               {
                    {"SpaceName", spaceNames},
                    {"Department", departments},
                    {"DepartmentId", departmentIds},
                    {"Quantity", quantities},
                    {"Width", widths},
                    {"Height", heights},
                    {"Area", areas},
                    {"Preference", preferences},
                    {"AdjacentSpaces", adjacentSpaces},
                    {"AdjacentDepartments", adjacentDepartments}
               };
        }


      [IsVisibleInDynamoLibrary(false)]
        public class Engine
        {
            internal DynaShape.Solver Solver = new DynaShape.Solver();
            internal List<Goal> Goals = new List<Goal>();
            internal List<GeometryBinder> GeometryBinders = new List<GeometryBinder>();

            internal List<string> DepartmentNames = new List<string>();
            internal List<List<string>> SpaceNames = new List<List<string>>();
            internal List<List<int>> SpaceIds = new List<List<int>>();

            internal ConvexPolygonContainmentGoal ContainmentGoal;
            internal OnPlaneGoal OnPlaneGoal;
            internal DirectionGoal GlobalDirectionGoal;
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


            public void ComputeScores()
            {
                SpaceAdjErrors.Clear();
                SpaceAdjErrorRatios.Clear();

                for (int i = 0; i < SpaceAdjacencyGoals.Count; i++)
                {
                    float currentDistance = (float) (SpaceAdjacencyGoals[i].GetOutput(Solver.Nodes)[0]);
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
                    return circles;
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
        }
        
        public static Engine Create(List<object> data, float dummy)
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

            List<Color> departmentColors = new List<Color>();

            for (int i = 0; i < engine.DepartmentNames.Count; i++)
                departmentColors.Add(Util.ColorFromHSL((float)i / (float)engine.DepartmentNames.Count, 1.0f, 0.4f).ToSharpDXColor());


            //===========================================================================
            // Read spaces 
            //===========================================================================

            List<string> spaceNames = new List<string>();
            List<int> departmentIds = new List<int>();
            List<int> quantities = new List<int>();
            List<double> areas = new List<double>();
            List<double> preferences = new List<double>();
            List<string> programTypes = new List<string>();
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

            for (int i = 0; i < engine.DepartmentNames.Count; i++)
            {
                double alpha = (double) i / engine.DepartmentNames.Count * Math.PI * 2.0;
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
                    Triple spaceCenter = departmentCenter + new Triple(random.NextDouble() * 2.0 * a - a, random.NextDouble() * 2.0 * a - a, 0.01 + random.NextDouble() * 1);
                    float spaceRadius = (float) Math.Sqrt(areas[i] / Math.PI);
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
            engine.GlobalDirectionGoal = new DirectionGoal(departmentCenters[0], departmentCenters[1], Triple.BasisX, 1.0f);
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


        [MultiReturn(
            "stats", 
            "departmentNames",
            "spaceIds",
            "spaceNames",
            "spaceCircles",
            "spaceAdjLines",
            "spaceAdjErrors", 
            "spaceAdjErrorRatios")]

        public static Dictionary<string, object> Execute(

            Engine engine,
            [DefaultArgument("null")] SilentModeSettings silentModeSettings,

            [DefaultArgument("true")] bool reset,
            [DefaultArgument("true")] bool execute,
            [DefaultArgument("false")] bool enableManipulation,
            [DefaultArgument("0")] int iterations,

            [DefaultArgument("0.95")] float dampingFactor,
            List<Point> boundaryVertices,
            [DefaultArgument("1.0")] float boundaryStrength,
            [DefaultArgument("1.0")] float planarConstraintStrength,
            [DefaultArgument("30.0")] float sphereCollisionStrength,
            [DefaultArgument("0.02")] float departmentCohesionStrength,
            [DefaultArgument("0.1")] float spaceAdjacencyStrength,
            [DefaultArgument("0.1")] float spaceDepartmentAdjacencyStrength,
            [DefaultArgument("true")] bool showSpaceNames,
            [DefaultArgument("true")] bool showSpaceAdjacency,
            [DefaultArgument("true")] bool showSpaceDepartmentAdjacency,
            
            List<object> data)
        {
            if (silentModeSettings != null)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

#if CLI == false
                engine.Solver.StopBackgroundExecution();
                engine.Solver.Display.ClearRender();
#endif

                engine = SpacePlanning.Create(data, 2f);

                engine.Solver.Reset();
                engine.Solver.EnableMouseInteraction = enableManipulation;
                engine.Solver.IterationCount = iterations;
                engine.Solver.DampingFactor = dampingFactor;

                foreach (var binder in engine.TextBinders) binder.Show = showSpaceNames;
                foreach (var binder in engine.SpaceAdjacencyLineBinders) binder.Show = showSpaceAdjacency;
                foreach (var binder in engine.SpaceDepartmentAdjacencyLineBinders)
                    binder.Show = showSpaceDepartmentAdjacency;

                engine.ContainmentGoal.PolygonVertices = boundaryVertices.ToTriples();
                engine.ContainmentGoal.Weight = boundaryStrength;
                engine.OnPlaneGoal.Weight = planarConstraintStrength;

                foreach (var goal in engine.DepartmentCohesionGoals) goal.Weight = departmentCohesionStrength;
                foreach (var goal in engine.SpaceAdjacencyGoals) goal.Weight = spaceAdjacencyStrength;
                foreach (var goal in engine.SpaceDepartmentAdjacencyGoals)
                    goal.Weight = spaceDepartmentAdjacencyStrength;


                while (engine.Solver.CurrentIteration < silentModeSettings.MaxIterationCount)
                {
                    engine.SphereCollisionGoal.Weight = engine.Solver.CurrentIteration < silentModeSettings.SphereCollisionKickin ? 0 : sphereCollisionStrength;
                    engine.OnPlaneGoal.Weight = engine.Solver.CurrentIteration < silentModeSettings.PlanarConstraintKickin ? 0 : planarConstraintStrength;
                    engine.Solver.Iterate();

                    if (
                        engine.Solver.GetLargestMove() < silentModeSettings.TerminationThreshold &&
                        engine.Solver.CurrentIteration > silentModeSettings.SphereCollisionKickin &&
                        engine.Solver.CurrentIteration > silentModeSettings.PlanarConstraintKickin)

                        break;
                }

                TimeSpan computationTime = stopwatch.Elapsed;
                stopwatch.Restart();

                engine.ComputeScores();

                return new Dictionary<string, object>
                {
                    {
                        "stats", String.Concat(
                            "Computation Time: " + computationTime,
                            "\nData Output Time: " + stopwatch.Elapsed,
                            "\nIterations Used : " + engine.Solver.CurrentIteration,
                            "\nLargest Movement: " + engine.Solver.GetLargestMove())
                    },
                    {"departmentNames", engine.DepartmentNames},
                    {"spaceIds", engine.SpaceIds},
                    {"spaceNames", engine.SpaceNames},
                    {"spaceCircles", engine.GetSpaceCircles()},
                    {"spaceAdjLines", engine.GetSpaceAdjLines()},
                    {"spaceAdjErrors", engine.SpaceAdjErrors},
                    {"spaceAdjErrorRatios", engine.SpaceAdjErrorRatios},
                };
            }

#if CLI
            throw new Exception("You are currently running the CLI-Compatible version of DynaShape, which only supports silent execution mode");
#else

            if (reset)
            {
                engine.Solver.StopBackgroundExecution();
                engine.Solver.Reset();
                engine.Solver.Display.Render();
            }
            else
            {
                engine.Solver.EnableMouseInteraction = enableManipulation;
                engine.Solver.IterationCount = iterations;
                engine.Solver.DampingFactor = dampingFactor;

                foreach (var binder in engine.TextBinders) binder.Show = showSpaceNames;
                foreach (var binder in engine.SpaceAdjacencyLineBinders) binder.Show = showSpaceAdjacency;
                foreach (var binder in engine.SpaceDepartmentAdjacencyLineBinders)
                    binder.Show = showSpaceDepartmentAdjacency;

                engine.ContainmentGoal.PolygonVertices = boundaryVertices.ToTriples();
                engine.ContainmentGoal.Weight = boundaryStrength;
                engine.OnPlaneGoal.Weight = planarConstraintStrength;

                engine.SphereCollisionGoal.Weight = sphereCollisionStrength;

                foreach (Goal goal in engine.DepartmentCohesionGoals) goal.Weight = departmentCohesionStrength;
                foreach (Goal goal in engine.SpaceAdjacencyGoals) goal.Weight = spaceAdjacencyStrength;
                foreach (Goal goal in engine.SpaceDepartmentAdjacencyGoals)
                    goal.Weight = spaceDepartmentAdjacencyStrength;

                if (execute) engine.Solver.StartBackgroundExecution();
                else
                {
                    engine.Solver.StopBackgroundExecution();
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
#endif
        }
    }
}
