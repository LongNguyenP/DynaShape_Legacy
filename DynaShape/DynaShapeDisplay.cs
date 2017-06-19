using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Dynamo.Controls;
using Dynamo.Wpf.ViewModels.Watch3D;
using DynaShape.GeometryBinders;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using Geometry3D = HelixToolkit.Wpf.SharpDX.Geometry3D;
using MeshGeometry3D = HelixToolkit.Wpf.SharpDX.MeshGeometry3D;
using Model3D = HelixToolkit.Wpf.SharpDX.Model3D;


namespace DynaShape
{
    [IsVisibleInDynamoLibrary(false)]
    public class DynaShapeDisplay : IDisposable
    {
        public static Color DefaultPointColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        public static Color DefaultLineColor = new Color(0.3f, 0.7f, 0.8f, 1f);
        public static Color DefaultMeshFaceColor = new Color(0, 0.7f, 1f, 0.3f);

        public static DebugDashboard debugDashboard = new DebugDashboard();


        private readonly Solver solver;

        private PointGeometryModel3D pointModel;
        private LineGeometryModel3D lineModel;
        public MeshGeometryModel3D meshModel;

        private PointGeometry3D pointGeometry;
        private LineGeometry3D lineGeometry;
        public MeshGeometry3D meshGeometry;

        //private List<MeshGeometryModel3D> meshModels;

        internal DispatcherOperation DispatcherOperation;

        public DynaShapeDisplay(Solver solver)
        {
            this.solver = solver;

            DynaShapeViewExtension.DynamoWindow.Dispatcher.Invoke(
                () =>
                {
                    pointModel = new PointGeometryModel3D
                    {
                        Size = new Size(5, 5),
                        Figure = PointGeometryModel3D.PointFigure.Ellipse,
                        Color = Color.White,
                    };


                    lineModel = new LineGeometryModel3D
                    {
                        Thickness = 0.5,
                        Color = Color.White,
                    };


                    meshModel = new MeshGeometryModel3D
                    {
                        Material = new PhongMaterial
                        {
                            DiffuseColor = DynaShapeDisplay.DefaultMeshFaceColor
                        }
                    };
                },   
                DispatcherPriority.Send);

            DynaShapeViewExtension.ViewModel.RequestViewRefresh += RequestViewRefreshHandler;

            DispatcherOperation = DynaShapeViewExtension.DynamoWindow.Dispatcher.InvokeAsync(() => { });
            while (DispatcherOperation.Status != DispatcherOperationStatus.Completed) { }
        }


        public void DrawLine(Triple start, Triple end, Color4 color)
        {
            lineGeometry.Positions.Add(new Vector3(start.X, start.Z, -start.Y));
            lineGeometry.Positions.Add(new Vector3(end.X, end.Z, -end.Y));
            lineGeometry.Indices.Add(lineGeometry.Indices.Count);
            lineGeometry.Indices.Add(lineGeometry.Indices.Count);
            lineGeometry.Colors.Add(color);
            lineGeometry.Colors.Add(color);
        }


        public void DrawPolyline(List<Triple> vertices, Color4 color)
        {
            for (int i = 1; i < vertices.Count; i++)
            {
                lineGeometry.Positions.Add(new Vector3(vertices[i - 1].X, vertices[i - 1].Z, -vertices[i - 1].Y));
                lineGeometry.Positions.Add(new Vector3(vertices[i].X, vertices[i].Z, -vertices[i].Y));
                lineGeometry.Indices.Add(lineGeometry.Indices.Count);
                lineGeometry.Indices.Add(lineGeometry.Indices.Count);
                lineGeometry.Colors.Add(color);
                lineGeometry.Colors.Add(color);
            }
        }


        //public void AddMeshModel(MeshGeometryModel3D meshModel)
        //{
        //    meshModels.Add(meshModel);
        //}


        internal void Render(bool async = false)
        {
            if (async)
            { 
                if (DispatcherOperation.Status == DispatcherOperationStatus.Completed)
                    DispatcherOperation = DynaShapeViewExtension.DynamoWindow.Dispatcher.InvokeAsync(RenderAction, DispatcherPriority.Render);     
            }
            else
                DynaShapeViewExtension.DynamoWindow.Dispatcher.Invoke(RenderAction, DispatcherPriority.Render);
        }


        

        private void RenderAction()
        {
            Viewport3DX viewport = DynaShapeViewExtension.GetViewport();
            List<Model3D> sceneItems = viewport.ItemsSource as List<Model3D>;

            pointGeometry = new PointGeometry3D
            {
                Positions = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection()
            };

            lineGeometry = new LineGeometry3D()
            {
                Positions = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection()
            };

            meshGeometry = new MeshGeometry3D()
            {
                Positions = new Vector3Collection(),
                Normals = new Vector3Collection(),
                Indices = new IntCollection(),
                //Colors = new Color4Collection()
            };

            //meshModels = new List<MeshGeometryModel3D>();


            //============================================
            // Render nodes as points
            //============================================

            for (int i = 0; i < solver.Nodes.Count; i++)
            {
                pointGeometry.Positions.Add(new Vector3(
                    solver.Nodes[i].Position.X,
                    solver.Nodes[i].Position.Z,
                    -solver.Nodes[i].Position.Y));

                pointGeometry.Colors.Add(new Color4(0.8f, 0.2f, 0.2f, 1f));
                pointGeometry.Indices.Add(i);
            }


            //==============================================================
            // Render geometries from geometry binders
            //==============================================================

            foreach (GeometryBinder geometryBinder in solver.GeometryBinders)
                geometryBinder.CreateDisplayedGeometries(this, solver.Nodes);


            //============================================================================
            // Render GUI elements
            //============================================================================

            RenderGUI();


            //==============================================================
            // Attach the geometries to Helix render host
            //==============================================================

            pointModel.Geometry = pointGeometry;
            lineModel.Geometry = lineGeometry;
            meshModel.Geometry = meshGeometry;


            if (pointGeometry.Positions.Count >= 1)
            {
                if (!pointModel.IsAttached) pointModel.Attach(viewport.RenderHost);
                if (!sceneItems.Contains(pointModel)) sceneItems.Add(pointModel);
            }


            if (lineGeometry.Positions.Count >= 2)
            {
                if (!lineModel.IsAttached) lineModel.Attach(viewport.RenderHost);
                if (!sceneItems.Contains(lineModel)) sceneItems.Add(lineModel);
            }


            //foreach (MeshGeometryModel3D meshModel in meshModels)
            //    if (meshModel.Geometry.Positions.Count >= 3)
            //    {
            //        meshModel.Attach(viewport.RenderHost);
            //        if (!sceneItems.Contains(meshModel)) sceneItems.Add(meshModel);
            //    }

            if (meshModel.Geometry.Positions.Count >= 3)
            {
                Console.WriteLine(counter + ": " + meshModel.IsAttached + ", " + sceneItems.Contains(meshModel) + "\n");

                var a = meshModel.IsAttached;
                var b = sceneItems.Contains(meshModel);

                if (!meshModel.IsAttached) meshModel.Attach(viewport.RenderHost);
                if (!sceneItems.Contains(meshModel)) sceneItems.Add(meshModel);
            }

            
        }


        private int counter = 0;
        private Stopwatch stopwatch = Stopwatch.StartNew();
        private bool temp = true;

        private void RenderGUI()
        {
            //============================================================================
            // Render a visual marker to highlight the node that is being manipulated
            //============================================================================

            if (solver.HandleNodeIndex != -1 || solver.NearestNodeIndex != -1)
            {
                CameraData camera = DynaShapeViewExtension.CameraData;

                Triple camOrigin = new Triple(camera.EyePosition.X, -camera.EyePosition.Z, camera.EyePosition.Y);
                Triple camZ = new Triple(camera.LookDirection.X, -camera.LookDirection.Z, camera.LookDirection.Y).Normalise();
                Triple camY = new Triple(camera.UpDirection.X, -camera.UpDirection.Z, camera.UpDirection.Y).Normalise();
                Triple camX = camZ.Cross(camY);

                int nodeIndex = solver.HandleNodeIndex != -1 ? solver.HandleNodeIndex : solver.NearestNodeIndex;
                Triple v = solver.Nodes[nodeIndex].Position - camOrigin;
                float screenDistance = (float)camera.NearPlaneDistance + 0.1f;
                v = camOrigin + v * screenDistance / v.Dot(camZ);

                float markerSize = 0.025f * screenDistance;

                Triple v1 = v + camX * markerSize;
                Triple v2 = v - camY * markerSize;
                Triple v3 = v - camX * markerSize;
                Triple v4 = v + camY * markerSize;

                lineGeometry.Positions.Add(v1.ToVector3());
                lineGeometry.Positions.Add(v3.ToVector3());
                lineGeometry.Positions.Add(v2.ToVector3());
                lineGeometry.Positions.Add(v4.ToVector3());

                markerSize *= 0.5f;
                v1 = v + camX * markerSize;
                v2 = v - camY * markerSize;
                v3 = v - camX * markerSize;
                v4 = v + camY * markerSize;

                lineGeometry.Positions.Add(v1.ToVector3());
                lineGeometry.Positions.Add(v2.ToVector3());
                lineGeometry.Positions.Add(v2.ToVector3());
                lineGeometry.Positions.Add(v3.ToVector3());
                lineGeometry.Positions.Add(v3.ToVector3());
                lineGeometry.Positions.Add(v4.ToVector3());
                lineGeometry.Positions.Add(v4.ToVector3());
                lineGeometry.Positions.Add(v1.ToVector3());


                int temp = lineGeometry.Indices.Count;
                for (int i = 0; i < 12; i++)
                {
                    lineGeometry.Indices.Add(temp + i);
                    lineGeometry.Colors.Add(Color.OrangeRed);
                }
            }
        }


        public void Dispose()
        {
            DynaShapeViewExtension.ViewModel.RequestViewRefresh -= RequestViewRefreshHandler;

            DynaShapeViewExtension.DynamoWindow.Dispatcher.Invoke(() =>
            {
                List<Model3D> sceneItems = DynaShapeViewExtension.GetViewport().ItemsSource as List<Model3D>;

                if (sceneItems.Contains(pointModel)) sceneItems.Remove(pointModel);
                pointModel.Detach();
                pointModel.Dispose();

                if (sceneItems.Contains(lineModel)) sceneItems.Remove(lineModel);
                lineModel.Detach();
                lineModel.Dispose();

                //foreach (MeshGeometryModel3D meshModel in meshModels)
                //{
                    //if (sceneItems.Contains(meshModel)) sceneItems.Remove(meshModel);
                    //meshModel.Detach();
                    //meshModel.Dispose();
                //}
            });
        }

        // This handler prevents flickering when geometries other than DynaShape ones exist in the viewport
        private void RequestViewRefreshHandler()
        {
            List<Model3D> sceneItems = DynaShapeViewExtension.ViewModel.SceneItems as List<Model3D>;

            if (pointGeometry.Positions.Count >= 1 && !sceneItems.Contains(pointModel)) sceneItems.Add(pointModel);
            if (lineGeometry.Positions.Count >= 2 && !sceneItems.Contains(lineModel)) sceneItems.Add(lineModel);
            //foreach (MeshGeometryModel3D meshModel in meshModels)
            //    if (meshModel.Geometry.Positions.Count >= 3 && !sceneItems.Contains(meshModel)) sceneItems.Add(meshModel);

            if (meshModel.Geometry.Positions.Count >= 3 && !sceneItems.Contains(meshModel)) sceneItems.Add(meshModel);
        }
    }
}