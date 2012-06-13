using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// ProjectionReader is an implemention of IEnumerable that converts data from a query result into
    /// objects via a projector function,
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProjectionReader<T> : IEnumerable<T>, IEnumerable, IDisposable
    {
        private Enumerator _enumerator;

        public ProjectionReader(EnumerableDataReader reader, Func<EnumerableDataReader, T> projector)
        {
            _enumerator = new Enumerator(reader, projector);
        }

        void IDisposable.Dispose()
        {
            if (_enumerator != null)
            {
                _enumerator.Dispose();
                _enumerator = null;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            Enumerator e = _enumerator;
            if (e == null)
            {
                throw new InvalidOperationException("Cannot iterate more than once.");
            }
            _enumerator = null;
            return e;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        class Enumerator : IEnumerator<T>, IEnumerator
        {
            private EnumerableDataReader _reader;
            private T _current;
            Func<EnumerableDataReader, T> _projector;

            internal Enumerator(EnumerableDataReader reader, Func<EnumerableDataReader, T> projector)
            {
                _reader = reader;
                _projector = projector;
            }

            public T Current
            {
                get { return _current; }
            }

            Object IEnumerator.Current
            {
                get { return Current; }
            }

            public Boolean MoveNext()
            {
                if (_reader.Read())
                {
                    _current = _projector(_reader);
                    return true;
                }
                _reader.Dispose();
                return false;
            }

            public void Reset()
            {
            }

            public void Dispose()
            {
                _reader.Dispose();
            }
        }
    }
}
