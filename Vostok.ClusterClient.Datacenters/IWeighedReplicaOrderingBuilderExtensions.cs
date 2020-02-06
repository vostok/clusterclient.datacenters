using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Ordering.Weighed;
using Vostok.Datacenters;

namespace Vostok.ClusterClient.Datacenters
{
    [PublicAPI]
    public static class IWeighedReplicaOrderingBuilderExtensions
    {
        /// <summary>
        /// Adds a <see cref="AvoidInactiveDatacentersModifier"/> that will apply zero weights for non-active datacenters.
        /// </summary>
        public static void SetupAvoidInactiveDatacentersWeightModifier(
            [NotNull] this IWeighedReplicaOrderingBuilder self,
            [NotNull] IDatacenters datacenters)
        {
            self.AddModifier(new AvoidInactiveDatacentersModifier(datacenters));
        }

        /// <summary>
        /// Adds a <see cref="BoostLocalDatacentersModifier"/> that will increase weight of replicas in local datacenter.
        /// </summary>
        public static void SetupBoostLocalDatacentersWeightModifier(
            [NotNull] this IWeighedReplicaOrderingBuilder self,
            [NotNull] IDatacenters datacenters,
            double boostMultiplier = 3.0,
            double minimumWeightForBoosting = 0.75)
        {
            self.AddModifier(new BoostLocalDatacentersModifier(datacenters, boostMultiplier, minimumWeightForBoosting));
        }
    }
}