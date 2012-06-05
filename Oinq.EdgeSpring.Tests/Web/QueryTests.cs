using System;
using Newtonsoft.Json;
using NUnit.Framework;
using Oinq.EdgeSpring.Web;

namespace Oinq.EdgeSpring.Tests.Web
{
    [TestFixture]
    public class When_a_query_is_converted_to_json
    {
        private Query _query;
        private const String QUERY_TEXT = "Fake Query Text";

        [SetUp]
        public void Setup()
        {
            _query = new Query(QUERY_TEXT);
        }

        [Test]
        public void it_produces_properly_formed_json()
        {
            // Act
            String json = JsonConvert.SerializeObject(_query);
 
            // Assert
            Assert.AreEqual(String.Format("{{\"action\":\"query\",\"query\":\"{0}\",\"otherscope\":{{}}}}", QUERY_TEXT), json);
        }
    }
}
