using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using SharpDX;
using Matrix3x3 = AForge.Math.Matrix3x3;
using Point = Autodesk.DesignScript.Geometry.Point;
using Vector = Autodesk.DesignScript.Geometry.Vector;


namespace DynaShape
{
    [IsVisibleInDynamoLibrary(false)]
    public static class Util
    {
        public static Vector ZeroVector => Vector.ByCoordinates(0.0, 0.0, 0.0);
        public static Point Duplicate(this Point point) => Point.ByCoordinates(point.X, point.Y, point.Z);
        public static Vector Duplicate(this Triple vector) => Vector.ByCoordinates(vector.X, vector.Y, vector.Z);

        public static Triple ToTriple(this Point point) => new Triple(point.X, point.Y, point.Z);
        public static Triple ToTriple(this Vector vector) => new Triple(vector.X, vector.Y, vector.Z);

        public static bool IsAlmostZero(this float number, float tolerance = 1e-6f) => -tolerance < number && number < tolerance;
        public static bool IsAlmostZero(this double number, double tolerance = 1.0e-6) => -tolerance < number && number < tolerance;

        public static bool IsNotAlmostZero(this float number, float tolerance = 1e-6f) => -number < tolerance || tolerance < number;
        public static bool IsNotAlmostZero(this double number, double tolerance = 1.0e-6) => -number < tolerance || tolerance < number;

        public static SharpDX.Color ToSharpDXColor(this DSCore.Color color) 
            => new SharpDX.Color(color.Red * by255, color.Green * by255, color.Blue * by255, color.Alpha * by255);

        public static Vector3 ToVector3(this Triple triple)
            => new Vector3(triple.X, triple.Z, -triple.Y);

        private static float by255 = 1f / 255f;

        public static List<Triple> ToTriples(this IEnumerable<Point> points)
        {
            List<Triple> triples = new List<Triple>();
            foreach (Point point in points)
                triples.Add(new Triple(point.X, point.Y, point.Z));
            return triples;
        }


        public static List<Triple> ToTriples(this IEnumerable<Vector> vectors)
        {
            List<Triple> triples = new List<Triple>();
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

        private static float toRadian = (float)Math.PI / 180f;
        private static float toDegree = 180f / (float)Math.PI;

        public static float ToRadian(this float Degree) => Degree * toRadian;
        public static float ToDegree(this float Radian) => Radian * toDegree;

        /// <summary>
        /// Compute the line that best fit a set of input points (least squared orthogonal distance)
        /// </summary>
        /// <param name="points">The input points</param>
        /// <param name="lineOrigin">The output line origin</param>
        /// <param name="lineDirection">The output line direction</param>
        /// <returns>0 if the input points are identical, 1 if the input points are already colinear, 2 otherwise</returns>
        //public static int ComputeBestFitLine(List<Triple> points, out Triple lineOrigin, out Triple lineDirection)
        //{
        //    Triple centroid = Triple.Zero;
        //    for (int i = 0; i < points.Count; i++) centroid += points[i];
        //    centroid /= points.Count;

        //    float[,] P = new float[points.Count, 3];

        //    for (int i = 0; i < points.Count; i++)
        //    {
        //        P[i, 0] = points[i].X - centroid.X;
        //        P[i, 1] = points[i].Y - centroid.Y;
        //        P[i, 2] = points[i].Z - centroid.Z;
        //    }

        //    Matrix<float> covariance = Matrix<float>.Build.Dense(3, 3);

        //    for (int i = 0; i < 3; i++)
        //        for (int j = 0; j < 3; j++)
        //            for (int k = 0; k < points.Count; k++)
        //                covariance[i, j] += P[k, i] * P[k, j];

        //    Evd<float> evd = covariance.Evd();

        //    lineOrigin = centroid;

        //    if (evd.Rank == 0) // The input points are idendtical, so we just pick an arbitrary direction for the line
        //        lineDirection = Triple.BasisX;
        //    else // Otherwise the direction of the best fit line is the most dominant eigen vector 
        //        lineDirection = new Triple(evd.EigenVectors[0, 2], evd.EigenVectors[1, 2], evd.EigenVectors[2, 2]);

        //    return evd.Rank > 2 ? 2 : evd.Rank;
        //}


        public static int ComputeBestFitLine(List<Triple> points, out Triple lineOrigin, out Triple lineDirection)
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

            Matrix3x3 covariance = new Matrix3x3();

            for (int k = 0; k < points.Count; k++)
            {
                covariance.V00 += P[k, 0] * P[k, 0];
                covariance.V01 += P[k, 0] * P[k, 1];
                covariance.V02 += P[k, 0] * P[k, 2];
                covariance.V10 += P[k, 1] * P[k, 0];
                covariance.V11 += P[k, 1] * P[k, 1];
                covariance.V12 += P[k, 1] * P[k, 2];
                covariance.V20 += P[k, 2] * P[k, 0];
                covariance.V21 += P[k, 2] * P[k, 1];
                covariance.V22 += P[k, 2] * P[k, 2];
            }

            Matrix3x3 u, v;
            AForge.Math.Vector3 e;
            covariance.SVD(out u, out e, out v);

            lineOrigin = centroid;

            lineDirection = new Triple(u.V00, u.V10, u.V20);

            return 3;
        }



        /// <summary>
        /// Compute the plane that best fit a set of input points (least squared orthogonal distance)
        /// </summary>
        /// <param name="points">The input points</param>
        /// <param name="planeOrigin">The output plane origin</param>
        /// <param name="planeNormal">The output plane normal vector</param>
        /// <returns>0 if the input points are identical, 1 if the input points are colinear, 2 if the input points are coplanar (already on a plane), 3 otherwise</returns>
        //public static int ComputeBestFitPlane(List<Triple> points, out Triple planeOrigin, out Triple planeNormal)
        //{
        //    Triple centroid = Triple.Zero;
        //    for (int i = 0; i < points.Count; i++) centroid += points[i];
        //    centroid /= points.Count;

        //    float[,] P = new float[points.Count, 3];

        //    for (int i = 0; i < points.Count; i++)
        //    {
        //        P[i, 0] = points[i].X - centroid.X;
        //        P[i, 1] = points[i].Y - centroid.Y;
        //        P[i, 2] = points[i].Z - centroid.Z;
        //    }

        //    Matrix<float> covariance = Matrix<float>.Build.Dense(3, 3);

        //    for (int i = 0; i < 3; i++)
        //    for (int j = 0; j < 3; j++)
        //    for (int k = 0; k < points.Count; k++)
        //        covariance[i, j] += P[k, i] * P[k, j];

        //    Evd<float> evd = covariance.Evd();

        //    planeOrigin = centroid;

        //    if (evd.Rank == 0) // The input points are idendtical, so we just pick an arbitrary normal vector
        //        planeNormal = Triple.BasisZ;
        //    else if (evd.Rank == 1
        //    ) // The input points are colinear, so we just pick an arbitrary vector perpendicular to the only eigen vector
        //        planeNormal = new Triple(evd.EigenVectors[0, 1], evd.EigenVectors[1, 1], evd.EigenVectors[2, 1])
        //            .GeneratePerpendicular();
        //    else // The normal is perpendicular to the two dominant eigen vectors
        //    {
        //        Triple e1 = new Triple(evd.EigenVectors[0, 1], evd.EigenVectors[1, 1], evd.EigenVectors[2, 1]);
        //        Triple e2 = new Triple(evd.EigenVectors[0, 2], evd.EigenVectors[1, 2], evd.EigenVectors[2, 2]);
        //        planeNormal = e2.Cross(e1).Normalise();
        //    }

        //    return evd.Rank;
        //}
        public static int ComputeBestFitPlane(List<Triple> points, out Triple planeOrigin, out Triple planeNormal)
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

            Matrix3x3 covariance = new Matrix3x3();

            for (int k = 0; k < points.Count; k++)
            {
                covariance.V00 += P[k, 0] * P[k, 0];
                covariance.V01 += P[k, 0] * P[k, 1];
                covariance.V02 += P[k, 0] * P[k, 2];
                covariance.V10 += P[k, 1] * P[k, 0];
                covariance.V11 += P[k, 1] * P[k, 1];
                covariance.V12 += P[k, 1] * P[k, 2];
                covariance.V20 += P[k, 2] * P[k, 0];
                covariance.V21 += P[k, 2] * P[k, 1];
                covariance.V22 += P[k, 2] * P[k, 2];
            }

            Matrix3x3 u, v;
            AForge.Math.Vector3 e;
            covariance.SVD(out u, out e, out v);

            planeOrigin = centroid;

            Triple e1 = new Triple(u.V00, u.V10, u.V20);
            Triple e2 = new Triple(u.V01, u.V11, u.V21);
            planeNormal = e2.Cross(e1).Normalise();
            
            return 3;
        }

        public static bool ComputeBestFitCircle(List<Triple> points, out Triple circleCenter, out Triple circleNormal,
            out float circleRadius)
        {
            // Apporach: Project the input points to the best fit plane, then fit a circle through these points using the the analytical approach ...
            // ... described in the paper "A simple approach for the estimation of circular arc center and its radius" by Thomas S & Chan Y.

            Triple planeOrigin, planeNormal;
            int planeFittingResult = ComputeBestFitPlane(points, out planeOrigin, out planeNormal);

            if (planeFittingResult < 2) // The points are either all identical, or colinear (i.e. already on the same "circle" of infinite radius)
            {
                circleCenter = circleNormal = new Triple(float.NaN);
                circleRadius = float.NaN;
                return false;
            }

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
            circleRadius =
                (float) Math.Sqrt((sumX2 - 2f * sumX * x + n * x * x + sumY2 - 2f * sumY * y + n * y * y) / n);
            circleNormal = planeNormal;
            return true;
        }


        public static bool ComputeBestFitSphere(List<Triple> points, out Triple sphereCenter, out float sphereRadius)
        {
            // Based on "Fast Geometric Fit Algorithm for Sphere Using Exact Solution" by Sumith YD

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

            if (Math.Abs(delta) < 1e-10)
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
