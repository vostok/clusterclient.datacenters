using System;
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
            double boostMultiplier = Constants.DefaultBoostMultiplier,
            double minimumWeightForBoosting = Constants.DefaultMinimumWeightForBoosting)
        {
            self.AddModifier(new BoostLocalDatacentersModifier(datacenters, boostMultiplier, minimumWeightForBoosting));
        }

        /// <summary>
        /// Adds a <see cref="BoostLocalDatacentersModifier"/> that will increase weight of replicas in local datacenter.
        /// </summary>
        public static void SetupBoostLocalDatacentersWeightModifier(
            [NotNull] this IWeighedReplicaOrderingBuilder self,
            [NotNull] IDatacenters datacenters,
            [NotNull] Func<double> boostMultiplierProvider,
            [NotNull] Func<double> minimumWeightForBoostingProvider)
        {
            self.AddModifier(new BoostLocalDatacentersModifier(datacenters, boostMultiplierProvider, minimumWeightForBoostingProvider));
        }

        /// <summary>
        /// <para>Adds a <see cref="BoostLocalAndAvoidInactiveDatacentersModifier"/> that combines <see cref="AvoidInactiveDatacentersModifier"/> and <see cref="BoostLocalDatacentersModifier"/>.</para>
        /// <para>It applies zero weights for non-active datacenters and increases weight of replicas in local datacenter, querying <see cref="IDatacenters.GetDatacenterWeak"/> at most once per replica.</para>
        /// </summary>
        // (perf): the optional-parameter overload intentionally mirrors SetupBoostLocalDatacentersWeightModifier.
        // RS0027 requires the overload with optional parameters to have the most parameters among its overloads;
        // here it has the same parameter count as the provider overload. The existing methods are grandfathered
        // in PublicAPI.Shipped.txt, while this new API is unshipped, so the analyzer flags it.
#pragma warning disable RS0027
        public static void SetupBoostLocalAndAvoidInactiveDatacentersWeightModifier(
            [NotNull] this IWeighedReplicaOrderingBuilder self,
            [NotNull] IDatacenters datacenters,
            double boostMultiplier = Constants.DefaultBoostMultiplier,
            double minimumWeightForBoosting = Constants.DefaultMinimumWeightForBoosting)
        {
            self.AddModifier(new BoostLocalAndAvoidInactiveDatacentersModifier(datacenters, boostMultiplier, minimumWeightForBoosting));
        }
#pragma warning restore RS0027

        /// <summary>
        /// <para>Adds a <see cref="BoostLocalAndAvoidInactiveDatacentersModifier"/> that combines <see cref="AvoidInactiveDatacentersModifier"/> and <see cref="BoostLocalDatacentersModifier"/>.</para>
        /// <para>It applies zero weights for non-active datacenters and increases weight of replicas in local datacenter, querying <see cref="IDatacenters.GetDatacenterWeak"/> at most once per replica.</para>
        /// </summary>
        public static void SetupBoostLocalAndAvoidInactiveDatacentersWeightModifier(
            [NotNull] this IWeighedReplicaOrderingBuilder self,
            [NotNull] IDatacenters datacenters,
            [NotNull] Func<double> boostMultiplierProvider,
            [NotNull] Func<double> minimumWeightForBoostingProvider)
        {
            self.AddModifier(new BoostLocalAndAvoidInactiveDatacentersModifier(datacenters, boostMultiplierProvider, minimumWeightForBoostingProvider));
        }
    }
}