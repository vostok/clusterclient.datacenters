using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Datacenters;

// ReSharper disable AssignNullToNotNullAttribute

namespace Vostok.Clusterclient.Datacenters.Tests
{
    [TestFixture]
    public class AvoidInactiveDatacentersModifier_Tests
    {
        private AvoidInactiveDatacentersModifier modifier;
        private IDatacenters datacenters;
        private double weight = 1.1;

        [SetUp]
        public void SetUp()
        {
            datacenters = Substitute.For<IDatacenters>();
            modifier = new AvoidInactiveDatacentersModifier(datacenters);
        }

        [Test]
        public void Modify_should_not_modify_weight_when_whitelist_is_empty()
        {
            datacenters.GetActiveDatacenters().Returns(new HashSet<string>());

            modifier.Modify(new Uri("http://url.com/"), null, null, null, null, ref weight);
            weight.Should().Be(1.1);
        }

        [Test]
        public void Modify_should_not_modify_weight_when_datacenter_not_found()
        {
            datacenters.GetActiveDatacenters().Returns(new HashSet<string> {"dc"});
            datacenters.GetDatacenterWeak(Arg.Any<string>()).Returns(null as string);
            modifier.Modify(new Uri("http://url.com/"), null, null, null, null, ref weight);
            weight.Should().Be(1.1);
        }

        [Test]
        public void Modify_should_not_modify_weight_when_replica_dc_in_whitelist()
        {
            datacenters.GetActiveDatacenters().Returns(new HashSet<string> {"dc"});
            datacenters.GetDatacenterWeak(Arg.Any<string>()).Returns("dc");
            modifier.Modify(new Uri("http://url.com/"), null, null, null, null, ref weight);
            weight.Should().Be(1.1);
        }

        [Test]
        public void Modify_should_set_zero_weight_when_replica_dc_not_in_whitelist()
        {
            datacenters.GetActiveDatacenters().Returns(new HashSet<string> {"dc1"});
            datacenters.GetDatacenterWeak(Arg.Any<string>()).Returns("dc");

            modifier.Modify(new Uri("http://url.com/"), null, null, null, null, ref weight);
            weight.Should().Be(0);
        }
    }
}