using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Oinq.EdgeSpring.Tests.Web
{
    [TestFixture]
    public class When_query_result_records_are_deserialized
    {
        private String _records;
        private const Int32 _recordCount = 100;
        private const String TEST_FILE = @"..\..\Web\JsonData\response.txt";

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            String results = null;
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

            JObject fullResults = JObject.Parse(results);
            _records = fullResults["results"]["records"].ToString();
        }

        [Test]
        public void it_can_deserialize_without_error()
        {
            // Act
            TestDelegate a = () => JsonConvert.DeserializeObject(_records);

            // Assert
            Assert.DoesNotThrow(a);
        }

        [Test]
        public void it_can_deserialize_into_a_collection_of_strings()
        {
            // Act
            IList<Object> records = JsonConvert.DeserializeObject<List<Object>>(_records);

            // Assert
            Assert.IsNotNull(records);
        }

        [Test]
        public void it_can_deserialize_each_record()
        {
            // Act
            IList<Object> records = JsonConvert.DeserializeObject<List<Object>>(_records);

            // Assert
            Assert.AreEqual(_recordCount, records.Count);
        }
    }
}
