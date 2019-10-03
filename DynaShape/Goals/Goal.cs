using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;


namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public abstract class Goal
    {
        public float Weight;
        internal float[] Weights = new float[0];
        internal Triple[] StartingPositions = new Triple[0];
        internal int[] NodeIndices = new int[0];
        internal Triple[] Moves = new Triple[0];

        public int NodeCount => StartingPositions.Length;
        internal abstract void Compute(List<Node> allNodes);
        public virtual List<object> GetOutput(List<Node> allNodes) => null;
    }
}
