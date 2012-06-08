using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;

namespace Oinq.Core.Tests
{
    [TestFixture]
    public class When_building_query_command_text
    {
        private String SOURCE_NAME = "FakeData";
        private const String PATH_NAME = "FakeData";
        private ISource _source;
        private Query<FakeData> _fakeData;

        [SetUp]
        public void Setup()
        {
            _source = MockRepository.GenerateStub<ISource>();
            _source.Stub(s => s.Name).Return(SOURCE_NAME);
            _source.Stub(s => s.Path).Return(PATH_NAME);
            _fakeData = new Query<FakeData>(new QueryProvider(_source));
        }

        [Test]
        public void it_can_filter_rows()
        {
            // Arrange
            IQueryable<FakeData> query = _fakeData.Where(f => f.Dim1 == "Fake");

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = FILTER t0 BY (Dim1 == 'Fake'); t2 = FOREACH t1 GENERATE Dim1 AS Dim1, Mea1 AS Mea1; ", queryText);
        }

        [Test]
        public void it_can_filter_rows_by_multiple_filters()
        {
            // Arrange
            IQueryable<FakeData> query = _fakeData.Where(f => f.Dim1 == "Fake" && f.Mea1 == 5);

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = FILTER t0 BY ((Dim1 == 'Fake') AND (Mea1 == 5)); t2 = FOREACH t1 GENERATE Dim1 AS Dim1, Mea1 AS Mea1; ", queryText);
        }

        [Test]
        public void it_can_filter_rows_using_a_variable_reference()
        {
            // Arrange
            String value = "Fake";
            IQueryable<FakeData> query = _fakeData.Where(f => f.Dim1 == value);

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = FILTER t0 BY (Dim1 == 'Fake'); t2 = FOREACH t1 GENERATE Dim1 AS Dim1, Mea1 AS Mea1; ", queryText);
        }

        [Test]
        public void it_can_filter_rows_and_select_columns()
        {
            // Arrange
            var query = _fakeData.Where(f => f.Dim1 == "Fake").Select(f => new { Measure1 = f.Mea1 });

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = FILTER t0 BY (Dim1 == 'Fake'); t2 = FOREACH t1 GENERATE Mea1 AS Mea1; ", queryText);
        }

        [Test]
        public void it_can_filter_rows_and_select_columns_reversed()
        {
            // Arrange
            var query = _fakeData.Select(f => new { Measure1 = f.Mea1 }).Where(f => f.Measure1 == 5);

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = FOREACH t0 GENERATE Mea1 AS Mea1; t2 = FILTER t1 BY (Mea1 == 5); t3 = FOREACH t2 GENERATE Mea1 AS Mea1; ", queryText);
        }

        [Test]
        public void it_can_select_fields()
        {
            // Arrange
            var query = _fakeData.Select(f => new { Measure1 = f.Mea1 });

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = FOREACH t0 GENERATE Mea1 AS Mea1; ", queryText);
        }

        [Test]
        public void it_can_select_multiple_fields()
        {
            // Arrange
            var query = _fakeData.Select(f => new { Measure1 = f.Mea1, Dimension1 = f.Dim1 });

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = FOREACH t0 GENERATE Dim1 AS Dim1, Mea1 AS Mea1; ", queryText);
        }

        [Test]
        public void it_can_group_data()
        {
            // Arrange
            var query = _fakeData.GroupBy(f => f.Dim1);

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = GROUP t0 BY Dim1; t2 = FOREACH t1 GENERATE Dim1 AS Dim1; ", queryText);
        }

        [Test]
        public void it_can_group_data_after_filtering()
        {
            // Arrange
            var query = _fakeData.Where(f => f.Mea1 > 5).GroupBy(f => f.Dim1);

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = FILTER t0 BY (Mea1 > 5); t2 = FOREACH t1 GENERATE Dim1 AS Dim1; t3 = GROUP t2 BY Dim1; t4 = FOREACH t3 GENERATE Dim1 AS Dim1; ", queryText);
        }

        [Test]
        public void it_can_group_data_with_an_aggregate()
        {
            // Arrange
            var query = _fakeData.GroupBy(f => f.Dim1, f => f.Mea1, (dimension, measure) => new { Dimension = dimension, Total = measure.Sum() });

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = GROUP t0 BY Dim1; t2 = FOREACH t1 GENERATE Dim1 AS Dim1, sum(Mea1) AS c0; ", queryText);
        }

        [Test]
        public void it_can_filter_grouped_data()
        {
            // Arrange
            var query = _fakeData.GroupBy(f => f.Dim1, f => f.Mea1, (dimension, measure) => new { Dimension = dimension, Total = measure.Sum() })
                .Where(g => g.Total > 5);

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = GROUP t0 BY Dim1; t2 = FOREACH t1 GENERATE Dim1 AS Dim1, sum(Mea1) AS c0; t3 = FILTER t2 BY (c0 > 5); t4 = FOREACH t3 GENERATE Dim1 AS Dim1, c0 AS c0; ", queryText);
        }
    }

    [TestFixture]
    public class When_using_comparison_operators
    {
        private String SOURCE_NAME = "FakeData";
        private const String PATH_NAME = "FakeData";
        private ISource _source;
        private Query<FakeData> _fakeData;

        [SetUp]
        public void Setup()
        {
            _source = MockRepository.GenerateStub<ISource>();
            _source.Stub(s => s.Name).Return(SOURCE_NAME);
            _source.Stub(s => s.Path).Return(PATH_NAME);
            _fakeData = new Query<FakeData>(new QueryProvider(_source));
        }

        [Test]
        public void it_can_implement_equals()
        {
            // Arrange
            IQueryable<FakeData> query = _fakeData.Where(f => f.Mea1 == 5);

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);
            
            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY (Mea1 == 5);"));
        }

        [Test]
        public void it_can_implement_not_equals()
        {
            // Arrange
            IQueryable<FakeData> query = _fakeData.Where(f => f.Mea1 != 5);

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY (Mea1 != 5);"));
        }

        [Test]
        public void it_can_implement_less_than()
        {
            // Arrange
            IQueryable<FakeData> query = _fakeData.Where(f => f.Mea1 < 5);

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY (Mea1 < 5);"));
        }

        [Test]
        public void it_can_implement_greater_than()
        {
            // Arrange
            IQueryable<FakeData> query = _fakeData.Where(f => f.Mea1 > 5);

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY (Mea1 > 5);"));
        }

        [Test]
        public void it_can_implement_less_than_or_equal_to()
        {
            // Arrange
            IQueryable<FakeData> query = _fakeData.Where(f => f.Mea1 <= 5);

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY (Mea1 <= 5);"));
        }

        [Test]
        public void it_can_implement_greater_than_or_equal_to()
        {
            // Arrange
            IQueryable<FakeData> query = _fakeData.Where(f => f.Mea1 >= 5);

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY (Mea1 >= 5);"));
        }
    }

    [TestFixture]
    public class When_using_arithmetic_operators
    {
        private String SOURCE_NAME = "FakeData";
        private const String PATH_NAME = "FakeData";
        private ISource _source;
        private Query<FakeData> _fakeData;

        [SetUp]
        public void Setup()
        {
            _source = MockRepository.GenerateStub<ISource>();
            _source.Stub(s => s.Name).Return(SOURCE_NAME);
            _source.Stub(s => s.Path).Return(PATH_NAME);
            _fakeData = new Query<FakeData>(new QueryProvider(_source));
        }

        [Test]
        public void it_can_implement_addition()
        {
            // Arrange
            var query = _fakeData.Where(f => f.Mea1 + f.Mea1 > 5);

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY ((Mea1 + Mea1) > 5);"));
        }

        [Test]
        public void it_can_implement_subtraction()
        {
            // Arrange
            var query = _fakeData.Where(f => f.Mea1 - f.Mea1 > 5);

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY ((Mea1 - Mea1) > 5);"));
        }
        
        [Test]
        public void it_can_implement_multiplication()
        {
            // Arrange
            var query = _fakeData.Where(f => f.Mea1 * f.Mea1 > 5);

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY ((Mea1 * Mea1) > 5);"));
        }

        [Test]
        public void it_can_implement_division()
        {
            // Arrange
            var query = _fakeData.Where(f => f.Mea1 / f.Mea1 > 5);

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY ((Mea1 / Mea1) > 5);"));
        }

        [Test]
        public void it_can_implement_modulo()
        {
            // Arrange
            var query = _fakeData.Where(f => f.Mea1 % f.Mea1 > 5);

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY ((Mea1 % Mea1) > 5);"));
        }
    }

    [TestFixture]
    public class When_using_boolean_operators
    {
        private String SOURCE_NAME = "FakeData";
        private const String PATH_NAME = "FakeData";
        private ISource _source;
        private Query<FakeData> _fakeData;

        [SetUp]
        public void Setup()
        {
            _source = MockRepository.GenerateStub<ISource>();
            _source.Stub(s => s.Name).Return(SOURCE_NAME);
            _source.Stub(s => s.Path).Return(PATH_NAME);
            _fakeData = new Query<FakeData>(new QueryProvider(_source));
        }

        [Test]
        public void it_can_implement_and()
        {
            // Arrange
            var query = _fakeData.Where(f => f.Mea1 == 5 & f.Dim1 == "Fake");

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY ((Mea1 == 5) AND (Dim1 == 'Fake'));"));
        }

        [Test]
        public void it_can_implement_andalso()
        {
            // Arrange
            var query = _fakeData.Where(f => f.Mea1 == 5 && f.Dim1 == "Fake");

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY ((Mea1 == 5) AND (Dim1 == 'Fake'));"));
        }

        [Test]
        public void it_can_implement_or()
        {
            // Arrange
            var query = _fakeData.Where(f => f.Mea1 == 5 | f.Dim1 == "Fake");

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY ((Mea1 == 5) OR (Dim1 == 'Fake'));"));
        }

        [Test]
        public void it_can_implement_orelse()
        {
            // Arrange
            var query = _fakeData.Where(f => f.Mea1 == 5 || f.Dim1 == "Fake");

            // Act
            var queryText = ((IQueryText)query.Provider).GetQueryText(query.Expression);

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY ((Mea1 == 5) OR (Dim1 == 'Fake'));"));
        }
    }
}