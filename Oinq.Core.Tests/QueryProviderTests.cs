using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Mocks;
using NUnit.Framework;
using System.Collections;

namespace Oinq.Core.Tests
{
    [TestFixture]
    public class When_executing_a_query
    {
        private String SOURCE_NAME = "FakeData";
        private const String PATH_NAME = "FakeData";
        private ISource _source;

        [SetUp]
        public void Setup()
        {
            _source = MockRepository.GenerateStub<ISource>();
            _source.Stub(s => s.Name).Return(SOURCE_NAME);
            _source.Stub(s => s.Path).Return(PATH_NAME);

        }

        [Test]
        public void it_can_return_IEnumerable()
        {
            // Arrange
            IList<FakeData> raw = new List<FakeData>();
            raw.Add(new FakeData { Dim1 = "UA", Mea1 = 5 });
            raw.Add(new FakeData { Dim1 = "EA", Mea1 = 15 });
            raw.Add(new FakeData { Dim1 = "UA", Mea1 = 10 });
            raw.Add(new FakeData { Dim1 = "US", Mea1 = 20 });

            var provider = new FakeQueryProvider(_source, raw.ToList<FakeData>());
            var query = provider.FakeData.Where(f => f.Dim1 == "Fake");

            // Act
            var results = provider.Execute<IEnumerable<FakeData>>(query.Expression);

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(4, results.Count());
        }
    }
}
