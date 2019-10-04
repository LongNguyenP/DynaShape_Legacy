using System.Collections.Generic;
using Autodesk.DesignScript.Interfaces;
using DSCore;
using DynaShape;
using DynaShape.GeometryBinders;
using SharpDX;
using Node = DynaShape.Node;


namespace DynaSpace
{
    public class SimplePoint : IGraphicItem
    {
        private double x, y, z;

        public SimplePoint(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            package.AddPointVertex(x, y, z);
        }
    }

    public class FooGeometryBinder : GeometryBinder
    {
        static private int t = 0;

        public int Extent = 100;
        public float Spacing = 0.1f;
        public float WaveAmplitude = 3f;
        public float WaveLength = 5f;
        public float WaveSpeed = 0.001f;


        public static void Set(FooGeometryBinder fooGeometryBinder, int extent, float spacing, float waveAmplitude, float waveLength, float waveSpeed)
        {
            fooGeometryBinder.Extent = extent;
            fooGeometryBinder.Spacing = spacing;
            fooGeometryBinder.WaveAmplitude = waveAmplitude;
            fooGeometryBinder.WaveLength = waveLength;
            fooGeometryBinder.WaveSpeed = waveSpeed;
        }


        public override void CreateDisplayedGeometries(DynaShapeDisplay display, List<Node> allNodes)
        {
            t++;
            for (int i = -Extent; i <= Extent; i++)
            for (int j = -Extent; j <= Extent; j++)
            {
                float d = (float)Math.Sqrt(i * i + j * j);
                display.DrawPoint(i, j, (float)Math.Cos(d / WaveLength + t * WaveSpeed) * WaveAmplitude, new Color4(0.5f, 0.1f, 0.1f, 1.0f));
            }
        }
    }
}