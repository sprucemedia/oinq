using System;
using System.Linq;
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
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = FILTER t0 BY (Dim1 == 'Fake'); t2 = FOREACH t1 GENERATE Dim1 AS Dim1, Mea1 AS Mea1; dump t2; ", queryText);
        }

        [Test]
        public void it_can_filter_rows_by_multiple_filters()
        {
            // Arrange
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => f.Dim1 == "Fake" && f.Mea1 == 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = FILTER t0 BY ((Dim1 == 'Fake') AND (Mea1 == 5)); t2 = FOREACH t1 GENERATE Dim1 AS Dim1, Mea1 AS Mea1; dump t2; ", queryText);
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
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = FILTER t0 BY (Dim1 == 'Fake'); t2 = FOREACH t1 GENERATE Dim1 AS Dim1, Mea1 AS Mea1; dump t2; ", queryText);
        }

        [Test]
        public void it_can_filter_rows_and_select_columns()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Dim1 == "Fake").Select(f => new { Measure1 = f.Mea1 });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = FILTER t0 BY (Dim1 == 'Fake'); t2 = FOREACH t1 GENERATE Mea1 AS Measure1; dump t2; ", queryText);
        }

        [Test]
        public void it_can_filter_rows_and_select_columns_reversed()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Select(f => new { Measure1 = f.Mea1 }).Where(f => f.Measure1 == 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = FILTER t0 BY (Mea1 == 5); t2 = FOREACH t1 GENERATE Mea1 AS Measure1; dump t2; ", queryText);
        }

        [Test]
        public void it_can_select_fields()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Select(f => new { Measure1 = f.Mea1 });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = FOREACH t0 GENERATE Mea1 AS Measure1; dump t1; ", queryText);
        }

        [Test]
        public void it_can_select_multiple_fields()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Select(f => new { Measure1 = f.Mea1, Dimension1 = f.Dim1 });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = FOREACH t0 GENERATE Mea1 AS Measure1, Dim1 AS Dimension1; dump t1; ", queryText);
        }

        [Test]
        public void it_can_order_by_a_field()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().OrderBy(f => f.Mea1);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = ORDER t0 BY Mea1 ASC; t2 = FOREACH t1 GENERATE Dim1 AS Dim1, Mea1 AS Mea1; dump t2; ", queryText);
        }

        [Test]
        public void it_can_order_by_a_field_in_descending_order()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().OrderByDescending(f => f.Mea1);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = ORDER t0 BY Mea1 DESC; t2 = FOREACH t1 GENERATE Dim1 AS Dim1, Mea1 AS Mea1; dump t2; ", queryText);
        }

        [Test]
        public void it_can_order_by_multiple_fields()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().OrderBy(f => f.Mea1).ThenBy(f => f.Dim1);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = ORDER t0 BY Mea1 ASC, Dim1 ASC; t2 = FOREACH t1 GENERATE Dim1 AS Dim1, Mea1 AS Mea1; dump t2; ", queryText);
        }

        [Test]
        public void it_can_order_by_multiple_fields_in_opposite_order()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().OrderBy(f => f.Mea1).ThenByDescending(f => f.Dim1);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = ORDER t0 BY Mea1 ASC, Dim1 DESC; t2 = FOREACH t1 GENERATE Dim1 AS Dim1, Mea1 AS Mea1; dump t2; ", queryText);
        }

        [Test]
        public void it_can_limit_the_number_of_results()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Take(100);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = FOREACH t0 GENERATE Dim1 AS Dim1, Mea1 AS Mea1; limit t1 100; ", queryText);
        }

        [Test]
        public void it_can_limit_the_numer_of_results_after_ordering()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().OrderBy(f => f.Mea1).Take(100);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = ORDER t0 BY Mea1 ASC; t2 = FOREACH t1 GENERATE Dim1 AS Dim1, Mea1 AS Mea1; limit t2 100; ", queryText);
        }

        [Test]
        public void it_can_group_data()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = GROUP t0 BY Dim1; t2 = FOREACH t1 GENERATE Dim1 AS Dim1; dump t2; ", queryText);
        }

        [Test]
        public void it_can_group_data_after_filtering()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 > 5).GroupBy(f => f.Dim1);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = FILTER t0 BY (Mea1 > 5); t2 = GROUP t1 BY Dim1; t3 = FOREACH t2 GENERATE Dim1 AS Dim1; dump t3; ", queryText);
        }

        [Test]
        public void it_can_group_data_with_an_aggregate()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f.Mea1, (dimension, measure) => new { Dimension = dimension, Total = measure.Sum() });
            
            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = GROUP t0 BY Dim1; t2 = FOREACH t1 GENERATE Dim1 AS Dimension, sum(Mea1) AS Total; dump t2; ", queryText);
        }

        [Test]
        public void it_can_group_data_with_an_aggregate_calculation()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f.Mea1, (dimension, measure) => new { Dimension = dimension, Total = measure.Sum() + measure.Sum() });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = GROUP t0 BY Dim1; t2 = FOREACH t1 GENERATE Dim1 AS Dimension, (sum(Mea1) + sum(Mea1)) AS Total; dump t2; ", queryText);
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
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = GROUP t0 BY Dim1; t2 = FILTER t1 BY (sum(Mea1) > 5); t3 = FOREACH t2 GENERATE Dim1 AS Dimension, sum(Mea1) AS Total; dump t3; ", queryText);
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
            Assert.True(queryText.Contains("t1 = FILTER t0 BY (Mea1 == 5);"));
        }

        [Test]
        public void it_can_implement_not_equals()
        {
            // Arrange
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 != 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY (Mea1 != 5);"));
        }

        [Test]
        public void it_can_implement_less_than()
        {
            // Arrange
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 < 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY (Mea1 < 5);"));
        }

        [Test]
        public void it_can_implement_greater_than()
        {
            // Arrange
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 > 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY (Mea1 > 5);"));
        }

        [Test]
        public void it_can_implement_less_than_or_equal_to()
        {
            // Arrange
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 <= 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY (Mea1 <= 5);"));
        }

        [Test]
        public void it_can_implement_greater_than_or_equal_to()
        {
            // Arrange
            IQueryable<FakeData> query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 >= 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY (Mea1 >= 5);"));
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
            Assert.True(queryText.Contains("t1 = FILTER t0 BY ((Mea1 + Mea1) > 5);"));
        }

        [Test]
        public void it_can_implement_subtraction()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 - f.Mea1 > 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY ((Mea1 - Mea1) > 5);"));
        }
        [Test]
        public void it_can_implement_multiplication()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 * f.Mea1 > 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY ((Mea1 * Mea1) > 5);"));
        }

        [Test]
        public void it_can_implement_division()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 / f.Mea1 > 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY ((Mea1 / Mea1) > 5);"));
        }

        [Test]
        public void it_can_implement_modulo()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 % f.Mea1 > 5);

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY ((Mea1 % Mea1) > 5);"));
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
            Assert.True(queryText.Contains("t1 = FILTER t0 BY ((Mea1 == 5) AND (Dim1 == 'Fake'));"));
        }

        [Test]
        public void it_can_implement_andalso()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 == 5 && f.Dim1 == "Fake");

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY ((Mea1 == 5) AND (Dim1 == 'Fake'));"));
        }

        [Test]
        public void it_can_implement_or()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 == 5 | f.Dim1 == "Fake");

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY ((Mea1 == 5) OR (Dim1 == 'Fake'));"));
        }

        [Test]
        public void it_can_implement_orelse()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().Where(f => f.Mea1 == 5 || f.Dim1 == "Fake");

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.True(queryText.Contains("t1 = FILTER t0 BY ((Mea1 == 5) OR (Dim1 == 'Fake'));"));
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
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = GROUP t0 BY Dim1; t2 = FOREACH t1 GENERATE Dim1 AS Dimension, sum(Mea1) AS Total; dump t2; ", queryText);
        }

        [Test]
        public void it_can_translate_a_complex_aggregate()
        {
            // Arrange
            var query = _source.AsQueryable<FakeData>().GroupBy(f => f.Dim1, f => f, (dimension, data) => new { Dimension = dimension, Total = data.AggOp() });

            // Act
            var queryText = ((IPigQueryable)query).GetPigQuery();

            // Assert
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = GROUP t0 BY Dim1; t2 = FOREACH t1 GENERATE Dim1 AS Dimension, (sum(Mea1) / sum(Mea1)) AS Total; dump t2; ", queryText);
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
            Assert.True(queryText.Contains("t0 = LOAD 'FakeData'; t1 = LOAD 'FakeDataMeta'; "));
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
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = LOAD 'FakeDataMeta'; t2 = JOIN t0 BY Dim1, t1 BY Dim1; t3 = FOREACH t2 GENERATE Dim1 AS Key, Mea1 AS Measure, DimDesc AS Description; dump t3; ", queryText);
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
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = LOAD 'FakeDataMeta'; t2 = FILTER t0 BY (Mea1 > 5); t3 = JOIN t2 BY Dim1, t1 BY Dim1; t4 = FOREACH t3 GENERATE Dim1 AS Key, Mea1 AS Measure, DimDesc AS Description; dump t4; ", queryText);
        }
    }
}
