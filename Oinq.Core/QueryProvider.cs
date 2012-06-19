using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Oinq.Core
{
    /// <summary>
    /// An implementation of IQueryProvider for querying a Pig data source.
    /// </summary>
    public class QueryProvider : IQueryProvider
    {
        // private fields
        private IDataFile _source;

        // constructors
        /// <summary>
        /// Initializes a new instance of the QueryProvider class.
        /// </summary>
        /// <param name="_source">The data _source being queried.</param>
        public QueryProvider(IDataFile source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            _source = source;
        }

        // public properties
        /// <summary>
        /// Gets the data source.
        /// </summary>
        public IDataFile Source
        {
            get { return _source; }
        }

        // public methods
        /// <summary>
        /// Builds the Pig query that will be sent when the LINQ query is executed.
        /// </summary>
        /// <typeparam name="T">The type of the objects being queried.</typeparam>
        /// <param name="query">The LINQ query.</param>
        /// <returns>The query text.</returns>
        public String BuildQueryText<T>(Query<T> query)
        {
            var translatedQuery = QueryTranslator.Translate(this, ((IQueryable)query).Expression);
            return ((SelectQuery)translatedQuery).CommandText;
        }

        /// <summary>
        /// Creates a new instance of Query{{T}} for this provider.
        /// </summary>
        /// <typeparam name="T">The type of the returned elements.</typeparam>
        /// <param name="expression">The query expression.</param>
        /// <returns>A new instance of Query{{T}}.</returns>
        public virtual IQueryable<T> CreateQuery<T>(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException("expression");
            }
            return new Query<T>(this, expression);
        }

        /// <summary>
        /// Creates a new instance Query{{T}} for this provider. Calls the generic CreateQuery{{T}}
        /// to actually create the new Query{{T}} instance.
        /// </summary>
        /// <param name="expression">The query expression.</param>
        /// <returns>A new instance of Query{{T}}.</returns>
        public virtual IQueryable CreateQuery(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            var elementType = TypeHelper.GetElementType(expression.Type);
            try
            {
                var queryableType = typeof(Query<>).MakeGenericType(elementType);
                return (IQueryable)Activator.CreateInstance(queryableType, new Object[] { this, expression });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Executes a query.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="expression">The query expression.</param>
        /// <returns>The result of the query.</returns>
        public virtual TResult Execute<TResult>(Expression expression)
        {
            return (TResult)Execute(expression);
        }

        /// <summary>
        /// Executes a query. Calls the generic method Execute{{T}} to actually execute the query.
        /// </summary>
        /// <param name="expression">The query expression.</param>
        /// <returns>The result of the query.</returns>
        public virtual Object Execute(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            var translatedQuery = QueryTranslator.Translate(this, expression);
            Delegate projector = ((SelectQuery)translatedQuery).Projection.Compile();

            var reader = (IList)Execute(translatedQuery);

            Type elementType = TypeHelper.GetElementType(expression.Type);
            return Activator.CreateInstance(
                typeof(ProjectionReader<>).MakeGenericType(elementType),
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                new Object[] { reader, projector },
                null
                );
        }

        // protected methods
        // Overridden in subclasses
        protected virtual Object Execute(TranslatedQuery translatedQuery)
        {
            throw new NotImplementedException();
        }
    }
}
