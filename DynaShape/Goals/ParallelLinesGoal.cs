using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;

namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public class ParallelLinesGoal : Goal
    {
        public ParallelLinesGoal(List<Triple> pointPairs, float weight = 1f)
        {
            Weight = weight;
            if (pointPairs.Count % 2 != 0) throw new Exception("Equal-Length Goal: Node count must be even");
            StartingPositions = pointPairs.ToArray();
            Moves = new Triple[StartingPositions.Length];          
            Weights = new float[StartingPositions.Length];
        }


        public override void Compute(List<Node> allNodes)
        {
            int lineCount = NodeCount / 2;
            Triple targetDirection = Triple.Zero;
            for (int i = 0; i < lineCount; i++)
                targetDirection += (allNodes[NodeIndices[2 * i + 1]].Position - allNodes[NodeIndices[2 * i]].Position).Normalise();

            targetDirection =
                targetDirection.IsAlmostZero()
                    ? Triple.BasisX
                    : targetDirection.Normalise();


            for (int i = 0; i < lineCount; i++)
            {
                Triple v = allNodes[NodeIndices[2 * i + 1]].Position - allNodes[NodeIndices[2 * i]].Position;
                Moves[2 * i] = 0.5f * (v - v.Dot(targetDirection) * targetDirection);
                Moves[2 * i + 1] = -Moves[2 * i];
                Weights[2 * i] = Weights[2 * i + 1] = Weight;
            }
        }
    }
}
