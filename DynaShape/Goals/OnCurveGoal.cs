using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;


namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public class OnCurveGoal : Goal
    {
        public Curve TargetCurve;


        public OnCurveGoal(List<Triple> nodeStartingPositions, Curve curve, float weight = 1f)
        {
            TargetCurve = curve;

            Weight = weight;
            StartingPositions = nodeStartingPositions.ToArray();
            Moves = new Triple[StartingPositions.Length];
        }


        public override void Compute(List<Node> allNodes)
        {
            if (TargetCurve == null) throw new Exception("OnCurveGoal: The target curve has not been set");
            for (int i = 0; i < NodeCount; i++)
            {
                Triple nodePosition = allNodes[NodeIndices[i]].Position;
                Moves[i] = TargetCurve.ClosestPointTo(nodePosition.ToPoint()).ToTriple() - nodePosition;
            }
        }
    }
}
