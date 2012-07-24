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
    public class When_serializing_a_query
    {
        private const String _commandText = "Fake Query Text";
        private Query _query;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _query = new Query(_commandText);
        }

        [Test]
        public void it_serializes_to_the_correct_format()
        {
            // Arrange
            var serializer = new RestSharp.Serializers.JsonSerializer();

            // Act
            var json = serializer.Serialize(_query);

            // Assert
            Assert.AreEqual(String.Format("{{\"action\":\"query\",\"otherscope\":{{}},\"query\":\"{0}\"}}", _commandText), json);

        }
    }

    [TestFixture]
    public class When_deserializing_a_query_response
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

        [Test]
        public void it_is_able_deserialize_an_edgespring_response_results()
        {
            // Act
            QueryResponse<FakeData> response = JsonConvert.DeserializeObject<QueryResponse<FakeData>>(_response.Content);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.results);
        }

        [Test]
        public void it_is_able_deserialize_an_edgespring_response_results_records()
        {
            // Act
            QueryResponse<FakeData> response = JsonConvert.DeserializeObject<QueryResponse<FakeData>>(_response.Content);

            // Assert
            Assert.IsNotNull(response.results.records);
            Assert.AreEqual(RECORD_COUNT, response.results.records.Count);
        }

        [Test]
        public void it_is_able_deserialize_an_edgespring_response_results_record_values()
        {
            // Act
            QueryResponse<FakeData> response = JsonConvert.DeserializeObject<QueryResponse<FakeData>>(_response.Content);

            // Assert
            Assert.IsNotNull(response.results.records);
            Assert.IsNotNull(response.results.records[0].carrier);
            Assert.AreNotEqual(default(Int32), response.results.records[0].miles);
        }
    }

    [TestFixture]
    public class When_deserializing_an_update_response
    {
        private const String TEST_FILE = @"..\..\Web\JsonData\updateresponse.txt";
        private IRestResponse<UpdateResponse> _response;

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

            _response = MockRepository.GenerateMock<IRestResponse<UpdateResponse>>();
            _response.Stub(s => s.Content).Return(results);
        }

        [Test]
        public void it_is_able_deserialize_an_edgespring_response()
        {
            // Act
            UpdateResponse response = JsonConvert.DeserializeObject<UpdateResponse>(_response.Content);

            // Assert
            Assert.IsNotNull(response);
        }
    }
}
