using System;
using NUnit.Framework;

namespace Oinq.EdgeSpring.Tests
{
    [TestFixture]
    public class When_creating_an_UrlBuilder
    {
        private const String SERVER_NAME = "edgespring.com";
        private const Int32 DEFAULT_PORT = 8000;
        private const String EDGEMART_NAME = "em";

        [Test]
        public void it_can_create_with_defaults()
        {
            // Arrange
            var connectionString = "http://";

            // Act
            var builder = new UrlBuilder();

            // Assert
            Assert.AreEqual(null, builder.Server);
            Assert.AreEqual(null, builder.EdgeMartName);
            Assert.AreEqual(connectionString, builder.ToString());
        }

        [Test]
        public void it_can_create_with_a_host()
        {
            // Arrange
            var connectionString = String.Format("http://{0}:{1}/remote", SERVER_NAME, DEFAULT_PORT.ToString());

            // Act
            var builder = new UrlBuilder() { Server = new ServerAddress(SERVER_NAME) };

            // Assert
            Assert.AreEqual(SERVER_NAME, builder.Server.Host);
            Assert.AreEqual(DEFAULT_PORT, builder.Server.Port);
            Assert.AreEqual(connectionString, builder.ToString());
            Assert.AreEqual(connectionString, new UrlBuilder(connectionString).ToString());
        }

        [Test]
        public void it_can_create_with_a_host_and_port()
        {
            // Arrange
            Int32 port = 12345;
            var connectionString = String.Format("http://{0}:{1}/remote", SERVER_NAME, port.ToString());

            // Act
            var builder = new UrlBuilder() { Server = new ServerAddress(SERVER_NAME, port) };

            // Assert
            Assert.AreEqual(SERVER_NAME, builder.Server.Host);
            Assert.AreEqual(port, builder.Server.Port);
            Assert.AreEqual(connectionString, builder.ToString());
            Assert.AreEqual(connectionString, new UrlBuilder(connectionString).ToString());
        }

        [Test]
        public void it_can_create_with_a_host_and_port_and_database()
        {
            // Arrange
            Int32 port = 12345;
            var connectionString = String.Format("http://{0}:{1}/remote?edgemart={2}", SERVER_NAME, port.ToString(), EDGEMART_NAME);

            // Act
            var builder = new UrlBuilder() { Server = new ServerAddress(SERVER_NAME, port), EdgeMartName = EDGEMART_NAME };

            // Assert
            Assert.AreEqual(SERVER_NAME, builder.Server.Host);
            Assert.AreEqual(port, builder.Server.Port);
            Assert.AreEqual(EDGEMART_NAME, builder.EdgeMartName);
            Assert.AreEqual(connectionString, builder.ToString());
            Assert.AreEqual(connectionString, new UrlBuilder(connectionString).ToString());
        }
    }
}