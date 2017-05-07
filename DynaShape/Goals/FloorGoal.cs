using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;


namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public class FloorGoal : Goal
    {
        public FloorGoal(List<Triple> nodeStartingPositions, float weight = 1000f)
        {
            Weight = weight;
            StartingPositions = nodeStartingPositions.ToArray();
            Moves = new Triple[NodeCount];
        }

        public override void Compute(List<Node> allNodes)
        {
            for (int i = 0; i < NodeCount; i++)
                Moves[i] = allNodes[i].Position.Z > 0f
                    ? Triple.Zero
                    : new Triple(0f, 0f, -allNodes[i].Position.Z);
        }
    }
}
