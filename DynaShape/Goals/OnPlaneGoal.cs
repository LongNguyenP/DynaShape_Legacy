using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;


namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public class OnPlaneGoal : Goal
    {
        public Triple TargetPlaneOrigin;
        public Triple TargetPlaneNormal
        {
            get => targetPlaneNormal;
            set { targetPlaneNormal = value.Normalise();  }
        }

        public Triple targetPlaneNormal;

        public OnPlaneGoal(List<Triple> nodeStartingPositions, Triple planeOrigin, Triple planeNormal, float weight = 1f)
        {
            TargetPlaneOrigin = planeOrigin;
            TargetPlaneNormal = planeNormal;
            Weight = weight;
            StartingPositions = nodeStartingPositions.ToArray();
            Moves = new Triple[StartingPositions.Length];
            Weights = new float[StartingPositions.Length];
        }


        public OnPlaneGoal(List<Triple> nodeStartingPositions, Plane plane, float weight = 1f)
            : this(nodeStartingPositions, plane.Origin.ToTriple(), plane.Normal.ToTriple(), weight)
        {
        }


        internal override void Compute(List<Node> allNodes)
        {
            for (int i = 0; i < NodeCount; i++)
            {
                Moves[i] = TargetPlaneNormal * TargetPlaneNormal.Dot(TargetPlaneOrigin - allNodes[NodeIndices[i]].Position);
                Weights[i] = Weight;
            }
        }
    }
}
