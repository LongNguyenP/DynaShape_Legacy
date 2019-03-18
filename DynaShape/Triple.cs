using System;
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
            X = (float)x;
            Y = (float)y;
            Z = (float)z;
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
            => "(" + Math.Round(X, decimalDigitsCount) + ", " + Math.Round(X, decimalDigitsCount) + ", " + Math.Round(X, decimalDigitsCount) + ")";


        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        public float LengthSquared => X * X + Y * Y + Z * Z;


        public bool IsZero => X == 0f && Y == 0f && Z == 0f;

        public bool IsAlmostZero(float threshold = 1E-12f)
            => -threshold < X && X < threshold && -threshold < Y && Y < -threshold && -threshold < Z && Z < threshold;


        public static Triple Zero => new Triple(0f, 0f, 0f);
        public static Triple One => new Triple(1f, 1f, 1f);
        public static Triple BasisX => new Triple(1f, 0f, 0f);
        public static Triple BasisY => new Triple(0f, 1f, 0f);
        public static Triple BasisZ => new Triple(0f, 0f, 1f);


        public static Triple operator +(Triple a, Triple b) => new Triple(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Triple operator -(Triple a, Triple b) => new Triple(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Triple operator -(Triple a) => new Triple(-a.X, -a.Y, -a.Z);
        public static Triple operator *(Triple a, Triple b) => new Triple(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        public static Triple operator *(Triple a, float b) => new Triple(a.X * b, a.Y * b, a.Z * b);
        public static Triple operator *(Triple a, double b) => new Triple(a.X * (float)b, a.Y * (float)b, a.Z * (float)b);
        public static Triple operator *(float b, Triple a) => new Triple(a.X * b, a.Y * b, a.Z * b);
        public static Triple operator *(double b, Triple a) => new Triple(a.X * (float)b, a.Y * (float)b, a.Z * (float)b);
        public static Triple operator /(Triple a, Triple b) => new Triple(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        public static Triple operator /(Triple a, float b) => new Triple(a.X / b, a.Y / b, a.Z / b);
        public static Triple operator /(Triple a, double b) => new Triple(a.X / (float)b, a.Y / (float)b, a.Z / (float)b);

        public float Dot(Triple t) => X * t.X + Y * t.Y + Z * t.Z;
        public Triple Cross(Triple t) => new Triple(Y * t.Z - Z * t.Y, Z * t.X - X * t.Z, X * t.Y - Y * t.X);


        public Triple Normalise()
        {
            float temp = 1f / (float)Math.Sqrt(X * X + Y * Y + Z * Z);
            return new Triple(X * temp, Y * temp, Z * temp);
        }


        public float DistanceTo(Triple t) => (float)Math.Sqrt((X - t.X) * (X - t.X) + (Y - t.Y) * (Y - t.Y) + (Z - t.Z) * (Z - t.Z));


        public Triple GeneratePerpendicular()
            => X == 0f && Y == 0f && Z == 0f
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
            if (X == 0f && Y == 0f && Z == 0f) throw new Exception("Cannot generate a perpendicular vector for a zero vector");
            return
                Math.Abs(X) < Math.Abs(Y)
                    ? Math.Abs(X) < Math.Abs(Z)
                        ? new Triple(0f, -Z, Y)
                        : new Triple(-Y, X, 0f)
                    : Math.Abs(Y) < Math.Abs(Z)
                        ? new Triple(Z, 0f, -X)
                        : new Triple(-Y, X, 0f);
        }


        public Triple Rotate(Triple origin, Triple axis, float angle)
        {
            Triple z = axis.Normalise();
            Triple x = z.GeneratePerpendicular().Normalise();
            Triple y = z.Cross(x).Normalise();

            Triple v = this - origin;

            float vx = x.Dot(v);
            float vy = y.Dot(v);
            float vz = z.Dot(v);

            float sin = (float)Math.Sin(angle);
            float cos = (float)Math.Cos(angle);

            float vx_ = cos * vx - sin * vy;
            float vy_ = sin * vx + cos * vy;

            return origin + x * vx_ + y * vy_ + z * vz;
        }
    }
}