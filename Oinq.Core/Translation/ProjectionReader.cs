using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oinq.Core
{
    public class ProjectionReader<T> : IEnumerable<T>, IEnumerable
    {
        Enumerator enumerator;

        public ProjectionReader(IList reader, Func<ProjectionRow, T> projector)
        {
            this.enumerator = new Enumerator(reader, projector);
        }

        public IEnumerator<T> GetEnumerator()
        {
            Enumerator e = this.enumerator;
            if (e == null)
            {
                throw new InvalidOperationException("Cannot enumerate more than once");
            }
            this.enumerator = null;
            return e;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        class Enumerator : ProjectionRow, IEnumerator<T>, IEnumerator, IDisposable
        {
            IList _reader;
            T _current;
            Int32 _currentIndex;
            Func<ProjectionRow, T> _projector;

            public Enumerator(IList reader, Func<ProjectionRow, T> projector)
            {
                this._reader = reader;
                this._projector = projector;
            }

            public override object GetValue(int index)
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

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
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