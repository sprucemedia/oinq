using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;

namespace Oinq.Core.Tests
{
    [TestFixture]
    public class When_creating_a_new_Query_instance
    {
        // Fake type
        private class C
        {
        }

        private IQueryProvider _provider;

        [SetUp]
        public void Setup()
        {
            _provider = MockRepository.GenerateStub<IQueryProvider>();
        }

        [Test]
        public void it_accepts_a_single_IQueryProvider()
        {
            // Arrange
            IQueryable query;

            // Act
            TestDelegate a = () => query = (IQueryable)new Query<C>(_provider);

            // Assert
            Assert.DoesNotThrow(a);
        }

        [Test]
        public void it_accepts_an_IQueryProvider_and_an_expression()
        {
            // Arrange
            IQueryable query;
            var queryable = new List<C>().AsQueryable<C>();

            // Act
            TestDelegate a = () => query = (IQueryable)new Query<C>(_provider, queryable.Expression);

            // Assert
            Assert.DoesNotThrow(a);
        }

        [Test]
        public void it_implements_IQueryable_properties_correctly_when_passed_a_provider()
        {
            // Act
            var query = (IQueryable)new Query<C>(_provider);

            // Assert
            Assert.AreSame(typeof(C), query.ElementType);
            Assert.AreSame(_provider, query.Provider);
        }

        [Test]
        public void it_implements_IQueryable_properties_correctly_when_passed_a_provider_and_expression()
        {
            // Arrange
            var queryable = new List<C>().AsQueryable<C>();

            // Act
            var query = (IQueryable)new Query<C>(_provider, queryable.Expression);

            // Assert
            Assert.AreSame(typeof(C), query.ElementType);
            Assert.AreSame(_provider, query.Provider);
            Assert.AreSame(queryable.Expression, query.Expression);
        }

        [Test]
        public void it_throws_an_exception_when_the_single_provider_is_null()
        {
            // Arrange
            IQueryable query;

            // Act
            TestDelegate a = () => query = (IQueryable)new Query<C>(null);

            // Assert
            Assert.Throws<ArgumentNullException>(a);
        }

        [Test]
        public void it_throws_an_exception_when_the_provider_is_null()
        {
            // Arrange
            IQueryable query;
            var queryable = new List<C>().AsQueryable<C>();

            // Act
            TestDelegate a = () => query = (IQueryable)new Query<C>(null, queryable.Expression);

            // Assert
            Assert.Throws<ArgumentNullException>(a);
        }

        [Test]
        public void it_throws_an_exception_when_the_expression_is_null()
        {
            // Arrange
            IQueryable query;

            // Act
            TestDelegate a = () => query = (IQueryable)new Query<C>(_provider, null);

            // Assert
            Assert.Throws<ArgumentNullException>(a);
        }

        [Test]
        public void it_throws_an_exception_when_the_expression_cannot_be_assigned_to_IQueryable()
        {
            // Arrange
            IQueryable query;
            var notQueryable = Expression.Constant(new C());

            // Act
            TestDelegate a = () => query = (IQueryable)new Query<C>(_provider, notQueryable);

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(a);
        }
    }
}
