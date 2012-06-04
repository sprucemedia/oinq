using System;
using NUnit.Framework;

namespace Oinq.EdgeSpring.Tests
{
    [TestFixture]
    public class When_creating_a_url
    {
        private const String SERVER_NAME = "edgespring.com";
        private const Int32 DEFAULT_PORT = 8000;
        private const String EDGEMART_NAME = "em";

        [Test]
        public void it_can_create_with_defaults()
        {
            // Arrange
            var connectionString = String.Format("http://{0}/remote", SERVER_NAME);

            // Act
            var url = new Url(connectionString);

            // Assert
            Assert.AreEqual(DEFAULT_PORT, url.Server.Port);
            Assert.AreEqual(SERVER_NAME, url.Server.Host);
        }

        [Test]
        public void it_can_create_with_a_host()
        {
            // Arrange
            var connectionString = String.Format("http://{0}:{1}/remote", SERVER_NAME, DEFAULT_PORT.ToString());

            // Act
            var url = new Url(connectionString);

            // Assert
            Assert.AreEqual(connectionString, url.ToString());
            Assert.AreEqual(DEFAULT_PORT, url.Server.Port);
            Assert.AreEqual(SERVER_NAME, url.Server.Host);
        }

        [Test]
        public void it_can_create_with_a_host_and_port()
        {
            // Arrange
            Int32 port = 12345;
            var connectionString = String.Format("http://{0}:{1}/remote", SERVER_NAME, port.ToString());

            // Act
            var url = new Url(connectionString);

            // Assert
            Assert.AreEqual(connectionString, url.ToString());
            Assert.AreEqual(port, url.Server.Port);
            Assert.AreEqual(SERVER_NAME, url.Server.Host);
        }

        [Test]
        public void it_can_create_with_a_host_and_port_and_database()
        {
            // Arrange
            Int32 port = 12345;
            var connectionString = String.Format("http://{0}:{1}/remote?edgemart={2}", SERVER_NAME, port.ToString(), EDGEMART_NAME);

            // Act
            var url = new Url(connectionString);

            // Assert
            Assert.AreEqual(connectionString, url.ToString());
            Assert.AreEqual(port, url.Server.Port);
            Assert.AreEqual(SERVER_NAME, url.Server.Host);
            Assert.AreEqual(EDGEMART_NAME, url.EdgeMartName);
        }
    }
}