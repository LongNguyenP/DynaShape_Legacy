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
            Moves = new Triple[nodeStartingPositions.Count];
            Weights = new float[nodeStartingPositions.Count];
        }

        public override void Compute(List<Node> allNodes)
        {
            // TODO: The math is quite unstable
            for (int i = 0; i < NodeCount; i++)
            {
                Moves[i] = new Triple(0f, 0f, allNodes[i].Position.Z > FloorHeight ? 0f : FloorHeight - allNodes[i].Position.Z);
                Weights[i] = allNodes[i].Position.Z > FloorHeight ? 0f : Weight;
            }
        }
    }
}
