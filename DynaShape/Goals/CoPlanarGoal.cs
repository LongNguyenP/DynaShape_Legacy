using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;


namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public class CoPlanarGoal : Goal
    {
        public CoPlanarGoal(List<Triple> nodeStartingPositions, float weight = 1f)
        {
            if (nodeStartingPositions.Count < 4) throw new Exception("CoPlanar Goal: Node count must be at least 4");
            Weight = weight;
            StartingPositions = nodeStartingPositions.ToArray();
            Moves = new Triple[StartingPositions.Length];
            Weights = new float[StartingPositions.Length];
        }


        public override void Compute(List<Node> allNodes)
        {
            List<Triple> points = new List<Triple>(NodeCount);
            for (int i = 0; i < NodeCount; i++)
                points.Add(allNodes[NodeIndices[i]].Position);

            // Here we use our own plane fitting function (implemented in the Util class)
            // .. which runs much faster than calling the Dynamo method Plane.ByBestFitThroughPoints

            Triple planeOrigin, planeNormal;
            Util.ComputeBestFitPlane(points, out planeOrigin, out planeNormal);

            for (int i = 0; i < NodeCount; i++)
            {
                Moves[i] = (planeOrigin - allNodes[NodeIndices[i]].Position).Dot(planeNormal) * planeNormal;
                Weights[i] = Weight;
            }
        }
    }
}
