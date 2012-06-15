using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;

namespace Oinq.Core.Tests
{
    [TestFixture]
    public class QueryProviderTestBase
    {
        protected const String SOURCE_NAME = "FakeData";
        protected const String PATH_NAME = "FakeData";
        protected IDataFile Source;

        [SetUp]
        public void Setup()
        {
            Source = MockRepository.GenerateStub<IDataFile>();
            Source.Stub(s => s.Name).Return(SOURCE_NAME);
            Source.Stub(s => s.AbsolutePath).Return(PATH_NAME);
        }
    }

    public class When_creating_a_query_provider : QueryProviderTestBase
    {
        [Test]
        public void it_accepts_an_ISource()
        {
            // Act
            TestDelegate a = () => new QueryProvider(Source);

            // Assert
            Assert.DoesNotThrow(a);
        }

        [Test]
        public void it_does_not_accept_a_null_ISource()
        {
            // Arrange
            IDataFile bad = null;

            // Act
            TestDelegate a = () => new QueryProvider(bad);

            // Assert
            Assert.Throws<ArgumentNullException>(a);
        }

        [Test]
        public void it_exposes_the_ISource()
        {
            // Act
            var provider = new QueryProvider(Source);

            // Assert
            Assert.AreSame(Source, provider.Source);
        }
    }

    public class When_executing_a_query : QueryProviderTestBase
    {
        [Test]
        public void it_can_return_IEnumerable()
        {
            // Arrange
            IList<FakeData> raw = new List<FakeData>();
            raw.Add(new FakeData { Dim1 = "UA", Mea1 = 5 });
            raw.Add(new FakeData { Dim1 = "EA", Mea1 = 15 });
            raw.Add(new FakeData { Dim1 = "UA", Mea1 = 10 });
            raw.Add(new FakeData { Dim1 = "US", Mea1 = 20 });

            var provider = new FakeQueryProvider(Source, raw.ToList<FakeData>());
            var query = provider.FakeData.Where(f => f.Dim1 == "Fake");

            // Act
            var results = provider.Execute<IEnumerable<FakeData>>(query.Expression);

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(4, results.Count());
        }
    }
}
