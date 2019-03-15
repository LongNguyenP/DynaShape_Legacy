using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;


namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public class SphereCollisionGoal : Goal
    {
        public float[] Radii;

        public SphereCollisionGoal(List<Triple> nodeStartingPositions, List<float> radii, float weight = 1000f)
        {
            if (nodeStartingPositions.Count != radii.Count)
                throw new Exception("nodeStartingPositions count does not match radii count");

            Weight = weight;
            Radii = radii.ToArray();
            StartingPositions = nodeStartingPositions.ToArray();
            Moves = new Triple[nodeStartingPositions.Count];
            Weights = new float[nodeStartingPositions.Count];
        }


        public override void Compute(List<Node> allNodes)
        {
            Moves = new Triple[NodeCount];
            Weights = new float[NodeCount];
            Weights = Util.InitializeArray(NodeCount, Weight);

            int[] counts = new int[NodeCount];

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
                        counts[i]++;
                        counts[j]++;
                    }
                }

            for (int i = 0; i < NodeIndices.Length; i++)
                if (counts[i] != 0) Moves[i] /= counts[i];
        }
    }
}
