using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Autodesk.DesignScript.Runtime;


namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public class SphereStaticLineCollisionGoal : Goal
    {
        public float[] Radii;
        public List<Triple> LineStarts;
        public List<Triple> LineEnds;

        public SphereStaticLineCollisionGoal(List<Triple> centers, List<float> radii, List<Triple> lineStarts, List<Triple> lineEnds, float weight = 1000f)
        {
            Weight = weight;
            Radii = radii.ToArray();
            StartingPositions = centers.ToArray();
            LineStarts = lineStarts;
            LineEnds = lineEnds;
            Moves = new Triple[centers.Count];
            Weights = new float[centers.Count];
        }


        public override void Compute(List<Node> allNodes)
        {
            if (LineStarts == null) return;

            Moves.FillArray(Triple.Zero);
            Weights.FillArray(Weight);

            int[] moveCounts = new int[NodeCount];

            for (int j = 0; j < LineStarts.Count; j++)
            {
                Triple s = LineStarts[j];
                Triple e = LineEnds[j];
                Triple d = e - s;
                float lineLength = d.Length;

                d /= lineLength;

                for (int i = 0; i < NodeIndices.Length; i++)
                {
                    Triple c = allNodes[NodeIndices[i]].Position;
                    float r = Radii[i];
                    Triple v = c - s;
                    float shadow = v.Dot(d);

                    if (0f < shadow && shadow < lineLength)
                    {
                        Triple move = v - d * shadow;
                        float distance = move.Length;
                        if (distance < r)
                            Moves[i] += move * (r - distance) / distance;
                        moveCounts[i]++;
                    }
                    else
                    {
                        if (shadow >= lineLength) v = c - e;
                        float vLength = v.Length;
                        if (vLength < r)
                        {
                            Moves[i] += v * (r - vLength) / vLength;
                            moveCounts[i]++;
                        }
                    }
                }
            }

            for (int i = 0; i < NodeIndices.Length; i++)
                if (moveCounts[i] != 0) Moves[i] /= moveCounts[i];
        }
    }
}
