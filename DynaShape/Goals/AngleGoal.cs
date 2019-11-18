using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;


namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public class AngleGoal : Goal
    {
        public float TargetAngle = 0f;

        public AngleGoal(Triple A, Triple B, Triple C, float targetAngle, float weight = 1f)
        {
            Weight = weight;
            TargetAngle = targetAngle;
            StartingPositions = new[] { A, B, C };
            Moves = new Triple[3];
            Moves[1] = Triple.Zero;
            Weights = new float[3];
        }


        public AngleGoal(Triple A, Triple B, Triple C, float weight = 1000f)
        {
            Weight = weight;
            StartingPositions = new[] { A, B, C };
            Moves = new Triple[3];
            Moves[1] = Triple.Zero;

            Triple BA = A - B;
            Triple BC = C - B;
            TargetAngle = (float)Math.Acos(BA.Dot(BC) / (BA.Length * BC.Length));
        }


        internal override void Compute(List<Node> allNodes)
        {
            Triple A = allNodes[NodeIndices[0]].Position;
            Triple B = allNodes[NodeIndices[1]].Position;
            Triple C = allNodes[NodeIndices[2]].Position;

            Triple BA = A - B;
            Triple BC = C - B;

            Triple m = (BA + BC).Normalise();

            Triple N = BA.Cross(BC);

            if (N.IsAlmostZero()) N = Triple.BasisX;

            Triple mA = m.Rotate(Triple.Zero, N, -0.5f * TargetAngle);
            Triple mB = m.Rotate(Triple.Zero, N, +0.5f * TargetAngle);


            Moves[0] = B + m * mA.Dot(BA) - A;
            Moves[2] = B + m * mB.Dot(BC) - C;

            Weights[0] = Weights[1] = Weights[2] = Weight;
        }
    }
}
