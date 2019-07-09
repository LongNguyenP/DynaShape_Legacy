namespace DynaSpace
{
    public class SilentModeSettings
    {
        public int MaxIterationCount;
        public float TerminationThreshold;
        public int SphereCollisionKickin;
        public int PlanarConstraintKickin;

        internal SilentModeSettings() { }

        public static SilentModeSettings Create(int maxIterationCount, float terminationThreshold, int sphereCollisionKickin = 2500, int planarConstraintKickin = 5000)
        {
            return new SilentModeSettings()
            {
                MaxIterationCount = maxIterationCount,
                TerminationThreshold = terminationThreshold,
                SphereCollisionKickin = sphereCollisionKickin,
                PlanarConstraintKickin = planarConstraintKickin
            };
        }

    }
}
