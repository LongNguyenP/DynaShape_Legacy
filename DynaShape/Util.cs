using System;
using System.Collections.Generic;
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
        public static Vector ZeroVector => Vector.ByCoordinates(0.0, 0.0, 0.0);
        public static Point Duplicate(this Point point) => Point.ByCoordinates(point.X, point.Y, point.Z);
        public static Vector Duplicate(this Vector vector) => Vector.ByCoordinates(vector.X, vector.Y, vector.Z);

        public static Triple ToTriple(this Point point) => new Triple(point.X, point.Y, point.Z);
        public static Triple ToTriple(this Vector vector) => new Triple(vector.X, vector.Y, vector.Z);

        public static bool IsAlmostZero(this float number, float tolerance = 1E-10f) => -tolerance < number && number < tolerance;
        public static bool IsAlmostZero(this double number, double tolerance = 1E-10f) => -tolerance < number && number < tolerance;

        public static bool IsNotAlmostZero(this float number, float tolerance = 1E-10f) => -number < tolerance || tolerance < number;
        public static bool IsNotAlmostZero(this double number, double tolerance = 1E-10f) => -number < tolerance || tolerance < number;

        public static SharpDX.Color ToSharpDXColor(this DSCore.Color color) 
            => new SharpDX.Color(color.Red * by255, color.Green * by255, color.Blue * by255, color.Alpha * by255);

        public static Vector3 ToVector3(this Triple triple)
            => new Vector3(triple.X, triple.Z, -triple.Y);

        private static readonly float by255 = 1f / 255f;

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

            float[,] covariance = new float[3, 3];

            for (int k = 0; k < points.Count; k++)
            {
                covariance[0, 0] += P[k, 0] * P[k, 0];
                covariance[0, 1] += P[k, 0] * P[k, 1];
                covariance[0, 2] += P[k, 0] * P[k, 2];
                covariance[1, 0] += P[k, 1] * P[k, 0];
                covariance[1, 1] += P[k, 1] * P[k, 1];
                covariance[1, 2] += P[k, 1] * P[k, 2];
                covariance[2, 0] += P[k, 2] * P[k, 0];
                covariance[2, 1] += P[k, 2] * P[k, 1];
                covariance[2, 2] += P[k, 2] * P[k, 2];
            }

            ComputeSvd(covariance, out float[] e, out float[,] _);
            float[,] u = covariance;

            lineOrigin = centroid;

            int eMaxIndex =
                e[0] > e[1]
                    ? e[0] > e[2] ? 0 : 2
                    : e[1] > e[2] ? 1 : 2;

            int eMinIndex =
                e[0] < e[1]
                    ? e[0] < e[2] ? 0 : 2
                    : e[1] < e[2] ? 1 : 2;

            int eMidIndex = 3 - eMaxIndex - eMinIndex;

            // Case 0: The input points are coincidental, so we just need to pick an arbitrary line direction
            if (Math.Abs(e[eMaxIndex]) < tolerance)
            {
                lineDirection = Triple.BasisX;
                return 0;
            }

            // Case 1: The input points are not coincidental, therefore we pick the dominant eigen vector as the line direction
            lineDirection = new Triple(u[0, eMaxIndex], u[1, eMaxIndex], u[2, eMaxIndex]);
            return Math.Abs(e[eMidIndex]) < tolerance
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

            float[,] covariance = new float[3,3];

            for (int k = 0; k < points.Count; k++)
            {
                covariance[0, 0] += P[k, 0] * P[k, 0];
                covariance[0, 1] += P[k, 0] * P[k, 1];
                covariance[0, 2] += P[k, 0] * P[k, 2];
                covariance[1, 0] += P[k, 1] * P[k, 0];
                covariance[1, 1] += P[k, 1] * P[k, 1];
                covariance[1, 2] += P[k, 1] * P[k, 2];
                covariance[2, 0] += P[k, 2] * P[k, 0];
                covariance[2, 1] += P[k, 2] * P[k, 1];
                covariance[2, 2] += P[k, 2] * P[k, 2];
            }

            ComputeSvd(covariance, out float[] e, out float[,] _);
            float[,] u = covariance;

            planeOrigin = centroid;

            int eMaxIndex =
                e[0] > e[1]
                    ? e[0] > e[2] ? 0 : 2
                    : e[1] > e[2] ? 1 : 2;

            int eMinIndex =
                e[0] < e[1]
                    ? e[0] < e[2] ? 0 : 2
                    : e[1] < e[2] ? 1 : 2;

            int eMidIndex = 3 - eMaxIndex - eMinIndex;

            // Case 0: The input points are coincidental, so we just need to pick an arbitrary normal vector
            if (Math.Abs(e[eMaxIndex]) < tolerance)
            {
                planeNormal = Triple.BasisZ;
                return 0;
            }

            // Case 1: The input points are colinear, so we just pick an arbitrary vector perpendicular to the dominant eigenvector
            if (Math.Abs(e[eMidIndex]) < tolerance)
            {
                planeNormal = new Triple(u[0, eMaxIndex], u[1, eMaxIndex], u[2, eMaxIndex]).GeneratePerpendicular();
                return 1;
            }

            // Case 2: The input points are neigher coincidental nor colinear, therefore the best fit plane is determined by the two dominant eigenvectors
            Triple eMax = new Triple(u[0, eMaxIndex], u[1, eMaxIndex], u[2, eMaxIndex]);
            Triple eMid = new Triple(u[0, eMidIndex], u[1, eMidIndex], u[2, eMidIndex]);

            planeNormal = eMax.Cross(eMid).Normalise();

            return Math.Abs(e[eMinIndex]) < tolerance
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

        internal static bool ComputeSvd(float[,] a, out float[] w, out float[,] v)
        {
            int m = a.GetLength(0); // Row count
            int n = a.GetLength(1); // Column count

            if (m < n) throw new ArgumentException("Number of rows in A must be greater or equal to number of columns");

            w = new float[n];
            v = new float[n, n];

            int flag, i, its, j, jj, k, l = 0, nm = 0;
            float anorm, c, f, g, h, s, scale, x, y, z;

            float[] rv1 = new float[n];

            // householder reduction to bidiagonal form
            g = scale = anorm = 0f;

            for (i = 0; i < n; i++)
            {
                l = i + 1;
                rv1[i] = scale * g;
                g = s = scale = 0;

                if (i < m)
                {
                    for (k = i; k < m; k++) scale += Math.Abs(a[k, i]);

                    if (scale != 0f)
                    {
                        for (k = i; k < m; k++)
                        {
                            a[k, i] /= scale;
                            s += a[k, i] * a[k, i];
                        }

                        f = a[i, i];
                        g = -Sign((float)Math.Sqrt(s), f);
                        h = f * g - s;
                        a[i, i] = f - g;

                        if (i != n - 1)
                            for (j = l; j < n; j++)
                            {
                                for (s = 0f, k = i; k < m; k++) s += a[k, i] * a[k, j];
                                f = s / h;
                                for (k = i; k < m; k++) a[k, j] += f * a[k, i];
                            }

                        for (k = i; k < m; k++) a[k, i] *= scale;
                    }
                }

                w[i] = scale * g;
                g = s = scale = 0f;

                if ((i < m) && (i != n - 1))
                {
                    for (k = l; k < n; k++) scale += Math.Abs(a[i, k]);

                    if (scale != 0.0)
                    {
                        for (k = l; k < n; k++)
                        {
                            a[i, k] /= scale;
                            s += a[i, k] * a[i, k];
                        }

                        f = a[i, l];
                        g = -Sign((float)Math.Sqrt(s), f);
                        h = f * g - s;
                        a[i, l] = f - g;

                        for (k = l; k < n; k++) rv1[k] = a[i, k] / h;

                        if (i != m - 1)
                            for (j = l; j < m; j++)
                            {
                                for (s = 0f, k = l; k < n; k++) s += a[j, k] * a[i, k];
                                for (k = l; k < n; k++) a[j, k] += s * rv1[k];
                            }

                        for (k = l; k < n; k++) a[i, k] *= scale;
                    }
                }
                anorm = Math.Max(anorm, (Math.Abs(w[i]) + Math.Abs(rv1[i])));
            }

            // accumulation of right-hand transformations
            for (i = n - 1; i >= 0; i--)
            {
                if (i < n - 1)
                {
                    if (g != 0.0)
                    {
                        for (j = l; j < n; j++) v[j, i] = (a[i, j] / a[i, l]) / g;
                        for (j = l; j < n; j++)
                        {
                            for (s = 0, k = l; k < n; k++) s += a[i, k] * v[k, j];
                            for (k = l; k < n; k++) v[k, j] += s * v[k, i];
                        }
                    }

                    for (j = l; j < n; j++) v[i, j] = v[j, i] = 0;
                }

                v[i, i] = 1;
                g = rv1[i];
                l = i;
            }

            // accumulation of left-hand transformations
            for (i = n - 1; i >= 0; i--)
            {
                l = i + 1;
                g = w[i];

                if (i < n - 1)
                    for (j = l; j < n; j++) a[i, j] = 0f;

                if (g != 0)
                {
                    g = 1f / g;
                    if (i != n - 1)
                    {
                        for (j = l; j < n; j++)
                        {
                            for (s = 0, k = l; k < m; k++) s += a[k, i] * a[k, j];
                            f = (s / a[i, i]) * g;
                            for (k = i; k < m; k++) a[k, j] += f * a[k, i];
                        }
                    }

                    for (j = i; j < m; j++) a[j, i] *= g;
                }
                else
                    for (j = i; j < m; j++) a[j, i] = 0;
                
                ++a[i, i];
            }

            // diagonalization of the bidiagonal form: Loop over singular values
            // and over allowed iterations
            for (k = n - 1; k >= 0; k--)
            {
                for (its = 1; its <= 30; its++)
                {
                    flag = 1;

                    for (l = k; l >= 0; l--)
                    {
                        // test for splitting
                        nm = l - 1;

                        if (Math.Abs(rv1[l]) + anorm == anorm)
                        {
                            flag = 0;
                            break;
                        }

                        if (Math.Abs(w[nm]) + anorm == anorm) break;
                    }

                    if (flag != 0)
                    {
                        s = 1f;
                        for (i = l; i <= k; i++)
                        {
                            f = s * rv1[i];

                            if (Math.Abs(f) + anorm != anorm)
                            {
                                g = w[i];
                                h = Pythag(f, g);
                                w[i] = h;
                                h = 1f / h;
                                c = g * h;
                                s = -f * h;

                                for (j = 0; j < m; j++)
                                {
                                    y = a[j, nm];
                                    z = a[j, i];
                                    a[j, nm] = y * c + z * s;
                                    a[j, i] = z * c - y * s;
                                }
                            }
                        }
                    }

                    z = w[k];

                    if (l == k)
                    {
                        // convergence
                        if (z < 0.0)
                        {
                            // singular value is made nonnegative
                            w[k] = -z;
                            for (j = 0; j < n; j++) v[j, k] = -v[j, k];
                        }
                        break;
                    }

                    if (its == 30) return false;

                    // shift from bottom 2-by-2 minor
                    x = w[l];
                    nm = k - 1;
                    y = w[nm];
                    g = rv1[nm];
                    h = rv1[k];
                    f = ((y - z) * (y + z) + (g - h) * (g + h)) / (2f* h * y);
                    g = Pythag(f, 1f);
                    f = ((x - z) * (x + z) + h * ((y / (f + Sign(g, f))) - h)) / x;

                    // next QR transformation
                    c = s = 1f;

                    for (j = l; j <= nm; j++)
                    {
                        i = j + 1;
                        g = rv1[i];
                        y = w[i];
                        h = s * g;
                        g = c * g;
                        z = Pythag(f, h);
                        rv1[j] = z;
                        c = f / z;
                        s = h / z;
                        f = x * c + g * s;
                        g = g * c - x * s;
                        h = y * s;
                        y *= c;

                        for (jj = 0; jj < n; jj++)
                        {
                            x = v[jj, j];
                            z = v[jj, i];
                            v[jj, j] = x * c + z * s;
                            v[jj, i] = z * c - x * s;
                        }

                        z = Pythag(f, h);
                        w[j] = z;

                        if (z != 0)
                        {
                            z = 1f / z;
                            c = f * z;
                            s = h * z;
                        }

                        f = c * g + s * y;
                        x = c * y - s * g;

                        for (jj = 0; jj < m; jj++)
                        {
                            y = a[jj, j];
                            z = a[jj, i];
                            a[jj, j] = y * c + z * s;
                            a[jj, i] = z * c - y * s;
                        }
                    }

                    rv1[l] = 0f;
                    rv1[k] = f;
                    w[k] = x;

                   
                }
            }

            return true;
        }

        private static float Sign(float a, float b)
        {
            return (b >= 0.0) ? Math.Abs(a) : -Math.Abs(a);
        }

        private static float Pythag(float a, float b)
        {
            float at = Math.Abs(a), bt = Math.Abs(b), ct;

            if (at > bt)
            {
                ct = bt / at;
                return at * (float)Math.Sqrt(1f + ct * ct);
            }

            if (bt > 0.0)
            {
                ct = at / bt;
                return bt * (float)Math.Sqrt(1f + ct * ct);
            }

            return 0f;
        }

        internal static float Determinant3x3(float[,] a)
        {
            return
                a[0, 0] * (a[1, 1] * a[2, 2] - a[2, 1] * a[1, 2]) -
                a[0, 1] * (a[1, 0] * a[2, 2] - a[2, 0] * a[1, 2]) +
                a[0, 2] * (a[1, 0] * a[2, 1] - a[2, 0] * a[1, 1]);
        }

        public static string DynaShapeName()
        {
            return "DynaShape";
        }
    }
}
