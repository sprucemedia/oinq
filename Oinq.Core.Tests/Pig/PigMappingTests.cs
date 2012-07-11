using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;

namespace Oinq.Tests.Pig
{
    [TestFixture]
    public class When_building_query_command_text_with_attributed_fields
    {
        private String SOURCE_NAME = "FakeData";
        private const String PATH_NAME = "FakeData";
        private IDataFile _source;

        [SetUp]
        public void Setup()
        {
            _source = MockRepository.GenerateStub<IDataFile>();
            _source.Stub(s => s.Name).Return(SOURCE_NAME);
            _source.Stub(s => s.AbsolutePath).Return(PATH_NAME);
        }

        [Test]
        public void it_can_filter_rows()
        {
            // Arrange
            IQueryable<AttributedFakeData> query = _source.AsQueryable<AttributedFakeData>().Where(f => f.Dim1 == "Fake");

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = filter t0 by (dimension == 'Fake'); t2 = foreach t1 generate dimension as Dim1, measure as Mea1; dump t2; ", queryText);
        }
    }
}
