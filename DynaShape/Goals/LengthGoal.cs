using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;


namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public class LengthGoal : Goal
    {
        public float TargetLength;


        public LengthGoal(Triple firstNodePosition, Triple secondNodePosition, float targetLength, float weight = 1000f)
        {
            Weight = weight;
            TargetLength = targetLength;
            StartingPositions = new[] { firstNodePosition, secondNodePosition };
            Moves = new Triple[2];
        }


        public LengthGoal(Triple firstNodePosition, Triple secondNodePosition, float weight = 1000f)
            : this(firstNodePosition, secondNodePosition, firstNodePosition.DistanceTo(secondNodePosition), weight)
        {
        }


        public override void Compute(List<Node> allNodes)
        {
            Triple move = allNodes[NodeIndices[1]].Position - allNodes[NodeIndices[0]].Position;
            if (move.IsAlmostZero(1E-5f)) move = new Triple(1E-5f);
            move *= 0.5f * (move.Length - TargetLength) / move.Length;

            Moves[0] = move;
            Moves[1] = -move;
        }
    }
}
