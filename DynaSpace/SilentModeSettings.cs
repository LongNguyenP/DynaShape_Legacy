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


        /// <summary>
        /// Create the settings that enable the DynaSpace engine to run in Silent Mode.
        /// </summary>
        /// <param name="maxIterationCount">The maximum number of iterations the engine can run before outputting the result</param>
        /// <param name="terminationThreshold">If the space bubbles (almost) stop moving (under this threshold value), then the DynaSpace engine will stop and output the result</param>
        /// <param name="sphereCollisionKickin">The iteration at which the sphereCollision constraint become active</param>
        /// <param name="planarConstraintKickin">The iteration at which the planar constraint become active</param>
        /// <param name="boundaryKickin">The iteration at which the boundary constraint becomes active</param>
        /// <returns>All the settings values  grouped into a single item</returns>
        public static SilentModeSettings Create(
                int maxIterationCount = 200000,
                float terminationThreshold = 0.0001f,
                int sphereCollisionKickin = 10000,
                int planarConstraintKickin = 20000,
                int boundaryKickin = 30000)
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
