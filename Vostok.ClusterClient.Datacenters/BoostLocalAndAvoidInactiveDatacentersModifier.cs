using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Storage;
using Vostok.Clusterclient.Core.Ordering.Weighed;
using Vostok.Datacenters;

namespace Vostok.ClusterClient.Datacenters
{
    /// <summary>
    /// <para>A weight modifier that combines <see cref="AvoidInactiveDatacentersModifier"/> and <see cref="BoostLocalDatacentersModifier"/>.</para>
    /// <para>It applies zero weights for non-active datacenters and increases weight of replicas in local datacenter.</para>
    /// <para>Unlike a pair of separate modifiers, it queries <see cref="IDatacenters.GetDatacenterWeak"/> at most once per replica.</para>
    /// </summary>
    [PublicAPI]
    public class BoostLocalAndAvoidInactiveDatacentersModifier : IReplicaWeightModifier
    {
        private readonly IDatacenters datacenters;
        private readonly Func<double> boostMultiplierProvider;
        private readonly Func<double> minimumWeightForBoostingProvider;

        // (perf): the optional-parameter overload intentionally mirrors BoostLocalDatacentersModifier.
        // RS0027 is a backcompat guard for adding optional parameters to existing APIs; this type is new.
#pragma warning disable RS0027
        public BoostLocalAndAvoidInactiveDatacentersModifier(
            [NotNull] IDatacenters datacenters,
            double boostMultiplier = Constants.DefaultBoostMultiplier,
            double minimumWeightForBoosting = Constants.DefaultMinimumWeightForBoosting)
            : this(datacenters, () => boostMultiplier, () => minimumWeightForBoosting)
        {
        }
#pragma warning restore RS0027

        public BoostLocalAndAvoidInactiveDatacentersModifier(
            [NotNull] IDatacenters datacenters,
            [NotNull] Func<double> boostMultiplierProvider,
            [NotNull] Func<double> minimumWeightForBoostingProvider)
        {
            this.datacenters = datacenters ?? throw new ArgumentNullException(nameof(datacenters));
            this.boostMultiplierProvider = boostMultiplierProvider ?? throw new ArgumentNullException(nameof(boostMultiplierProvider));
            this.minimumWeightForBoostingProvider = minimumWeightForBoostingProvider ?? throw new ArgumentNullException(nameof(minimumWeightForBoostingProvider));
        }

        public void Modify(Uri replica, IList<Uri> allReplicas, IReplicaStorageProvider storageProvider, Request request, RequestParameters parameters, ref double weight)
        {
            var activeDatacenters = datacenters.GetActiveDatacenters();

            // (perf): resolve the replica datacenter at most once and reuse it for both checks.
            var replicaDatacenterResolved = activeDatacenters.Count > 0;
            var replicaDatacenter = replicaDatacenterResolved ? datacenters.GetDatacenterWeak(replica.Host) : null;

            if (replicaDatacenter != null && !activeDatacenters.Contains(replicaDatacenter))
            {
                weight = 0;
                return;
            }

            if (weight < minimumWeightForBoostingProvider())
                return;

            if (!replicaDatacenterResolved)
                replicaDatacenter = datacenters.GetDatacenterWeak(replica.Host);

            var localDatacenter = datacenters.GetLocalDatacenter();
            if (string.Equals(localDatacenter, replicaDatacenter, StringComparison.OrdinalIgnoreCase))
                weight *= boostMultiplierProvider();
        }

        public void Learn(ReplicaResult result, IReplicaStorageProvider storageProvider)
        {
        }
    }
}
