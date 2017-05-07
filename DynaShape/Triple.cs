using System;
using System.Security.Cryptography.X509Certificates;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace DynaShape
{
    [IsVisibleInDynamoLibrary(false)]
    public struct Triple
    {
        public float X, Y, Z;


        public Triple(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }


        public Triple(double x, double y, double z)
        {
            X = (float) x;
            Y = (float) y;
            Z = (float) z;
        }


        public Triple(float v)
        {
            X = Y = Z = v;
        }


        public Triple(Triple t)
        {
            X = t.X;
            Y = t.Y;
            Z = t.Z;
        }


        public override string ToString() => "(" + X + ", " + Y + ", " + Z + ")";


        public string ToString(int decimalDigitsCount)
            =>
                "(" + Math.Round(X, decimalDigitsCount) + ", " + Math.Round(X, decimalDigitsCount) + ", " +
                Math.Round(X, decimalDigitsCount) + ")";


        public float Length => (float) Math.Sqrt(X * X + Y * Y + Z * Z);
        public float LengthSquared => X * X + Y * Y + Z * Z;


        public bool IsZero => X == 0f && Y == 0f && Z == 0f;

        public bool IsAlmostZero(float threshold = 1E-12f)
            => -threshold < X && X < threshold && -threshold < Y && Y < -threshold && -threshold < Z && Z < threshold;


        public static Triple Zero => new Triple(0f, 0f, 0f);
        public static Triple One => new Triple(1f, 1f, 1f);
        public static Triple BasisX => new Triple(1f, 0f, 0f);
        public static Triple BasisY => new Triple(0f, 1f, 0f);
        public static Triple BasisZ => new Triple(0f, 0f, 1f);


        public static Triple FromPoint(Point p) => new Triple((float) p.X, (float) p.Y, (float) p.Z);
        public static Triple FromVector(Point v) => new Triple((float) v.X, (float) v.Y, (float) v.Z);


        public Point ToPoint() => Point.ByCoordinates(X, Y, Z);
        public Vector ToVector() => Vector.ByCoordinates(X, Y, Z);


        public Triple Duplicate(Triple a) => new Triple(a.X, a.Y, a.Z);


        public static Triple operator +(Triple a, Triple b) => new Triple(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Triple operator -(Triple a, Triple b) => new Triple(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Triple operator -(Triple a) => new Triple(-a.X, -a.Y, -a.Z);
        public static Triple operator *(Triple a, Triple b) => new Triple(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        public static Triple operator *(Triple a, float b) => new Triple(a.X * b, a.Y * b, a.Z * b);
        public static Triple operator *(float b, Triple a) => new Triple(a.X * b, a.Y * b, a.Z * b);
        public static Triple operator /(Triple a, Triple b) => new Triple(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        public static Triple operator /(Triple a, float b) => new Triple(a.X / b, a.Y / b, a.Z / b);


        public float Dot(Triple t) => X * t.X + Y * t.Y + Z * t.Z;
        public Triple Cross(Triple t) => new Triple(Y * t.Z - Z * t.Y, Z * t.X - X * t.Z, X * t.Y - Y * t.X);


        public Triple Normalise()
        {
            float temp = 1f / (float) Math.Sqrt(X * X + Y * Y + Z * Z);
            return new Triple(X * temp, Y * temp, Z * temp);
        }


        public float DistanceTo(Triple t)
            => (float) Math.Sqrt((X - t.X) * (X - t.X) + (Y - t.Y) * (Y - t.Y) + (Z - t.Z) * (Z - t.Z));


        public Triple GeneratePerpendicular()
            =>
                IsZero
                    ? new Triple(0f, 0f, 0f)
                    : Math.Abs(X) < Math.Abs(Y)
                        ? Math.Abs(X) < Math.Abs(Z)
                            ? new Triple(0f, -Z, Y)
                            : new Triple(-Y, X, 0f)
                        : Math.Abs(Y) < Math.Abs(Z)
                            ? new Triple(Z, 0f, -X)
                            : new Triple(-Y, X, 0f);


        public Triple TryGeneratePerpendicular()
        {
            if (IsZero) throw new Exception("Cannot genernate perpendicular vector for a zero vector");
            return
                Math.Abs(X) < Math.Abs(Y)
                    ? Math.Abs(X) < Math.Abs(Z)
                        ? new Triple(0f, -Z, Y)
                        : new Triple(-Y, X, 0f)
                    : Math.Abs(Y) < Math.Abs(Z)
                        ? new Triple(Z, 0f, -X)
                        : new Triple(-Y, X, 0f);
        }
    }
}