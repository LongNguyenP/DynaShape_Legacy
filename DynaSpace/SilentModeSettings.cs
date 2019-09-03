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

        public static SilentModeSettings Create(int maxIterationCount, float terminationThreshold, int sphereCollisionKickin = 10000, int planarConstraintKickin = 20000, int boundaryKickin = 30000)
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
