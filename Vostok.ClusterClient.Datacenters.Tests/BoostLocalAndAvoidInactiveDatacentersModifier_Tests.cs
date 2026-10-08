using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Datacenters;

namespace Vostok.ClusterClient.Datacenters.Tests
{
    [TestFixture]
    public class BoostLocalAndAvoidInactiveDatacentersModifier_Tests
    {
        private readonly double boostMultiplier = 2.0;
        private readonly double minimumWeightForBoosting = 1.0;
        private BoostLocalAndAvoidInactiveDatacentersModifier modifier;
        private IDatacenters datacenters;

        private double weight;

        [SetUp]
        public void SetUp()
        {
            weight = 1.0;
            datacenters = Substitute.For<IDatacenters>();
            modifier = new BoostLocalAndAvoidInactiveDatacentersModifier(datacenters, boostMultiplier, minimumWeightForBoosting);
        }

        [Test]
        public void Modify_should_not_modify_weight_when_whitelist_is_empty_and_replica_is_not_local()
        {
            datacenters.GetActiveDatacenters().Returns(new HashSet<string>());
            datacenters.GetDatacenterWeak(Arg.Any<string>()).Returns("dc1");
            datacenters.GetLocalDatacenter().Returns("dc");

            modifier.Modify(new Uri("http://url.com/"), null, null, null, null, ref weight);
            weight.Should().Be(1.0);
        }

        [Test]
        public void Modify_should_boost_weight_when_whitelist_is_empty_and_replica_is_local()
        {
            datacenters.GetActiveDatacenters().Returns(new HashSet<string>());
            datacenters.GetDatacenterWeak(Arg.Any<string>()).Returns("dc");
            datacenters.GetLocalDatacenter().Returns("dc");

            modifier.Modify(new Uri("http://url.com/"), null, null, null, null, ref weight);
            weight.Should().Be(1.0 * boostMultiplier);
        }

        [Test]
        public void Modify_should_not_modify_weight_when_datacenter_not_found()
        {
            datacenters.GetActiveDatacenters().Returns(new HashSet<string> {"dc"});
            string result = null;
            datacenters.GetDatacenterWeak(Arg.Any<string>()).Returns(result);
            datacenters.GetLocalDatacenter().Returns("dc");

            modifier.Modify(new Uri("http://url.com/"), null, null, null, null, ref weight);
            weight.Should().Be(1.0);
        }

        [Test]
        public void Modify_should_not_modify_weight_when_replica_dc_in_whitelist_and_not_local()
        {
            datacenters.GetActiveDatacenters().Returns(new HashSet<string> {"dc"});
            datacenters.GetDatacenterWeak(Arg.Any<string>()).Returns("dc");
            datacenters.GetLocalDatacenter().Returns("dc1");

            modifier.Modify(new Uri("http://url.com/"), null, null, null, null, ref weight);
            weight.Should().Be(1.0);
        }

        [Test]
        public void Modify_should_boost_weight_when_replica_dc_in_whitelist_and_local()
        {
            datacenters.GetActiveDatacenters().Returns(new HashSet<string> {"dc"});
            datacenters.GetDatacenterWeak(Arg.Any<string>()).Returns("dc");
            datacenters.GetLocalDatacenter().Returns("dc");

            modifier.Modify(new Uri("http://url.com/"), null, null, null, null, ref weight);
            weight.Should().Be(1.0 * boostMultiplier);
        }

        [Test]
        public void Modify_should_set_zero_weight_when_replica_dc_not_in_whitelist()
        {
            datacenters.GetActiveDatacenters().Returns(new HashSet<string> {"dc1"});
            datacenters.GetDatacenterWeak(Arg.Any<string>()).Returns("dc");

            modifier.Modify(new Uri("http://url.com/"), null, null, null, null, ref weight);
            weight.Should().Be(0);
        }

        [Test]
        public void Modify_should_not_modify_weight_when_weight_is_less_than_minimumWeightForBoosting()
        {
            weight = 0.9;
            datacenters.GetActiveDatacenters().Returns(new HashSet<string> {"dc"});
            datacenters.GetDatacenterWeak(Arg.Any<string>()).Returns("dc");
            datacenters.GetLocalDatacenter().Returns("dc");

            modifier.Modify(new Uri("http://url.com/"), null, null, null, null, ref weight);
            weight.Should().Be(0.9);
        }

        [Test]
        public void Modify_should_query_datacenter_only_once()
        {
            datacenters.GetActiveDatacenters().Returns(new HashSet<string> {"dc"});
            datacenters.GetDatacenterWeak(Arg.Any<string>()).Returns("dc");
            datacenters.GetLocalDatacenter().Returns("dc");

            modifier.Modify(new Uri("http://url.com/"), null, null, null, null, ref weight);

            datacenters.Received(1).GetDatacenterWeak(Arg.Any<string>());
            datacenters.Received(1).GetLocalDatacenter();
        }

        [Test]
        public void Modify_should_not_query_local_datacenter_when_weight_is_less_than_minimumWeightForBoosting()
        {
            weight = 0.9;
            datacenters.GetActiveDatacenters().Returns(new HashSet<string> {"dc"});
            datacenters.GetDatacenterWeak(Arg.Any<string>()).Returns("dc");

            modifier.Modify(new Uri("http://url.com/"), null, null, null, null, ref weight);

            datacenters.DidNotReceive().GetLocalDatacenter();
        }

        [Test]
        public void Modify_should_not_query_datacenter_when_whitelist_is_empty_and_weight_is_less_than_minimumWeightForBoosting()
        {
            weight = 0.9;
            datacenters.GetActiveDatacenters().Returns(new HashSet<string>());

            modifier.Modify(new Uri("http://url.com/"), null, null, null, null, ref weight);

            datacenters.DidNotReceive().GetDatacenterWeak(Arg.Any<string>());
        }
    }
}
