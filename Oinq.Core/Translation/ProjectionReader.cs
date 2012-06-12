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

        public ProjectionReader(IEnumerable<Object> reader, Func<IEnumerable<Object>, T> projector)
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
            private IList<Object> _reader;
            private T _current;
            Func<IEnumerable<Object>, T> _projector;

            internal Enumerator(IEnumerable<Object> reader, Func<IEnumerable<Object>, T> projector)
            {
                _reader = reader.ToList();
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
                _current = _projector(_reader);
                return true;
            }

            public void Reset()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
