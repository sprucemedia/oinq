using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;

namespace Oinq.Core
{
    /// <summary>
    /// ProjectionReader is an implemention of IEnumerable that converts data from a query result into
    /// objects via a projector function,
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProjectionReader<T> : IEnumerable<T>, IEnumerable
    {
        private Enumerator _enumerator;

        public ProjectionReader(IEnumerable<Object> reader, Func<ProjectionRow, T> projector, IQueryProvider provider)
        {
            _enumerator = new Enumerator(reader, projector, provider);
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

        class Enumerator : ProjectionRow, IEnumerator<T>, IEnumerator
        {
            private IList<Object> _reader;
            private Int32 _currentIndex = 0;
            private T _current;
            private Func<ProjectionRow, T> _projector;
            private IQueryProvider _provider;

            internal Enumerator(IEnumerable<Object> reader, Func<ProjectionRow, T> projector, IQueryProvider provider)
            {
                _reader = reader.ToList();
                _projector = projector;
                _provider = provider;
            }

            public override Object GetValue(Int32 index)
            {
                if (index >= 0)
                {
                    return _reader[index];
                }
                throw new IndexOutOfRangeException();
            }

            public override IEnumerable<T> ExecuteSubQuery<T>(LambdaExpression query)
            {
                ProjectionExpression projection = (ProjectionExpression)Replacer.Replace(query.Body, query.Parameters[0], Expression.Constant(this));
                projection = (ProjectionExpression)PartialEvaluator.Evaluate(projection, CanEvaluateLocally);
                IEnumerable<T> result = (IEnumerable<T>)_provider.Execute(projection);
                IList<T> list = new List<T>(result);
                if (typeof(IQueryable<T>).IsAssignableFrom(query.Body.Type))
                {
                    return list.AsQueryable();
                }
                return list;
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
                _current = _projector(this);
                _currentIndex += 1;
                return true;
            }

            private static Boolean CanEvaluateLocally(Expression expression)
            {
                return (expression.NodeType != ExpressionType.Parameter);
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
