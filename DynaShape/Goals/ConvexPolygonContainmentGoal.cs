using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;


namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public class ConvexPolygonContainmentGoal : Goal
    {
        public float[] Radii;

        public List<Triple> PolygonVertices
        {
            get
            {
                List<Triple> result = new List<Triple>();
                for (int i = 0; i < polygonVertices.Count; i++)
                    result.Add(polygonVertices[i]);
                return result;
            }
            set
            {
                if (value == null)
                {
                    polygonVertices = null;
                    return;
                }

                polygonVertices = value;
                planeNormal = Triple.Zero;
                for (int i = 0; i < polygonVertices.Count; i++)
                {
                    int j = i + 1;
                    int k = i + 2;
                    if (j >= polygonVertices.Count) j -= polygonVertices.Count;
                    if (k >= polygonVertices.Count) k -= polygonVertices.Count;
                    planeNormal += (polygonVertices[j] - polygonVertices[i]).Cross(polygonVertices[k] - polygonVertices[j]);
                }
                planeNormal.Normalise();
            }
        }

        private List<Triple> polygonVertices;
        private Triple planeNormal;

        public ConvexPolygonContainmentGoal(
            List<Triple> centers,
            List<float> radii,
            List<Triple> polygonVertices,
            float weight = 1000f)
        {
            Weight = weight;
            Radii = radii.ToArray();
            PolygonVertices = polygonVertices;
            StartingPositions = centers.ToArray();
            Moves = new Triple[centers.Count];
            Weights = new float[centers.Count];
        }


        internal override void Compute(List<Node> allNodes)
        {
            Moves = new Triple[NodeCount];
            Weights = new float[NodeCount];

            if (polygonVertices == null || polygonVertices.Count <=3) return;

            Moves = new Triple[NodeCount];
            Weights = new float[NodeCount];

            int[] moveCounts = new int[NodeCount];

            for (int j = 0; j < polygonVertices.Count; j++)
            {
                Triple s = polygonVertices[j];
                Triple e = polygonVertices[j + 1 < polygonVertices.Count ? j + 1 : 0];
                Triple d = e - s;
                Triple n = d.Cross(planeNormal).Normalise();

                for (int i = 0; i < NodeCount; i++)
                {
                    Triple c = allNodes[NodeIndices[i]].Position;
                    float r = Radii[i];
                    Triple v = c - s;
                    float shadow = n.Dot(v);

                    if (shadow <= -r) continue;

                    Moves[i] += n * (-r - shadow);
                    moveCounts[i]++;
                }

                for (int i = 0; i < NodeIndices.Length; i++)
                    if (moveCounts[i] != 0)
                    {
                        Moves[i] /= moveCounts[i];
                        Weights[i] = Weight;
                    }
            }
        }
    }
}