using System;
using NUnit.Framework;
using Oinq.EdgeSpring.Web;
using Rhino.Mocks;
using System.Collections.Generic;

namespace Oinq.EdgeSpring.Tests
{
    [TestFixture]
    public class When_creating_an_update
    {
        private readonly String _edgeMartUrl = "http://server.com:8000/remote?edgemart=FakeData";
        private IUpdateable _originalFake;
        private IUpdateable _newFake;
        private IDictionary<String, String> _keys = new Dictionary<String, String>();

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _keys.Add("Key1", "Key1Value");
            _keys.Add("Key2", "Key2Value");
            _originalFake = MockRepository.GenerateMock<IUpdateable>();
            _originalFake.Stub(s => s.GetKeys()).Return(_keys);

            _newFake = MockRepository.GenerateMock<IUpdateable>();
        }

        [Test]
        public void it_can_create_with_a_edgemart_and_object()
        {
            // Act
            var update = new Update(_edgeMartUrl, _originalFake, _newFake);

            // Assert
            Assert.IsNotNull(update);
        }

        [Test]
        public void it_requires_an_edgemart()
        {
            // Arrange
            String nullFake = null;

            // Act
            TestDelegate a = () => new Update(nullFake, _originalFake, _newFake);

            // Assert
            Assert.Throws<ArgumentNullException>(a);
        }

        [Test]
        public void it_requires_an_original_object()
        {
            // Arrange
            FakeData nullFake = null;

            // Act
            TestDelegate a = () => new Update(_edgeMartUrl, nullFake, _newFake);

            // Assert
            Assert.Throws<ArgumentNullException>(a);
        }

        [Test]
        public void it_requires_a_modified_object()
        {
            // Arrange
            FakeData nullFake = null;

            // Act
            TestDelegate a = () => new Update(_edgeMartUrl, _originalFake, nullFake);

            // Assert
            Assert.Throws<ArgumentNullException>(a);
        }

        [Test]
        public void it_can_return_the_server_information()
        {
            // Arrange
            var update = new Update(_edgeMartUrl, _originalFake, _newFake);

            // Act
            var absolute = update.ToUri();

            // Assert
            Assert.AreEqual("server.com", absolute.Host);
            Assert.AreEqual(8000, absolute.Port);
            Assert.AreEqual("/remote", absolute.AbsolutePath);
        }

        [Test]
        public void it_can_return_the_action()
        {
            // Assert
            var update = new Update(_edgeMartUrl, _originalFake, _newFake);

            // Act
            var absolute = update.ToUri();

            // Assert
            Assert.True(absolute.Query.Contains("action=update")); 
        }

        [Test]
        public void it_can_return_the_edgemart()
        {
            // Assert
            var update = new Update(_edgeMartUrl, _originalFake, _newFake);

            // Act
            var absolute = update.ToUri();

            // Assert
            Assert.True(absolute.Query.Contains("edgemart=FakeData"));
        }
    }

    [TestFixture]
    public class When_building_an_update_url
    {
        private readonly String _edgeMartUrl = "http://server.com:8000/remote?edgemart=FakeData";
        private IUpdateable _originalFake;
        private IUpdateable _newFake;
        private IDictionary<String, String> _keys = new Dictionary<String, String>();

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _keys.Add("Key1", "Key1Value");
            _keys.Add("Key2", "Key2Value");
            _originalFake = MockRepository.GenerateMock<IUpdateable>();
            _originalFake.Stub(s => s.GetKeys()).Return(_keys);

            _newFake = MockRepository.GenerateMock<IUpdateable>();
        }

        [Test]
        public void it_can_return_the_filters()
        {
            // Assert
            var update = new Update(_edgeMartUrl, _originalFake, _newFake);

            // Act
            var absolute = update.ToUri();

            // Assert
            Assert.True(absolute.Query.Contains("&filters=Key1:Key1Value&filters=Key2:Key2Value"));
        }
    }
}
