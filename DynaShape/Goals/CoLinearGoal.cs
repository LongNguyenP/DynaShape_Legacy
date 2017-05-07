using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;


namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public class CoLinearGoal : Goal
    {
        public CoLinearGoal(List<Triple> nodeStartingPositions, float weight = 1f)
        {
            if (nodeStartingPositions.Count < 3) throw new Exception("CoLinear Goal: Node count must be at least 3");
            Weight = weight;
            StartingPositions = nodeStartingPositions.ToArray();
            Moves = new Triple[StartingPositions.Length];
        }

        public override void Compute(List<Node> allNodes)
        {
            List<Triple> points = new List<Triple>(NodeCount);
            for (int i = 0; i < NodeCount; i++)
                points.Add(allNodes[NodeIndices[i]].Position);

            // Here we use our own line fitting function (implemented in the Util class)
            // .. which runs much faster than calling the Dynamo method Line.ByBestFitThroughPoints()

            Triple lineOrigin, lineDirection;
            Util.ComputeBestFitLine(points, out lineOrigin, out lineDirection);

            for (int i = 0; i < NodeCount; i++)
            {
                Triple v = allNodes[NodeIndices[i]].Position - lineOrigin;
                Moves[i] = v.Dot(lineDirection) * lineDirection - v;
            }
        }
    }
}
