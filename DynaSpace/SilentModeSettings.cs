namespace DynaSpace
{
    public class SilentModeSettings
    {
        public int MaxIterationCount;
        public float TerminationThreshold;
        public int SphereCollisionKickin;
        public int PlanarConstraintKickin;
        public int BoundaryKickin;

        internal SilentModeSettings() { }

        public static SilentModeSettings Create(int maxIterationCount = 500000, float terminationThreshold = 0.0003f, int sphereCollisionKickin = 5000, int planarConstraintKickin = 10000, int boundaryKickin = 15000)
        {
            return new SilentModeSettings()
            {
                MaxIterationCount = maxIterationCount,
                TerminationThreshold = terminationThreshold,
                SphereCollisionKickin = sphereCollisionKickin,
                PlanarConstraintKickin = planarConstraintKickin,
                BoundaryKickin = boundaryKickin
            };
        }

    }
}
