using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;


namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public class FloorGoal : Goal
    {
        public float FloorHeight;

        public FloorGoal(List<Triple> nodeStartingPositions, float floorHeight = 0f, float weight = 1000f)
        {
            Weight = weight;
            FloorHeight = floorHeight;
            StartingPositions = nodeStartingPositions.ToArray();
            Moves = new Triple[NodeCount];
        }

        public override void Compute(List<Node> allNodes)
        {
            for (int i = 0; i < NodeCount; i++)
                Moves[i] = allNodes[i].Position.Z > FloorHeight
                    ? Triple.Zero
                    : new Triple(0f, 0f, FloorHeight - allNodes[i].Position.Z);
        }
    }
}
