using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using DynaShape.Goals;
using Mesh = Autodesk.Dynamo.MeshToolkit.Mesh;


namespace DynaShape.ZeroTouch
{
    public static class Goals
    {
        //==================================================================
        // Change the weight value of a goal (of any type)
        //==================================================================

        /// <summary>
        /// Change the weight value of a goal (of any kind)
        /// </summary>
        /// <param name="goal"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static Goal Goal_ChangeWeight(Goal goal, float weight)
        {
            goal.Weight = weight;
            return goal;
        }


        //==================================================================
        // Anchor
        //==================================================================

        /// <summary>
        /// Keep a node an the specified anchor point.
        /// By default the weight for this goal set very high to ensure the node really "sticks" to the anchor
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="anchor"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static AnchorGoal AnchorGoal_Create(
            Point startPosition,
            [DefaultArgument("null")] Point anchor,
            [DefaultArgument("1000.0")] float weight)
        {
            return new AnchorGoal(
                startPosition.ToTriple(),
                anchor?.ToTriple() ?? startPosition.ToTriple(),
                weight);
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="anchorGoal"></param>
        /// <param name="anchor"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static AnchorGoal AnchorGoal_Change(
            AnchorGoal anchorGoal,
            [DefaultArgument("null")] Point anchor,
            [DefaultArgument("-1.0")] float weight)
        {
            if (anchor != null) anchorGoal.Anchor = anchor.ToTriple();
            if (weight >= 0.0) anchorGoal.Weight = weight;
            return anchorGoal;
        }


        //==================================================================
        // Angle
        //==================================================================

        /// <summary>
        /// Keep the angle formed by three nodes at a target value
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        /// <param name="targetAngle"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static AngleGoal AngleGoal_Create(
            Point A,
            Point B,
            Point C,
            [DefaultArgument("0.0")] float targetAngle,
            [DefaultArgument("1.0")] float weight)
        {
            return new AngleGoal(
                A.ToTriple(),
                B.ToTriple(),
                C.ToTriple(),
                (targetAngle).ToRadian(),
                weight);
        }


        /// <summary>
        /// Maintain the angle formed by three nodes
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static AngleGoal AngleGoal_Create(
            Point A,
            Point B,
            Point C,
            [DefaultArgument("1.0")] float weight)
        {
            return new AngleGoal(
                A.ToTriple(),
                B.ToTriple(),
                C.ToTriple(),
                weight);
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="angleGoal"></param>
        /// <param name="targetAngle"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static AngleGoal AngleGoal_Change(
            AngleGoal angleGoal,
            float targetAngle,
            [DefaultArgument("-1.0")] float weight)
        {
            angleGoal.TargetAngle = targetAngle.ToRadian();
            if (weight >= 0.0) angleGoal.Weight = weight;
            return angleGoal;
        }


        //==================================================================
        // CoCircular
        //==================================================================

        /// <summary>
        /// Force a set of nodes to lie on a common circular arc
        /// </summary>
        /// <param name="startPositions"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static CoCircularGoal CoCircularGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("1.0")] float weight)
        {
            return new CoCircularGoal(startPositions.ToTriples(), weight);
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="cyclicPolygonGoal"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static CoCircularGoal CoCircularGoal_Change(
            CoCircularGoal cyclicPolygonGoal,
            [DefaultArgument("-1.0")] float weight)
        {
            if (weight >= 0.0) cyclicPolygonGoal.Weight = weight;
            return cyclicPolygonGoal;
        }


        //==================================================================
        // CoLinear
        //==================================================================

        /// <summary>
        /// Force a set of nodes to lie on a common line.
        /// The line position and orientation are computed based on the current node positions.
        /// This is different from the OnLine goal, where the target line is fixed and defined in advance.
        /// </summary>
        /// <param name="startPositions"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static CoLinearGoal CoLinearGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("1000.0")] float weight)
        {
            return new CoLinearGoal(startPositions.ToTriples(), weight);
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="coLinearGoal"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static CoLinearGoal CoLinearGoal_Change(
            CoLinearGoal coLinearGoal,
            [DefaultArgument("-1.0")] float weight)
        {
            if (weight >= 0.0) coLinearGoal.Weight = weight;
            return coLinearGoal;
        }


        //==================================================================
        // Constant
        //==================================================================

        /// <summary>
        /// Apply a constant directional offset to the specified nodes. 
        /// For example, this is useful to simulate gravity
        /// </summary>
        /// <param name="startPositions"></param>
        /// <param name="constant"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static ConstantGoal ConstantGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("Vector.ByCoordinates(0, 0, -0.1)")] Vector constant,
            [DefaultArgument("1.0")] float weight)
        {
            return new ConstantGoal(startPositions.ToTriples(), constant.ToTriple(), weight);
        }

        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="constantGoal"></param>
        /// <param name="constant"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static ConstantGoal ConstantGoal_Change(
            ConstantGoal constantGoal,
            [DefaultArgument("null")] Vector constant,
            [DefaultArgument("-1.0")] float weight)
        {
            if (constant != null) constantGoal.Move = constant.ToTriple();
            if (weight >= 0.0) constantGoal.Weight = weight;
            return constantGoal;
        }



        //==================================================================
        // ConstantPressure
        //==================================================================

        /// <summary>
        /// Applying a force perpendicular to a triangular surface, with magnitude proportional to the surface area.
        /// </summary>
        /// <param name="startPosition1"></param>
        /// <param name="startPosition2"></param>
        /// <param name="startPosition3"></param>
        /// <param name="pressure">The pressure being applied on the triangle</param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static ConstantPressureGoal ConstantPressureGoal_Create(
            Point startPosition1,
            Point startPosition2,
            Point startPosition3,
            [DefaultArgument("0.1")]float pressure,
            [DefaultArgument("1.0")] float weight)
        {
            return new ConstantPressureGoal(startPosition1.ToTriple(), startPosition2.ToTriple(), startPosition3.ToTriple(), pressure, weight);
        }


        /// <summary>
        /// Applying forces perpendicular to each triangular face of a mesh, with magnitude proportional to the surface area of the triangle
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="pressure"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static List<ConstantPressureGoal> ConstantPressureGoal_Create(
            Mesh mesh,
            [DefaultArgument("0.1")]float pressure,
            [DefaultArgument("1.0")] float weight)
        {
            List<ConstantPressureGoal> pressureGoals = new List<ConstantPressureGoal>();

            List<double> vertices = mesh.TrianglesAsNineNumbers.ToList();

            int faceCount = vertices.Count / 9;

            for (int i = 0; i < faceCount; i++)
            {
                int j = i * 9;
                pressureGoals.Add(
                    new ConstantPressureGoal(
                        new Triple(vertices[j + 0], vertices[j + 1], vertices[j + 2]),
                        new Triple(vertices[j + 3], vertices[j + 4], vertices[j + 5]),
                        new Triple(vertices[j + 6], vertices[j + 7], vertices[j + 8]),
                        pressure,
                        weight));
            }

            return pressureGoals;
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="constantPressureGoal"></param>
        /// <param name="pressure"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static ConstantPressureGoal ConstantPressureGoal_Change(
            ConstantPressureGoal constantPressureGoal,
            [DefaultArgument("-1.0")] float pressure,
            [DefaultArgument("-1.0")] float weight)
        {
            if (pressure >= 0.0) constantPressureGoal.Pressure = pressure;
            if (weight >= 0.0) constantPressureGoal.Weight = weight;
            return constantPressureGoal;
        }


        //==================================================================
        // ConstantVolumePressure
        //==================================================================

        /// <summary>
        /// Simulate pressure trapped inside a closed volume. The pressure decreases as the volume expands (Boyle's law)
        /// </summary>
        /// <param name="mesh">A closed mesh</param>
        /// <param name="volumePressureConstant">The constant that is the product of the pressure and volume. This means that pressure will automatically decrease as the mesh volume increases (and vice versa)</param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static ConstantVolumePressureGoal ConstantVolumePressureGoal_Create(
            Mesh mesh,
            [DefaultArgument("0.0")] float volumePressureConstant,
            [DefaultArgument("1.0")] float weight)
        {
            return new ConstantVolumePressureGoal(mesh, volumePressureConstant,  weight);
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="constantVolumePressureGoal"></param>
        /// <param name="volumePressureConstant"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static ConstantVolumePressureGoal ConstantVolumePressureGoal_Change(
            ConstantVolumePressureGoal constantVolumePressureGoal,
            [DefaultArgument("-1.0")] float volumePressureConstant,
            [DefaultArgument("-1.0")] float weight)
        {
            if (volumePressureConstant >= 0.0) constantVolumePressureGoal.VolumePressureConstant = volumePressureConstant;
            if (weight >= 0.0) constantVolumePressureGoal.Weight = weight;
            return constantVolumePressureGoal;
        }



        //==================================================================
        // CoPlanar
        //==================================================================

        /// <summary>
        /// Force a set of nodes to lie on a common plane.
        /// The plane position and orientation are computed based on the current node positions.
        /// This is different from the OnPlane goal, where the target plane is fixed and defined in advance.
        /// </summary>
        /// <param name="startPositions"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static CoPlanarGoal CoPlanarGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("1.0")] float weight)
        {
            return new CoPlanarGoal(startPositions.ToTriples(), weight);
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="coPlanarGoal"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static CoPlanarGoal CoPlanarGoal_Change(
            CoPlanarGoal coPlanarGoal,
            [DefaultArgument("-1.0")] float weight)
        {
            if (weight >= 0.0) coPlanarGoal.Weight = weight;
            return coPlanarGoal;
        }


        //==================================================================
        // CoSpherical
        //==================================================================

        /// <summary>
        /// Force a set of nodes to lie on a common spherical surface.
        /// The sphere position and radius are computed based the current node positions
        /// </summary>
        /// <param name="startPositions"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static CoSphericalGoal CoSphericalGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("1.0")] float weight)
        {
            return new CoSphericalGoal(startPositions.ToTriples(), weight);
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="coSphericalGoal"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static CoSphericalGoal CoSphericalGoal_Change(
            CoSphericalGoal coSphericalGoal,
            [DefaultArgument("-1.0")] float weight)
        {
            if (weight >= 0.0) coSphericalGoal.Weight = weight;
            return coSphericalGoal;
        }


        //==================================================================
        // Directional Wind
        //==================================================================

        /// <summary>
        /// Simulate wind by applying a constant force on the three vertices of a triangle,
        /// scaled by the cosine of the angle between the wind vector and the triangle's normal.
        /// This way, the wind has full effect when it hits the triangle head-on, and zero
        /// effect if it blows paralell to the triangle.
        /// </summary>
        /// <param name="startPosition1"></param>
        /// <param name="startPosition2"></param>
        /// <param name="startPosition3"></param>
        /// <param name="windVector"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static DirectionalWindGoal DirectionalWindGoal_Create(
            Point startPosition1,
            Point startPosition2,
            Point startPosition3,
            [DefaultArgument("Vector.ByCoordinates(1.0, 0, 0)")] Vector windVector,
            [DefaultArgument("1.0")] float weight)
        {
            return new DirectionalWindGoal(
                startPosition1.ToTriple(),
                startPosition2.ToTriple(),
                startPosition3.ToTriple(),
                windVector.ToTriple(),
                weight);
        }


        /// <summary>
        /// Simulate wind blowing along a specified direction, by applying a force on the three vertices of a triangle,
        /// The force magnitude is addtionally scaled by the cosine of the angle between the wind vector and the triangle's normal.
        /// This way, the wind has full effect when it hits the triangle head-on, and zero
        /// effect if it blows paralell to the triangle.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="windVector"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static List<DirectionalWindGoal> DirectionalWindGoal_Create(
            Mesh mesh,
            [DefaultArgument("Vector.ByCoordinates(1.0, 0, 0)")] Vector windVector,
            [DefaultArgument("1.0")] float weight)
        {
            List<DirectionalWindGoal> windGoals = new List<DirectionalWindGoal>();

            List<double> vertices = mesh.TrianglesAsNineNumbers.ToList();

            int faceCount = vertices.Count / 9;

            for (int i = 0; i < faceCount; i++)
            {
                int j = i * 9;
                windGoals.Add(
                    new DirectionalWindGoal(
                        new Triple(vertices[j + 0], vertices[j + 1], vertices[j + 2]),
                        new Triple(vertices[j + 3], vertices[j + 4], vertices[j + 5]),
                        new Triple(vertices[j + 6], vertices[j + 7], vertices[j + 8]),
                        windVector.ToTriple(),
                        weight));
            }

            return windGoals;
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="windGoal"></param>
        /// <param name="windVector"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static DirectionalWindGoal DirectionalWindGoal_Change(
            DirectionalWindGoal windGoal,
            [DefaultArgument("null")] Vector windVector,
            [DefaultArgument("-1.0")] float weight)
        {
            if (windVector != null) windGoal.WindVector = windVector.ToTriple();
            if (weight >= 0.0) windGoal.Weight = weight;
            return windGoal;
        }


        //==================================================================
        // Direction
        //==================================================================

        /// <summary>
        /// Force the line connecting two nodes to be parallel to the specified direction.
        /// </summary>
        /// <param name="startPosition1"></param>
        /// <param name="startPosition2"></param>
        /// <param name="targetDirection"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static DirectionGoal DirectionGoal_Create(
            Point startPosition1,
            Point startPosition2,
            [DefaultArgument("null")] Vector targetDirection,
            [DefaultArgument("1.0")] float weight)
        {
            return new DirectionGoal(
                startPosition1.ToTriple(),
                startPosition2.ToTriple(),
                targetDirection?.ToTriple() ?? (startPosition2.ToTriple() - startPosition1.ToTriple()).Normalise(),
                weight);
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="directionGoal"></param>
        /// <param name="targetDirection"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static DirectionGoal DirectionGoal_Change(
            DirectionGoal directionGoal,
            [DefaultArgument("null")] Vector targetDirection,
            [DefaultArgument("-1.0")] float weight)
        {
            if (targetDirection != null) directionGoal.TargetDirection = targetDirection.ToTriple();
            if (weight >= 0.0) directionGoal.Weight = weight;
            return directionGoal;
        }


        //==================================================================
        // EqualLength
        //==================================================================

        /// <summary>
        /// Force a sequence of nodes to maintain equal distances.
        /// </summary>
        /// <param name="startPositions"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static EqualLengthsGoal EqualLengthsGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("1.0")] float weight)
        {
            List<Triple> triples = new List<Triple> { startPositions[0].ToTriple() };

            for (int i = 1; i < startPositions.Count - 1; i++)
            {
                triples.Add(startPositions[i].ToTriple());
                triples.Add(triples[triples.Count - 1]);
            }

            triples.Add(startPositions[startPositions.Count - 1].ToTriple());

            return new EqualLengthsGoal(triples, weight);
        }


        /// <summary>
        /// Force a set of line segments to maintain equal lengths.
        /// </summary>
        /// <param name="lineStarts"></param>
        /// <param name="lineEnds"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static EqualLengthsGoal EqualLengthsGoal_Create(
            List<Point> lineStarts,
            List<Point> lineEnds,
            [DefaultArgument("1.0")] float weight)
        {
            List<Triple> triples = new List<Triple>();

            int n = lineStarts.Count < lineEnds.Count ? lineStarts.Count : lineEnds.Count;

            for (int i = 0; i < n; i++)
            {
                triples.Add(lineStarts[i].ToTriple());
                triples.Add(lineEnds[i].ToTriple());
            }

            return new EqualLengthsGoal(triples, weight);
        }


        /// <summary>
        /// Force a set of line segments to maintain equal lengths.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static EqualLengthsGoal EqualLengthsGoal_Create(
            List<Line> lines,
            [DefaultArgument("1.0")] float weight)
        {
            List<Triple> startPositions = new List<Triple>();
            foreach (Line line in lines)
            {
                startPositions.Add(line.StartPoint.ToTriple());
                startPositions.Add(line.EndPoint.ToTriple());
            }
            return new EqualLengthsGoal(startPositions, weight);
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="equalLengthsGoal"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static EqualLengthsGoal EqualLengthsGoal_Change(
            EqualLengthsGoal equalLengthsGoal,
            [DefaultArgument("-1.0")] float weight)
        {
            if (weight >= 1.0) equalLengthsGoal.Weight = weight;
            return equalLengthsGoal;
        }


        //==================================================================
        // Floor
        //==================================================================

        /// <summary>
        /// Force a set of nodes to stay above a horizontal floor plane.
        /// </summary>
        /// <param name="startPositions"></param>
        /// <param name="floorHeight"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static FloorGoal FloorGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("0.0")] float floorHeight,
            [DefaultArgument("1000.0")] float weight)
        {
            return new FloorGoal(startPositions.ToTriples(), floorHeight, weight);
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="floorGoal"></param>
        /// <param name="floorHeight"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static FloorGoal FloorGoal_Change(
            FloorGoal floorGoal,
            [DefaultArgument("0.0")] float floorHeight,
            [DefaultArgument("-1.0")] float weight)
        {
            floorGoal.FloorHeight = floorHeight;
            if (weight > 0.0) floorGoal.Weight = weight;
            return floorGoal;
        }


        //==================================================================
        // Length
        //==================================================================

        /// <summary>
        /// Force a pair of nodes to maintain the specified distance/length
        /// </summary>
        /// <param name="startPosition1"></param>
        /// <param name="startPosition2"></param>
        /// <param name="targetLength">If unspecified, the target length will be the distance between the starting node positions</param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static LengthGoal LengthGoal_Create(
            Point startPosition1,
            Point startPosition2,
            [DefaultArgument("-1.0")] float targetLength,
            [DefaultArgument("1.0")] float weight)
        {
            return new LengthGoal(
                startPosition1.ToTriple(),
                startPosition2.ToTriple(),
                targetLength >= 0.0
                    ? targetLength
                    : (startPosition2.ToTriple() - startPosition1.ToTriple()).Length,
                weight);
        }


        /// <summary>
        /// Maintain the specified distance between two nodes located at the start and end of the given line
        /// </summary>
        /// <param name="line">This line will be used to create two nodes located at is start and end point</param>
        /// <param name="targetLength">If unspecified, the target length will be the distance between the starting node positions</param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static LengthGoal LengthGoal_Create(
            Line line,
            [DefaultArgument("-1.0")] float targetLength,
            [DefaultArgument("1.0")] float weight)
        {
            return new LengthGoal(
                line.StartPoint.ToTriple(),
                line.EndPoint.ToTriple(),
                targetLength >= 0.0 ? targetLength : (float)line.Length,
                weight);
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="lengthGoal"></param>
        /// <param name="targetLength"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static LengthGoal LengthGoal_Change(
            LengthGoal lengthGoal,
            [DefaultArgument("-1.0")] float targetLength,
            [DefaultArgument("-1.0")] float weight)
        {
            if (targetLength >= 0.0) lengthGoal.TargetLength = targetLength;
            if (weight >= 0.0) lengthGoal.Weight = weight;
            return lengthGoal;
        }


        //==================================================================
        // Merge
        //==================================================================

        /// <summary>
        /// Pull a set of nodes to their center of mass. 
        /// This is useful to force the nodes to have coincident positions.
        /// By default the weight value is set very high to ensure that the nodes really merge together at one position.
        /// </summary>
        /// <param name="startPositions"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static MergeGoal MergeGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("1000.0")] float weight)
        {
            return new MergeGoal(startPositions.ToTriples(), weight);
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="mergeGoal"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static MergeGoal MergeGoal_Change(
            MergeGoal mergeGoal,
            [DefaultArgument("-1.0")] float weight)
        {
            if (weight >= 0.0) mergeGoal.Weight = weight;
            return mergeGoal;
        }


        //==================================================================
        // OnCurve
        //==================================================================

        /// <summary>
        /// Force a set of nodes to lie on the specified curve.
        /// </summary>
        /// <param name="startPositions"></param>
        /// <param name="targetCurve"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static OnCurveGoal OnCurveGoal_Create(
            List<Point> startPositions,
            Curve targetCurve,
            [DefaultArgument("1.0")] float weight)
        {
            return new OnCurveGoal(startPositions.ToTriples(), targetCurve, weight);
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="onCurveGoal"></param>
        /// <param name="targetCurve"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static OnCurveGoal OnCurveGoal_Change(
            OnCurveGoal onCurveGoal,
            [DefaultArgument("null")] Curve targetCurve,
            float weight)
        {
            if (targetCurve != null) onCurveGoal.TargetCurve = targetCurve;
            if (weight >= 0.0) onCurveGoal.Weight = weight;
            return onCurveGoal;
        }


        //==================================================================
        // OnLine
        //==================================================================

        /// <summary>
        /// Force a set of nodes to lie on the specified line.
        /// This is different from othe CoLinear goal, where the target line is computed based on the current node positions rather than being defined and fixed in advance.
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="targetLineOrigin"></param>
        /// <param name="targetLineDirection"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static OnLineGoal OnLineGoal_Create(
            List<Point> startPosition,
            [DefaultArgument("Point.Origin()")] Point targetLineOrigin,
            [DefaultArgument("Vector.XAxis()")] Vector targetLineDirection,
            [DefaultArgument("1.0")] float weight)
        {
            return new OnLineGoal(
                startPosition.ToTriples(),
                targetLineOrigin.ToTriple(),
                targetLineDirection.ToTriple().Normalise(),
                weight);
        }


        /// <summary>
        /// Force a set of nodes to lie on the specified line.
        /// This is different from othe CoLinear goal, where the target line is computed based on the current node positions rather than being defined and fixed in advance.
        /// </summary>
        /// <param name="startPositions"></param>
        /// <param name="targetLine"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static OnLineGoal OnLineGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("Line.ByStartPointEndPoint(Point.Origin(), Point.ByCoordinates(1.0, 0.0, 0.0))")] Line
                targetLine,
            [DefaultArgument("1.0")] float weight)
        {
            return new OnLineGoal(startPositions.ToTriples(), targetLine, weight);
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="onLineGoal"></param>
        /// <param name="targetLineOrigin"></param>
        /// <param name="targetLineDirection"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static OnLineGoal OnLineGoal_Change(
            OnLineGoal onLineGoal,
            [DefaultArgument("null")] Point targetLineOrigin,
            [DefaultArgument("null")] Vector targetLineDirection,
            [DefaultArgument("-1.0")] float weight)
        {
            if (targetLineOrigin != null) onLineGoal.TargetLineOrigin = targetLineOrigin.ToTriple();
            if (targetLineDirection != null) onLineGoal.TargetLineDirection = targetLineDirection.ToTriple();
            if (weight >= 0.0) onLineGoal.Weight = weight;
            return onLineGoal;
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="onLineGoal"></param>
        /// <param name="targetLine"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static OnLineGoal OnLineGoal_Change(
            OnLineGoal onLineGoal,
            [DefaultArgument("null")] Line targetLine,
            [DefaultArgument("-1.0")] float weight)
        {
            if (targetLine != null)
            {
                onLineGoal.TargetLineOrigin = targetLine.StartPoint.ToTriple();
                onLineGoal.TargetLineDirection =
                    (targetLine.EndPoint.ToTriple() - targetLine.StartPoint.ToTriple()).Normalise();
            }

            if (weight >= 0.0) onLineGoal.Weight = weight;
            return onLineGoal;
        }


        //==================================================================
        // OnPlane
        //==================================================================

        /// <summary>
        /// Force a set of nodes to lie on the specified plane.
        /// This is different from othe CoPlanar goal, where the target plane is computed based on the current node positions rather than being defined and fixed in advance.
        /// </summary>
        /// <param name="startPositions"></param>
        /// <param name="targetPlaneOrigin"></param>
        /// <param name="targetPlaneNormal"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static OnPlaneGoal OnPlaneGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("Point.Origin()")] Point targetPlaneOrigin,
            [DefaultArgument("Vector.ZAxis()")] Vector targetPlaneNormal,
            [DefaultArgument("1.0")] float weight)
        {
            return new OnPlaneGoal(
                startPositions.ToTriples(),
                targetPlaneOrigin.ToTriple(),
                targetPlaneNormal.ToTriple(),
                weight);
        }

        /// <summary>
        /// Force a set of nodes to lie on the specified plane.
        /// This is different from othe CoPlanar goal, where the target plane is computed based on the current node positions rather than being defined and fixed in advance.
        /// </summary>
        /// <param name="startPositions"></param>
        /// <param name="targetPlane"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static OnPlaneGoal OnPlaneGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("Plane.ByOriginNormal(Point.Origin(), Vector.ZAxis())")] Plane targetPlane,
            [DefaultArgument("1.0")] float weight)
        {
            return new OnPlaneGoal(startPositions.ToTriples(), targetPlane.Origin.ToTriple(),
                targetPlane.Normal.ToTriple(), weight);
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="onPlaneGoal"></param>
        /// <param name="targetPlaneOrigin"></param>
        /// <param name="targetPlaneNormal"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static OnPlaneGoal OnPlaneGoal_Change(
            OnPlaneGoal onPlaneGoal,
            [DefaultArgument("null")] Point targetPlaneOrigin,
            [DefaultArgument("null")] Vector targetPlaneNormal,
            [DefaultArgument("-1.0")] float weight)
        {
            if (targetPlaneOrigin != null) onPlaneGoal.TargetPlaneOrigin = targetPlaneOrigin.ToTriple();
            if (targetPlaneNormal != null) onPlaneGoal.TargetPlaneNormal = targetPlaneNormal.ToTriple();
            if (weight >= 0.0) onPlaneGoal.Weight = weight;
            return onPlaneGoal;
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="onPlaneGoal"></param>
        /// <param name="targetPlane"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static OnPlaneGoal OnPlaneGoal_Change(
            OnPlaneGoal onPlaneGoal,
            [DefaultArgument("null")] Plane targetPlane,
            [DefaultArgument("-1.0")] float weight)
        {
            if (targetPlane != null)
            {
                onPlaneGoal.TargetPlaneOrigin = targetPlane.Origin.ToTriple();
                onPlaneGoal.TargetPlaneNormal = targetPlane.Normal.ToTriple();
            }
            if (weight >= 0.0) onPlaneGoal.Weight = weight;
            return onPlaneGoal;
        }


        //==================================================================
        // OnSurface
        //==================================================================

        /// <summary>
        /// Force a set of nodes to lie on the specified surface.
        /// </summary>
        /// <param name="startPositions"></param>
        /// <param name="targetSurface"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static OnSurfaceGoal OnSurfaceGoal_Create(
            List<Point> startPositions,
            Surface targetSurface,
            [DefaultArgument("1.0")] float weight)
        {
            return new OnSurfaceGoal(startPositions.ToTriples(), targetSurface, weight);
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="onSurfaceGoal"></param>
        /// <param name="targetSurface"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static OnSurfaceGoal OnSurfaceGoal_Change(
            OnSurfaceGoal onSurfaceGoal,
            [DefaultArgument("null")] Surface targetSurface,
            float weight)
        {
            if (targetSurface != null) onSurfaceGoal.TargetSurface = targetSurface;
            if (weight >= 0.0) onSurfaceGoal.Weight = weight;
            return onSurfaceGoal;
        }


        //==================================================================
        // ParallelLines
        //==================================================================

        /// <summary>
        /// Force a set of lines to be parallel.
        /// </summary>
        /// <param name="startPositions">The nodes at the linie start points and end points, in the following order: start1, end1, start2, end2, etc...</param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static ParallelLinesGoal ParallelLinesGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("1.0")] float weight)
        {
            return new ParallelLinesGoal(startPositions.ToTriples(), weight);
        }


        /// <summary>
        /// Force a set of lines to be parallel.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static ParallelLinesGoal ParallelLinesGoal_Create(
            List<Line> lines,
            [DefaultArgument("1.0")] float weight)
        {
            List<Triple> startPositions = new List<Triple>();
            foreach (Line line in lines)
            {
                startPositions.Add(line.StartPoint.ToTriple());
                startPositions.Add(line.EndPoint.ToTriple());
            }
            return new ParallelLinesGoal(startPositions, weight);
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="parallelLinesGoal"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static ParallelLinesGoal ParallelLinesGoal_Change(
            ParallelLinesGoal parallelLinesGoal,
            [DefaultArgument("-1.0")] float weight)
        {
            if (weight >= 0.0) parallelLinesGoal.Weight = weight;
            return parallelLinesGoal;
        }



        //==================================================================
        // ShapeMatching
        //==================================================================

        /// <summary>
        /// Move a set of nodes to positions that resemble a target shape.
        /// The target shape is defined by a sequence of points, in the same order as how the nodes are specified.
        /// </summary>
        /// <param name="startPositions"></param>
        /// <param name="targetShapePoints"></param>
        /// <param name="allowScaling"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static ShapeMatchingGoal ShapeMatchingGoal_Create(
            List<Point> startPositions,
            [DefaultArgument("null")] List<Point> targetShapePoints,
            [DefaultArgument("false")] bool allowScaling,
            [DefaultArgument("1.0")] float weight)
        {
            return targetShapePoints == null
                ? new ShapeMatchingGoal(startPositions.ToTriples(), allowScaling, weight)
                : new ShapeMatchingGoal(startPositions.ToTriples(), targetShapePoints.ToTriples(), allowScaling,
                    weight);
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="shapeMatchingGoal"></param>
        /// <param name="targetShapePoints"></param>
        /// <param name="allowScaling"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static ShapeMatchingGoal ShapeMatchingGoal_Change(
            ShapeMatchingGoal shapeMatchingGoal,
            [DefaultArgument("null")] List<Point> targetShapePoints,
            [DefaultArgument("false")] bool? allowScaling,
            [DefaultArgument("-1.0")] float weight)
        {
            if (targetShapePoints != null) shapeMatchingGoal.SetTargetShapePoints(targetShapePoints.ToTriples());
            if (weight >= 0.0) shapeMatchingGoal.Weight = weight;
            if (allowScaling.HasValue) shapeMatchingGoal.AllowScaling = allowScaling.Value;
            return shapeMatchingGoal;
        }


        //==================================================================
        // SphereCollision
        //==================================================================

        /// <summary>
        /// Move a set of nodes to positions that resemble a target shape.
        /// The target shape is defined by a sequence of points, in the same order as how the nodes are specified.
        /// </summary>
        /// <param name="startPositions"></param>
        /// <param name="targetShapePoints"></param>
        /// <param name="allowScaling"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static SphereCollisionGoal SphereCollisionGoal_Create(
            List<Point> startPositions,
            List<float> radii,
            [DefaultArgument("1.0")] float weight)
        {
            return new SphereCollisionGoal(startPositions.ToTriples(), radii, weight);
        }


        /// <summary>
        /// Adjust the goal's parameters while the solver is running.
        /// </summary>
        /// <param name="shapeMatchingGoal"></param>
        /// <param name="targetShapePoints"></param>
        /// <param name="allowScaling"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static SphereCollisionGoal SphereCollisionGoal_Change(
            SphereCollisionGoal sphereCollisionGoal,
            [DefaultArgument("null")] List<float> radii,
            [DefaultArgument("-1.0")] float weight)
        {
            if (radii != null) sphereCollisionGoal.Radii = radii.ToArray();
            if (weight >= 0.0) sphereCollisionGoal.Weight = weight;
            return sphereCollisionGoal;
        }
    }
}