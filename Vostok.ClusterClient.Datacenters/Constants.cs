namespace Vostok.ClusterClient.Datacenters
{
    internal static class Constants
    {
        // Note(kungurtsev): amount of local traffic is x/(x + n - 1), where n is the number of datacenters.
        public const double DefaultBoostMultiplier = 5;
        
        public const double DefaultMinimumWeightForBoosting = 0.3;
    }
}