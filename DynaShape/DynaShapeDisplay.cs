using System;
using System.Collections.Generic;
using System.Drawing;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Dynamo.Wpf.ViewModels.Watch3D;


namespace DynaShape
{
    [IsVisibleInDynamoLibrary(false)]
    public class DynaShapeDisplay : IGraphicItem
    {
        private readonly Solver solver;

        public DynaShapeDisplay(Solver solver)
        {
            this.solver = solver;
        }

        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            for (int i = 0; i < solver.Nodes.Count; i++)
                GeometryRender.DrawPoint(package, solver.Nodes[i].Position);

            for (int i = 0; i < solver.GeometryBinders.Count; i++)
                solver.GeometryBinders[i].DrawGraphics(package, parameters, solver.Nodes);


            //==================================================================================================
            // Draw a visual marker to highlight the node that is being manipulated/hovered-over by the mouse
            // The marker is 2D, so we need to draw it in the camera coordinate system, manually
            //==================================================================================================

            CameraData camera = ((HelixWatch3DViewModel)Solver.Viewport).Camera.Dispatcher.Invoke(() => Solver.Viewport.GetCameraInformation());
            Triple camOrigin = new Triple(camera.EyePosition.X, -camera.EyePosition.Z, camera.EyePosition.Y);
            Triple camZ = new Triple(camera.LookDirection.X, -camera.LookDirection.Z, camera.LookDirection.Y).Normalise();
            Triple camY = new Triple(camera.UpDirection.X, -camera.UpDirection.Z, camera.UpDirection.Y).Normalise();
            Triple camX = camZ.Cross(camY);


            if (solver.HandleNodeIndex != -1 || solver.NearestNodeIndex != -1)
            {
                int i = solver.HandleNodeIndex != -1 ? solver.HandleNodeIndex : solver.NearestNodeIndex;
                Color markerColor = solver.HandleNodeIndex != -1 ? Color.OrangeRed : Color.OrangeRed;

                Triple v = solver.Nodes[i].Position - camOrigin;
                v = camOrigin + ((float)camera.NearPlaneDistance + 0.5f) * v / v.Dot(camZ);

                float handleSize = 0.008f;

                Triple v1 = v + camX * handleSize;
                Triple v2 = v - camY * handleSize;
                Triple v3 = v - camX * handleSize;
                Triple v4 = v + camY * handleSize;
                GeometryRender.DrawPolyline(package, new List<Triple> { v1, v2, v3, v4, v1 }, markerColor);

                handleSize = 0.015f;
                v1 = v + camX * handleSize;
                v2 = v - camY * handleSize;
                v3 = v - camX * handleSize;
                v4 = v + camY * handleSize;

                GeometryRender.DrawLine(package, v1, v3, markerColor);
                GeometryRender.DrawLine(package, v2, v4, markerColor);
            }
        }
    }
}
