using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;

namespace Oinq.Core.Tests
{
    public class DataObj
    {
        public String Name { get; set; }
        public Int32 Age { get; set; }
    }

    [TestFixture]
    public class When_using_an_enumerable_data_reader
    {
        private static IEnumerable<DataObj> DataSource
        {
            get
            {
                return new List<DataObj>
                           {
                               new DataObj {Name = "1", Age = 16},
                               new DataObj {Name = "2", Age = 26},
                               new DataObj {Name = "3", Age = 36},
                               new DataObj {Name = "4", Age = 46}
                           };
            }
        }

        [Test]
        public void it_can_be_created_with_an_IEnumerable_and_type()
        {
            var r = new EnumerableDataReader(DataSource, typeof(DataObj));
            while (r.Read())
            {
                var values = new Object[2];
                Int32 count = r.GetValues(values);
                Assert.AreEqual(2, count);

                values = new Object[1];
                count = r.GetValues(values);
                Assert.AreEqual(1, count);

                values = new Object[3];
                count = r.GetValues(values);
                Assert.AreEqual(2, count);

                Assert.IsInstanceOf(typeof(String), r.GetValue(0));
                Assert.IsInstanceOf(typeof(Int32), r.GetValue(1));

                Console.WriteLine("Name: {0}, Age: {1}", r.GetValue(0), r.GetValue(1));
            }
        }

        [Test]
        public void it_can_be_created_with_an_IEnumerable_of_anonymous_type()
        {
            // create generic list
            Func<Type, IList> toGenericList =
                type => (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(new[] { type }));

            // create generic list of anonymous type
            IList listOfAnonymousType = toGenericList(new { Name = "1", Age = 16 }.GetType());

            listOfAnonymousType.Add(new { Name = "1", Age = 16 });
            listOfAnonymousType.Add(new { Name = "2", Age = 26 });
            listOfAnonymousType.Add(new { Name = "3", Age = 36 });
            listOfAnonymousType.Add(new { Name = "4", Age = 46 });

            var r = new EnumerableDataReader(listOfAnonymousType);
            while (r.Read())
            {
                var values = new Object[2];
                Int32 count = r.GetValues(values);
                Assert.AreEqual(2, count);

                values = new Object[1];
                count = r.GetValues(values);
                Assert.AreEqual(1, count);

                values = new Object[3];
                count = r.GetValues(values);
                Assert.AreEqual(2, count);

                Assert.IsInstanceOf(typeof(String), r.GetValue(0));
                Assert.IsInstanceOf(typeof(Int32), r.GetValue(1));

                Console.WriteLine("Name: {0}, Age: {1}", r.GetValue(0), r.GetValue(1));
            }
        }

        [Test]
        public void it_can_be_created_with_an_IEnumerable_of_type_T()
        {
            var r = new EnumerableDataReader(DataSource);
            while (r.Read())
            {
                var values = new Object[2];
                Int32 count = r.GetValues(values);
                Assert.AreEqual(2, count);

                values = new Object[1];
                count = r.GetValues(values);
                Assert.AreEqual(1, count);

                values = new Object[3];
                count = r.GetValues(values);
                Assert.AreEqual(2, count);

                Assert.IsInstanceOf(typeof(String), r.GetValue(0));
                Assert.IsInstanceOf(typeof(Int32), r.GetValue(1));

                Console.WriteLine("Name: {0}, Age: {1}", r.GetValue(0), r.GetValue(1));
            }
        }

        private static IEnumerable<FakeDataObject> DataSource2
        {
            get
            {
                var returnValue = new List<FakeDataObject>
                                  {
                                      new FakeDataObject
                                          {
                                              BooleanProp = false,
                                              StringProp = "stringProp",
                                              ByteProp = 1,
                                              CharProp = 'c',
                                              DateTimeProp = new DateTime(2007, 12, 18),
                                              DecimalProp = (Decimal) 100.1,
                                              DoubleProp = 100.2,
                                              FloatProp = (Single) 100.3,
                                              GuidProp = new Guid("{C14E0CFE-0FFC-484c-A695-DB80D6F4FFD4}"),
                                              Int16Prop = 5,
                                              Int32Prop = 6,
                                              Int64Prop = 7,
                                              NullProp = null
                                          }
                                  };


                return returnValue;
            }
        }

        [Test, ExpectedException(typeof(NotImplementedException))]
        public void it_can_get_the_depth()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            Int32 d = r.Depth;
        }

        [Test]
        public void it_can_get_the_field_count()
        {
            var r = new EnumerableDataReader(DataSource2);
            Assert.AreEqual(13, r.FieldCount);
        }

        [Test]
        public void it_can_get_boolean()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            Assert.AreEqual(false, r.GetBoolean(0));
            Assert.IsInstanceOf(typeof(Boolean), r.GetBoolean(0));
        }

        [Test]
        public void it_can_get_byte()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            Assert.AreEqual(1, r.GetByte(2));
            Assert.IsInstanceOf(typeof(Byte), r.GetByte(2));
        }

        [Test, ExpectedException(typeof(NotImplementedException))]
        public void it_can_get_bytes()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            Int32 i = 0;
            Int64 fieldOffset = 0;
            Byte[] buffer = null;
            Int32 bufferOffset = 0;
            Int32 length = 0;
            r.GetBytes(i, fieldOffset, buffer, bufferOffset, length);
        }

        [Test]
        public void it_can_get_char()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            Object value = r.GetChar(3);
            Assert.AreEqual('c', value);
            Assert.IsInstanceOf(typeof(Char), value);
        }

        [Test, ExpectedException(typeof(NotImplementedException))]
        public void it_can_get_chars()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            Int32 i = 0;
            Int64 fieldOffset = 0;
            Char[] buffer = null;
            Int32 bufferOffset = 0;
            Int32 length = 0;
            r.GetChars(i, fieldOffset, buffer, bufferOffset, length);
        }

        [Test, ExpectedException(typeof(NotImplementedException))]
        public void it_can_get_data()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            r.GetData(9);
        }

        [Test]
        public void it_can_get_data_type_name()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            String value = r.GetDataTypeName(0);
            Assert.AreEqual("Boolean", value);
        }

        [Test]
        public void it_can_get_date_time()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            Object value = r.GetDateTime(4);
            Assert.AreEqual(new DateTime(2007, 12, 18), value);
            Assert.IsInstanceOf(typeof(DateTime), value);
        }

        [Test]
        public void it_can_get_decimal()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            Object value = r.GetDecimal(5);
            Assert.AreEqual((Decimal)100.1, value);
            Assert.IsInstanceOf(typeof(Decimal), value);
        }

        [Test]
        public void it_can_get_double()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            Object value = r.GetDouble(6);
            Assert.AreEqual(100.2, value);
            Assert.IsInstanceOf(typeof(Double), value);
        }

        [Test]
        public void it_can_get_field_type()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            Type type = r.GetFieldType(0);
            Assert.AreEqual(typeof(Boolean), type);
        }

        [Test]
        public void it_can_get_float()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            Object value = r.GetFloat(7);
            Assert.AreEqual((Single)100.3, value);
            Assert.IsInstanceOf(typeof(Single), value);
        }

        [Test]
        public void it_can_get_guid()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            Object value = r.GetGuid(8);
            Assert.AreEqual(new Guid("{C14E0CFE-0FFC-484c-A695-DB80D6F4FFD4}"), value);
            Assert.IsInstanceOf(typeof(Guid), value);
        }

        [Test]
        public void it_can_get_int16()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            Object value = r.GetInt16(9);
            Assert.AreEqual((Int16)5, value);
            Assert.IsInstanceOf(typeof(Int16), value);
        }

        [Test]
        public void it_can_get_int32()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            Object value = r.GetInt32(10);
            Assert.AreEqual(6, value);
            Assert.IsInstanceOf(typeof(Int32), value);
        }

        [Test]
        public void it_can_get_int64()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            Object value = r.GetInt64(11);
            Assert.AreEqual((Int64)7, value);
            Assert.IsInstanceOf(typeof(Int64), value);
        }

        [Test]
        public void it_can_get_name()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            String result = r.GetName(0);
            Assert.AreEqual("BooleanProp", result);
        }

        [Test]
        public void it_can_get_ordinal()
        {
            var r = new EnumerableDataReader(DataSource2);
            Assert.AreEqual(0, r.GetOrdinal("BooleanProp"));
        }

        [Test]
        public void it_can_get_schema_table()
        {
            var r = new EnumerableDataReader(DataSource2);
            Object result = r.GetSchemaTable();
            Assert.IsInstanceOf(typeof(DataTable), result);
            var dt = (DataTable)result;
            Assert.AreEqual(13, dt.Columns.Count);

            Assert.AreEqual(typeof(bool), dt.Columns[0].DataType);
            Assert.AreEqual(typeof(string), dt.Columns[1].DataType);
            Assert.AreEqual(typeof(Byte), dt.Columns[2].DataType);
            Assert.AreEqual(typeof(Char), dt.Columns[3].DataType);
            Assert.AreEqual(typeof(DateTime), dt.Columns[4].DataType);
            Assert.AreEqual(typeof(decimal), dt.Columns[5].DataType);
            Assert.AreEqual(typeof(double), dt.Columns[6].DataType);
            Assert.AreEqual(typeof(float), dt.Columns[7].DataType);
            Assert.AreEqual(typeof(Guid), dt.Columns[8].DataType);
            Assert.AreEqual(typeof(Int16), dt.Columns[9].DataType);
            Assert.AreEqual(typeof(Int32), dt.Columns[10].DataType);
            Assert.AreEqual(typeof(Int64), dt.Columns[11].DataType);
            Assert.AreEqual(typeof(string), dt.Columns[12].DataType);
            Assert.AreEqual("BooleanProp", dt.Columns[0].ColumnName);
            Assert.AreEqual("StringProp", dt.Columns[01].ColumnName);
            Assert.AreEqual("ByteProp", dt.Columns[02].ColumnName);
            Assert.AreEqual("CharProp", dt.Columns[03].ColumnName);
            Assert.AreEqual("DateTimeProp", dt.Columns[04].ColumnName);
            Assert.AreEqual("DecimalProp", dt.Columns[05].ColumnName);
            Assert.AreEqual("DoubleProp", dt.Columns[06].ColumnName);
            Assert.AreEqual("FloatProp", dt.Columns[07].ColumnName);
            Assert.AreEqual("GuidProp", dt.Columns[08].ColumnName);
            Assert.AreEqual("Int16Prop", dt.Columns[09].ColumnName);
            Assert.AreEqual("Int32Prop", dt.Columns[10].ColumnName);
            Assert.AreEqual("Int64Prop", dt.Columns[11].ColumnName);
            Assert.AreEqual("NullProp", dt.Columns[12].ColumnName);
        }

        [Test]
        public void it_can_get_string()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            Object value = r.GetString(1);
            Assert.AreEqual("stringProp", value);
            Assert.IsInstanceOf(typeof(String), value);
        }

        [Test]
        public void it_can_get_value()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            Object value = r.GetValue(1);
            Assert.AreEqual("stringProp", value);
            Assert.IsInstanceOf(typeof(String), value);
        }

        [Test]
        public void it_can_get_values()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            var values = new Object[13];
            int result = r.GetValues(values);
            Assert.AreEqual(13, result);

            values = new Object[15];
            result = r.GetValues(values);
            Assert.AreEqual(13, result);

            values = new Object[5];
            result = r.GetValues(values);
            Assert.AreEqual(5, result);
        }

        [Test]
        public void it_can_get_check_if_closed()
        {
            var r = new EnumerableDataReader(DataSource2);
            Assert.IsFalse(r.IsClosed);
            r.Close();
            Assert.IsTrue(r.IsClosed);
        }

        [Test]
        public void it_can_check_if_null()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.Read();
            Assert.IsTrue(r.IsDBNull(12));
        }

        [Test, ExpectedException(typeof(NotImplementedException))]
        public void it_cannot_get_next_result()
        {
            var r = new EnumerableDataReader(DataSource2);
            r.NextResult();
        }

        [Test]
        public void it_can_get_records_affected()
        {
            var r = new EnumerableDataReader(DataSource2);
            Assert.AreEqual(-1, r.RecordsAffected);
            r.Read();
        }

        [Test]
        public void it_cannot_get_read_twice()
        {
            var r = new EnumerableDataReader(DataSource2);
            Assert.IsTrue(r.Read());
            Assert.IsFalse(r.Read(), "should be finished");
        }
    }
}