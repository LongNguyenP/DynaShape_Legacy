using System.Windows;
using Autodesk.DesignScript.Runtime;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.ViewModels.Watch3D;
using HelixToolkit.Wpf.SharpDX;


namespace DynaShape
{
    [IsVisibleInDynamoLibrary(false)]
    public class DynaShapeViewExtension : IViewExtension
    {
        public static CameraData CameraData = null;
        public static IWatch3DViewModel Viewport = null;


        public void Dispose()
        {
        }


        public void Startup(ViewStartupParams p)
        {
        }


        public void Loaded(ViewLoadedParams p)
        {
            Viewport = p.BackgroundPreviewViewModel;
            Viewport.ViewCameraChanged += ViewCameraChangedHandler;
        }


        public void Shutdown()
        {
            Viewport.ViewCameraChanged -= ViewCameraChangedHandler;
        }
    

        private void ViewCameraChangedHandler(object sender, RoutedEventArgs e)
        {
            CameraData = Viewport.GetCameraInformation();
        }


        public string UniqueId => "{DC7A6415-8512-423B-9BC3-1A1C1AEE5144}";
        public string Name => "DynaShapeViewExtension";
    }
}
