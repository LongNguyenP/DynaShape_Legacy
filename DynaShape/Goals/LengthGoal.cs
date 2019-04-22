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
            Weights = new float[2];
        }


        public LengthGoal(Triple firstNodePosition, Triple secondNodePosition, float weight = 1000f)
            : this(firstNodePosition, secondNodePosition, firstNodePosition.DistanceTo(secondNodePosition), weight)
        {
        }


        internal override void Compute(List<Node> allNodes)
        {
            Triple move = allNodes[NodeIndices[1]].Position - allNodes[NodeIndices[0]].Position;
            if (move.IsAlmostZero(1E-5f)) move = new Triple(0.01f);
            move *= 0.5f * (move.Length - TargetLength) / move.Length;

            Moves[0] = move;
            Moves[1] = -move;
            Weights[0] = Weights[1] = Weight;
        }

        public override List<object> GetOutput(List<Node> allNodes)
        {
            return new List<object> {allNodes[NodeIndices[1]].Position.DistanceTo(allNodes[NodeIndices[0]].Position)};
        }
    }
}
