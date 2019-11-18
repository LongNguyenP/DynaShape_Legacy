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

        public static Settings Create(
        [DefaultArgument("0.95")] float dampingFactor,
        [DefaultArgument("0")] int iterations,
        [DefaultArgument("1.0")] float boundaryStrength,
        [DefaultArgument("1.0")] float planarConstraintStrength,
        [DefaultArgument("30.0")] float sphereCollisionStrength,
        [DefaultArgument("0.02")] float departmentCohesionStrength,
        [DefaultArgument("0.1")] float spaceAdjacencyStrength,
        [DefaultArgument("0.1")] float spaceDepartmentAdjacencyStrength)
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
