using System;
using NUnit.Framework;

namespace Oinq.EdgeSpring.Tests
{
    public class When_creating_an_EdgeMart
    {
        private const String SERVER_NAME = "edgespring.com";
        private const Int32 DEFAULT_PORT = 8000;
        private const String EDGEMART_NAME = "em";

        [Test]
        public void it_can_be_created_with_a_uri()
        {
            // Arrange
            var uri = new Uri(String.Format("http://{0}:{1}/remote?edgemart={2}", SERVER_NAME, DEFAULT_PORT.ToString(), EDGEMART_NAME));

            // Act
            var em = new EdgeMart<Object>(uri);

            // Assert
            Assert.AreEqual(EDGEMART_NAME, em.Name);
            Assert.AreEqual(DEFAULT_PORT, em.Server.Port);
            Assert.AreEqual(SERVER_NAME, em.Server.Host);
        }

        [Test]
        public void it_can_be_created_with_a_connection_string()
        {
            // Arrange
            var connectionString = String.Format("http://{0}:{1}/remote?edgemart={2}", SERVER_NAME, DEFAULT_PORT.ToString(), EDGEMART_NAME);

            // Act
            var em = new EdgeMart<Object>(connectionString);

            // Assert
            Assert.AreEqual(EDGEMART_NAME, em.Name);
            Assert.AreEqual(DEFAULT_PORT, em.Server.Port);
            Assert.AreEqual(SERVER_NAME, em.Server.Host);
        }

        [Test]
        public void it_can_be_created_with_a_url()
        {
            // Arrange
            var esUrl = new Url(String.Format("http://{0}:{1}/remote?edgemart={2}", SERVER_NAME, DEFAULT_PORT.ToString(), EDGEMART_NAME));

            // Act
            var em = new EdgeMart<Object>(esUrl);

            // Assert
            Assert.AreEqual(EDGEMART_NAME, em.Name);
            Assert.AreEqual(DEFAULT_PORT, em.Server.Port);
            Assert.AreEqual(SERVER_NAME, em.Server.Host);
        }

        [Test]
        public void it_requires_a_database_name_in_a_url()
        {
            // Arrange
            var esUrl = new Url(String.Format("http://{0}:{1}/remote", SERVER_NAME, DEFAULT_PORT.ToString()));
            EdgeMart em;

            // Act
            TestDelegate a = () => em = new EdgeMart<Object>(esUrl);

            // Assert
            Assert.Throws<ArgumentException>(a);
        }

        [Test]
        public void it_can_return_the_server_url()
        {
            // Arrange
            var esUrl = new Url(String.Format("http://{0}:{1}/remote?edgemart={2}", SERVER_NAME, DEFAULT_PORT.ToString(), EDGEMART_NAME));

            // Act
            var em = new EdgeMart<Object>(esUrl);

            // Assert
            Assert.AreEqual(String.Format("http://{0}:{1}/remote", SERVER_NAME, DEFAULT_PORT.ToString()), em.AbsoluteUri.ToString());
            
        }

        [Test]
        public void it_can_handle_a_path_to_the_edgemart()
        {
            // Arrange
            var edgeMartPath = "folder/mart";
            var esUrl = new Url(String.Format("http://{0}:{1}/remote?edgemart={2}", SERVER_NAME, DEFAULT_PORT.ToString(), edgeMartPath));

            // Act
            var em = new EdgeMart<Object>(esUrl);

            // Assert
            Assert.AreEqual(edgeMartPath, em.AbsolutePath);
        }
    }
}