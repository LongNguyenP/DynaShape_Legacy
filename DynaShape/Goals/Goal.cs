using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;


namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public abstract class Goal
    {
        public float Weight;
        public float[] Weights;
        public Triple[] StartingPositions;
        public int[] NodeIndices;
        public Triple[] Moves;

        public int NodeCount => StartingPositions.Length;
        public abstract void Compute(List<Node> allNodes);
        public virtual List<object> GetOutput(List<Node> allNodes) => null;
    }
}
