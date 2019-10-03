using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;


namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public class SphereCollisionGoal : Goal
    {
        public float[] Radii;

        public SphereCollisionGoal(List<Triple> centers, List<float> radii, float weight = 1000f)
        {
            Weight = weight;
            Radii = radii.ToArray();
            StartingPositions = centers.ToArray();
            Moves = new Triple[centers.Count];
            Weights = new float[centers.Count];
        }


        internal override void Compute(List<Node> allNodes)
        {
            Moves = new Triple[NodeCount];
            Weights = new float[NodeCount];
            Weights = Util.InitializeArray(NodeCount, Weight);

            int[] moveCounts = new int[NodeCount];

            for (int i = 0; i < NodeIndices.Length; i++)
                for (int j = i + 1; j < NodeIndices.Length; j++)
                {
                    Triple move = allNodes[NodeIndices[i]].Position - allNodes[NodeIndices[j]].Position;
                    float d = move.Length;

                    if (d < Radii[i] + Radii[j])
                    {
                        move *= 0.5f * (Radii[i] + Radii[j] - d) / d;
                        Moves[i] += move;
                        Moves[j] -= move;
                        moveCounts[i]++;
                        moveCounts[j]++;
                    }
                }

            for (int i = 0; i < NodeIndices.Length; i++)
                if (moveCounts[i] != 0) Moves[i] /= moveCounts[i];
        }
    }
}
