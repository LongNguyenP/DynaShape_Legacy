using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Dynamo.Controls;
using Dynamo.Wpf.ViewModels.Watch3D;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;


namespace DynaShape
{
    [IsVisibleInDynamoLibrary(false)]
    public class DsDisplay : IDisposable
    {
        private readonly Solver solver;

        private delegate void DynaShapeDisplayDelegate();

        public DsDisplay(Solver solver)
        {
            this.solver = solver;
        }

        private PointGeometryModel3D pointModel;
        private LineGeometryModel3D lineModel;

        public void Display()
        {
            DynaShapeViewExtension.DynamoWindow.Dispatcher.BeginInvoke(
                DispatcherPriority.Render,
                new DynaShapeDisplayDelegate(DisplayAsync));
        }


        public void DisplayAsync()
        {
            Viewport3DX Viewport = (((DynaShapeViewExtension.DynamoWindow.Content as Grid).Children[2] as Grid).Children[1] as Watch3DView).View;
            List<Model3D> SceneItems = Viewport.ItemsSource as List<Model3D>;

            if (pointModel == null)
            {
                pointModel = new PointGeometryModel3D { Size = new Size(5, 5)};
                
            }

            if (lineModel == null)
            {
                lineModel = new LineGeometryModel3D { Thickness = 2 };              
            }


            if (!SceneItems.Contains(pointModel)) SceneItems.Add(pointModel);
            if (!SceneItems.Contains(lineModel)) SceneItems.Add(lineModel);

            PointGeometry3D pointGeometry = new PointGeometry3D();
            pointGeometry.Positions = new Vector3Collection();
            pointGeometry.Indices = new IntCollection();
            pointGeometry.Colors = new Color4Collection();

            for (int i = 0; i < solver.Nodes.Count; i++)
            {
                pointGeometry.Positions.Add(new Vector3(solver.Nodes[i].Position.X, solver.Nodes[i].Position.Z, -solver.Nodes[i].Position.Y));
                pointGeometry.Colors.Add(new Color4(1f, 1f, 0f, 1f));
                pointGeometry.Indices.Add(i);
  
            }

            pointModel.Geometry = pointGeometry;
            pointModel.Color = Color.Red;



            lineModel.Geometry = new LineGeometry3D
            {
                Positions = new Vector3Collection {new Vector3(0, 0, 0), new Vector3(-10, 10, 0)},
                Indices = new IntCollection {0, 1},
                Colors = new Color4Collection {new Color4(1f, 0f, 0f, 1f), new Color4(0f, 1f, 1f, 1f)}
            };

            lineModel.Color = new Color(0f, 0f, 0f, 1f);

            pointModel.Attach(Viewport.RenderHost);
            lineModel.Attach(Viewport.RenderHost);
        }

        public void Dispose()
        {
            DynaShapeViewExtension.DynamoWindow.Dispatcher.BeginInvoke(
                DispatcherPriority.Render,
                new DynaShapeDisplayDelegate(DisposeAsync));
        }

        public void DisposeAsync()
        { 
            Viewport3DX Viewport = (((DynaShapeViewExtension.DynamoWindow.Content as Grid).Children[2] as Grid).Children[1] as Watch3DView).View;
            List<Model3D> SceneItems = Viewport.ItemsSource as List<Model3D>;

            pointModel.Geometry = null;
            
            if (SceneItems.Contains(pointModel)) SceneItems.Remove(pointModel);
            pointModel.Detach();
        }
    }
}
