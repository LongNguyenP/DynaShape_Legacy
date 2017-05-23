using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Dynamo.Controls;
using Dynamo.Wpf.ViewModels.Watch3D;
using DynaShape.GeometryBinders;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;


namespace DynaShape
{
    [IsVisibleInDynamoLibrary(false)]
    public class DynaShapeDisplay : IDisposable
    {
        public static Color4 DefaultPointColor = new Color4(0.8f, 0.2f, 0.2f, 1f);
        public static Color4 DefaultLineColor = new Color4(0.3f, 0.7f, 0.8f, 1f);
        public static Color4 DefaultMeshFaceColor = new Color4(0, 0.7f, 1f, 0.3f);


        private readonly Solver solver;

        private DynamoPointGeometryModel3D pointModel;
        private DynamoLineGeometryModel3D lineModel;
        private MeshGeometryModel3D meshModel;


        private PointGeometry3D pointGeometry;
        private LineGeometry3D lineGeometry;
        private MeshGeometry3D meshGeometry;


        public DynaShapeDisplay(Solver solver)
        {
            this.solver = solver;

            pointGeometry = new PointGeometry3D
            {
                Positions = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection()
            };


            lineGeometry = new LineGeometry3D
            {
                Positions = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection()
            };


            meshGeometry = new MeshGeometry3D
            {
                Positions = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection()
            };


            DynaShapeViewExtension.DynamoWindow.Dispatcher.InvokeAsync(            
                () =>
                {
                    pointModel = new DynamoPointGeometryModel3D
                    {
                        Size = new Size(7, 7),
                        Figure = PointGeometryModel3D.PointFigure.Ellipse,
                        Color = Color.White,
                        Geometry = pointGeometry
                    };


                    lineModel = new DynamoLineGeometryModel3D()
                    {
                        Thickness = 0.7,
                        Color = Color.White,
                        Geometry = lineGeometry
                    };


                    meshModel = new MeshGeometryModel3D()
                    {
                        Geometry = meshGeometry
                    };
                },
                DispatcherPriority.Send);

            DynaShapeViewExtension.ViewModel.RequestViewRefresh += RequestViewRefreshHandler;
        }


        private void RequestViewRefreshHandler()
        {
            List<Model3D> sceneItems = DynaShapeViewExtension.ViewModel.SceneItems as List<Model3D>;
            if (pointGeometry.Positions.Count >= 1 && !sceneItems.Contains(pointModel)) sceneItems.Add(pointModel);
            if (lineGeometry.Positions.Count >= 2 && !sceneItems.Contains(lineModel)) sceneItems.Add(lineModel);             
            if (meshGeometry.Positions.Count >= 3 && !sceneItems.Contains(meshModel)) sceneItems.Add(meshModel);         
        }


        internal void ClearAllGeometries()
        {
            pointGeometry.Positions.Clear();
            pointGeometry.Indices.Clear();
            pointGeometry.Colors.Clear();
            lineGeometry.Positions.Clear();
            lineGeometry.Indices.Clear();
            lineGeometry.Colors.Clear();
            meshGeometry.Positions.Clear();
            meshGeometry.Indices.Clear();
            meshGeometry.Colors.Clear();
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
                lineGeometry.Positions.Add(new Vector3(vertices[i-1].X, vertices[i-1].Z, -vertices[i-1].Y));
                lineGeometry.Positions.Add(new Vector3(vertices[i].X, vertices[i].Z, -vertices[i].Y));              
                lineGeometry.Indices.Add(lineGeometry.Indices.Count);
                lineGeometry.Indices.Add(lineGeometry.Indices.Count);
                lineGeometry.Colors.Add(color);
                lineGeometry.Colors.Add(color);
            }
        }


        public void Render()
        {
            DynaShapeViewExtension.DynamoWindow.Dispatcher.InvokeAsync(RenderAsync, DispatcherPriority.Render);
        }


        private void TransformPointsToScreenSpace()
        {
            Vector3 camOrigin = new Vector3(
                (float)DynaShapeViewExtension.CameraData.EyePosition.X,
                (float)DynaShapeViewExtension.CameraData.EyePosition.Y,
                (float)DynaShapeViewExtension.CameraData.EyePosition.Z);

            Vector3 camLook = new Vector3(
                (float)DynaShapeViewExtension.CameraData.LookDirection.X,
                (float)DynaShapeViewExtension.CameraData.LookDirection.Y,
                (float)DynaShapeViewExtension.CameraData.LookDirection.Z).Normalized();

            float screenDistance = (float)DynaShapeViewExtension.CameraData.NearPlaneDistance + 0.1f;

            for (int i = 0; i < pointGeometry.Positions.Count; i++)
            {
                Vector3 p = pointGeometry.Positions[i];
                Vector3 v = (p - camOrigin);
                pointGeometry.Positions[i] = camOrigin + v * screenDistance / Vector3.Dot(v, camLook);
            }

        }


        public void RenderAsync()
        {
            ClearAllGeometries();

            Viewport3DX viewport = GetViewport();
            List<Model3D> sceneItems = viewport.ItemsSource as List<Model3D>;

            for (int i = 0; i < solver.Nodes.Count; i++)
            {
                pointGeometry.Positions.Add(new Vector3(solver.Nodes[i].Position.X, solver.Nodes[i].Position.Z,
                    -solver.Nodes[i].Position.Y));
                pointGeometry.Colors.Add(new Color4(0.8f, 0.2f, 0.2f, 1f));
                pointGeometry.Indices.Add(i);

            }

            //TransformPointsToScreenSpace();

            foreach (GeometryBinder geometryBinder in solver.GeometryBinders)
            {
                geometryBinder.DrawGraphics(this, solver.Nodes);
            }


            if (pointGeometry.Positions.Count >= 1)
            {
                pointModel.Attach(viewport.RenderHost);
                if (!sceneItems.Contains(pointModel)) sceneItems.Add(pointModel);
            }


            if (lineGeometry.Positions.Count >= 2)
            {
                lineModel.Attach(viewport.RenderHost);
                if (!sceneItems.Contains(lineModel)) sceneItems.Add(lineModel);
            }


            if (meshGeometry.Positions.Count >= 3)
            {
                meshModel.Attach(viewport.RenderHost);
                if (!sceneItems.Contains(meshModel)) sceneItems.Add(meshModel);
            }
        }


        public void Dispose()
        {
            DynaShapeViewExtension.ViewModel.RequestViewRefresh -= RequestViewRefreshHandler;

            DynaShapeViewExtension.DynamoWindow.Dispatcher.Invoke(() =>
            {
                List<Model3D> SceneItems = GetViewport().ItemsSource as List<Model3D>;

                if (SceneItems.Contains(pointModel)) SceneItems.Remove(pointModel);
                pointModel.Detach();
            });
        }


        private Viewport3DX GetViewport()
        {
            return
                ((Watch3DView)
                    ((Grid)
                        ((Grid)
                            DynaShapeViewExtension.DynamoWindow.Content)
                        .Children[2])
                    .Children[1])
                .View;
        }
    }
}