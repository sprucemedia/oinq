using System;
using NUnit.Framework;
using Oinq.EdgeSpring.Web;

namespace Oinq.EdgeSpring.Tests
{
    [TestFixture]
    public class When_creating_an_update
    {
        private readonly Uri _uri = new Uri("http://server.com:8000/remote?edgemart=FakeData");
        private EdgeMart<FakeData> _edgeMart;
        private FakeData _originalFake = new FakeData();
        private FakeData _newFake = new FakeData();

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _edgeMart = new EdgeMart<FakeData>(_uri);
        }

        [Test]
        public void it_can_create_with_a_edgemart_and_object()
        {
            // Act
            var update = new Update<FakeData>(_edgeMart, _originalFake, _newFake);

            // Assert
            Assert.IsNotNull(update);
        }

        [Test]
        public void it_requires_an_edgemart()
        {
            // Arrange
            EdgeMart<FakeData> nullFake = null;

            // Act
            TestDelegate a = () => new Update<FakeData>(nullFake, _originalFake, _newFake);

            // Assert
            Assert.Throws<ArgumentNullException>(a);
        }

        [Test]
        public void it_requires_an_original_object()
        {
            // Arrange
            FakeData nullFake = null;

            // Act
            TestDelegate a = () => new Update<FakeData>(_edgeMart, nullFake, _newFake);

            // Assert
            Assert.Throws<ArgumentNullException>(a);
        }

        [Test]
        public void it_requires_a_modified_object()
        {
            // Arrange
            FakeData nullFake = null;

            // Act
            TestDelegate a = () => new Update<FakeData>(_edgeMart, _originalFake, nullFake);

            // Assert
            Assert.Throws<ArgumentNullException>(a);
        }

        [Test]
        public void it_can_return_the_server_information()
        {
            // Assert
            var update = new Update<FakeData>(_edgeMart, _originalFake, _newFake);

            // Act
            var absolute = update.ToUri();

            // Assert
            Assert.AreEqual(_edgeMart.Server.Host, absolute.Host);
            Assert.AreEqual(_edgeMart.Server.Port, absolute.Port);
            Assert.AreEqual(_uri.AbsolutePath, absolute.AbsolutePath);
        }

        [Test]
        public void it_can_return_the_action()
        {
            // Assert
            var update = new Update<FakeData>(_edgeMart, _originalFake, _newFake);

            // Act
            var absolute = update.ToUri();

            // Assert
            Assert.True(absolute.Query.Contains("action=update")); 
        }

        [Test]
        public void it_can_return_the_edgemart()
        {
            // Assert
            var update = new Update<FakeData>(_edgeMart, _originalFake, _newFake);

            // Act
            var absolute = update.ToUri();

            // Assert
            Assert.True(absolute.Query.Contains(String.Format("edgemart={0}", _edgeMart.Name)));
        }
    }
}
