using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using Autodesk.DesignScript.Runtime;
using Dynamo.Wpf.ViewModels.Watch3D;
using DynaShape.GeometryBinders;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using Model3D = HelixToolkit.Wpf.SharpDX.Model3D;


namespace DynaShape
{
    [IsVisibleInDynamoLibrary(false)]
    public class DynaShapeDisplay
#if CLI == false
        : IDisposable
#endif
    {
        public static readonly Color4 DefaultPointColor    = new Color4(0.8f, 0.2f, 0.2f, 1.0f);
        public static readonly Color4 DefaultLineColor     = new Color4(0.3f, 0.7f, 0.8f, 1.0f);
        public static readonly Color4 DefaultMeshFaceColor = new Color4(0.0f, 0.7f, 1.0f, 0.3f);
        public static readonly Color4 DefaultTextColor     = new Color4(0.0f, 0.0f, 0.0f, 1.0f);

#if CLI == false
        private readonly Solver solver;

        private PointGeometryModel3D pointModel;
        private LineGeometryModel3D lineModel;
        private BillboardTextModel3D billboardTextModel;

        private PointGeometry3D pointGeometry;
        private LineGeometry3D lineGeometry;
        private BillboardText3D billboardText;

        private List<MeshGeometryModel3D> meshModels = new List<MeshGeometryModel3D>();

        internal DispatcherOperation DispatcherOperation;

        public DynaShapeDisplay(Solver solver)
        {
            this.solver = solver;

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

            billboardText = new BillboardText3D();

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

                    billboardTextModel = new BillboardTextModel3D();
                },
                DispatcherPriority.Send);

            DynaShapeViewExtension.ViewModel.RequestViewRefresh += RequestViewRefreshHandler;
            DynaShapeViewExtension.DynamoWindow.Closed += (sender, args) => Dispose();
        }


        public void DrawPoint(float x, float y, float z, Color4 color)
        {
            pointGeometry.Positions.Add(new Vector3(x, z, -y));
            pointGeometry.Colors.Add(color);
            pointGeometry.Indices.Add(pointGeometry.Indices.Count);
        }


        public void DrawPoint(Triple position, Color4 color)
        {
            pointGeometry.Positions.Add(new Vector3(position.X, position.Z, -position.Y));
            pointGeometry.Colors.Add(color);
            pointGeometry.Indices.Add(pointGeometry.Indices.Count);
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


        public void DrawPolyline(List<Triple> vertices, Color4 color, bool loop)
        {
            if (loop) vertices.Add(vertices[0]);

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


        public void DrawText(string text, Triple position)
        {
            billboardText.TextInfo.Add(new TextInfo(text, position.ToVector3()));
        }


        public void AddMeshModel(MeshGeometryModel3D meshModel)
        {
            meshModels.Add(meshModel);
        }


        internal void Render(bool async = false)
        {
            if (async)
            {
                if (DispatcherOperation == null ||
                    DispatcherOperation != null && DispatcherOperation.Status == DispatcherOperationStatus.Completed)
                {
                    DispatcherOperation =
                        DynaShapeViewExtension.DynamoWindow.Dispatcher.InvokeAsync(
                            RenderAction, DispatcherPriority.Send);
                }
            }
            else DynaShapeViewExtension.DynamoWindow.Dispatcher.Invoke(RenderAction, DispatcherPriority.Send);
        }


        internal void ClearRender()
        {
            DynaShapeViewExtension.ViewModel.RequestViewRefresh -= RequestViewRefreshHandler;
            DynaShapeViewExtension.DynamoWindow.Dispatcher.Invoke(() =>
            {
                List<Model3D> sceneItems = DynaShapeViewExtension.GetSceneItems();

                if (sceneItems.Contains(pointModel)) sceneItems.Remove(pointModel);
                pointModel.Detach();
                pointModel.Dispose();

                if (sceneItems.Contains(lineModel)) sceneItems.Remove(lineModel);
                lineModel.Detach();
                lineModel.Dispose();

                if (sceneItems.Contains(billboardTextModel)) sceneItems.Remove(billboardTextModel);
                billboardTextModel.Detach();
                billboardTextModel.Dispose();

                foreach (MeshGeometryModel3D meshModel in meshModels)
                {
                    if (sceneItems.Contains(meshModel)) sceneItems.Remove(meshModel);
                    meshModel.Detach();
                    meshModel.Dispose();
                }
            });
        }


        private void RenderAction()
        {
            IRenderHost renderHost = DynaShapeViewExtension.GetViewport().RenderHost;
            List<Model3D> sceneItems = DynaShapeViewExtension.GetSceneItems();

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


            billboardText = new BillboardText3D();

            foreach (MeshGeometryModel3D meshModel in meshModels)
            {
                meshModel.Detach();
                if (sceneItems.Contains(meshModel)) sceneItems.Remove(meshModel);
            }

            meshModels.Clear();

            //============================================
            // Render nodes as points
            //============================================

            int n = pointGeometry.Indices.Count;
            for (int i = 0; i < solver.Nodes.Count; i++)
            {
                pointGeometry.Positions.Add(solver.Nodes[i].Position.ToVector3());
                pointGeometry.Colors.Add(DefaultPointColor);
                pointGeometry.Indices.Add(n + i);
            }

            //==============================================================
            // Render geometries from geometry binders
            //==============================================================

            foreach (GeometryBinder geometryBinder in solver.GeometryBinders)
                if (geometryBinder.Show)
                    geometryBinder.CreateDisplayedGeometries(this, solver.Nodes);

            //============================================================================
            // Render GUI elements
            //============================================================================

            RenderGUI();

            //==============================================================
            // Attach the geometries to Helix render host
            //==============================================================

            if (pointGeometry.Positions.Count >= 1)
            {
                pointModel.Geometry = pointGeometry;
                if (!sceneItems.Contains(pointModel)) sceneItems.Add(pointModel);
                if (!pointModel.IsAttached) pointModel.Attach(renderHost);
            }
            else
            {
                if (sceneItems.Contains(pointModel)) sceneItems.Remove(pointModel);
                pointModel.Detach();
            }

            if (lineGeometry.Positions.Count >= 2)
            {
                lineModel.Geometry = lineGeometry;
                if (!sceneItems.Contains(lineModel)) sceneItems.Add(lineModel);
                if (!lineModel.IsAttached) lineModel.Attach(renderHost);
            }
            else
            {
                if (sceneItems.Contains(lineModel)) sceneItems.Remove(lineModel);
                lineModel.Detach();
            }

            foreach (MeshGeometryModel3D meshModel in meshModels)
            {
                sceneItems.Add(meshModel);
                meshModel.Attach(renderHost);
            }

            if (billboardText.TextInfo.Count >= 1)
            {
                billboardTextModel.Geometry = billboardText;
                if (!sceneItems.Contains(billboardTextModel)) sceneItems.Add(billboardTextModel);
                if (!billboardTextModel.IsAttached) billboardTextModel.Attach(renderHost);
            }
            else
            {
                if (sceneItems.Contains(billboardTextModel)) sceneItems.Remove(billboardTextModel);
                billboardTextModel.Detach();
            }
        }


        private void RenderGUI()
        {
            //============================================================================
            // Render a visual marker to highlight the node that is being manipulated
            //============================================================================

            if (solver.HandleNodeIndex != -1 || solver.NearestNodeIndex != -1)
            {
                CameraData camera = DynaShapeViewExtension.CameraData;

                Triple camOrigin = new Triple(camera.EyePosition.X, -camera.EyePosition.Z, camera.EyePosition.Y);
                Triple camZ = new Triple(camera.LookDirection.X, -camera.LookDirection.Z, camera.LookDirection.Y)
                   .Normalise();
                Triple camY = new Triple(camera.UpDirection.X, -camera.UpDirection.Z, camera.UpDirection.Y).Normalise();
                Triple camX = camZ.Cross(camY);

                int nodeIndex = solver.HandleNodeIndex != -1 ? solver.HandleNodeIndex : solver.NearestNodeIndex;
                Triple v = solver.Nodes[nodeIndex].Position - camOrigin;
                float screenDistance = (float) camera.NearPlaneDistance + 0.1f;
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
            ClearRender();
        }

        // This handler prevents flickering when geometries other than DynaShape ones exist in the viewport
        private void RequestViewRefreshHandler()
        {
            List<Model3D> sceneItems = DynaShapeViewExtension.GetSceneItems();
            if (pointGeometry.Positions.Count >= 1 && !sceneItems.Contains(pointModel)) sceneItems.Add(pointModel);
            if (lineGeometry.Positions.Count >= 2 && !sceneItems.Contains(lineModel)) sceneItems.Add(lineModel);
            if (billboardText.TextInfo.Count >= 1 && !sceneItems.Contains(billboardTextModel)) sceneItems.Add(billboardTextModel);
            foreach (MeshGeometryModel3D meshModel in meshModels)
                if (!sceneItems.Contains(meshModel))
                    sceneItems.Add(meshModel);
        }

#endif
    }
}

