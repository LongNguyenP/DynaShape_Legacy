using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;


namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public class ConvexPolygonCollisionGoal : Goal
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
                polygonVertices = value;

                if (polygonVertices == null || polygonVertices.Count < 3) return;

                //=====================================================
                // Update the plane normal
                //=====================================================

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

                //=====================================================
                // Update the normals of the polygon edges
                //=====================================================

                normals = new List<Triple>();
                for (int i = 0; i < polygonVertices.Count; i++)
                {
                    int j = i + 1;
                    if (j >= polygonVertices.Count) j -= polygonVertices.Count;
                    normals.Add((polygonVertices[j] - polygonVertices[i]).Cross(planeNormal).Normalise());
                }
            }
        }

        private List<Triple> polygonVertices;
        private List<Triple> normals; // The normals vectors of the polygon edges
        private Triple planeNormal; // The normal of the 2D plane that contains the polygon

        public ConvexPolygonCollisionGoal(
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
            if (polygonVertices == null || polygonVertices.Count < 3) return;

            Moves = new Triple[NodeCount];
            Weights = new float[NodeCount];

            for (int i = 0; i < NodeCount; i++)
            {
                Triple c = allNodes[NodeIndices[i]].Position;
                float r = Radii[i];

                float escapeDistance = float.MaxValue;
                int nearestNormalIndex = -1;
                for (int j = 0; j < polygonVertices.Count; j++)
                {
                    float shadow = normals[j].Dot(c - polygonVertices[j]);
                    if (shadow >= r)
                    {
                        nearestNormalIndex = -1;
                        break;
                    }

                    if (r - shadow < escapeDistance)
                    {
                        escapeDistance = r - shadow;
                        nearestNormalIndex = j;
                    }
                }

                if (nearestNormalIndex >= 0)
                {
                    Moves[i] = normals[nearestNormalIndex] * escapeDistance;
                    Weights[i] = Weight;
                }
            }
        }
    }
}
