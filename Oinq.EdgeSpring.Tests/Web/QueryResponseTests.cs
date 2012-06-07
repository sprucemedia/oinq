using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Oinq.EdgeSpring.Web;

namespace Oinq.EdgeSpring.Tests.Web
{
    public class When_creating_a_query_response
    {
        private String _results;
        private const Int32 _recordCount = 100;
        private const String TEST_FILE = @"..\..\Web\JsonData\response.txt";

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            StreamReader testFile = null;
            try
            {
                testFile = new StreamReader(TEST_FILE);
                _results = testFile.ReadToEnd();
            }
            finally
            {
                testFile.Close();
            }
        }

        [Test]
        public void it_can_create_a_query_response_from_a_response()
        {
            // Act
            var qr = new QueryResponse(_results);

            // Assert
            Assert.IsNotNull(qr);
        }

        [Test]
        public void it_can_create_a_query_response_result()
        {
            // Act
            var qr = new QueryResponse(_results);

            // Assert
            Assert.IsNotNull(qr.Result);
            Assert.IsInstanceOf<QueryResponseResult>(qr.Result);
        }

        [Test]
        public void it_can_create_a_list_of_record_objects()
        {
            // Act
            var qr = new QueryResponse(_results);

            // Assert
            Assert.IsNotNull(qr.Result.Records);
        }

        [Test]
        public void it_can_create_a_list_of_record_objects_of_correct_count()
        {
            // Act
            var qr = new QueryResponse(_results);

            // Assert
            Assert.IsNotNull(qr.Result.Records);
            Assert.AreEqual(_recordCount, qr.Result.Records.Count());
        }

        [Test]
        public void it_can_return_a_null_result_when_response_is_not_json()
        {
            // Arrange
            String badResult = "Not Json";

            // Act
            var qr = new QueryResponse(badResult);

            // Assert
            Assert.IsNull(qr.Result);
        }

        [Test]
        public void it_can_return_an_empty_record_list_when_there_are_no_results()
        {
            // Arrange
            String emptyResult = "{\"otherscope\": {}, \"responseId\": \"\", \"results\": {\"records\": []}}";

            // Act
            var qr = new QueryResponse(emptyResult);

            // Assert
            Assert.IsNotNull(qr);
            Assert.IsNotNull(qr.Result.Records);
            Assert.IsEmpty(qr.Result.Records);
        }
    }
}
