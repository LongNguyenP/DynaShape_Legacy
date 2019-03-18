using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Animation;
using Autodesk.DesignScript.Runtime;
using SharpDX;
using Point = Autodesk.DesignScript.Geometry.Point;
using Vector = Autodesk.DesignScript.Geometry.Vector;
using Math = System.Math;


namespace DynaShape
{
    [IsVisibleInDynamoLibrary(false)]
    public static class Util
    {
        public static Point ToPoint(this Triple t) => Point.ByCoordinates(t.X, t.Y, t.Z);
        public static Vector ToVector(this Triple t) => Vector.ByCoordinates(t.X, t.Y, t.Z);

        public static Triple ToTriple(this Point point) => new Triple(point.X, point.Y, point.Z);
        public static Triple ToTriple(this Vector vector) => new Triple(vector.X, vector.Y, vector.Z);

        public static Vector ZeroVector => Vector.ByCoordinates(0.0, 0.0, 0.0);
        public static Point Duplicate(this Point point) => Point.ByCoordinates(point.X, point.Y, point.Z);
        public static Vector Duplicate(this Vector vector) => Vector.ByCoordinates(vector.X, vector.Y, vector.Z);

        public static bool IsAlmostZero(this float number, float tolerance = 1E-10f) => -tolerance < number && number < tolerance;
        public static bool IsAlmostZero(this double number, double tolerance = 1E-10) => -tolerance < number && number < tolerance;

        public static SharpDX.Color ToSharpDXColor(this DSCore.Color color) 
            => new SharpDX.Color(color.Red * by255, color.Green * by255, color.Blue * by255, color.Alpha * by255);

        public static Vector3 ToVector3(this Triple triple) => new Vector3(triple.X, triple.Z, -triple.Y);

        private static readonly float by255 = 1f / 255f;

        public static List<Triple> ToTriples(this IEnumerable<Point> points)
        {
            List<Triple> triples = new List<Triple>(points.Count());
            foreach (Point point in points)
                triples.Add(new Triple(point.X, point.Y, point.Z));
            return triples;
        }

        public static List<Triple> ToTriples(this IEnumerable<Vector> vectors)
        {
            List<Triple> triples = new List<Triple>(vectors.Count());
            foreach (Vector vector in vectors)
                triples.Add(new Triple(vector.X, vector.Y, vector.Z));
            return triples;
        }

        public static T[] InitializeArray<T>(int length, T value)
        {
            T[] array = new T[length];
            for (int i = 0; i < length; i++) array[i] = value;
            return array;
        }

        public static void FillArray<T>(this T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++) array[i] = value;
        }

        private const float toRadian = (float)Math.PI / 180f;
        private const float toDegree = 180f / (float)Math.PI;

        public static float ToRadian(this float Degree) => Degree * toRadian;
        public static float ToDegree(this float Radian) => Radian * toDegree;

        /// <summary>
        /// Compute the line that best fit a set of input points (least squared orthogonal distance)
        /// </summary>
        /// <param name="points">The input points</param>
        /// <param name="lineOrigin">Origin of the best fit line</param>
        /// <param name="lineDirection">Direction of the best fit line</param>
        /// <param name="tolerance">The tolerance that is used the determined if the input points are coincidental, colinear, or non-colinear</param>
        /// <returns>0 if the input points are coincidental; 1 if they are colinear; 2 otherwise</returns>       
        public static int ComputeBestFitLine(List<Triple> points, out Triple lineOrigin, out Triple lineDirection, float tolerance = 1E-10f)
        {
            Triple centroid = Triple.Zero;
            for (int i = 0; i < points.Count; i++) centroid += points[i];
            centroid /= points.Count;

            float[,] P = new float[points.Count, 3];

            for (int i = 0; i < points.Count; i++)
            {
                P[i, 0] = points[i].X - centroid.X;
                P[i, 1] = points[i].Y - centroid.Y;
                P[i, 2] = points[i].Z - centroid.Z;
            }

            float c00 = 0f, c01 = 0f, c02 = 0f;
            float c10 = 0f, c11 = 0f, c12 = 0f;
            float c20 = 0f, c21 = 0f, c22 = 0f;

            for (int k = 0; k < points.Count; k++)
            {
                c00 += P[k, 0] * P[k, 0];
                c01 += P[k, 0] * P[k, 1];
                c02 += P[k, 0] * P[k, 2];
                c10 += P[k, 1] * P[k, 0];
                c11 += P[k, 1] * P[k, 1];
                c12 += P[k, 1] * P[k, 2];
                c20 += P[k, 2] * P[k, 0];
                c21 += P[k, 2] * P[k, 1];
                c22 += P[k, 2] * P[k, 2];
            }

            FastSvd3x3.Compute(
                c00, c01, c02,
                c10, c11, c12,
                c20, c21, c22,
                out float u00, out float u01, out float u02,
                out float u10, out float u11, out float u12,
                out float u20, out float u21, out float u22,
                out float s00, out float s11, out float s22,
                out float v00, out float v01, out float v02,
                out float v10, out float v11, out float v12,
                out float v20, out float v21, out float v22);

            lineOrigin = centroid;

            // Case 0: The input points are coincidental, so we just need to pick an arbitrary line direction
            if (s00 < tolerance)
            {
                lineDirection = Triple.BasisX;
                return 0;
            }

            // Case 1: The input points are not coincidental, therefore we pick the dominant eigen vector as the line direction
            lineDirection = new Triple(u00, u10, u20);
            return s11 < tolerance
                ? 1 // The input points are colinear
                : 2; // The input points are NOT colinear
        }

        /// <summary>
        /// Compute the plane that best fit a set of input points (least squared orthogonal distance)
        /// </summary>
        /// <param name="points">The input points</param>
        /// <param name="planeOrigin">Origin of the best fit plane</param>
        /// <param name="planeNormal">Normal of the best fit plane</param>
        /// <param name="tolerance">The tolerance that is used the determined if the input points are coincidental, colinear, coplanar, or non-coplanar</param>
        /// <returns>0 if the input points are coincidental; 1 if they are colinear; 2 if they are coplanar; 3 otherwise</returns>
        public static int ComputeBestFitPlane(List<Triple> points, out Triple planeOrigin, out Triple planeNormal, float tolerance = 1E-10f)
        {
            Triple centroid = Triple.Zero;
            for (int i = 0; i < points.Count; i++) centroid += points[i];
            centroid /= points.Count;

            float[,] P = new float[points.Count, 3];

            for (int i = 0; i < points.Count; i++)
            {
                P[i, 0] = points[i].X - centroid.X;
                P[i, 1] = points[i].Y - centroid.Y;
                P[i, 2] = points[i].Z - centroid.Z;
            }

            float c00 = 0f, c01 = 0f, c02 = 0f;
            float c10 = 0f, c11 = 0f, c12 = 0f;
            float c20 = 0f, c21 = 0f, c22 = 0f;

            for (int k = 0; k < points.Count; k++)
            {
                c00 += P[k, 0] * P[k, 0];
                c01 += P[k, 0] * P[k, 1];
                c02 += P[k, 0] * P[k, 2];
                c10 += P[k, 1] * P[k, 0];
                c11 += P[k, 1] * P[k, 1];
                c12 += P[k, 1] * P[k, 2];
                c20 += P[k, 2] * P[k, 0];
                c21 += P[k, 2] * P[k, 1];
                c22 += P[k, 2] * P[k, 2];
            }

            FastSvd3x3.Compute(
                c00, c01, c02,
                c10, c11, c12,
                c20, c21, c22,
                out float u00, out float u01, out float u02,
                out float u10, out float u11, out float u12,
                out float u20, out float u21, out float u22,
                out float s00, out float s11, out float s22,
                out float v00, out float v01, out float v02,
                out float v10, out float v11, out float v12,
                out float v20, out float v21, out float v22);

            planeOrigin = centroid;

           
            // Case 0: The input points are coincidental, so we just need to pick an arbitrary normal vector
            if (s00 < tolerance)
            {
                planeNormal = Triple.BasisZ;
                return 0;
            }

            // Case 1: The input points are colinear, so we just pick an arbitrary vector perpendicular to the dominant eigenvector
            if (s11 < tolerance)
            {
                planeNormal = new Triple(u00, u10, u20).GeneratePerpendicular();
                return 1;
            }

            // Case 2: The input points are neigher coincidental nor colinear, therefore the best fit plane is determined by the two dominant eigenvectors
            planeNormal = new Triple(u00, u10, u20).Cross(new Triple(u01, u11, u21)).Normalise();

            return s22 < tolerance
                ? 2  // The input points are coplanar
                : 3; // The input points are NOT coplanar
        }

        /// <summary>
        /// Approximate the circle that best fit a set of input points
        /// </summary>
        /// <param name="points">The input points</param>
        /// <param name="circleCenter">Center of the best fit circle</param>
        /// <param name="circleNormal">Normal of the best fit circle</param>
        /// <param name="circleRadius">Radius of the best fit circle</param>
        /// <param name="tolerance">The tolerance that is used the determined if the input points are coincidental, colinear, or non-colinear</param>
        /// <returns>False if the input points are coincidental or colinear; True otherwise</returns>
        public static bool ComputeBestFitCircle(List<Triple> points, out Triple circleCenter, out Triple circleNormal, out float circleRadius, float tolerance = 1E-10f)
        {
            // The core idea is to project the input points to the best fit plane, then fit a circle through these points via an analytical approach.
            // Reference: Thomas S & Chan Y: "A simple approach for the estimation of circular arc center and its radius

            int planeFittingResult = ComputeBestFitPlane(points, out Triple planeOrigin, out Triple planeNormal, tolerance);

            // Case 0: The input points are either coincidental or colinear
            if (planeFittingResult < 2) 
            {
                circleCenter = circleNormal = new Triple(float.NaN);
                circleRadius = float.NaN;
                return false;
            }

            // Case 1: The input points are neither coincidental nor colinear
            Triple planeBasisX = planeNormal.GeneratePerpendicular().Normalise();
            Triple planeBasisY = planeNormal.Cross(planeBasisX);

            float sumX = 0f;
            float sumY = 0f;
            float sumX2 = 0f;
            float sumY2 = 0f;
            float sumXY = 0f;
            float sumX3 = 0f;
            float sumY3 = 0f;
            float sumX2Y = 0f;
            float sumXY2 = 0f;

            float x, y;

            for (int i = 0; i < points.Count; i++)
            {
                Triple p = points[i] - planeOrigin;
                x = p.Dot(planeBasisX);
                y = p.Dot(planeBasisY);
                sumX += x;
                sumY += y;
                sumX2 += x * x;
                sumY2 += y * y;
                sumXY += x * y;
                sumX3 += x * x * x;
                sumY3 += y * y * y;
                sumX2Y += x * x * y;
                sumXY2 += x * y * y;
            }

            float n = points.Count;

            float a1 = 2f * (sumX * sumX - n * sumX2);
            float a2 = 2f * (sumX * sumY - n * sumXY);
            float b1 = a2;
            float b2 = 2f * (sumY * sumY - n * sumY2);
            float c1 = sumX2 * sumX - n * sumX3 + sumX * sumY2 - n * sumXY2;
            float c2 = sumX2 * sumY - n * sumY3 + sumY * sumY2 - n * sumX2Y;

            float temp = 1f / (a1 * b2 - a2 * b1);
            x = (c1 * b2 - c2 * b1) * temp;
            y = (a1 * c2 - a2 * c1) * temp;

            circleCenter = planeOrigin + (x * planeBasisX + y * planeBasisY);
            circleRadius = (float) Math.Sqrt((sumX2 - 2f * sumX * x + n * x * x + sumY2 - 2f * sumY * y + n * y * y) / n);
            circleNormal = planeNormal;
            return true;
        }

        /// <summary>
        /// Compute the sphere that best fit a set of input points (least squared orthogonal distance)
        /// </summary>
        /// <param name="points">The input points</param>
        /// <param name="sphereCenter">Center of the best fit sphere</param>
        /// <param name="sphereRadius">Radius of the best fit sphere</param>
        /// <param name="tolerance">The tolerance that is used the determined if the input points are coincidental, colinear, coplanar, or non-coplanar</param>
        /// <returns>False if the input points are coincidental, colinear or coplanar; True otherwise</returns>
        public static bool ComputeBestFitSphere(List<Triple> points, out Triple sphereCenter, out float sphereRadius, float tolerance = 1E-10f)
        {
            // Reference: Sumith YD: Fast Geometric Fit Algorithm for Sphere Using Exact Solution

            float sX = 0f;
            float sY = 0f;
            float sZ = 0f;

            float sXX = 0f;
            float sYY = 0f;
            float sZZ = 0f;

            float sXXX = 0f;
            float sYYY = 0f;
            float sZZZ = 0f;

            float sXY = 0f;
            float sXZ = 0f;
            float sYZ = 0f;

            float sXYY = 0f;
            float sXZZ = 0f;
            float sXXY = 0f;
            float sYZZ = 0f;
            float sXXZ = 0f;
            float sYYZ = 0f;

            for (int i = 0; i < points.Count; i++)
            {
                Triple p = points[i];

                sX += p.X;
                sY += p.Y;
                sZ += p.Z;

                sXX += p.X * p.X;
                sYY += p.Y * p.Y;
                sZZ += p.Z * p.Z;

                sXXX += p.X * p.X * p.X;
                sYYY += p.Y * p.Y * p.Y;
                sZZZ += p.Z * p.Z * p.Z;

                sXY += p.X * p.Y;
                sXZ += p.X * p.Z;
                sYZ += p.Y * p.Z;

                sXYY += p.X * p.Y * p.Y;
                sXZZ += p.X * p.Z * p.Z;
                sXXY += p.X * p.X * p.Y;
                sYZZ += p.Y * p.Z * p.Z;
                sXXZ += p.X * p.X * p.Z;
                sYYZ += p.Y * p.Y * p.Z;
            }

            float N = points.Count;

            float temp = sXX + sYY + sZZ;

            float a = 2f * sX * sX - 2f * N * sXX;
            float b = 2f * sX * sY - 2f * N * sXY;
            float c = 2f * sX * sZ - 2f * N * sXZ;
            float d = sX * temp - N * (sXXX + sXYY + sXZZ);

            float e = b;
            float f = 2f * sY * sY - 2f * N * sYY;
            float g = 2f * sY * sZ - 2f * N * sYZ;
            float h = sY * temp - N * (sXXY + sYYY + sYZZ);

            float j = c;
            float k = g;
            float l = 2f * sZ * sZ - 2f * N * sZZ;
            float m = sZ * temp - N * (sXXZ + sYYZ + sZZZ);

            float delta = a * (f * l - g * k) - e * (b * l - c * k) + j * (b * g - c * f);

            if (Math.Abs(delta) < tolerance)
            {
                sphereCenter = new Triple(float.NaN);
                sphereRadius = float.NaN;
                return false;
            }

            delta = 1f / delta;

            float x0 = delta * (d * (f * l - g * k) - h * (b * l - c * k) + m * (b * g - c * f));
            float y0 = delta * (a * (h * l - m * g) - e * (d * l - m * c) + j * (d * g - h * c));
            float z0 = delta * (a * (f * m - h * k) - e * (b * m - d * k) + j * (b * h - d * f));

            sphereCenter = new Triple(x0, y0, z0);
            sphereRadius = (float) Math.Sqrt(
                x0 * x0 + y0 * y0 + z0 * z0 +
                (temp - 2f * (x0 * sX + y0 * sY + z0 * sZ)) / N);

            return true;
        }
    }
}
