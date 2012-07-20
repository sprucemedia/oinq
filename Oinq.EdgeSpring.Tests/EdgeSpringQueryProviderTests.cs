using System;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using Oinq.EdgeSpring.Web;
using RestSharp;
using Rhino.Mocks;

namespace Oinq.EdgeSpring.Tests
{
    [TestFixture]
    public class When_executing_with_the_query_provider
    {
        private const Int32 RECORD_COUNT = 100;
        private const String TEST_FILE = @"..\..\Web\JsonData\response.txt";
        private IRestResponse<QueryResponse<FakeData>> _response;

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            String results;
            StreamReader testFile = null;
            try
            {
                testFile = new StreamReader(TEST_FILE);
                results = testFile.ReadToEnd();
            }
            finally
            {
                testFile.Close();
            }

            _response = MockRepository.GenerateMock<IRestResponse<QueryResponse<FakeData>>>();
            _response.Stub(s => s.Content).Return(results);
        }

        [Test]
        public void it_is_able_deserialize_an_edgespring_response()
        {
            // Act
            QueryResponse<FakeData> response = JsonConvert.DeserializeObject<QueryResponse<FakeData>>(_response.Content);

            // Assert
            Assert.IsNotNull(response);
        }
    }
}
