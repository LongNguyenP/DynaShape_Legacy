using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;


namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public abstract class Goal
    {
        internal float Weight;
        internal float[] Weights;
        internal Triple[] StartingPositions;
        internal int[] NodeIndices;
        internal Triple[] Moves;

        public int NodeCount => StartingPositions.Length;
        internal abstract void Compute(List<Node> allNodes);
        public virtual List<object> GetOutput(List<Node> allNodes) => null;
    }
}
