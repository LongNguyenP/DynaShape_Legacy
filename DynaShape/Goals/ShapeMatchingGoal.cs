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
                targetShapePoints[i] = points[i] - center;

            float sigma = 0f;
            for (int i = 0; i < NodeCount; i++)
                sigma += targetShapePoints[i].LengthSquared;

            sigmaInversed = 1f / sigma;

            //===================================================================================================================
            // Determine if the target shape is 2D, as this case required special handling when doing shape matching operator
            //===================================================================================================================

            float p00 = 0f, p01 = 0f, p02 = 0f;
            float p10 = 0f, p11 = 0f, p12 = 0f;
            float p20 = 0f, p21 = 0f, p22 = 0f;

            for (int i = 0; i < NodeCount; i++)
            {
                p00 += targetShapePoints[i].X * targetShapePoints[i].X;
                p01 += targetShapePoints[i].X * targetShapePoints[i].Y;
                p02 += targetShapePoints[i].X * targetShapePoints[i].Z;
                p10 += targetShapePoints[i].Y * targetShapePoints[i].X;
                p11 += targetShapePoints[i].Y * targetShapePoints[i].Y;
                p12 += targetShapePoints[i].Y * targetShapePoints[i].Z;
                p20 += targetShapePoints[i].Z * targetShapePoints[i].X;
                p21 += targetShapePoints[i].Z * targetShapePoints[i].Y;
                p22 += targetShapePoints[i].Z * targetShapePoints[i].Z;
            }

            float determinant = p00 * (p11 * p22 - p21 * p12) -
                                p01 * (p10 * p22 - p20 * p12) +
                                p02 * (p10 * p21 - p20 * p11);

            is2D = determinant.IsAlmostZero();
        }

        private void ShapeMatch(Triple[] positions)
        {
            // Here we compute the "best" translation & rotation (and optionally scalling) that bring the targetShapePoints as close as possible to the current node positions
            // Reference: Umeyama S. 1991, Least-Squares Estimation of Transformation Paramters Between Two Point Patterns

            //=====================================================
            // Compute the center of the current node positions
            //=====================================================

            Triple center = Triple.Zero;
            for (int i = 0; i < NodeCount; i++) center += positions[i];
            center /= NodeCount;

            //=====================================================================
            // Mean Centering
            // i.e. Substract the current center from the current positions ...
            //=====================================================================

            Triple[] positionsCentered = new Triple[NodeCount];
            for (int i = 0; i < NodeCount; i++) positionsCentered[i] = positions[i] - center;

            //=====================================================================================================================
            // Compute the rotation matrix that bring the original rest positions to the current positions as close as possible
            //=====================================================================================================================

            float a00 = 0f, a01 = 0f, a02 = 0f;
            float a10 = 0f, a11 = 0f, a12 = 0f;
            float a20 = 0f, a21 = 0f, a22 = 0f;

            for (int i = 0; i < NodeCount; i++)
            {
                a00 += positionsCentered[i].X * targetShapePoints[i].X;
                a01 += positionsCentered[i].X * targetShapePoints[i].Y;
                a02 += positionsCentered[i].X * targetShapePoints[i].Z;
                a10 += positionsCentered[i].Y * targetShapePoints[i].X;
                a11 += positionsCentered[i].Y * targetShapePoints[i].Y;
                a12 += positionsCentered[i].Y * targetShapePoints[i].Z;
                a20 += positionsCentered[i].Z * targetShapePoints[i].X;
                a21 += positionsCentered[i].Z * targetShapePoints[i].Y;
                a22 += positionsCentered[i].Z * targetShapePoints[i].Z;
            }

            FastSvd3x3.Compute(
                a00, a01, a02,
                a10, a11, a12,
                a20, a21, a22,
                out float u00, out float u01, out float u02,
                out float u10, out float u11, out float u12,
                out float u20, out float u21, out float u22,
                out float s00, out float s11, out float s22,
                out float v00, out float v01, out float v02,
                out float v10, out float v11, out float v12,
                out float v20, out float v21, out float v22);

            // Compute the rotation matrix r, defined as u * transpose(v)
            float r00 = u00 * v00 + u01 * v01 + u02 * v02;
            float r01 = u00 * v10 + u01 * v11 + u02 * v12;
            float r02 = u00 * v20 + u01 * v21 + u02 * v22;
            float r10 = u10 * v00 + u11 * v01 + u12 * v02;
            float r11 = u10 * v10 + u11 * v11 + u12 * v12;
            float r12 = u10 * v20 + u11 * v21 + u12 * v22;
            float r20 = u20 * v00 + u21 * v01 + u22 * v02;
            float r21 = u20 * v10 + u21 * v11 + u22 * v12;
            float r22 = u20 * v20 + u21 * v21 + u22 * v22;
             
            float determinant = r00 * (r11 * r22 - r21 * r12) -
                                r01 * (r10 * r22 - r20 * r12) +
                                r02 * (r10 * r21 - r20 * r11);

            //Handle the special case where the rotation matrix happens to give a reflected version of target shape
            if (determinant < 0f && !is2D)
            {
                r20 = -r20;
                r21 = -r21;
                r22 = -r22;
            }

            // TODO: Check the correctness of scaling math below, especially the indicator
            if (AllowScaling)
            {
                float temp = sigmaInversed * ((determinant > 0 ? s00 : -s00) + s11 + s22); ;
                r00 *= temp; r01 *= temp; r02 *= temp;
                r10 *= temp; r11 *= temp; r12 *= temp;
                r20 *= temp; r21 *= temp; r22 *= temp;
            }

            //=================================================================================================
            // Apply the rotation and translation to obtain the target positions that the nodes will move to
            //=================================================================================================

            for (int i = 0; i < NodeCount; i++)
            {
                Moves[i].X = r00 * targetShapePoints[i].X + r01 * targetShapePoints[i].Y + r02 * targetShapePoints[i].Z + center.X - positions[i].X;
                Moves[i].Y = r10 * targetShapePoints[i].X + r11 * targetShapePoints[i].Y + r12 * targetShapePoints[i].Z + center.Y - positions[i].Y;
                Moves[i].Z = r20 * targetShapePoints[i].X + r21 * targetShapePoints[i].Y + r22 * targetShapePoints[i].Z + center.Z - positions[i].Z;
                Weights[i] = Weight;
            }
        }

        //private void ShapeMatchAForge(Triple[] positions)
        //{
        //    // Here we compute the "best" translation & rotation (and optionally scalling) that bring the targetShapePoints as close as possible to the current node positions
        //    // Reference: Umeyama S. 1991, Least-Squares Estimation of Transformation Paramters Between Two Point Patterns

        //    //==========================================================================================
        //    // Compute the center of the current node positions
        //    //==========================================================================================

        //    Triple center = Triple.Zero;
        //    for (int i = 0; i < NodeCount; i++) center += positions[i];
        //    center /= NodeCount;

        //    //==========================================================================================
        //    // Mean Centering
        //    // i.e. Substract the current center from the current positions ...
        //    //==========================================================================================

        //    Triple[] positionsCentered = new Triple[NodeCount];

        //    for (int i = 0; i < NodeCount; i++)
        //        positionsCentered[i] = new Triple(
        //           positions[i].X - center.X,
        //           positions[i].Y - center.Y,
        //           positions[i].Z - center.Z);

        //    //==================================================================================================================
        //    // Compute the rotation matrix that bring the original rest positions to the current positions as close as possible
        //    //==================================================================================================================

        //    float[,] u = new float[3, 3];

        //    for (int i = 0; i < NodeCount; i++)
        //    {
        //        u[0, 0] += positionsCentered[i].X * targetShapePoints[i].X;
        //        u[0, 1] += positionsCentered[i].X * targetShapePoints[i].Y;
        //        u[0, 2] += positionsCentered[i].X * targetShapePoints[i].Z;
        //        u[1, 0] += positionsCentered[i].Y * targetShapePoints[i].X;
        //        u[1, 1] += positionsCentered[i].Y * targetShapePoints[i].Y;
        //        u[1, 2] += positionsCentered[i].Y * targetShapePoints[i].Z;
        //        u[2, 0] += positionsCentered[i].Z * targetShapePoints[i].X;
        //        u[2, 1] += positionsCentered[i].Z * targetShapePoints[i].Y;
        //        u[2, 2] += positionsCentered[i].Z * targetShapePoints[i].Z;
        //    }

        //    if (!Util.ComputeSvd(u, out float[] e, out float[,] v))
        //    {
        //        // Svd computation did not converge :( !
        //        for (int i = 0; i < NodeCount; i++)
        //        {
        //            Moves[i].X = Moves[i].Y = Moves[i].Z = 0f;
        //            Weights[i] = Weight;
        //        }
        //        return;
        //    }

        //    // The rotation matrix, which equals u * transpose(v)
        //    float[,] r =
        //    {
        //        {
        //            u[0, 0] * v[0, 0] + u[0, 1] * v[0, 1] + u[0, 2] * v[0, 2],
        //            u[0, 0] * v[1, 0] + u[0, 1] * v[1, 1] + u[0, 2] * v[1, 2],
        //            u[0, 0] * v[2, 0] + u[0, 1] * v[2, 1] + u[0, 2] * v[2, 2]
        //        },
        //        {
        //            u[1, 0] * v[0, 0] + u[1, 1] * v[0, 1] + u[1, 2] * v[0, 2],
        //            u[1, 0] * v[1, 0] + u[1, 1] * v[1, 1] + u[1, 2] * v[1, 2],
        //            u[1, 0] * v[2, 0] + u[1, 1] * v[2, 1] + u[1, 2] * v[2, 2]
        //        },
        //        {
        //            u[2, 0] * v[0, 0] + u[2, 1] * v[0, 1] + u[2, 2] * v[0, 2],
        //            u[2, 0] * v[1, 0] + u[2, 1] * v[1, 1] + u[2, 2] * v[1, 2],
        //            u[2, 0] * v[2, 0] + u[2, 1] * v[2, 1] + u[2, 2] * v[2, 2]
        //        }
        //    };

        //    float indicator = 1f;

        //    // Handle the special case where the rotation matrix happens to give a reflected version of target shape
        //    if (Util.Determinant3x3(r) < 0f && !is2D)
        //    {
        //        r[2, 0] *= -1f;
        //        r[2, 1] *= -1f;
        //        r[2, 2] *= -1f;
        //        indicator = -1f;
        //    }

        //    // TODO: Check the correctness of scaling math below, especially the indicator
        //    if (AllowScaling)
        //    {
        //        float temp = sigmaInversed * (indicator * e[0] + e[1] + e[2]);;
        //        r[0, 0] *= temp; r[0, 1] *= temp; r[0, 2] *= temp;
        //        r[1, 0] *= temp; r[1, 1] *= temp; r[1, 2] *= temp;
        //        r[2, 0] *= temp; r[2, 1] *= temp; r[2, 2] *= temp;
        //    }

        //    //=========================================================================
        //    // Apply the rotation and translation to obtain the target rest positions,
        //    // And find the move vectors
        //    //=========================================================================

        //    for (int i = 0; i < NodeCount; i++)
        //    {
        //        Moves[i].X = r[0, 0] * targetShapePoints[i].X + r[0, 1] * targetShapePoints[i].Y + r[0, 2] * targetShapePoints[i].Z + center.X - positions[i].X;
        //        Moves[i].Y = r[1, 0] * targetShapePoints[i].X + r[1, 1] * targetShapePoints[i].Y + r[1, 2] * targetShapePoints[i].Z + center.Y - positions[i].Y;
        //        Moves[i].Z = r[2, 0] * targetShapePoints[i].X + r[2, 1] * targetShapePoints[i].Y + r[2, 2] * targetShapePoints[i].Z + center.Z - positions[i].Z;
        //        Weights[i] = Weight;
        //    }
        //}
    }
}
