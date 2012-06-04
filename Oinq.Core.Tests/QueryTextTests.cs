using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Oinq.Core.Tests
{
    [TestFixture]
    public class When_building_query_command_text
    {
        private Query<FakeData> _fakeData = new Query<FakeData>(new QueryProvider());

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
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = FILTER t0 BY (Mea1 == 5); t2 = FOREACH t1 GENERATE Mea1 AS Mea1; ", queryText);
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
            Assert.AreEqual("t0 = LOAD 'FakeData'; t1 = FILTER t0 BY (Mea1 > 5); t2 = GROUP t1 BY Dim1; t3 = FOREACH t2 GENERATE Dim1 AS Dim1; ", queryText);
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
    }
}