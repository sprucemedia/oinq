using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Oinq.EdgeSpring.Entity;
using Oinq.EdgeSpring.Web;
using Rhino.Mocks;

namespace Oinq.EdgeSpring.Tests
{
    [TestFixture]
    public class When_creating_an_update
    {
        private readonly String _edgeMartUrl = "http://server.com:8000/remote?edgemart=FakeData";
        private IEntity _original;
        private IEntity _new;
        private IEntityInfo _entityInfo;
        private UpdateType _updateType = UpdateType.Dimension; // The choice is irrelevant for these tests.

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
           _original = new FakeDimensionEntity
            {
                Key1 = "Key1Value",
                Key2 = "Key2Value",
                Dim1 = "Dim1Value",
                Dim2 = "Dim2Value",
                Dim3 = "Dim3Value"
            };
            _new = _original;
            _entityInfo = _original.GetEntityProperties();
        }

        [Test]
        public void it_can_create_with_a_edgemart_and_object()
        {
            // Act
            var update = new Update(_edgeMartUrl, _original, _new, _entityInfo);

            // Assert
            Assert.IsNotNull(update);
        }

        [Test]
        public void it_requires_an_edgemart()
        {
            // Arrange
            String nullFake = null;

            // Act
            TestDelegate a = () => new Update(nullFake, _original, _new, _entityInfo);

            // Assert
            Assert.Throws<ArgumentNullException>(a);
        }

        [Test]
        public void it_requires_an_original_object()
        {
            // Arrange
            FakeData nullFake = null;

            // Act
            TestDelegate a = () => new Update(_edgeMartUrl, nullFake, _new, _entityInfo);

            // Assert
            Assert.Throws<ArgumentNullException>(a);
        }

        [Test]
        public void it_requires_a_modified_object()
        {
            // Arrange
            FakeData nullFake = null;

            // Act
            TestDelegate a = () => new Update(_edgeMartUrl, _original, nullFake, _entityInfo);

            // Assert
            Assert.Throws<ArgumentNullException>(a);
        }

        [Test]
        public void it_requires_entity_info()
        {
            // Arrange
            IEntityInfo nullFake = null;

            // Act
            TestDelegate a = () => new Update(_edgeMartUrl, _original, _new, nullFake);

            // Assert
            Assert.Throws<ArgumentNullException>(a);
        }

        [Test]
        public void it_can_return_the_server_information()
        {
            // Arrange
            var update = new Update(_edgeMartUrl, _original, _new, _entityInfo);

            // Act
            var absolute = update.ToUri(_updateType);

            // Assert
            Assert.AreEqual("server.com", absolute.Host);
            Assert.AreEqual(8000, absolute.Port);
            Assert.AreEqual("/remote", absolute.AbsolutePath);
        }

        [Test]
        public void it_can_return_the_action()
        {
            // Assert
            var update = new Update(_edgeMartUrl, _original, _new, _entityInfo);

            // Act
            var absolute = update.ToUri(_updateType);

            // Assert
            Assert.True(absolute.Query.Contains("action=update")); 
        }

        [Test]
        public void it_can_return_the_edgemart()
        {
            // Assert
            var update = new Update(_edgeMartUrl, _original, _new, _entityInfo);

            // Act
            var absolute = update.ToUri(_updateType);

            // Assert
            Assert.True(absolute.Query.Contains("edgemart=FakeData"));
        }
    }

    [TestFixture]
    public class When_building_an_update_url
    {
        private readonly String _edgeMartUrl = "http://server.com:8000/remote?edgemart=FakeData";

        [Test]
        public void it_can_return_the_filters()
        {
            // Arrange
            var original = new FakeDimensionEntity
            {
                Key1 = "Key1Value",
                Key2 = "Key2Value",
                Dim1 = "Dim1Value",
                Dim2 = "Dim2Value",
                Dim3 = "Dim3Value"
            };
            var delta = original;
            var entityInfo = original.GetEntityProperties();
            var update = new Update(_edgeMartUrl, original, delta, entityInfo);

            // Act
            var absolute = update.ToUri(UpdateType.Dimension);

            // Assert
            Assert.True(absolute.Query.Contains("&filters=Key1:Key1Value&filters=Key2:Key2Value"));
        }

        [Test]
        public void it_can_return_changed_dimensions()
        {
            // Arrange
            var original = new FakeDimensionEntity
            {
                Key1 = "Key1Value",
                Key2 = "Key2Value",
                Dim1 = "Dim1Value",
                Dim2 = "Dim2Value",
                Dim3 = "Dim3Value"
            };
            var delta = new FakeDimensionEntity
            {
                Key1 = "Key1Value",
                Key2 = "Key2Value",
                Dim1 = "Changed",
                Dim2 = "Dim2Value",
                Dim3 = "Dim3Value"
            };
            var entityInfo = original.GetEntityProperties();
            var update = new Update(_edgeMartUrl, original, delta, entityInfo);

            // Act
            var absolute = update.ToUri(UpdateType.Dimension);

            // Assert
            Assert.True(absolute.Query.Contains("&filters=Key1:Key1Value&filters=Key2:Key2Value"));
            Assert.True(absolute.Query.Contains("&dims=Dim1:Dim1Value"));
            Assert.True(absolute.Query.Contains("&values=Dim1:Changed"));
            Assert.False(absolute.Query.Contains("&dims=Dim2"));
            Assert.False(absolute.Query.Contains("&dims=Dim3"));
        }

        [Test]
        public void it_can_return_changed_measures()
        {
            // Arrange
            var original = new FakeMeasureEntity
            {
                Key1 = "Key1Value",
                Key2 = "Key2Value",
                Mea1 = 1,
                Mea2 = 2,
                Mea3 = 3,
                Mea4 = 4
            };
            var delta = new FakeMeasureEntity
            {
                Key1 = "Key1Value",
                Key2 = "Key2Value",
                Mea1 = 11,
                Mea2 = 2,
                Mea3 = 3,
                Mea4 = 4
            };
            var entityInfo = original.GetEntityProperties();
            var update = new Update(_edgeMartUrl, original, delta, entityInfo);

            // Act
            var absolute = update.ToUri(UpdateType.Measure);

            // Assert
            Assert.True(absolute.Query.Contains("&filters=Key1:Key1Value&filters=Key2:Key2Value"));
            Assert.True(absolute.Query.Contains("&measures=Mea1:1"));
            Assert.True(absolute.Query.Contains("&values=Mea1:11"));
            Assert.False(absolute.Query.Contains("&measures=Mea2"));
            Assert.False(absolute.Query.Contains("&measures=Mea3"));
            Assert.False(absolute.Query.Contains("&measures=Mea4"));
        }
    }
}
