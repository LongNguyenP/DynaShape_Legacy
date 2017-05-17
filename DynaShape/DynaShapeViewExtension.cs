using System.Windows;
using System.Windows.Input;
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
        public static Triple MouseRayOrigin;
        public static Triple MouseRayDirection;

        public void Dispose()
        {
        }


        public void Startup(ViewStartupParams p)
        {
        }


        public void Loaded(ViewLoadedParams p)
        {
            Viewport = p.BackgroundPreviewViewModel;
            Viewport.ViewCameraChanged += ViewportViewCameraChangedHandler;
            Viewport.ViewMouseMove += ViewportViewMouseMoveHandler;
        }


        public void Shutdown()
        {
            Viewport.ViewCameraChanged -= ViewportViewCameraChangedHandler;
            Viewport.ViewMouseMove -= ViewportViewMouseMoveHandler;
        }
    

        private void ViewportViewCameraChangedHandler(object sender, RoutedEventArgs e)
        {
            CameraData = Viewport.GetCameraInformation();
        }


        private void ViewportViewMouseMoveHandler(object sender, MouseEventArgs e)
        {
            IRay clickRay = Viewport.GetClickRay(e);
            MouseRayOrigin = new Triple(clickRay.Origin.X, clickRay.Origin.Y, clickRay.Origin.Z);
            MouseRayDirection = new Triple(clickRay.Direction.X, clickRay.Direction.Y, clickRay.Direction.Z);
        }


        public string UniqueId => "{DC7A6415-8512-423B-9BC3-1A1C1AEE5144}";
        public string Name => "DynaShapeViewExtension";
    }
}
