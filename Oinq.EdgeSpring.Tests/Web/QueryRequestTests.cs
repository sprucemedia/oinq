using System;
using System.IO;
using System.Net;
using NUnit.Framework;
using Oinq.EdgeSpring.Web;

namespace Oinq.EdgeSpring.Tests.Web
{
    [TestFixture]
    public class When_sending_a_query_request
    {
        private readonly Uri _uri = new Uri("test://mock.com");
        private const String COMMAND_TEXT = "Fake Query Text";
        private const String EXPECTED_REQUEST = "{\"action\":\"query\",\"query\":\"Fake Query Text\",\"otherscope\":{}}";
        private const String _response = "Fake Response Text";
        private WebRequestMock _request;

        private Query _query;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            WebRequest.RegisterPrefix("test", new WebRequestCreateMock());
            _query = new Query(COMMAND_TEXT);
            _request = WebRequestCreateMock.CreateWebRequestMock(_response);
        }

        [SetUp]
        public void Setup()
        {
            // Act
            QueryRequest.SendQuery(_uri, _query);
        }

        [Test]
        public void it_sends_a_properly_formatted_header()
        {
            // Assert
            Assert.AreEqual(EXPECTED_REQUEST, _request.ContentAsString());
        }

        [Test]
        public void it_sends_to_the_correct_method()
        {
            // Assert
            Assert.AreEqual("POST", _request.Method);
        }

        [Test]
        public void it_sends_the_correct_content_type()
        {
            // Assert
            Assert.AreEqual("application/json", _request.ContentType);
        }
    }
}
