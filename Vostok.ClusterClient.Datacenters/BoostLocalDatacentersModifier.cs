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
        private readonly double boostMultiplier;
        private readonly double minimumWeightForBoosting;

        public BoostLocalDatacentersModifier(
            [NotNull] IDatacenters datacenters,
            double boostMultiplier = 3.0,
            double minimumWeightForBoosting = 0.75)
        {
            this.datacenters = datacenters ?? throw new ArgumentNullException(nameof(datacenters));
            this.boostMultiplier = boostMultiplier;
            this.minimumWeightForBoosting = minimumWeightForBoosting;
        }

        public void Modify(Uri replica, IList<Uri> allReplicas, IReplicaStorageProvider storageProvider, Request request, RequestParameters parameters, ref double weight)
        {
            if (weight < minimumWeightForBoosting)
                return;

            var localDatacenter = datacenters.GetLocalDatacenter();
            var replicaDatacenter = datacenters.GetDatacenter(replica.Host);

            if (string.Equals(localDatacenter, replicaDatacenter, StringComparison.OrdinalIgnoreCase))
                weight *= boostMultiplier;
        }

        public void Learn(ReplicaResult result, IReplicaStorageProvider storageProvider)
        {
        }
    }
}