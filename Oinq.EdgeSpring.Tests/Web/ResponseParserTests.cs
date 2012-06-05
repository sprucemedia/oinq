using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using Oinq.EdgeSpring.Web;

namespace Oinq.EdgeSpring.Tests.Web
{
    public class When_parsing_a_valid_query_response
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
        public void it_can_create_a_query_response_result()
        {
            // Act
            var qrr = ResponseParser.BuildQueryResponseResult(_results);

            // Assert
            Assert.IsNotNull(qrr);
            Assert.IsInstanceOf<QueryResponseResult>(qrr);
        }

        [Test]
        public void it_can_create_a_list_of_record_objects()
        {
            // Act
            var qrr = ResponseParser.BuildQueryResponseResult(_results);

            // Assert
            Assert.IsNotNull(qrr.Records);
        }

        [Test]
        public void it_can_create_a_list_of_record_objects_of_correct_count()
        {
            // Act
            var qrr = ResponseParser.BuildQueryResponseResult(_results);

            // Assert
            Assert.IsNotNull(qrr.Records);
            Assert.AreEqual(_recordCount, qrr.Records.Count);
        }
    }
}
