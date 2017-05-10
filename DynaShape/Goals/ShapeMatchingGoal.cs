using System;
using System.Collections.Generic;
using System.IO.Packaging;
using Autodesk.DesignScript.Runtime;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;


namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public class ShapeMatchingGoal : Goal
    {
        private Triple[] targetShapePoints;
        private bool is2D;
        private float sigmaInversed;
        public bool AllowScaling;

        public ShapeMatchingGoal(List<Triple> nodeStartingPositions, List<Triple> targetShapePositions, bool allowScaling = false, float weight = 1f)
        {
            Weight = weight;
            StartingPositions = nodeStartingPositions.ToArray();
            Moves = new Triple[StartingPositions.Length];
            AllowScaling = allowScaling;
            SetTargetShapePoints(targetShapePositions);
        }


        public ShapeMatchingGoal(List<Triple> nodeStartingPositions, bool allowScaling = false, float weight = 1f)
           : this(nodeStartingPositions, nodeStartingPositions, allowScaling, weight)
        {
        }


        public override void Compute(List<Node> allNodes)
        {
            Triple[] positions = new Triple[NodeCount];
            for (int i = 0; i < NodeCount; i++) positions[i] = allNodes[NodeIndices[i]].Position;
            ShapeMatch(positions);
        }


        public void SetTargetShapePoints(List<Triple> points)
        {
            Triple center = Triple.Zero;
            for (int i = 0; i < NodeCount; i++) center += points[i];
            center /= NodeCount;

            targetShapePoints = new Triple[NodeCount];
            for (int i = 0; i < NodeCount; i++)
                targetShapePoints[i] = new Triple(points[i].X - center.X, points[i].Y - center.Y, points[i].Z - center.Z);

            float sigma = 0f;
            for (int i = 0; i < NodeCount; i++)
                sigma += targetShapePoints[i].LengthSquared;

            sigmaInversed = 1f / sigma;


            //===================================================================================================================
            // Determine if the target shape is 2D, as this case required special handling when doing shape matching operator
            //===================================================================================================================

            Matrix<float> covariance = Matrix<float>.Build.Dense(3, 3);

            for (int i = 0; i < NodeCount; i++)
            {
                covariance[0, 0] += targetShapePoints[i].X * targetShapePoints[i].X;
                covariance[0, 1] += targetShapePoints[i].X * targetShapePoints[i].Y;
                covariance[0, 2] += targetShapePoints[i].X * targetShapePoints[i].Z;

                covariance[1, 0] += targetShapePoints[i].Y * targetShapePoints[i].X;
                covariance[1, 1] += targetShapePoints[i].Y * targetShapePoints[i].Y;
                covariance[1, 2] += targetShapePoints[i].Y * targetShapePoints[i].Z;

                covariance[2, 0] += targetShapePoints[i].Z * targetShapePoints[i].X;
                covariance[2, 1] += targetShapePoints[i].Z * targetShapePoints[i].Y;
                covariance[2, 2] += targetShapePoints[i].Z * targetShapePoints[i].Z;
            }

            is2D = covariance.Determinant().IsAlmostZero();
        }



        internal void ShapeMatch(Triple[] positions)
        {
            // Here we compute the most optimal translation, rotation (and optionally scalling) that bring the targetShapePoints as close as possible to the current node positions
            // Reference: Umeyama S. 1991, Least-Squares Estimation of Transformation Paramters Between Two Point Patterns


            //==========================================================================================
            // Compute the center of the current node positions
            //==========================================================================================

            Triple center = Triple.Zero;
            for (int i = 0; i < NodeCount; i++) center += positions[i];
            center /= NodeCount;


            //==========================================================================================
            // Mean Centering
            // i.e. Substract the current center from the current positions ...
            //==========================================================================================

            Triple[] positionsCentered = new Triple[NodeCount];

            for (int i = 0; i < NodeCount; i++)
                positionsCentered[i] = new Triple(
                   positions[i].X - center.X,
                   positions[i].Y - center.Y,
                   positions[i].Z - center.Z);


            //================================================================================================================
            // Compute the rotation matrix that bring the original rest positions to the current positions as close as possible
            //================================================================================================================

            Matrix<float> covariance = Matrix<float>.Build.Dense(3, 3);

            for (int i = 0; i < NodeCount; i++)
            {
                covariance[0, 0] += positionsCentered[i].X * targetShapePoints[i].X;
                covariance[0, 1] += positionsCentered[i].X * targetShapePoints[i].Y;
                covariance[0, 2] += positionsCentered[i].X * targetShapePoints[i].Z;

                covariance[1, 0] += positionsCentered[i].Y * targetShapePoints[i].X;
                covariance[1, 1] += positionsCentered[i].Y * targetShapePoints[i].Y;
                covariance[1, 2] += positionsCentered[i].Y * targetShapePoints[i].Z;

                covariance[2, 0] += positionsCentered[i].Z * targetShapePoints[i].X;
                covariance[2, 1] += positionsCentered[i].Z * targetShapePoints[i].Y;
                covariance[2, 2] += positionsCentered[i].Z * targetShapePoints[i].Z;
            }

            Svd<float> svd = covariance.Svd();

            Matrix<float> R = svd.U * svd.VT;

            float indicator = 1f;

            if (R.Determinant() < 0f && !is2D)
            {
                R[2, 0] *= -1f;
                R[2, 1] *= -1f;
                R[2, 2] *= -1f;
                indicator = -1f;
            }

            if (AllowScaling)
                R *= sigmaInversed * (indicator * svd.S[0] + svd.S[1] + svd.S[2]);


            //=========================================================================
            // Apply the rotation and translation to obtain the target rest positions,
            // And find the move vectors
            //=========================================================================

            for (int i = 0; i < NodeCount; i++)
            {
                Moves[i].X = R[0, 0] * targetShapePoints[i].X + R[0, 1] * targetShapePoints[i].Y + R[0, 2] * targetShapePoints[i].Z + center.X - positions[i].X;
                Moves[i].Y = R[1, 0] * targetShapePoints[i].X + R[1, 1] * targetShapePoints[i].Y + R[1, 2] * targetShapePoints[i].Z + center.Y - positions[i].Y;
                Moves[i].Z = R[2, 0] * targetShapePoints[i].X + R[2, 1] * targetShapePoints[i].Y + R[2, 2] * targetShapePoints[i].Z + center.Z - positions[i].Z;
            }

        }
    }
}
