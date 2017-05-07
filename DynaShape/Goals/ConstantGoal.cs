using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;


namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public class ConstantGoal : Goal
    {
        public Triple Move;

        public ConstantGoal(List<Triple> nodeStartingPositions, Triple move, float weight = 1f)
        {
            Move = move;
            Weight = weight;
            StartingPositions = nodeStartingPositions.ToArray();
            Moves = new Triple[StartingPositions.Length];
        }

        public override void Compute(List<Node> allNodes)
        {
            Moves.FillArray(Move);
        }
    }
}
