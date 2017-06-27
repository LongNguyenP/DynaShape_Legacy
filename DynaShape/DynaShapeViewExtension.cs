using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.DesignScript.Runtime;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.ViewModels.Watch3D;
using Dynamo.Graph.Workspaces;
using System;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using HelixToolkit.Wpf.SharpDX;


namespace DynaShape
{
    [IsVisibleInDynamoLibrary(false)]
    public class DynaShapeViewExtension : IViewExtension
    {
        public static ViewLoadedParams Parameters;
        public static Window DynamoWindow;
        public static HelixWatch3DViewModel ViewModel;
        public static CameraData CameraData;
        public static Triple MouseRayOrigin;
        public static Triple MouseRayDirection;
        

        public void Dispose() {}
        public void Startup(ViewStartupParams p) {}


        public void Loaded(ViewLoadedParams parameters)
        {
            Parameters = parameters;
            DynamoWindow = parameters.DynamoWindow;         
            ViewModel = parameters.BackgroundPreviewViewModel as HelixWatch3DViewModel;
            if (ViewModel == null) throw new Exception("Could not obtain HelixWatch3DViewModel. Sad!");

            ViewModel.ViewCameraChanged += ViewModelViewCameraChangedHandler;
            ViewModel.ViewMouseDown += ViewModelViewMouseDownHandler;
            ViewModel.ViewMouseMove += ViewModelViewMouseMoveHandler;
            ViewModel.RequestViewRefresh += ViewModelRequestViewRefreshHandler;


            DynaShapeViewExtension.DynamoWindow.Closing += (sender, args) =>
            {
                var boo = args;
            };
        }

        private void foo(object o, List<string> info, int indent)
        {
            if (!(o is Grid))
            {
                string pad = "";
                for (int i = 0; i < indent; i++) pad += "  ";
                info.Add(pad + o.GetType());
                return;
            }

            foreach (object element in (o as Grid).Children)
                foo(element, info, indent + 1);
        }

        private void ViewModelRequestViewRefreshHandler()
        {
        }


        private void ViewModelViewMouseDownHandler(object sender, MouseButtonEventArgs e)
        {   
        }


        internal static Viewport3DX GetViewport()
        {
            return
                ((Watch3DView)
                    ((Grid)
                        ((Grid)
                            DynamoWindow.Content)
                        .Children[2])
                    .Children[1])
                .View;
        }


        private void ListContent(Grid grid, int level, List<string> info)
        {
            foreach (var element in grid.Children)
            {
                string indent = "";
                for (int i = 0; i < level; i++) indent += "   ";
                info.Add(indent + element.GetType() + " == " + element);
                if (element is Grid)
                    ListContent(element as Grid, level + 1, info);
            }
        }


        public void Shutdown()
        {
            ViewModel.ViewCameraChanged -= ViewModelViewCameraChangedHandler;
            ViewModel.ViewMouseDown -= ViewModelViewMouseDownHandler;
            ViewModel.ViewMouseMove -= ViewModelViewMouseMoveHandler;          
            ViewModel.RequestViewRefresh -= ViewModelRequestViewRefreshHandler;
        }
    

        private void ViewModelViewCameraChangedHandler(object sender, RoutedEventArgs e)
        {
            CameraData = ViewModel.GetCameraInformation();
        }


        private void ViewModelViewMouseMoveHandler(object sender, MouseEventArgs e)
        {
            IRay clickRay = ViewModel.GetClickRay(e);
            MouseRayOrigin = new Triple(clickRay.Origin.X, clickRay.Origin.Y, clickRay.Origin.Z);
            MouseRayDirection = new Triple(clickRay.Direction.X, clickRay.Direction.Y, clickRay.Direction.Z); 
        }


        public string UniqueId => "{DC7A6415-8512-423B-9BC3-1A1C1AEE5144}";
        public string Name => "DynaShapeViewExtension";
    }
}
