using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Storage;
using Vostok.Clusterclient.Core.Ordering.Weighed;
using Vostok.Datacenters;

namespace Vostok.ClusterClient.Datacenters
{
    /// <summary>
    /// <para>A weight modifier that increases weight of replicas in local datacenter.</para>
    /// </summary>
    [PublicAPI]
    public class BoostLocalDatacentersModifier : IReplicaWeightModifier
    {
        private readonly IDatacenters datacenters;
        private readonly Func<double> boostMultiplierProvider;
        private readonly Func<double> minimumWeightForBoostingProvider;

        public BoostLocalDatacentersModifier(
            [NotNull] IDatacenters datacenters,
            double boostMultiplier = Constants.DefaultBoostMultiplier,
            double minimumWeightForBoosting = Constants.DefaultMinimumWeightForBoosting)
            : this(datacenters, () => boostMultiplier, () => minimumWeightForBoosting)
        {
        }

        public BoostLocalDatacentersModifier(
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
            if (weight < minimumWeightForBoostingProvider())
                return;

            var localDatacenter = datacenters.GetLocalDatacenter();
            var replicaDatacenter = datacenters.GetDatacenter(replica.Host);

            if (string.Equals(localDatacenter, replicaDatacenter, StringComparison.OrdinalIgnoreCase))
                weight *= boostMultiplierProvider();
        }

        public void Learn(ReplicaResult result, IReplicaStorageProvider storageProvider)
        {
        }
    }
}