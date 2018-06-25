using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Autodesk.DesignScript.Runtime;
using Dynamo.Wpf.ViewModels.Watch3D;
using DynaShape.Goals;
using DynaShape.GeometryBinders;

using Point = Autodesk.DesignScript.Geometry.Point;
using Vector = Autodesk.DesignScript.Geometry.Vector;


namespace DynaShape
{
    [IsVisibleInDynamoLibrary(false)]
    public class Solver : IDisposable
    {
        public bool EnableMouseInteraction = true;
        public bool EnableMomentum = true;
        public bool EnableFastDisplay = true;

        public List<Node> Nodes = new List<Node>();
        public List<Goal> Goals = new List<Goal>();
        public List<GeometryBinder> GeometryBinders = new List<GeometryBinder>();
        
        internal int HandleNodeIndex = -1;
        internal int NearestNodeIndex = -1;

        internal DynaShapeDisplay Display;

        private Task backgroundExecutionTask;

        public int IterationCount = 0;

        public int CurrentIteration { get; private set; }

        public readonly DateTime TimeCreated;

        public Solver()
        {
            TimeCreated = DateTime.Now;

            if (DynaShapeViewExtension.ViewModel == null) throw new Exception("Oh no, DynaShape could not get access to the Helix ViewModel. Sad!");

            CurrentIteration = 0;
            Display = new DynaShapeDisplay(this);

            DynaShapeViewExtension.ViewModel.ViewMouseDown += ViewportMouseDownHandler;
            DynaShapeViewExtension.ViewModel.ViewMouseUp += ViewportMouseUpHandler;
            DynaShapeViewExtension.ViewModel.ViewMouseMove += ViewportMouseMoveHandler;
            DynaShapeViewExtension.ViewModel.ViewCameraChanged += ViewportCameraChangedHandler;
            DynaShapeViewExtension.ViewModel.CanNavigateBackgroundPropertyChanged += ViewportCanNavigateBackgroundPropertyChangedHandler;
        }

        CancellationTokenSource ctSource;

        private void BackgroundExecutionAction()
        {
            while (!ctSource.Token.IsCancellationRequested)
            {
                // Even when the workspace is closed, the background task still runs
                // Therefore we need to check for this case and terminate the while loop, so that the task can completed
                if (!DynaShapeViewExtension.Parameters.CurrentWorkspaceModel.Nodes.Any())
                {
                    Dispose();
                    break;
                }

                if (IterationCount > 0) Iterate(IterationCount);
                else Iterate(25.0);

                if (EnableFastDisplay) Display.Render();

            }
        }

        public void StartBackgroundExecution()
        {
            if (backgroundExecutionTask != null && backgroundExecutionTask.Status == TaskStatus.Running) return;
            ctSource = new CancellationTokenSource();
            backgroundExecutionTask = Task.Factory.StartNew(BackgroundExecutionAction, ctSource.Token);  
        }

        public void StopBackgroundExecution()
        {
            if (backgroundExecutionTask == null) return;
            ctSource?.Cancel();
            backgroundExecutionTask?.Wait(300);
            Display.DispatcherOperation?.Task.Wait();
        }

        public void AddGoals(IEnumerable<Goal> goals, double nodeMergeThreshold = 0.0001)
        {
            foreach (Goal goal in goals)
                AddGoal(goal, nodeMergeThreshold);
        }

        public void AddGeometryBinders(IEnumerable<GeometryBinder> geometryBinders, double nodeMergeThreshold = 0.0001)
        {
            foreach (GeometryBinder geometryBinder in geometryBinders)
                AddGeometryBinder(geometryBinder, nodeMergeThreshold);
        }

        public void AddGoal(Goal goal, double nodeMergeThreshold = 0.0001)
        {
            if (goal == null) return;

            Goals.Add(goal);

            if (goal.StartingPositions == null && goal.NodeIndices != null) return;

            goal.NodeIndices = new int[goal.NodeCount];

            for (int i = 0; i < goal.NodeCount; i++)
            {
                bool nodeAlreadyExist = false;

                for (int j = 0; j < Nodes.Count; j++)
                    if ((goal.StartingPositions[i] - Nodes[j].Position).LengthSquared <
                        nodeMergeThreshold * nodeMergeThreshold)
                    {
                        goal.NodeIndices[i] = j;
                        nodeAlreadyExist = true;
                        break;
                    }

                if (!nodeAlreadyExist)
                {
                    Nodes.Add(new Node(goal.StartingPositions[i]));
                    goal.NodeIndices[i] = Nodes.Count - 1;
                }
            }
        }

        public void AddGeometryBinder(GeometryBinder geometryBinder, double nodeMergeThreshold = 0.0001)
        {
            GeometryBinders.Add(geometryBinder);

            if (geometryBinder.StartingPositions == null && geometryBinder.NodeIndices != null) return;

            geometryBinder.NodeIndices = new int[geometryBinder.NodeCount];

            for (int i = 0; i < geometryBinder.NodeCount; i++)
            {
                bool nodeAlreadyExist = false;

                for (int j = 0; j < Nodes.Count; j++)
                    if ((geometryBinder.StartingPositions[i] - Nodes[j].Position).LengthSquared <
                        nodeMergeThreshold * nodeMergeThreshold)
                    {
                        geometryBinder.NodeIndices[i] = j;
                        nodeAlreadyExist = true;
                        break;
                    }

                if (!nodeAlreadyExist)
                {
                    Nodes.Add(new Node(geometryBinder.StartingPositions[i]));
                    geometryBinder.NodeIndices[i] = Nodes.Count - 1;
                }
            }
        }

        public List<Triple> GetNodePositions()
        {
            List<Triple> nodePositions = new List<Triple>(Nodes.Count);
            for (int i = 0; i < Nodes.Count; i++)
                nodePositions.Add(Nodes[i].Position);
            return nodePositions;
        }

        public List<Point> GetNodePositionsAsPoints()
        {
            List<Point> nodePositions = new List<Point>(Nodes.Count);
            for (int i = 0; i < Nodes.Count; i++)
                nodePositions.Add(Nodes[i].Position.ToPoint());
            return nodePositions;
        }

        public List<List<Triple>> GetStructuredNodePositions()
        {
            List<List<Triple>> nodePositions = new List<List<Triple>>(Goals.Count);
            for (int i = 0; i < Goals.Count; i++)
            {
                List<Triple> goalNodePositions = new List<Triple>(Goals[i].NodeCount);
                for (int j = 0; j < Goals[i].NodeCount; j++)
                    goalNodePositions.Add(Nodes[Goals[i].NodeIndices[j]].Position);
                nodePositions.Add(goalNodePositions);
            }
            return nodePositions;
        }

        public List<List<Point>> GetStructuredNodePositionsAsPoints()
        {
            List<List<Point>> nodePositions = new List<List<Point>>(Goals.Count);
            for (int i = 0; i < Goals.Count; i++)
            {
                List<Point> goalNodePositions = new List<Point>(Goals[i].NodeCount);
                for (int j = 0; j < Goals[i].NodeCount; j++)
                    goalNodePositions.Add(Nodes[Goals[i].NodeIndices[j]].Position.ToPoint());
                nodePositions.Add(goalNodePositions);
            }
            return nodePositions;
        }

        public List<Triple> GetNodeVelocities()
        {
            List<Triple> nodeVelocities = new List<Triple>(Nodes.Count);
            for (int i = 0; i < Nodes.Count; i++)
                nodeVelocities.Add(Nodes[i].Velocity);
            return nodeVelocities;
        }

        public List<Vector> GetNodeVelocitiesAsVectors()
        {
            List<Vector> nodeVelocities = new List<Vector>(Nodes.Count);
            for (int i = 0; i < Nodes.Count; i++)
                nodeVelocities.Add(Nodes[i].Velocity.ToVector());
            return nodeVelocities;
        }

        public List<List<object>> GetGeometries()
        {
            List<List<object>> geometries = new List<List<object>>(GeometryBinders.Count);
            for (int i = 0; i < GeometryBinders.Count; i++)
                geometries.Add(GeometryBinders[i].CreateGeometryObjects(Nodes));
            return geometries;
        }

        public List<List<object>> GetGoalOutputs()
        {
            List<List<object>> goalOutputs = new List<List<object>>(Goals.Count);
            for (int i = 0; i < Goals.Count; i++)
                goalOutputs.Add(Goals[i].GetOutput(Nodes));
            return goalOutputs;
        }

        public void Clear()
        {
            Nodes.Clear();
            Goals.Clear();
            GeometryBinders.Clear();
        }

        public void Reset()
        {
            foreach (Node node in Nodes) node.Reset();
        }

        public void Iterate()
        {
            CurrentIteration++;

            //=================================================================================
            // Apply momentum
            //=================================================================================

            if (EnableMomentum)
                foreach (Node node in Nodes)
                    node.Position += node.Velocity;

            //=================================================================================
            // Process each goal indepently, in parallel
            //=================================================================================

            Parallel.ForEach(Goals, goal => goal.Compute(Nodes));

            //=================================================================================
            // Compute the total move vector that acts on each node
            //=================================================================================

            Triple[] nodeMoveSums = new Triple[Nodes.Count];
            float[] nodeWeightSums = new float[Nodes.Count];

            for (int j = 0; j < Goals.Count; j++)
            {
                Goal goal = Goals[j];
                for (int i = 0; i < goal.NodeCount; i++)
                {
                    nodeMoveSums[goal.NodeIndices[i]] += goal.Moves[i] * goal.Weights[i];
                    nodeWeightSums[goal.NodeIndices[i]] += goal.Weights[i];
                }
            }

            //=================================================================================
            // Move the manipulated node toward the mouse ray
            //=================================================================================

            if (HandleNodeIndex != -1)
            {
                float manipulationWeight = 30f;
                nodeWeightSums[HandleNodeIndex] += manipulationWeight;

                Triple v = Nodes[HandleNodeIndex].Position - DynaShapeViewExtension.MouseRayOrigin;
                Triple mouseRayPull = v.Dot(DynaShapeViewExtension.MouseRayDirection) * DynaShapeViewExtension.MouseRayDirection - v;
                nodeMoveSums[HandleNodeIndex] += manipulationWeight * mouseRayPull;
            }

            //=============================================================================================
            // Move the nodes to their new positions
            //=============================================================================================

            if (EnableMomentum)
                for (int i = 0; i < Nodes.Count; i++)
                {
                    if (nodeWeightSums[i] == 0f) continue;
                    Triple move = nodeMoveSums[i] / nodeWeightSums[i];
                    Nodes[i].Position += move;
                    Nodes[i].Velocity += move;
                    if (Nodes[i].Velocity.Dot(move) < 0.0)
                        Nodes[i].Velocity *= 0.95f;
                }
            else
                for (int i = 0; i < Nodes.Count; i++)
                {
                    if (nodeWeightSums[i] == 0f) continue;
                    Triple move = nodeMoveSums[i] / nodeWeightSums[i];
                    Nodes[i].Position += move;
                    Nodes[i].Velocity = Triple.Zero;
                }
        }

        public void Iterate(int iterationCount)
        {
            for (int i = 0; i < iterationCount; i++) Iterate();
        }

        public void Iterate(double miliseconds)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed.TotalMilliseconds < miliseconds)
                Iterate();
        }

        public void Execute(int maxIterationCount, double keThreshold)
        {
            while (CurrentIteration < maxIterationCount)
            {
                Iterate();
                if (GetKineticEnergy() < keThreshold) break;
            }
        }

        public double GetKineticEnergy()
        {
            double ke = 0.0;

            for (int i = 0; i < Nodes.Count; i++)
                ke += Nodes[i].Velocity.LengthSquared;

            return ke;
        }

        private void ViewportCameraChangedHandler(object sender, RoutedEventArgs args)
        {
            NearestNodeIndex = -1;
        }

        private void ViewportMouseDownHandler(object sender, MouseButtonEventArgs args)
        {
            if (args.LeftButton == MouseButtonState.Pressed && EnableMouseInteraction)
                HandleNodeIndex = FindNearestNodeIndex();
        }

        private void ViewportMouseUpHandler(object sender, MouseButtonEventArgs args)
        {
            HandleNodeIndex = -1;
            NearestNodeIndex = -1;
        }

        private void ViewportMouseMoveHandler(object sender, MouseEventArgs args)
        {
            if (!EnableMouseInteraction) return;
            if (args.LeftButton == MouseButtonState.Released) HandleNodeIndex = -1;
            NearestNodeIndex = FindNearestNodeIndex();
        }

        internal int FindNearestNodeIndex(float range = 0.03f)
        {
            CameraData cameraData = DynaShapeViewExtension.CameraData;

            Triple camZ = new Triple( cameraData.LookDirection.X,
                                     -cameraData.LookDirection.Z,
                                      cameraData.LookDirection.Y).Normalise();

            Triple camY = new Triple( cameraData.UpDirection.X,
                                     -cameraData.UpDirection.Z,
                                      cameraData.UpDirection.Y).Normalise();

            Triple camX = camY.Cross(camZ).Normalise();

            Triple mousePosition2D = new Triple(DynaShapeViewExtension.MouseRayDirection.Dot(camX),
                                                DynaShapeViewExtension.MouseRayDirection.Dot(camY),
                                                DynaShapeViewExtension.MouseRayDirection.Dot(camZ));

            mousePosition2D /= mousePosition2D.Z;

            int nearestNodeIndex = -1;

            float minDistSquared = range * range;

            for (int i = 0; i < Nodes.Count; i++)
            {
                Triple v = Nodes[i].Position - DynaShapeViewExtension.MouseRayOrigin;
                v = new Triple(v.Dot(camX), v.Dot(camY), v.Dot(camZ));
                Triple nodePosition2D = v / v.Z;

                float distSquared = (mousePosition2D - nodePosition2D).LengthSquared;

                if (distSquared < minDistSquared)
                {
                    minDistSquared = distSquared;
                    nearestNodeIndex = i;
                }
            }

            return nearestNodeIndex;
        }

        private void ViewportCanNavigateBackgroundPropertyChangedHandler(bool canNavigate)
        {
            HandleNodeIndex = -1;
            NearestNodeIndex = -1;
        }

        public void Dispose()
        {
            StopBackgroundExecution();
            Clear();
            DynaShapeViewExtension.ViewModel.ViewMouseDown -= ViewportMouseDownHandler;
            DynaShapeViewExtension.ViewModel.ViewMouseUp -= ViewportMouseUpHandler;
            DynaShapeViewExtension.ViewModel.ViewMouseMove -= ViewportMouseMoveHandler;
            DynaShapeViewExtension.ViewModel.ViewCameraChanged -= ViewportCameraChangedHandler;
            DynaShapeViewExtension.ViewModel.CanNavigateBackgroundPropertyChanged -= ViewportCanNavigateBackgroundPropertyChangedHandler;
            Display.Dispose();
        }
    }
}
