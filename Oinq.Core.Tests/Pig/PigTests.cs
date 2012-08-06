using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;

namespace Oinq.Tests
{
    [TestFixture]
    public class When_building_query_command_text
    {
        private String SOURCE_NAME = "FakeData";
        private const String PATH_NAME = "FakeData";
        private IDataFile _source;
        private Query<FakeData> _fakeData;

        [SetUp]
        public void Setup()
        {
            _source = MockRepository.GenerateStub<IDataFile>();
            _source.Stub(s => s.Name).Return(SOURCE_NAME);
            _source.Stub(s => s.AbsolutePath).Return(PATH_NAME);
            _fakeData = new Query<FakeData>(new QueryProvider(_source));
        }

        [Test]
        public void it_can_filter_rows()
        {
            // Arrange
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => f.Dim1 == "Fake");

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = filter t0 by (Dim1 == 'Fake'); t2 = foreach t1 generate Dim1 as Dim1, Mea1 as Mea1; t3 = limit t2 1000; ", queryText);
        }

        [Test]
        public void it_can_filter_rows_by_multiple_filters()
        {
            // Arrange
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => f.Dim1 == "Fake" && f.Mea1 == 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = filter t0 by ((Dim1 == 'Fake') and (Mea1 == 5)); t2 = foreach t1 generate Dim1 as Dim1, Mea1 as Mea1; t3 = limit t2 1000; ", queryText);
        }

        [Test]
        public void it_can_filter_rows_using_a_variable_reference()
        {
            // Arrange
            String value = "Fake";
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => f.Dim1 == value);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = filter t0 by (Dim1 == 'Fake'); t2 = foreach t1 generate Dim1 as Dim1, Mea1 as Mea1; t3 = limit t2 1000; ", queryText);
        }

        [Test]
        public void it_can_filter_using_contains_on_a_list()
        {
            var values = new List<String> { "a", "b"};
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => values.Contains(f.Dim1));

            var queryText = ((IPigQueryable)query).GetPigQuery();

            Assert.AreEqual("t0 = load 'FakeData'; t1 = filter t0 by (Dim1 in ['a', 'b']); t2 = foreach t1 generate Dim1 as Dim1, Mea1 as Mea1; t3 = limit t2 1000; ", queryText);
        }

        [Test]
        public void it_can_filter_rows_and_select_columns()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Dim1 == "Fake").Select(f => new { Measure1 = f.Mea1 });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = filter t0 by (Dim1 == 'Fake'); t2 = foreach t1 generate Mea1 as Measure1; t3 = limit t2 1000; ", queryText);
        }

        [Test]
        public void it_can_filter_rows_and_select_columns_reversed()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Select(f => new { Measure1 = f.Mea1 }).Where(f => f.Measure1 == 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = filter t0 by (Mea1 == 5); t2 = foreach t1 generate Mea1 as Measure1; t3 = limit t2 1000; ", queryText);
        }

        [Test]
        public void it_can_select_fields()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Select(f => new { Measure1 = f.Mea1 });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = foreach t0 generate Mea1 as Measure1; t2 = limit t1 1000; ", queryText);
        }

        [Test]
        public void it_can_select_multiple_fields()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Select(f => new { Measure1 = f.Mea1, Dimension1 = f.Dim1 });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = foreach t0 generate Mea1 as Measure1, Dim1 as Dimension1; t2 = limit t1 1000; ", queryText);
        }

        [Test]
        public void it_can_order_by_a_field()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().OrderBy(f => f.Mea1);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = foreach t0 generate Dim1 as Dim1, Mea1 as Mea1; t2 = order t1 by (Mea1 asc); t3 = limit t2 1000; ", queryText);
        }

        [Test]
        public void it_can_order_by_a_field_in_descending_order()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().OrderByDescending(f => f.Mea1);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = foreach t0 generate Dim1 as Dim1, Mea1 as Mea1; t2 = order t1 by (Mea1 desc); t3 = limit t2 1000; ", queryText);
        }

        [Test]
        public void it_can_order_by_multiple_fields()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().OrderBy(f => f.Mea1).ThenBy(f => f.Dim1);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = foreach t0 generate Dim1 as Dim1, Mea1 as Mea1; t2 = order t1 by (Mea1 asc, Dim1 asc); t3 = limit t2 1000; ", queryText);
        }

        [Test]
        public void it_can_order_by_multiple_fields_in_opposite_order()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().OrderBy(f => f.Mea1).ThenByDescending(f => f.Dim1);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = foreach t0 generate Dim1 as Dim1, Mea1 as Mea1; t2 = order t1 by (Mea1 asc, Dim1 desc); t3 = limit t2 1000; ", queryText);
        }

        [Test]
        public void it_can_order_by_a_dynamically_constructed_expression()
        {
            // Arrange
            var param = Expression.Parameter(typeof(FakeData), "fake");
            var sortExpression = Expression.Lambda<Func<FakeData, Object>>(Expression.Convert(Expression.Property(param, "Mea1"), typeof(Object)), param);
            var query = _source.AsQueryable<FakeData>().OrderBy(sortExpression);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = foreach t0 generate Dim1 as Dim1, Mea1 as Mea1; t2 = order t1 by (Mea1 asc); t3 = limit t2 1000; ", queryText);
        }

        [Test]
        public void it_can_order_by_a_dynamically_constructed_expression_with_a_column_alias()
        {
            // Arrange
            var param = Expression.Parameter(typeof(FakeProjection), "fake");
            var sortExpression = Expression.Lambda<Func<FakeProjection, Object>>(Expression.Convert(Expression.Property(param, "Measure"), typeof(Object)), param);
            var query = _source.AsQueryable<FakeData>().Select(f => new FakeProjection { Key = f.Dim1, Measure = f.Mea1 }).OrderBy(sortExpression);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = foreach t0 generate Dim1 as Key, Mea1 as Measure; t2 = order t1 by (Measure asc); t3 = limit t2 1000; ", queryText);
        }

        [Test]
        public void it_can_order_by_a_dynamically_constructed_expression_with_an_aggregate()
        {
            // Arrange
            var param = Expression.Parameter(typeof(FakeProjection), "fake");
            var sortExpression = Expression.Lambda<Func<FakeProjection, Object>>(Expression.Convert(Expression.Property(param, "Measure"), typeof(Object)), param);
            var query = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f, (dim, f) => new FakeProjection { Key = dim, Measure = f.Sum(x => x.Mea1) })
                .OrderByDescending(sortExpression);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = group t0 by (Dim1); t2 = foreach t1 generate Dim1 as Key, sum(Mea1) as Measure; t3 = order t2 by (Measure desc); t4 = limit t3 1000; ", queryText);
        }

        [Test]
        public void it_can_order_by_a_dynamically_constructed_expression_with_a_custom_extension()
        {
            // Arrange
            var param = Expression.Parameter(typeof(FakeProjection), "fake");
            var sortExpression = Expression.Lambda<Func<FakeProjection, Object>>(Expression.Convert(Expression.Property(param, "Measure"), typeof(Object)), param);
            var query = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f, (dim, f) => new FakeProjection { Key = dim, Measure = f.AggOp() })
                .OrderByDescending(sortExpression);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = group t0 by (Dim1); t2 = foreach t1 generate Dim1 as Key, ((sum(Mea1) + sum(Mea1)) + sum(Mea1)) as Measure; t3 = order t2 by (Measure desc); t4 = limit t3 1000; ", queryText);
        }

        [Test]
        public void it_can_limit_the_number_of_results()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Take(100);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = foreach t0 generate Dim1 as Dim1, Mea1 as Mea1; t2 = limit t1 100; ", queryText);
        }

        [Test]
        public void it_can_limit_the_number_of_results_after_ordering()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().OrderBy(f => f.Mea1).Take(100);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = foreach t0 generate Dim1 as Dim1, Mea1 as Mea1; t2 = order t1 by (Mea1 asc); t3 = limit t2 100; ", queryText);
        }

        [Test]
        public void it_can_group_data()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = group t0 by (Dim1); t2 = foreach t1 generate Dim1 as Dim1; t3 = limit t2 1000; ", queryText);
        }

        [Test]
        public void it_can_group_data_after_filtering()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 > 5).GroupBy(f => f.Dim1);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = filter t0 by (Mea1 > 5); t2 = group t1 by (Dim1); t3 = foreach t2 generate Dim1 as Dim1; t4 = limit t3 1000; ", queryText);
        }

        [Test]
        public void it_can_group_data_with_an_aggregate()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f.Mea1, (dimension, measure) => new { Dimension = dimension, Total = measure.Sum() });
            
            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = group t0 by (Dim1); t2 = foreach t1 generate Dim1 as Dimension, sum(Mea1) as Total; t3 = limit t2 1000; ", queryText);
        }

        [Test]
        public void it_can_group_data_with_an_aggregate_calculation()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f.Mea1, (dimension, measure) => new { Dimension = dimension, Total = measure.Sum() + measure.Sum() });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = group t0 by (Dim1); t2 = foreach t1 generate Dim1 as Dimension, (sum(Mea1) + sum(Mea1)) as Total; t3 = limit t2 1000; ", queryText);
        }

        [Test]
        public void it_can_filter_grouped_data()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f.Mea1, (dimension, measure) => new { Dimension = dimension, Total = measure.Sum() })
                .Where(g => g.Total > 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = group t0 by (Dim1); t2 = filter t1 by (sum(Mea1) > 5); t3 = foreach t2 generate Dim1 as Dimension, sum(Mea1) as Total; t4 = limit t3 1000; ", queryText);
        }

        [Test]
        public void it_can_count_grouped_data()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f.Mea1, (dimension, measure) => new { Dimension = dimension, Total = measure.Count() });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = group t0 by (Dim1); t2 = foreach t1 generate Dim1 as Dimension, count() as Total; t3 = limit t2 1000; ", queryText);
        }

        [Test]
        public void it_can_execute_methods_against_the_projection_locally()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Select(p => new { DimInt = Int32.Parse(p.Dim1) });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = foreach t0 generate Dim1 as DimInt; t2 = limit t1 1000; ", queryText);
        }
    }

    [TestFixture]
    public class When_using_comparison_operators
    {
        private String SOURCE_NAME = "FakeData";
        private const String PATH_NAME = "FakeData";
        private IDataFile _source;
        private Query<FakeData> _fakeData;

        [SetUp]
        public void Setup()
        {
            _source = MockRepository.GenerateStub<IDataFile>();
            _source.Stub(s => s.Name).Return(SOURCE_NAME);
            _source.Stub(s => s.AbsolutePath).Return(PATH_NAME);
            _fakeData = new Query<FakeData>(new QueryProvider(_source));
        }

        [Test]
        public void it_can_implement_equals()
        {
            // Arrange
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 == 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = filter t0 by (Mea1 == 5);"));
        }

        [Test]
        public void it_can_implement_not_equals()
        {
            // Arrange
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 != 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = filter t0 by (Mea1 != 5);"));
        }

        [Test]
        public void it_can_implement_less_than()
        {
            // Arrange
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 < 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = filter t0 by (Mea1 < 5);"));
        }

        [Test]
        public void it_can_implement_greater_than()
        {
            // Arrange
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 > 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = filter t0 by (Mea1 > 5);"));
        }

        [Test]
        public void it_can_implement_less_than_or_equal_to()
        {
            // Arrange
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 <= 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = filter t0 by (Mea1 <= 5);"));
        }

        [Test]
        public void it_can_implement_greater_than_or_equal_to()
        {
            // Arrange
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 >= 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = filter t0 by (Mea1 >= 5);"));
        }

        [Test]
        public void it_can_implement_contains()
        {
            // Arrange
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => f.Dim1.Contains("test"));

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = filter t0 by (Dim1 MATCHES '.*test.*');"));
        }

        [Test]
        public void it_can_implement_startswith()
        {
            // Arrange
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => f.Dim1.StartsWith("test"));

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = filter t0 by (Dim1 MATCHES 'test.*');"));
        }

        [Test]
        public void it_can_implement_endswith()
        {
            // Arrange
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => f.Dim1.EndsWith("test"));

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = filter t0 by (Dim1 MATCHES '.*test');"));
        }

        [Test]
        public void it_can_implement_contains_using_a_variable()
        {
            // Arrange
            String match = "test";
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => f.Dim1.Contains(match));

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = filter t0 by (Dim1 MATCHES '.*test.*');"));
        }
    }

    [TestFixture]
    public class When_using_arithmetic_operators
    {
        private String SOURCE_NAME = "FakeData";
        private const String PATH_NAME = "FakeData";
        private IDataFile _source;
        private Query<FakeData> _fakeData;

        [SetUp]
        public void Setup()
        {
            _source = MockRepository.GenerateStub<IDataFile>();
            _source.Stub(s => s.Name).Return(SOURCE_NAME);
            _source.Stub(s => s.AbsolutePath).Return(PATH_NAME);
            _fakeData = new Query<FakeData>(new QueryProvider(_source));
        }

        [Test]
        public void it_can_implement_addition()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 + f.Mea1 > 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = filter t0 by ((Mea1 + Mea1) > 5);"));
        }

        [Test]
        public void it_can_implement_subtraction()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 - f.Mea1 > 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = filter t0 by ((Mea1 - Mea1) > 5);"));
        }
        [Test]
        public void it_can_implement_multiplication()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 * f.Mea1 > 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = filter t0 by ((Mea1 * Mea1) > 5);"));
        }

        [Test]
        public void it_can_implement_division()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 / f.Mea1 > 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = filter t0 by ((Mea1 / Mea1) > 5);"));
        }

        [Test]
        public void it_can_implement_modulo()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 % f.Mea1 > 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = filter t0 by ((Mea1 % Mea1) > 5);"));
        }
    }

    [TestFixture]
    public class When_using_boolean_operators
    {
        private String SOURCE_NAME = "FakeData";
        private const String PATH_NAME = "FakeData";
        private IDataFile _source;
        private Query<FakeData> _fakeData;

        [SetUp]
        public void Setup()
        {
            _source = MockRepository.GenerateStub<IDataFile>();
            _source.Stub(s => s.Name).Return(SOURCE_NAME);
            _source.Stub(s => s.AbsolutePath).Return(PATH_NAME);
            _fakeData = new Query<FakeData>(new QueryProvider(_source));
        }

        [Test]
        public void it_can_implement_and()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 == 5 & f.Dim1 == "Fake");

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = filter t0 by ((Mea1 == 5) and (Dim1 == 'Fake'));"));
        }

        [Test]
        public void it_can_implement_andalso()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 == 5 && f.Dim1 == "Fake");

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = filter t0 by ((Mea1 == 5) and (Dim1 == 'Fake'));"));
        }

        [Test]
        public void it_can_implement_or()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 == 5 | f.Dim1 == "Fake");

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = filter t0 by ((Mea1 == 5) or (Dim1 == 'Fake'));"));
        }

        [Test]
        public void it_can_implement_orelse()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 == 5 || f.Dim1 == "Fake");

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = filter t0 by ((Mea1 == 5) or (Dim1 == 'Fake'));"));
        }
    }

    [TestFixture]
    public class When_using_custom_extension_methods
    {
        private String SOURCE_NAME = "FakeData";
        private const String PATH_NAME = "FakeData";
        private IDataFile _source;
        private Query<FakeData> _fakeData;

        [SetUp]
        public void Setup()
        {
            _source = MockRepository.GenerateStub<IDataFile>();
            _source.Stub(s => s.Name).Return(SOURCE_NAME);
            _source.Stub(s => s.AbsolutePath).Return(PATH_NAME);
            _fakeData = new Query<FakeData>(new QueryProvider(_source));
        }

        [Test]
        public void it_can_translate_an_aggregate()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f, (dimension, data) => new { Dimension = dimension, Total = data.AddUp() });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = group t0 by (Dim1); t2 = foreach t1 generate Dim1 as Dimension, sum(Mea1) as Total; t3 = limit t2 1000; ", queryText);
        }

        [Test]
        public void it_can_translate_a_complex_aggregate()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f, (dimension, data) => new { Dimension = dimension, Total = data.AggOp() });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = group t0 by (Dim1); t2 = foreach t1 generate Dim1 as Dimension, ((sum(Mea1) + sum(Mea1)) + sum(Mea1)) as Total; t3 = limit t2 1000; ", queryText);
        }

        [Test]
        public void it_can_translate_an_aggregate_with_a_class_level_attribute()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f, (dimension, data) => new { Dimension = dimension, Total = data.MultiplyIt() });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = group t0 by (Dim1); t2 = foreach t1 generate Dim1 as Dimension, (sum(Mea1) * 2) as Total; t3 = limit t2 1000; ", queryText);
        }

        [Test]
        public void it_can_filter_using_a_custom_extension()
        {
            var query = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f, (dimension, data) => new { Dimension = dimension, Total = data.MultiplyIt() }).Where(x => x.Total > 3);

            var queryText = ((IPigQueryable)query).GetPigQuery();

            Assert.AreEqual("t0 = load 'FakeData'; t1 = group t0 by (Dim1); t2 = filter t1 by ((sum(Mea1) * 2) > 3); t3 = foreach t2 generate Dim1 as Dimension, (sum(Mea1) * 2) as Total; t4 = limit t3 1000; ", queryText);
        }
    }

    [TestFixture]
    public class When_using_the_join_method
    {
        private String SOURCE_NAME = "FakeData";
        private const String PATH_NAME = "FakeData";
        private const String EXT_SOURCE_NAME = "FakeDataMeta";
        private const String EXT_PATH_NAME = "FakeDataMeta";
        private IDataFile _source;
        private IDataFile _extendedSource;
        private Query<FakeData> _fakeData;
        private Query<FakeDataMeta> _fakeDataDim;

        [SetUp]
        public void Setup()
        {
            _source = MockRepository.GenerateStub<IDataFile>();
            _source.Stub(s => s.Name).Return(SOURCE_NAME);
            _source.Stub(s => s.AbsolutePath).Return(PATH_NAME);
            _fakeData = new Query<FakeData>(new QueryProvider(_source));

            _extendedSource = MockRepository.GenerateStub<IDataFile>();
            _extendedSource.Stub(s => s.Name).Return(EXT_SOURCE_NAME);
            _extendedSource.Stub(s => s.AbsolutePath).Return(EXT_PATH_NAME);
            _fakeDataDim = new Query<FakeDataMeta>(new QueryProvider(_extendedSource));
        }

        [Test]
        public void it_can_load_the_joined_file()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Join(_extendedSource.AsQueryable<FakeDataMeta>(),
                fd => fd.Dim1, e => e.Dim1, (fd, e) => new { Key = fd.Dim1, Measure = fd.Mea1, Description = e.DimDesc });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t0 = load 'FakeData'; t1 = load 'FakeDataMeta'; "));
        }

        [Test]
        public void it_can_join_the_file()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Join(_extendedSource.AsQueryable<FakeDataMeta>(),
                fd => fd.Dim1, e => e.Dim1, (fd, e) => new { Key = fd.Dim1, Measure = fd.Mea1, Description = e.DimDesc });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = load 'FakeDataMeta'; t2 = group t1 by (DimDesc); t3 = group t0 by Dim1, t2 by Dim1; t4 = foreach t3 generate Dim1 as Key, Mea1 as Measure, DimDesc as Description; t5 = limit t4 1000; ", queryText);
        }

        [Test]
        public void it_can_join_the_file_with_multiple_output_fields()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Join(_extendedSource.AsQueryable<FakeDataMeta>(),
                fd => fd.Dim1, e => e.Dim1, (fd, e) => new { Key = fd.Dim1, Measure = fd.Mea1, Description = e.DimDesc, Description2 = e.DimDesc2 });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = load 'FakeDataMeta'; t2 = group t1 by (DimDesc, DimDesc2); t3 = group t0 by Dim1, t2 by Dim1; t4 = foreach t3 generate Dim1 as Key, Mea1 as Measure, DimDesc as Description, DimDesc2 as Description2; t5 = limit t4 1000; ", queryText);
        }

        [Test]
        public void it_can_join_the_file_with_multiple_output_fields_and_an_aggregate()
        {
            // Arrange
            var fakeData = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f, (dim1, f) => new { Dim1 = dim1, Measure = f.Sum(x => x.Mea1) });
            var fakeDataExt = _extendedSource.AsQueryable<FakeDataMeta>().Where(e => e.Dim1 == "5");
            var joined = fakeData.Join(fakeDataExt, f => f.Dim1, e => e.Dim1, (f, e) => new { Dim = f.Dim1, Description = e.DimDesc, Total = f.Measure });

            // Act
            var queryText = ((IPigQueryable)joined).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = load 'FakeDataMeta'; t2 = filter t1 by (Dim1 == '5'); t3 = group t2 by (DimDesc); t4 = group t0 by Dim1, t3 by Dim1; t5 = foreach t4 generate Dim1 as Dim, DimDesc as Description, sum(Mea1) as Total; t6 = limit t5 1000; ", queryText);
        }

        [Test]
        public void it_can_join_the_file_with_multiple_output_fields_and_an_aggregate_and_a_sort()
        {
            // Arrange
            var fakeData = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f, (dim1, f) => new { Dim1 = dim1, Measure = f.Sum(x => x.Mea1) });
            var fakeDataExt = _extendedSource.AsQueryable<FakeDataMeta>().Where(e => e.Dim1 == "5");
            var joined = fakeData.Join(fakeDataExt, f => f.Dim1, e => e.Dim1, (f, e) => new { Dim = f.Dim1, Description = e.DimDesc, Total = f.Measure })
                .OrderByDescending(p => p.Dim);

            // Act
            var queryText = ((IPigQueryable)joined).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = load 'FakeDataMeta'; t2 = filter t1 by (Dim1 == '5'); t3 = group t2 by (DimDesc); t4 = group t0 by Dim1, t3 by Dim1; t5 = foreach t4 generate Dim1 as Dim, DimDesc as Description, sum(Mea1) as Total; t6 = order t5 by (Dim desc); t7 = limit t6 1000; ", queryText);
        }

        [Test]
        public void it_can_join_the_file_with_multiple_output_fields_and_a_sorted_aggregate()
        {
            // Arrange
            var fakeData = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f, (dim1, f) => new { Dim1 = dim1, Measure = f.Sum(x => x.Mea1) });
            var fakeDataExt = _extendedSource.AsQueryable<FakeDataMeta>().Where(e => e.Dim1 == "5");
            var joined = fakeData.Join(fakeDataExt, f => f.Dim1, e => e.Dim1, (f, e) => new { Dim = f.Dim1, Description = e.DimDesc, Total = f.Measure })
                .OrderByDescending(p => p.Total);

            // Act
            var queryText = ((IPigQueryable)joined).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = load 'FakeDataMeta'; t2 = filter t1 by (Dim1 == '5'); t3 = group t2 by (DimDesc); t4 = group t0 by Dim1, t3 by Dim1; t5 = foreach t4 generate Dim1 as Dim, DimDesc as Description, sum(Mea1) as Total; t6 = order t5 by (Total desc); t7 = limit t6 1000; ", queryText);
        }

        [Test]
        public void it_can_join_the_file_with_a_take()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Join(_extendedSource.AsQueryable<FakeDataMeta>(),
                fd => fd.Dim1, e => e.Dim1, (fd, e) => new { Key = fd.Dim1, Measure = fd.Mea1, Description = e.DimDesc }).Take(10);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = load 'FakeDataMeta'; t2 = group t1 by (DimDesc); t3 = group t0 by Dim1, t2 by Dim1; t4 = foreach t3 generate Dim1 as Key, Mea1 as Measure, DimDesc as Description; t5 = limit t4 10; ", queryText);
        }

        [Test]
        public void it_can_join_the_file_after_a_filter()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 > 5).Join(_extendedSource.AsQueryable<FakeDataMeta>(),
                fd => fd.Dim1, e => e.Dim1, (fd, e) => new { Key = fd.Dim1, Measure = fd.Mea1, Description = e.DimDesc });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = load 'FakeDataMeta'; t2 = filter t0 by (Mea1 > 5); t3 = group t1 by (DimDesc); t4 = group t2 by Dim1, t3 by Dim1; t5 = foreach t4 generate Dim1 as Key, Mea1 as Measure, DimDesc as Description; t6 = limit t5 1000; ", queryText);
        }

        [Test]
        public void it_can_translate_an_extension_in_a_join()
        {
            // Arrange
            var fakeData = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f, (dimension, data) => new { Dim1 = dimension, Total = data.AggOp() });
            var fakeDataExt = _extendedSource.AsQueryable<FakeDataMeta>().Where(e => e.Dim1 == "5");
            var joined = fakeData.Join(fakeDataExt, f => f.Dim1, e => e.Dim1, (f, e) => new { Dim = f.Dim1, Description = e.DimDesc, Total = f.Total });

            // Act
            var queryText = ((IPigQueryable)joined).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = load 'FakeDataMeta'; t2 = filter t1 by (Dim1 == '5'); t3 = group t2 by (DimDesc); t4 = group t0 by Dim1, t3 by Dim1; t5 = foreach t4 generate Dim1 as Dim, DimDesc as Description, ((sum(Mea1) + sum(Mea1)) + sum(Mea1)) as Total; t6 = limit t5 1000; ", queryText);
        }
    }

    [TestFixture]
    public class When_projecting_to_a_strong_type
    {
        private String SOURCE_NAME = "FakeData";
        private const String PATH_NAME = "FakeData";
        private const String EXT_SOURCE_NAME = "FakeDataMeta";
        private const String EXT_PATH_NAME = "FakeDataMeta";
        private IDataFile _source;
        private IDataFile _extendedSource;
        private Query<FakeData> _fakeData;
        private Query<FakeDataMeta> _fakeDataDim;

        [SetUp]
        public void Setup()
        {
            _source = MockRepository.GenerateStub<IDataFile>();
            _source.Stub(s => s.Name).Return(SOURCE_NAME);
            _source.Stub(s => s.AbsolutePath).Return(PATH_NAME);
            _fakeData = new Query<FakeData>(new QueryProvider(_source));

            _extendedSource = MockRepository.GenerateStub<IDataFile>();
            _extendedSource.Stub(s => s.Name).Return(EXT_SOURCE_NAME);
            _extendedSource.Stub(s => s.AbsolutePath).Return(EXT_PATH_NAME);
            _fakeDataDim = new Query<FakeDataMeta>(new QueryProvider(_extendedSource));
        }

        [Test]
        public void it_can_project_to_a_class()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Join(_extendedSource.AsQueryable<FakeDataMeta>(),
                fd => fd.Dim1, e => e.Dim1, (fd, e) => new FakeProjection { Key = fd.Dim1, Measure = fd.Mea1, Description = e.DimDesc });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = load 'FakeDataMeta'; t2 = group t1 by (DimDesc); t3 = group t0 by Dim1, t2 by Dim1; t4 = foreach t3 generate Dim1 as Key, Mea1 as Measure, DimDesc as Description; t5 = limit t4 1000; ", queryText);
        }

        [Test]
        public void it_can_project_to_a_class_with_a_parse()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Join(_extendedSource.AsQueryable<FakeDataMeta>(),
                fd => fd.Dim1, e => e.Dim1, (fd, e) => new FakeProjectionInt { Key = Int32.Parse(fd.Dim1), Measure = fd.Mea1, Description = e.DimDesc });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = load 'FakeDataMeta'; t2 = group t1 by (DimDesc); t3 = group t0 by Dim1, t2 by Dim1; t4 = foreach t3 generate Dim1 as Key, Mea1 as Measure, DimDesc as Description; t5 = limit t4 1000; ", queryText);
        }

        [Test]
        public void it_can_project_to_a_class_with_a_parse_and_aggregate()
        {
            // Arrange
            var fakeData = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f.Mea1, (dimension, data) => new { Dim1 = dimension, Total = data.Sum() });
            var fakeDataExt = _extendedSource.AsQueryable<FakeDataMeta>().Where(e => e.Dim1 == "5");
            var joined = fakeData.Join(fakeDataExt, f => f.Dim1, e => e.Dim1, (f, e) => new FakeProjectionInt { Key = Int32.Parse(f.Dim1), Measure = f.Total, Description = e.DimDesc });

            // Act
            var queryText = ((IPigQueryable)joined).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = load 'FakeDataMeta'; t2 = filter t1 by (Dim1 == '5'); t3 = group t2 by (DimDesc); t4 = group t0 by Dim1, t3 by Dim1; t5 = foreach t4 generate Dim1 as Key, sum(Mea1) as Measure, DimDesc as Description; t6 = limit t5 1000; ", queryText);
        }

        [Test]
        public void it_can_project_to_a_class_with_a_parse_and_aggregate_and_sort()
        {
            // Arrange
            var fakeData = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f.Mea1, (dimension, data) => new { Dim1 = dimension, Total = data.Sum() });
            var fakeDataExt = _extendedSource.AsQueryable<FakeDataMeta>().Where(e => e.Dim1 == "5");
            var joined = fakeData.Join(fakeDataExt, f => f.Dim1, e => e.Dim1, (f, e) => new FakeProjectionInt { Key = Int32.Parse(f.Dim1), Measure = f.Total, Description = e.DimDesc }).OrderByDescending(e => e.Key);

            // Act
            var queryText = ((IPigQueryable)joined).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = load 'FakeDataMeta'; t2 = filter t1 by (Dim1 == '5'); t3 = group t2 by (DimDesc); t4 = group t0 by Dim1, t3 by Dim1; t5 = foreach t4 generate Dim1 as Key, sum(Mea1) as Measure, DimDesc as Description; t6 = order t5 by (Key desc); t7 = limit t6 1000; ", queryText);
        }

        [Test]
        public void it_can_project_to_a_class_with_a_parse_and_sort_and_convert()
        {
            // Arrange
            var fakeData = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f.Mea1, (dimension, data) => new { Dim1 = dimension, Total = Convert.ToInt32(data.Sum() + data.Sum()) });
            var fakeDataExt = _extendedSource.AsQueryable<FakeDataMeta>().Where(e => e.Dim1 == "5");
            var joined = fakeData.Join(fakeDataExt, f => f.Dim1, e => e.Dim1, (f, e) => new FakeProjectionInt { Key = Int32.Parse(f.Dim1), Measure = f.Total, Description = e.DimDesc }).OrderByDescending(e => e.Key);

            // Act
            var queryText = ((IPigQueryable)joined).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = load 'FakeDataMeta'; t2 = filter t1 by (Dim1 == '5'); t3 = group t2 by (DimDesc); t4 = group t0 by Dim1, t3 by Dim1; t5 = foreach t4 generate Dim1 as Key, (sum(Mea1) + sum(Mea1)) as Measure, DimDesc as Description; t6 = order t5 by (Key desc); t7 = limit t6 1000; ", queryText);
        }

        //[Test]
        public void complex_case()
        {
            // Arrange
            var fakeData = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f.Mea1, (dimension, data) => new { Dim1 = dimension, Total = Convert.ToInt32(data.Sum() + data.Sum()), Total2 = data.Sum() });
            var fakeDataExt = _extendedSource.AsQueryable<FakeDataMeta>().Where(e => e.Dim1 == "5");
            var joined = fakeData.Join(fakeDataExt, f => f.Dim1, e => e.Dim1, (f, e) => new { Fake = f, Extension = e }).OrderByDescending(e => e.Fake.Dim1).Take(10);

            // Act
            var queryText = ((IPigQueryable)joined).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = load 'FakeData'; t1 = load 'FakeDataMeta'; t2 = filter t1 by (Dim1 == '5'); t3 = group t2 by (DimDesc); t4 = group t0 by Dim1, t3 by Dim1; t5 = foreach t4 generate Dim1 as Key, (sum(Mea1) + sum(Mea1)) as Measure, DimDesc as Description; t6 = order t5 by (Key desc); t7 = limit t6 1000; ", queryText);
        }
    }
}
