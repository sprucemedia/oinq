using System;
using NUnit.Framework;

namespace Oinq.EdgeSpring.Tests
{
    [TestFixture]
    public class When_creating_a_new_server_address
    {
        private const String HOST_NAME = "host";
        private const Int32 DEFAULT_PORT = 8000;

        [Test]
        public void it_can_accept_a_hostname()
        {
            // Act
            var addr = new ServerAddress(HOST_NAME);

            // Assert
            Assert.AreEqual(HOST_NAME, addr.Host);
        }

        [Test]
        public void it_uses_the_default_port_when_none_is_provided()
        {
            // Act
            var addr = new ServerAddress(HOST_NAME);

            // Assert
            Assert.AreEqual(DEFAULT_PORT, addr.Port);
        }

        [Test]
        public void it_can_accept_a_hostname_and_port()
        {
            // Arrange
            Int32 port = 123;

            // Act
            var addr = new ServerAddress(HOST_NAME, port);

            // Assert
            Assert.AreEqual(HOST_NAME, addr.Host);
            Assert.AreEqual(port, addr.Port);
        }
    }

    [TestFixture]
    public class When_working_with_a_server_address
    {
        private const String HOST_NAME = "host";
        private const Int32 DEFAULT_PORT = 8000;

        [Test]
        public void it_can_parse_a_hostname()
        {
            // Act
            var addr = ServerAddress.Parse(HOST_NAME);

            // Assert
            Assert.AreEqual(HOST_NAME, addr.Host);
        }

        [Test]
        public void it_can_parse_a_hostname_without_a_port()
        {
            // Act
            var addr = ServerAddress.Parse(HOST_NAME);

            // Assert
            Assert.AreEqual(DEFAULT_PORT, addr.Port);
        }

        [Test]
        public void it_can_parse_a_hostname_and_port()
        {
            // Arrange
            Int32 port = 123;
            String host = String.Format("{0}:{1}", HOST_NAME, port.ToString());

            // Act
            var addr = ServerAddress.Parse(host);

            // Assert
            Assert.AreEqual(HOST_NAME, addr.Host);
            Assert.AreEqual(port, addr.Port);
        }

        [Test]
        public void it_can_compare_with_other_addresses()
        {
            // Arrange
            var a = new ServerAddress("host1");
            var b = new ServerAddress("host1");
            var c = new ServerAddress("host2");
            var n = (ServerAddress)null;

            // Act and Assert
            Assert.IsTrue(Object.Equals(a, b));
            Assert.IsFalse(Object.Equals(a, c));
            Assert.IsFalse(a.Equals(n));
            Assert.IsFalse(a.Equals(null));

            Assert.IsTrue(a == b);
            Assert.IsFalse(a == c);
            Assert.IsFalse(a == null);
            Assert.IsFalse(null == a);
            Assert.IsTrue(n == null);
            Assert.IsTrue(null == n);

            Assert.IsFalse(a != b);
            Assert.IsTrue(a != c);
            Assert.IsTrue(a != null);
            Assert.IsTrue(null != a);
            Assert.IsFalse(n != null);
            Assert.IsFalse(null != n);
        }
    }
}