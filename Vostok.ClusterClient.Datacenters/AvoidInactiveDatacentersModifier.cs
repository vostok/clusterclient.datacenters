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
    /// <para>A weight modifier that applies zero weights for non-active datacenters.</para>
    /// </summary>
    [PublicAPI]
    public class AvoidInactiveDatacentersModifier : IReplicaWeightModifier
    {
        private readonly IDatacenters datacenters;

        public AvoidInactiveDatacentersModifier([NotNull] IDatacenters datacenters)
        {
            this.datacenters = datacenters ?? throw new ArgumentNullException(nameof(datacenters));
        }

        public void Modify(Uri replica, IList<Uri> allReplicas, IReplicaStorageProvider storageProvider, Request request, RequestParameters parameters, ref double weight)
        {
            var active = datacenters.GetActiveDatacenters();
            if (active.Count == 0)
                return;

            var replicaDatacenter = datacenters.GetDatacenterWeak(replica.Host);
            if (replicaDatacenter == null)
                return;

            if (active.Contains(replicaDatacenter))
                return;

            weight = 0;
        }

        public void Learn(ReplicaResult result, IReplicaStorageProvider storageProvider)
        {
        }
    }
}