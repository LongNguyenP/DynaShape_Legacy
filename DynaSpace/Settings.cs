using Autodesk.DesignScript.Runtime;

namespace DynaSpace
{
    public class Settings
    {
        public float DampingFactor;
        public int Iterations;
        public float BoundaryStrength;
        public float PlanarConstraintStrength;
        public float SphereCollisionStrength;
        public float DepartmentCohesionStrength;
        public float SpaceAdjacencyStrength;
        public float SpaceDepartmentAdjacencyStrength;

        internal Settings() { }

        /// <summary>
        /// These settings are used to fine-tune how the DynaSpace engine runs.
        /// </summary>
        /// <param name="dampingFactor"></param>
        /// <param name="iterations">The number of iterations that the solver will execute in the background before display the intermediate result. If set to 0 (the default value), the solver will attempt run as many iterations as possible within approximately 25 milliseconds, which is sufficient for real-time visual feedback. Using a small value (e.g. 1) will make the solver appears to run more slowly and display more intermediate result, allowing us to better observe and understand how the nodes and goals behave.</param>
        /// <param name="boundaryStrength">Control the degree to which the space bubbles should respect the site boundary and not cross over it (Note: in this early DynaSpace version, the boundary polygon MUST be a convex polygon).</param>
        /// <param name="planarConstraintStrength">Control the degree to which the space bubbles stick the 2D plane horizontal plane. If this value is set to 0 or too low, the space bubbles will deviate from the 2D plane and start floating in 3D space, which is of course will look weird. However, initially setting this value to 0 and slowing increase it might give a better result.</param>
        /// <param name="sphereCollisionStrength">Control the degree to which the space bubbles do not overlap with each other.</param>
        /// <param name="departmentCohesionStrength">Control the degree to which space bubbles within the same departments should stay close together.</param>
        /// <param name="spaceAdjacencyStrength">Control the degree to which an adjacent pair of space bubbles stay close together.</param>
        /// <param name="spaceDepartmentAdjacencyStrength">Control the degree to which an adjacent pair of space bubbles stay close together.</param>
        /// <returns>All the setting values grouped together as a single item, which can be input into the Engine.Execute node</returns>
        public static Settings Create(
            [DefaultArgument("0.0")] float dampingFactor,
            [DefaultArgument("0")] int iterations,
            [DefaultArgument("200.0")] float boundaryStrength,
            [DefaultArgument("10.0")] float planarConstraintStrength,
            [DefaultArgument("100.0")] float sphereCollisionStrength,
            [DefaultArgument("0.5")] float departmentCohesionStrength,
            [DefaultArgument("0.5")] float spaceAdjacencyStrength,
            [DefaultArgument("0.0")] float spaceDepartmentAdjacencyStrength)
        {
            return new Settings()
            {
                DampingFactor = dampingFactor,
                Iterations = iterations,
                BoundaryStrength = boundaryStrength,
                PlanarConstraintStrength = planarConstraintStrength,
                SphereCollisionStrength = sphereCollisionStrength,
                DepartmentCohesionStrength = departmentCohesionStrength,
                SpaceAdjacencyStrength = spaceAdjacencyStrength,
                SpaceDepartmentAdjacencyStrength = spaceDepartmentAdjacencyStrength
            };
        }
    }
}
