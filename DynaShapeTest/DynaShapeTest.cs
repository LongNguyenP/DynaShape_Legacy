//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using DynaShape;

//namespace DynaShapeTest
//{
//    [TestClass]
//    public class DynaShapeTest
//    {
//        public static void Main()
//        {
//            FastSvd3x3Test();
//        }
      
//        [TestMethod]
//        public static void FastSvd3x3Test()
//        {
//            Random random = new Random(1);
//            float[,] a =
//            {
//                {(float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble()},
//                {(float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble()},
//                {(float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble()},
//            };

//            FastSvd3x3.Compute(
//                a[0, 0], a[0, 1], a[0, 2],
//                a[1, 0], a[1, 1], a[1, 2],
//                a[2, 0], a[2, 1], a[2, 2],
//                out float u11, out float u12, out float u13,
//                out float u21, out float u22, out float u23,
//                out float u31, out float u32, out float u33,
//                out float s11, out float s22, out float s33,
//                out float v11, out float v12, out float v13,
//                out float v21, out float v22, out float v23,
//                out float v31, out float v32, out float v33);

//            Util.ComputeSvd(a, out float[] s, out float[,] v);

//            string specifier = "0#.###0";

//            Console.WriteLine(s11.ToString(specifier) + " > " + s[0].ToString(specifier));
//            Console.WriteLine(s22.ToString(specifier) + " > " + s[1].ToString(specifier));
//            Console.WriteLine(s33.ToString(specifier) + " > " + s[2].ToString(specifier));
//            Console.WriteLine();
//            Console.WriteLine(u11.ToString(specifier) + ", " + u12.ToString(specifier) + ", " + u13.ToString(specifier));
//            Console.WriteLine(u21.ToString(specifier) + ", " + u22.ToString(specifier) + ", " + u23.ToString(specifier));
//            Console.WriteLine(u31.ToString(specifier) + ", " + u32.ToString(specifier) + ", " + u33.ToString(specifier));
//            Console.WriteLine("----------------------------------------------------------------");
//            Console.WriteLine(a[0, 0].ToString(specifier) + ", " + a[0, 1].ToString(specifier) + ", " + a[0, 2].ToString(specifier));
//            Console.WriteLine(a[1, 0].ToString(specifier) + ", " + a[1, 1].ToString(specifier) + ", " + a[1, 2].ToString(specifier));
//            Console.WriteLine(a[2, 0].ToString(specifier) + ", " + a[2, 1].ToString(specifier) + ", " + a[2, 2].ToString(specifier));
//            Console.WriteLine();
//            Console.WriteLine(v11.ToString(specifier) + ", " + v12.ToString(specifier) + ", " + v13.ToString(specifier));
//            Console.WriteLine(v21.ToString(specifier) + ", " + v22.ToString(specifier) + ", " + v23.ToString(specifier));
//            Console.WriteLine(v31.ToString(specifier) + ", " + v32.ToString(specifier) + ", " + v33.ToString(specifier));
//            Console.WriteLine("----------------------------------------------------------------");
//            Console.WriteLine(v[0, 0].ToString(specifier) + ", " + v[0, 1].ToString(specifier) + ", " + v[0, 2].ToString(specifier));
//            Console.WriteLine(v[1, 0].ToString(specifier) + ", " + v[1, 1].ToString(specifier) + ", " + v[1, 2].ToString(specifier));
//            Console.WriteLine(v[2, 0].ToString(specifier) + ", " + v[2, 1].ToString(specifier) + ", " + v[2, 2].ToString(specifier));

//            Console.Read();
//        }

//        public static void FastSvd3x3Benchmark()
//        {
//            int n = 8000;

//            //==============================================================================

//            for (int j = 0; j < 50; j++)
//            {
//                Random random = new Random(j);

//                List<float[,]> matrices = new List<float[,]>();

//                for (int i = 0; i < n; i++)
//                    matrices.Add(new[,] {
//                        {(float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble()},
//                        {(float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble()},
//                        {(float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble()}});

//                Stopwatch stopwatch = Stopwatch.StartNew();
//                for (int i = 0; i < n; i++)
//                {
//                    float[,] a = matrices[i];
//                    FastSvd3x3.Compute(
//                        a[0, 0], a[0, 1], a[0, 2],
//                        a[1, 0], a[1, 1], a[1, 2],
//                        a[2, 0], a[2, 1], a[2, 2],
//                        out float u11, out float u12, out float u13,
//                        out float u21, out float u22, out float u23,
//                        out float u31, out float u32, out float u33,
//                        out float s11, out float s22, out float s33,
//                        out float v11, out float v12, out float v13,
//                        out float v21, out float v22, out float v23,
//                        out float v31, out float v32, out float v33);
//                }

//                Console.Write(stopwatch.ElapsedMilliseconds.ToString(" ##"));

//                stopwatch.Restart();
//                for (int i = 0; i < n; i++)
//                    Util.ComputeSvd(matrices[i], out float[] s, out float[,] v);
//                Console.WriteLine(", " + stopwatch.ElapsedMilliseconds.ToString(" ##"));
//            }

//            Console.Read();
//        }
//    }
//}
