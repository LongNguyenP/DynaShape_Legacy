using System;
using System.Collections.Generic;
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
            Weights = new float[StartingPositions.Length];
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

            float[,] p = new float[3, 3];

            for (int i = 0; i < NodeCount; i++)
            {
                p[0, 0] += targetShapePoints[i].X * targetShapePoints[i].X;
                p[0, 1] += targetShapePoints[i].X * targetShapePoints[i].Y;
                p[0, 2] += targetShapePoints[i].X * targetShapePoints[i].Z;
                p[1, 0] += targetShapePoints[i].Y * targetShapePoints[i].X;
                p[1, 1] += targetShapePoints[i].Y * targetShapePoints[i].Y;
                p[1, 2] += targetShapePoints[i].Y * targetShapePoints[i].Z;
                p[2, 0] += targetShapePoints[i].Z * targetShapePoints[i].X;
                p[2, 1] += targetShapePoints[i].Z * targetShapePoints[i].Y;
                p[2, 2] += targetShapePoints[i].Z * targetShapePoints[i].Z;
            }

            is2D = Util.Determinant3x3(p).IsAlmostZero();
        }

        private void ShapeMatch(Triple[] positions)
        {
            // Here we compute the "best" translation & rotation (and optionally scalling) that bring the targetShapePoints as close as possible to the current node positions
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

            float[,] u = new float[3, 3];

            for (int i = 0; i < NodeCount; i++)
            {
                u[0, 0] += positionsCentered[i].X * targetShapePoints[i].X;
                u[0, 1] += positionsCentered[i].X * targetShapePoints[i].Y;
                u[0, 2] += positionsCentered[i].X * targetShapePoints[i].Z;
                u[1, 0] += positionsCentered[i].Y * targetShapePoints[i].X;
                u[1, 1] += positionsCentered[i].Y * targetShapePoints[i].Y;
                u[1, 2] += positionsCentered[i].Y * targetShapePoints[i].Z;
                u[2, 0] += positionsCentered[i].Z * targetShapePoints[i].X;
                u[2, 1] += positionsCentered[i].Z * targetShapePoints[i].Y;
                u[2, 2] += positionsCentered[i].Z * targetShapePoints[i].Z;
            }

            if (!Util.ComputeSvd(u, out float[] e, out float[,] v))
            {
                // Svd computation did not converge :( !
                for (int i = 0; i < NodeCount; i++)
                {
                    Moves[i].X = Moves[i].Y = Moves[i].Z = 0f;
                    Weights[i] = Weight;
                }
                return;
            }

            float[,] r = new float[3, 3]; // This is the rotation matrix
            
            r[0, 0] += u[0, 0] * v[0, 0]; r[0, 0] += u[0, 1] * v[0, 1]; r[0, 0] += u[0, 2] * v[0, 2];
            r[0, 1] += u[0, 0] * v[1, 0]; r[0, 1] += u[0, 1] * v[1, 1]; r[0, 1] += u[0, 2] * v[1, 2];
            r[0, 2] += u[0, 0] * v[2, 0]; r[0, 2] += u[0, 1] * v[2, 1]; r[0, 2] += u[0, 2] * v[2, 2];

            r[1, 0] += u[1, 0] * v[0, 0]; r[1, 0] += u[1, 1] * v[0, 1]; r[1, 0] += u[1, 2] * v[0, 2];
            r[1, 1] += u[1, 0] * v[1, 0]; r[1, 1] += u[1, 1] * v[1, 1]; r[1, 1] += u[1, 2] * v[1, 2];
            r[1, 2] += u[1, 0] * v[2, 0]; r[1, 2] += u[1, 1] * v[2, 1]; r[1, 2] += u[1, 2] * v[2, 2];

            r[2, 0] += u[2, 0] * v[0, 0]; r[2, 0] += u[2, 1] * v[0, 1]; r[2, 0] += u[2, 2] * v[0, 2];
            r[2, 1] += u[2, 0] * v[1, 0]; r[2, 1] += u[2, 1] * v[1, 1]; r[2, 1] += u[2, 2] * v[1, 2];
            r[2, 2] += u[2, 0] * v[2, 0]; r[2, 2] += u[2, 1] * v[2, 1]; r[2, 2] += u[2, 2] * v[2, 2];

            float indicator = 1f;

            // Handle the special case where the rotation matrix happens to give a reflected version of target shape 
            if (Util.Determinant3x3(r) < 0f && !is2D)
            {
                r[2, 0] *= -1f;
                r[2, 1] *= -1f;
                r[2, 2] *= -1f;
                indicator = -1f;
            }

            // TODO: Check the correctness of scaling math below, especially the indicator
            if (AllowScaling)
            {
                float temp = sigmaInversed * (indicator * e[0] + e[1] + e[2]);;
                r[0, 0] *= temp; r[0, 1] *= temp; r[0, 2] *= temp;
                r[1, 0] *= temp; r[1, 1] *= temp; r[1, 2] *= temp;
                r[2, 0] *= temp; r[2, 1] *= temp; r[2, 2] *= temp;
            }

            //=========================================================================
            // Apply the rotation and translation to obtain the target rest positions,
            // And find the move vectors
            //=========================================================================

            for (int i = 0; i < NodeCount; i++)
            {
                Moves[i].X = r[0, 0] * targetShapePoints[i].X + r[0, 1] * targetShapePoints[i].Y + r[0, 2] * targetShapePoints[i].Z + center.X - positions[i].X;
                Moves[i].Y = r[1, 0] * targetShapePoints[i].X + r[1, 1] * targetShapePoints[i].Y + r[1, 2] * targetShapePoints[i].Z + center.Y - positions[i].Y;
                Moves[i].Z = r[2, 0] * targetShapePoints[i].X + r[2, 1] * targetShapePoints[i].Y + r[2, 2] * targetShapePoints[i].Z + center.Z - positions[i].Z;
                Weights[i] = Weight;
            }
        }
    }
}
