using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using DynaShape.Goals;


namespace DynaShape.ZeroTouch
{
    public static class Goals
    {
        //==================================================================
        // Change the weight value of a goal (of any type)
        //==================================================================

        public static Goal Goal_ChangeWeight(Goal goal, double weight)
        {
            goal.Weight = (float)weight;
            return goal;
        }


        //==================================================================
        // Anchor
        //==================================================================

        public static AnchorGoal AnchorGoal_Create(
            Point startPosition,
            [DefaultArgument("null")] Point anchor,
            [DefaultArgument("1000.0")] double weight)
        {
            return new AnchorGoal(
                startPosition.ToTriple(),
                anchor?.ToTriple() ?? startPosition.ToTriple(),
                (float)weight);
        }


        public static AnchorGoal AnchorGoal_Change(
            AnchorGoal anchorGoal,
            [DefaultArgument("null")] Point anchor,
            [DefaultArgument("-1.0")] double weight)
        {
            if (anchor != null) anchorGoal.Anchor = anchor.ToTriple();
            if (weight >= 0.0) anchorGoal.Weight = (float)weight;
            return anchorGoal;
        }


        //==================================================================
        // CoCircular
        //==================================================================

        public static CoCircularGoal CoCircularGoal_Create(
           List<Point> startPositions,
           [DefaultArgument("1.0")] double weight)
        {
            return new CoCircularGoal(startPositions.ToTriples(), (float)weight);
        }


        public static CoCircularGoal CoCircularGoal_Change(
           CoCircularGoal cyclicPolygonGoal,
           [DefaultArgument("-1.0")] double weight)
        {
            if (weight >= 0.0) cyclicPolygonGoal.Weight = (float)weight;
            return cyclicPolygonGoal;
        }


        //==================================================================
        // CoLinear
        //==================================================================

        public static CoLinearGoal CoLinearGoal_Create(
              List<Point> startPositions,
              [DefaultArgument("1000.0")] double weight)
        {
            return new CoLinearGoal(startPositions.ToTriples(), (float)weight);
        }


        public static CoLinearGoal CoLinearGoal_Change(
            CoLinearGoal coLinearGoal,
            [DefaultArgument("-1.0")] double weight)
        {
            if (weight >= 0.0) coLinearGoal.Weight = (float)weight;
            return coLinearGoal;
        }


        //==================================================================
        // Constant
        //==================================================================

        public static ConstantGoal ConstantGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("Vector.ByCoordinates(0, 0, -0.1)")] Vector constant,
            [DefaultArgument("1.0")] double weight)
        {
            return new ConstantGoal(startPositions.ToTriples(), constant.ToTriple(), (float)weight);
        }

        public static ConstantGoal ConstantGoal_Change(
            ConstantGoal constantGoal,
            [DefaultArgument("null")] Vector constant,
            [DefaultArgument("-1.0")] double weight)
        {
            if (constant != null) constantGoal.Move = constant.ToTriple();
            if (weight >= 0.0) constantGoal.Weight = (float)weight;
            return constantGoal;
        }


        //==================================================================
        // CoPlanar
        //==================================================================

        public static CoPlanarGoal CoPlanarGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("1.0")] double weight)
        {
            return new CoPlanarGoal(startPositions.ToTriples(), (float)weight);
        }


        public static CoPlanarGoal CoPlanarGoal_Change(
            CoPlanarGoal coPlanarGoal,
            [DefaultArgument("-1.0")] double weight)
        {
            if (weight >= 0.0) coPlanarGoal.Weight = (float)weight;
            return coPlanarGoal;
        }


        //==================================================================
        // CoSpherical
        //==================================================================

        public static CoSphericalGoal CoSphericalGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("1.0")] double weight)
        {
            return new CoSphericalGoal(startPositions.ToTriples(), (float)weight);
        }


        public static CoSphericalGoal CoSphericalGoal_Change(
            CoSphericalGoal coSphericalGoal,
            [DefaultArgument("-1.0")] double weight)
        {
            if (weight >= 0.0) coSphericalGoal.Weight = (float)weight;
            return coSphericalGoal;
        }


        //==================================================================
        // Direction
        //==================================================================

        public static DirectionGoal DirectionGoal_Create(
            Point startPosition1,
            Point startPosition2,
            [DefaultArgument("null")]Vector targetDirection,
            [DefaultArgument("1.0")] double weight)
        {
            return new DirectionGoal(
                startPosition1.ToTriple(),
                startPosition2.ToTriple(),
                targetDirection?.ToTriple() ?? (startPosition2.ToTriple() - startPosition1.ToTriple()).Normalise(),
                (float)weight);
        }


        public static DirectionGoal DirectionGoal_Change(
            DirectionGoal directionGoal,
            [DefaultArgument("null")]Vector targetDirection,
            [DefaultArgument("-1.0")] double weight)
        {
            if (targetDirection != null) directionGoal.TargetDirection = targetDirection.ToTriple();
            if (weight >= 0.0) directionGoal.Weight = (float)weight;
            return directionGoal;
        }


        //==================================================================
        // EqualLength
        //==================================================================

        public static EqualLengthsGoal EqualLengthsGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("1.0")] double weight)
        {
            return new EqualLengthsGoal(startPositions.ToTriples(), (float)weight);
        }


        public static EqualLengthsGoal EqualLengthsGoal_Change(
            EqualLengthsGoal equalLengthsGoal,
            [DefaultArgument("-1.0")] double weight)
        {
            if (weight >= 1.0) equalLengthsGoal.Weight = (float)weight;
            return equalLengthsGoal;
        }


        //==================================================================
        // Length
        //==================================================================

        public static LengthGoal LengthGoal_Create(
            Point startPosition1,
            Point startPosition2,
            [DefaultArgument("-1.0")] double targetLength,
            [DefaultArgument("1.0")] double weight)
        {
            return new LengthGoal(
                startPosition1.ToTriple(),
                startPosition2.ToTriple(),
                targetLength >= 0.0 ? (float)targetLength : (startPosition2.ToTriple() - startPosition1.ToTriple()).Length,
                (float)weight);
        }


        public static LengthGoal LengthGoal_Create(
            Line line,
            [DefaultArgument("-1.0")] double targetLength,
            [DefaultArgument("1.0")] double weight)
        {
            return new LengthGoal(
                line.StartPoint.ToTriple(),
                line.EndPoint.ToTriple(),
                targetLength >= 0.0 ? (float)targetLength : (float)line.Length,
                (float)weight);
        }


        public static LengthGoal LengthGoal_Change(
            LengthGoal lengthGoal,
            [DefaultArgument("-1.0")] double targetLength,
            [DefaultArgument("-1.0")] double weight)
        {
            if (targetLength >= 0.0) lengthGoal.TargetLength = (float)targetLength;
            if (weight >= 0.0) lengthGoal.Weight = (float)weight;
            return lengthGoal;
        }


        //==================================================================
        // Merge
        //==================================================================

        public static MergeGoal MergeGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("1000.0")] double weight)
        {
            return new MergeGoal(startPositions.ToTriples(), (float)weight);
        }


        public static MergeGoal MergeGoal_Change(
            MergeGoal mergeGoal,
            [DefaultArgument("-1.0")] double weight)
        {
            if (weight >= 0.0) mergeGoal.Weight = (float)weight;
            return mergeGoal;
        }


        //==================================================================
        // OnCurve
        //==================================================================

        public static OnCurveGoal OnCurveGoal_Create(
            List<Point> startPositions,
            Curve targetCurve,
            [DefaultArgument("1.0")] double weight)
        {
            return new OnCurveGoal(startPositions.ToTriples(), targetCurve, (float)weight);
        }


        public static OnCurveGoal OnCurveGoal_Change(
            OnCurveGoal onCurveGoal,
            [DefaultArgument("null")] Curve targetCurve,
            double weight)
        {
            if (targetCurve != null) onCurveGoal.TargetCurve = targetCurve;
            if (weight >= 0.0) onCurveGoal.Weight = (float)weight;
            return onCurveGoal;
        }


        //==================================================================
        // OnLine
        //==================================================================

        public static OnLineGoal OnLineGoal_Create(
            List<Point> startPosition,
            [DefaultArgument("Point.Origin()")] Point targetLineOrigin,
            [DefaultArgument("Vector.XAxis()")] Vector targetLineDirection,
            [DefaultArgument("1.0")] double weight)
        {
            return new OnLineGoal(
                startPosition.ToTriples(),
                targetLineOrigin.ToTriple(),
                targetLineDirection.ToTriple().Normalise(),
                (float)weight);
        }


        public static OnLineGoal OnLineGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("Line.ByStartPointEndPoint(Point.Origin(), Point.ByCoordinates(1.0, 0.0, 0.0))")] Line targetLine,
            [DefaultArgument("1.0")] double weight)
        {
            return new OnLineGoal(startPositions.ToTriples(), targetLine, (float)weight);
        }


        public static OnLineGoal OnLineGoal_Change(
            OnLineGoal onLineGoal,
            [DefaultArgument("null")] Point targetLineOrigin,
            [DefaultArgument("null")] Vector targetLineDirection,
            [DefaultArgument("-1.0")] double weight)
        {
            if (targetLineOrigin != null) onLineGoal.TargetLineOrigin = targetLineOrigin.ToTriple();
            if (targetLineDirection != null) onLineGoal.TargetLineDirection = targetLineDirection.ToTriple();
            if (weight >= 0.0) onLineGoal.Weight = (float)weight;
            return onLineGoal;
        }


        public static OnLineGoal OnLineGoal_Change(
            OnLineGoal onLineGoal,
            [DefaultArgument("null")] Line targetLine,
            [DefaultArgument("-1.0")] double weight)
        {
            if (targetLine != null)
            {
                onLineGoal.TargetLineOrigin = targetLine.StartPoint.ToTriple();
                onLineGoal.TargetLineDirection = (targetLine.EndPoint.ToTriple() - targetLine.StartPoint.ToTriple()).Normalise();
            }

            if (weight >= 0.0) onLineGoal.Weight = (float)weight;
            return onLineGoal;
        }


        //==================================================================
        // OnPlane
        //==================================================================

        public static OnPlaneGoal OnPlaneGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("Point.Origin()")] Point targetPlaneOrigin,
            [DefaultArgument("Vector.ZAxis()")] Vector targetPlaneNormal,
            [DefaultArgument("1.0")] double weight)
        {
            return new OnPlaneGoal(
                startPositions.ToTriples(),
                targetPlaneOrigin.ToTriple(),
                targetPlaneNormal.ToTriple(),
                (float)weight);
        }

        public static OnPlaneGoal OnPlaneGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("Plane.ByOriginNormal(Point.Origin(), Vector.ZAxis())")] Plane targetPlane,
            [DefaultArgument("1.0")] double weight)
        {
            return new OnPlaneGoal(startPositions.ToTriples(), targetPlane.Origin.ToTriple(), targetPlane.Normal.ToTriple(), (float)weight);
        }


        public static OnPlaneGoal OnPlaneGoal_Change(
            OnPlaneGoal onPlaneGoal,
            [DefaultArgument("null")] Point targetPlaneOrigin,
            [DefaultArgument("null")] Vector targetPlaneNormal,
            [DefaultArgument("-1.0")] double weight)
        {
            if (targetPlaneOrigin != null) onPlaneGoal.TargetPlaneOrigin = targetPlaneOrigin.ToTriple();
            if (targetPlaneNormal != null) onPlaneGoal.TargetPlaneNormal = targetPlaneNormal.ToTriple();
            if (weight >= 0.0) onPlaneGoal.Weight = (float)weight;
            return onPlaneGoal;
        }


        public static OnPlaneGoal OnPlaneGoal_Change(
            OnPlaneGoal onPlaneGoal,
            [DefaultArgument("null")] Plane targetPlane,
            [DefaultArgument("-1.0")] double weight)
        {
            if (targetPlane != null)
            {
                onPlaneGoal.TargetPlaneOrigin = targetPlane.Origin.ToTriple();
                onPlaneGoal.TargetPlaneNormal = targetPlane.Normal.ToTriple();
            }
            if (weight >= 0.0) onPlaneGoal.Weight = (float)weight;
            return onPlaneGoal;
        }


        //==================================================================
        // ParallelLines
        //==================================================================

        public static ParallelLinesGoal ParallelLinesGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("1.0")] double weight)
        {
            return new ParallelLinesGoal(startPositions.ToTriples(), (float)weight);
        }


        public static ParallelLinesGoal ParallelLinesGoal_Change(
            ParallelLinesGoal parallelLinesGoal,
            [DefaultArgument("-1.0")] double weight)
        {
            if (weight >= 0.0) parallelLinesGoal.Weight = (float)weight;
            return parallelLinesGoal;
        }


        //==================================================================
        // ShapeMatching
        //==================================================================

        public static ShapeMatchingGoal ShapeMatchingGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("null")] List<Point> targetShapePoints,
            [DefaultArgument("false")] bool allowScaling,
            [DefaultArgument("1.0")] double weight)
        {
            return targetShapePoints == null
                ? new ShapeMatchingGoal(startPositions.ToTriples(), allowScaling, (float)weight)
                : new ShapeMatchingGoal(startPositions.ToTriples(), targetShapePoints.ToTriples(), allowScaling, (float)weight);
        }


        public static ShapeMatchingGoal ShapeMatchingGoal_Change(
            ShapeMatchingGoal shapeMatchingGoal,
            [DefaultArgument("null")] List<Point> targetShapePoints,
            [DefaultArgument("null")] bool? allowScaling,
            [DefaultArgument("-1.0")] double weight)
        {
            if (targetShapePoints != null) shapeMatchingGoal.SetTargetShapePoints(targetShapePoints.ToTriples());
            if (weight >= 0.0) shapeMatchingGoal.Weight = (float)weight;
            if (allowScaling.HasValue) shapeMatchingGoal.AllowScaling = allowScaling.Value;
            return shapeMatchingGoal;
        }
    }
}