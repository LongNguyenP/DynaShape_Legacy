using System;
using System.Collections.Generic;
using System.IO.Packaging;
using AForge.Math;
using Autodesk.DesignScript.Runtime;


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

            Matrix3x3 covariance = new Matrix3x3();

            for (int i = 0; i < NodeCount; i++)
            {
                covariance.V00 += targetShapePoints[i].X * targetShapePoints[i].X;
                covariance.V01 += targetShapePoints[i].X * targetShapePoints[i].Y;
                covariance.V02 += targetShapePoints[i].X * targetShapePoints[i].Z;

                covariance.V10 += targetShapePoints[i].Y * targetShapePoints[i].X;
                covariance.V11 += targetShapePoints[i].Y * targetShapePoints[i].Y;
                covariance.V12 += targetShapePoints[i].Y * targetShapePoints[i].Z;

                covariance.V20 += targetShapePoints[i].Z * targetShapePoints[i].X;
                covariance.V21 += targetShapePoints[i].Z * targetShapePoints[i].Y;
                covariance.V22 += targetShapePoints[i].Z * targetShapePoints[i].Z;
            }

            is2D = covariance.Determinant.IsAlmostZero();
        }



        internal void ShapeMatch(Triple[] positions)
        {
            // Here we compute the "best" translation, rotation (and optionally scalling) that bring the targetShapePoints as close as possible to the current node positions
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


            //==================================================================================================================
            // Compute the rotation matrix that bring the original rest positions to the current positions as close as possible
            //==================================================================================================================

            Matrix3x3 covariance = new Matrix3x3();

            for (int i = 0; i < NodeCount; i++)
            {
                covariance.V00 += positionsCentered[i].X * targetShapePoints[i].X;
                covariance.V01 += positionsCentered[i].X * targetShapePoints[i].Y;
                covariance.V02 += positionsCentered[i].X * targetShapePoints[i].Z;

                covariance.V10 += positionsCentered[i].Y * targetShapePoints[i].X;
                covariance.V11 += positionsCentered[i].Y * targetShapePoints[i].Y;
                covariance.V12 += positionsCentered[i].Y * targetShapePoints[i].Z;

                covariance.V20 += positionsCentered[i].Z * targetShapePoints[i].X;
                covariance.V21 += positionsCentered[i].Z * targetShapePoints[i].Y;
                covariance.V22 += positionsCentered[i].Z * targetShapePoints[i].Z;
            }

            Matrix3x3 u, v;
            Vector3 e;

            covariance.SVD(out u, out e, out v);

            Matrix3x3 R = u * v.Transpose();

            float indicator = 1f;

            if (R.Determinant < 0f && !is2D)
            {
                R.V20 *= -1f;
                R.V21 *= -1f;
                R.V22 *= -1f;
                indicator = -1f;
            }

            if (AllowScaling)
                R *= sigmaInversed * (indicator * e.X + e.Y + e.Z);


            //=========================================================================
            // Apply the rotation and translation to obtain the target rest positions,
            // And find the move vectors
            //=========================================================================

            for (int i = 0; i < NodeCount; i++)
            {
                Moves[i].X = R.V00 * targetShapePoints[i].X + R.V01 * targetShapePoints[i].Y + R.V02 * targetShapePoints[i].Z + center.X - positions[i].X;
                Moves[i].Y = R.V10 * targetShapePoints[i].X + R.V11 * targetShapePoints[i].Y + R.V12 * targetShapePoints[i].Z + center.Y - positions[i].Y;
                Moves[i].Z = R.V20 * targetShapePoints[i].X + R.V21 * targetShapePoints[i].Y + R.V22 * targetShapePoints[i].Z + center.Z - positions[i].Z;
            }
        }
    }
}
