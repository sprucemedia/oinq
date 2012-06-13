using System;
using System.Collections.Generic;
using System.Data;

namespace Oinq.Core
{
    public abstract class ObjectDataReader : IDataReader
    {
        protected Boolean Closed;
        protected IList<ReflectionExtensions.Property> Fields;

        protected ObjectDataReader()
        {
        }

        protected ObjectDataReader(Type elementType)
        {
            SetFields(elementType);
            Closed = false;
        }

        /// <summary>
        /// Return the value of the specified field.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Object"/> which will contain the field value upon return.
        /// </returns>
        /// <param name="i">The index of the field to find. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        public abstract object GetValue(Int32 i);

        /// <summary>
        /// Advances the <see cref="T:System.Data.IDataReader"/> to the next record.
        /// </summary>
        /// <returns>
        /// true if there are more rows; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public abstract Boolean Read();

        /// <summary>
        /// Gets the number of columns in the current row.
        /// </summary>
        /// <returns>
        /// When not positioned in a valid recordset, 0; otherwise, the number of columns in the current record. The default is -1.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public Int32 FieldCount
        {
            get { return Fields.Count; }
        }

        /// <summary>
        /// Return the index of the named field.
        /// </summary>
        /// <returns>
        /// The index of the named field.
        /// </returns>
        /// <param name="name">The name of the field to find. 
        /// </param><filterpriority>2</filterpriority>
        public virtual Int32 GetOrdinal(String name)
        {
            for (Int32 i = 0; i < Fields.Count; i++)
            {
                if (Fields[i].Info.Name == name)
                {
                    return i;
                }
            }

            throw new IndexOutOfRangeException("name");
        }


        /// <summary>
        /// Gets the column located at the specified index.
        /// </summary>
        /// <returns>
        /// The column located at the specified index as an <see cref="T:System.Object"/>.
        /// </returns>
        /// <param name="i">The zero-based index of the column to get. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        object IDataRecord.this[Int32 i]
        {
            get { return GetValue(i); }
        }


        /// <summary>
        /// Gets the value of the specified column as a Boolean.
        /// </summary>
        /// <returns>
        /// The value of the column.
        /// </returns>
        /// <param name="i">The zero-based column ordinal. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        public virtual Boolean GetBoolean(Int32 i)
        {
            return (Boolean)GetValue(i);
        }

        /// <summary>
        /// Gets the 8-bit unsigned integer value of the specified column.
        /// </summary>
        /// <returns>
        /// The 8-bit unsigned integer value of the specified column.
        /// </returns>
        /// <param name="i">The zero-based column ordinal. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        public virtual byte GetByte(Int32 i)
        {
            return (Byte)GetValue(i);
        }

        /// <summary>
        /// Gets the character value of the specified column.
        /// </summary>
        /// <returns>
        /// The character value of the specified column.
        /// </returns>
        /// <param name="i">The zero-based column ordinal. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        public virtual char GetChar(Int32 i)
        {
            return (Char)GetValue(i);
        }

        /// <summary>
        /// Gets the date and time data value of the specified field.
        /// </summary>
        /// <returns>
        /// The date and time data value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        public virtual DateTime GetDateTime(Int32 i)
        {
            return (DateTime)GetValue(i);
        }

        /// <summary>
        /// Gets the fixed-position numeric value of the specified field.
        /// </summary>
        /// <returns>
        /// The fixed-position numeric value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        public virtual decimal GetDecimal(Int32 i)
        {
            return (Decimal)GetValue(i);
        }

        /// <summary>
        /// Gets the double-precision floating poInt32 number of the specified field.
        /// </summary>
        /// <returns>
        /// The double-precision floating poInt32 number of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        public virtual double GetDouble(Int32 i)
        {
            return (Double)GetValue(i);
        }

        /// <summary>
        /// Gets the <see cref="T:System.Type"/> information corresponding to the type of <see cref="T:System.Object"/> that would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Type"/> information corresponding to the type of <see cref="T:System.Object"/> that would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)"/>.
        /// </returns>
        /// <param name="i">The index of the field to find. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        public virtual Type GetFieldType(Int32 i)
        {
            return Fields[i].Info.PropertyType;
        }

        /// <summary>
        /// Gets the single-precision floating poInt32 number of the specified field.
        /// </summary>
        /// <returns>
        /// The single-precision floating poInt32 number of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        public virtual float GetFloat(Int32 i)
        {
            return (float)GetValue(i);
        }

        /// <summary>
        /// Returns the GUID value of the specified field.
        /// </summary>
        /// <returns>
        /// The GUID value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        public virtual Guid GetGuid(Int32 i)
        {
            return (Guid)GetValue(i);
        }

        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field.
        /// </summary>
        /// <returns>
        /// The 16-bit signed integer value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        public virtual short GetInt16(Int32 i)
        {
            return (Int16)GetValue(i);
        }

        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field.
        /// </summary>
        /// <returns>
        /// The 32-bit signed integer value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        public virtual Int32 GetInt32(Int32 i)
        {
            return (Int32)GetValue(i);
        }

        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field.
        /// </summary>
        /// <returns>
        /// The 64-bit signed integer value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        public virtual long GetInt64(Int32 i)
        {
            return (Int64)GetValue(i);
        }

        /// <summary>
        /// Gets the String value of the specified field.
        /// </summary>
        /// <returns>
        /// The String value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        public virtual String GetString(Int32 i)
        {
            return (String)GetValue(i);
        }

        /// <summary>
        /// Return whether the specified field is set to null.
        /// </summary>
        /// <returns>
        /// true if the specified field is set to null; otherwise, false.
        /// </returns>
        /// <param name="i">The index of the field to find. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        public virtual Boolean IsDBNull(Int32 i)
        {
            return GetValue(i) == null;
        }

        /// <summary>
        /// Gets the column with the specified name.
        /// </summary>
        /// <returns>
        /// The column with the specified name as an <see cref="T:System.Object"/>.
        /// </returns>
        /// <param name="name">The name of the column to find. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">No column with the specified name was found. 
        /// </exception><filterpriority>2</filterpriority>
        object IDataRecord.this[String name]
        {
            get { return GetValue(GetOrdinal(name)); }
        }


        /// <summary>
        /// Gets the data type information for the specified field.
        /// </summary>
        /// <returns>
        /// The data type information for the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        public virtual String GetDataTypeName(Int32 i)
        {
            return GetFieldType(i).Name;
        }


        /// <summary>
        /// Gets the name for the field to find.
        /// </summary>
        /// <returns>
        /// The name of the field or the empty String (""), if there is no value to return.
        /// </returns>
        /// <param name="i">The index of the field to find.</param>
        /// <exception cref="T:System.IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception>
        /// <filterpriority>2</filterpriority>
        public virtual String GetName(Int32 i)
        {
            if (i < 0 || i >= Fields.Count)
            {
                throw new IndexOutOfRangeException("name");
            }
            return Fields[i].Info.Name;
        }

        /// <summary>
        /// Gets all the attribute fields in the collection for the current record.
        /// </summary>
        /// <returns>
        /// The number of instances of <see cref="T:System.Object"/> in the array.
        /// </returns>
        /// <param name="values">An array of <see cref="T:System.Object"/> to copy the attribute fields into. 
        /// </param><filterpriority>2</filterpriority>
        public virtual Int32 GetValues(Object[] values)
        {
            Int32 i = 0;
            for (; i < Fields.Count; i++)
            {
                if (values.Length <= i)
                {
                    return i;
                }
                values[i] = GetValue(i);
            }
            return i;
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.IDataReader"/> for the specified column ordinal.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.IDataReader"/>.
        /// </returns>
        /// <param name="i">The index of the field to find. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        public virtual IDataReader GetData(Int32 i)
        {
            // need to think about this one
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a stream of bytes from the specified column offset into the buffer as an array, starting at the given buffer offset.
        /// </summary>
        /// <returns>
        /// The actual number of bytes read.
        /// </returns>
        /// <param name="i">The zero-based column ordinal. 
        /// </param><param name="fieldOffset">The index within the field from which to start the read operation. 
        /// </param><param name="buffer">The buffer into which to read the stream of bytes. 
        /// </param><param name="bufferoffset">The index for <paramref name="buffer"/> to start the read operation. 
        /// </param><param name="length">The number of bytes to read. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        public virtual long GetBytes(Int32 i, long fieldOffset, byte[] buffer, Int32 bufferoffset, Int32 length)
        {
            // need to keep track of the bytes got for each record - more work than i want to do right now
            // http://msdn.microsoft.com/en-us/library/system.data.idatarecord.getbytes.aspx
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a stream of characters from the specified column offset into the buffer as an array, starting at the given buffer offset.
        /// </summary>
        /// <returns>
        /// The actual number of characters read.
        /// </returns>
        /// <param name="i">The zero-based column ordinal. 
        /// </param><param name="fieldoffset">The index within the row from which to start the read operation. 
        /// </param><param name="buffer">The buffer into which to read the stream of bytes. 
        /// </param><param name="bufferoffset">The index for <paramref name="buffer"/> to start the read operation. 
        /// </param><param name="length">The number of bytes to read. 
        /// </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. 
        /// </exception><filterpriority>2</filterpriority>
        public virtual long GetChars(Int32 i, long fieldoffset, char[] buffer, Int32 bufferoffset, Int32 length)
        {
            // need to keep track of the bytes got for each record - more work than i want to do right now
            // http://msdn.microsoft.com/en-us/library/system.data.idatarecord.getchars.aspx
            throw new NotImplementedException();
        }

        /// <summary>
        /// Closes the <see cref="T:System.Data.IDataReader"/> Object.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public virtual void Close()
        {
            Closed = true;
        }

        /// <summary>
        /// Returns a <see cref="T:System.Data.DataTable"/> that describes the column metadata of the <see cref="T:System.Data.IDataReader"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.DataTable"/> that describes the column metadata.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Data.IDataReader"/> is closed. 
        /// </exception><filterpriority>2</filterpriority>
        public virtual DataTable GetSchemaTable()
        {
            var dt = new DataTable();
            foreach (ReflectionExtensions.Property field in Fields)
            {
                Type colType;
                Boolean nullable = false;
                if (field.Info.PropertyType.IsGenericType && field.Info.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    colType = field.Info.PropertyType.GetGenericArguments()[0];
                    nullable = true;
                }
                else
                {
                    colType = field.Info.PropertyType;
                }

                var col = new DataColumn(field.Info.Name, colType) { AllowDBNull = nullable };
                dt.Columns.Add(col);
            }
            return dt;
        }

        /// <summary>
        /// Advances the data reader to the next result, when reading the results of batch SQL statements.
        /// </summary>
        /// <returns>
        /// true if there are more rows; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public virtual Boolean NextResult()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Gets a value indicating the depth of nesting for the current row.
        /// </summary>
        /// <returns>
        /// The level of nesting.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public virtual Int32 Depth
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets a value indicating whether the data reader is closed.
        /// </summary>
        /// <returns>
        /// true if the data reader is closed; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public virtual Boolean IsClosed
        {
            get { return Closed; }
        }

        /// <summary>
        /// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
        /// </summary>
        /// <returns>
        /// The number of rows changed, inserted, or deleted; 0 if no rows were affected or the statement failed; and -1 for SELECT statements.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public virtual Int32 RecordsAffected
        {
            get
            {
                // assuming select only?
                return -1;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public virtual void Dispose()
        {
            Fields = null;
        }

        protected void SetFields(Type elementType)
        {
            Fields = ReflectionExtensions.CreatePropertyMethods(elementType);
        }
    }
}