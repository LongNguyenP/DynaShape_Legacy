using Autodesk.DesignScript.Runtime;
using Dynamo.Wpf.Extensions;


namespace DynaShape
{
    [IsVisibleInDynamoLibrary(false)]
    public class DynaShapeViewExtension : IViewExtension
    {
        public void Dispose()
        {
        }


        public void Startup(ViewStartupParams p)
        {
        }


        public void Loaded(ViewLoadedParams p)
        {
            Solver.Viewport = p.BackgroundPreviewViewModel;
        }


        public void Shutdown()
        {
        }


        public string UniqueId => "{DC7A6415-8512-423B-9BC3-1A1C1AEE5144}";
        public string Name => "DynaShapeViewExtension";
    }
}
