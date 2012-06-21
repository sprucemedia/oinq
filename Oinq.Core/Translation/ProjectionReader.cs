using System;
using System.Collections;
using System.Collections.Generic;

namespace Oinq
{
    internal class ProjectionReader<T> : IEnumerable<T>, IEnumerable, IDisposable
    {
        // private fields
        private Enumerator _enumerator;

        // constructors
        internal ProjectionReader(IList reader, Func<ProjectionRow, T> projector)
        {
            _enumerator = new Enumerator(reader, projector);
        }

        // public fields
        public IEnumerator<T> GetEnumerator()
        {
            Enumerator e = _enumerator;
            if (e == null)
            {
                throw new InvalidOperationException("Cannot enumerate more than once");
            }
            _enumerator = null;
            return e;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Disposes of class.
        /// </summary>
        public void Dispose()
        {
            _enumerator.Dispose();
        }

        class Enumerator : ProjectionRow, IEnumerator<T>, IEnumerator, IDisposable
        {
            // private fields
            private IList _reader;
            private Int32 _currentIndex;
            private Func<ProjectionRow, T> _projector;

            // constructors
            internal Enumerator(IList reader, Func<ProjectionRow, T> projector)
            {
                _reader = reader;
                _projector = projector;
            }

            // public methods
            public override Object GetValue(Int32 index)
            {
                if (index >= 0)
                {
                    return _reader[index];
                }
                throw new IndexOutOfRangeException();
            }

            public T Current
            {
                get
                {
                    try
                    {
                        return _projector(this);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            Object IEnumerator.Current
            {
                get { return Current; }
            }

            public Boolean MoveNext()
            {
                _currentIndex++;
                return (_currentIndex < _reader.Count);
            }

            public void Reset()
            {
                _currentIndex = -1;
            }

            public void Dispose()
            {
            }
        }
    }
}