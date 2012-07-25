using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Oinq.EdgeSpring.Entity;

namespace Oinq.EdgeSpring.Tests
{
    [TestFixture]
    public class When_building_entity_info
    {
        [Test]
        public void it_can_get_lists_for_an_entity_with_all_types()
        {
            // Arrange
            IEntity entity = new FakeFullEntity();

            // Act
            IEntityInfo ei = entity.GetEntityProperties();

            // Assert
            Assert.NotNull(ei);
            Assert.NotNull(ei.Keys);
            Assert.NotNull(ei.Dimensions);
            Assert.NotNull(ei.Measures);
            Assert.AreEqual(2, ei.Keys.Count);
            Assert.AreEqual(3, ei.Dimensions.Count);
            Assert.AreEqual(4, ei.Measures.Count);
        }

        [Test]
        public void it_can_get_lists_for_an_entity_with_no_measures()
        {
            // Arrange
            IEntity entity = new FakeDimensionEntity();

            // Act
            IEntityInfo ei = entity.GetEntityProperties();

            // Assert
            Assert.NotNull(ei);
            Assert.NotNull(ei.Keys);
            Assert.NotNull(ei.Dimensions);
            Assert.NotNull(ei.Measures);
            Assert.AreEqual(2, ei.Keys.Count);
            Assert.AreEqual(3, ei.Dimensions.Count);
            Assert.AreEqual(0, ei.Measures.Count);
        }

        [Test]
        public void it_can_get_lists_for_an_entity_with_no_dimensions()
        {
            // Arrange
            IEntity entity = new FakeMeasureEntity();

            // Act
            IEntityInfo ei = entity.GetEntityProperties();

            // Assert
            Assert.NotNull(ei);
            Assert.NotNull(ei.Keys);
            Assert.NotNull(ei.Dimensions);
            Assert.NotNull(ei.Measures);
            Assert.AreEqual(2, ei.Keys.Count);
            Assert.AreEqual(0, ei.Dimensions.Count);
            Assert.AreEqual(4, ei.Measures.Count);
        }

        [Test]
        public void it_throws_an_exception_for_an_entity_with_no_keys()
        {
            // Arrange
            IEntity entity = new FakeNoKeyEntity();
            IEntityInfo ei;

            // Act
            TestDelegate a = () => ei = entity.GetEntityProperties();

            // Assert
            Assert.Throws<ArgumentException>(a);
        }
    }
}
