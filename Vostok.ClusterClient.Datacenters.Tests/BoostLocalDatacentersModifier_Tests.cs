using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Datacenters;

// ReSharper disable AssignNullToNotNullAttribute

namespace Vostok.Clusterclient.Datacenters.Tests
{
    [TestFixture]
    public class BoostLocalDatacentersModifier_Tests
    {
        private readonly double boostMultiplier = 2.0;
        private readonly double minimumWeightForBoosting = 1.0;
        private BoostLocalDatacentersModifier modifier;
        private IDatacenters datacenters;

        private double weight;

        [SetUp]
        public void SetUp()
        {
            weight = 1.0;
            datacenters = Substitute.For<IDatacenters>();
            modifier = new BoostLocalDatacentersModifier(datacenters, boostMultiplier, minimumWeightForBoosting);
        }

        [Test]
        public void Modify_should_not_modify_weight_when_weight_is_less_than_minimumWeightForBoosting()
        {
            weight = 0.9;
            modifier.Modify(new Uri("http://url.com/"), null, null, null, null, ref weight);
            weight.Should().Be(0.9);
        }

        [Test]
        public void Modify_should_not_modify_weight_when_is_not_local_dc()
        {
            datacenters.GetLocalDatacenter().Returns("dc");
            datacenters.GetDatacenterWeak(Arg.Any<string>()).Returns("dc1");
            modifier.Modify(new Uri("http://url.com/"), null, null, null, null, ref weight);
            weight.Should().Be(1.0);
        }

        [Test]
        public void Modify_should_modify_weight_when_is_local_dc()
        {
            datacenters.GetLocalDatacenter().Returns("dc");
            datacenters.GetDatacenterWeak(Arg.Any<string>()).Returns("dc");
            modifier.Modify(new Uri("http://url.com/"), null, null, null, null, ref weight);
            weight.Should().Be(1.0 * boostMultiplier);
        }
    }
}