using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Oinq.Core.Tests
{
    [TestFixture]
    public class When_translating_a_query_with_the_translator
    {
        private IDataFile _source;
        private IQueryable<FakeData> _query;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _source = new FakeDataSource();
            _query = from c in _source.AsQueryable<FakeData>()
                     where c.Mea1 == 5
                     select c;
        }

        [Test]
        public void it_captures_the_source()
        {
            // Act
            var translatedQuery = QueryTranslator.Translate(_query);

            // Assert
            Assert.AreSame(_source, translatedQuery.Source);
        }

        [Test]
        public void it_captures_the_source_type()
        {
            // Act
            var translatedQuery = QueryTranslator.Translate(_query);

            // Assert
            Assert.AreSame(typeof(FakeData), translatedQuery.SourceType);
        }

        [Test]
        public void it_returns_a_select_query()
        {
            // Act
            var translatedQuery = QueryTranslator.Translate(_query);

            // Assert
            Assert.IsInstanceOf<SelectQuery>(translatedQuery);
        }
    }
}
