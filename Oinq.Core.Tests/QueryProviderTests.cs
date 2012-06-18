using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        public virtual void Setup()
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

    public class When_creating_a_query : QueryProviderTestBase
    {
        private Expression _expression;
        private QueryProvider _provider;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _expression = Source.AsQueryable<FakeData>().Expression;
            _provider = new QueryProvider(Source);
        }

        [Test]
        public void it_can_use_a_generic_method()
        {
            // Act
            var query = _provider.CreateQuery<FakeData>(_expression);

            // Assert
            Assert.AreSame(typeof(FakeData), query.ElementType);
            Assert.AreSame(_provider, query.Provider);
            Assert.AreSame(_expression, query.Expression);
        }

        [Test]
        public void it_can_use_a_nongeneric_method()
        {
            // Act
            var query = _provider.CreateQuery(_expression);

            // Assert
            Assert.AreSame(typeof(FakeData), query.ElementType);
            Assert.AreSame(_provider, query.Provider);
            Assert.AreSame(_expression, query.Expression);
        }
    }
}
