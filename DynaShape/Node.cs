using Autodesk.DesignScript.Runtime;


namespace DynaShape
{
    [IsVisibleInDynamoLibrary(false)]
    public class Node
    {
        public Triple StartingPosition;
        public Triple Position;
        public Triple Velocity;

        internal Triple Move;

        public Node(Triple startingPosition)
        {
            StartingPosition = startingPosition;
            Position = startingPosition;
            Velocity = Triple.Zero;
        }

        public void Reset()
        {
            Position = StartingPosition;
            Velocity = Triple.Zero;
        }
    }
}
