using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;


namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public class EqualLengthsGoal : Goal
    {
        public EqualLengthsGoal(List<Triple> pointPairs, float weight = 1f)
        {
            if (pointPairs.Count % 2 != 0) throw new Exception("Equal-Length Goal: Node count must be even");
            StartingPositions = pointPairs.ToArray();
            Moves = new Triple[StartingPositions.Length];
            Weight = weight;
        }


        public override void Compute(List<Node> allNodes)
        {
            int segmentCount = NodeCount / 2;
            float totalLength = 0f;
            for (int i = 0; i < segmentCount; i++)
                totalLength += allNodes[NodeIndices[2 * i]].Position.DistanceTo(allNodes[NodeIndices[2 * i + 1]].Position);

            float targetLength = totalLength / segmentCount;

            for (int i = 0; i < segmentCount; i++)
            {
                Triple move = allNodes[NodeIndices[2 * i + 1]].Position - allNodes[NodeIndices[2 * i]].Position;
                move *= 0.5f * (move.Length - targetLength) / targetLength;
                Moves[2 * i] = move;
                Moves[2 * i + 1] = -move;
            }
        }
    }
}
