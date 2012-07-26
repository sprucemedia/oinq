using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Oinq.Translation;

namespace Oinq
{
    /// <summary>
    /// An implementation of IQueryProvider for querying a Pig data source.
    /// </summary>
    public class QueryProvider : IQueryProvider
    {
        // private fields
        private readonly IDataFile _source;

        // constructors
        /// <summary>
        /// Initializes a new member of the QueryProvider class.
        /// </summary>
        /// <param name="source">The data source being queried.</param>
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

        #region IQueryProvider Members

        /// <summary>
        /// Creates a new member of Query{{T}} for this provider.
        /// </summary>
        /// <typeparam name="T">The type of the returned elements.</typeparam>
        /// <param name="expression">The query expression.</param>
        /// <returns>A new member of Query{{T}}.</returns>
        public virtual IQueryable<T> CreateQuery<T>(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            if (!typeof (IQueryable<T>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException("expression");
            }
            return new Query<T>(this, expression);
        }

        /// <summary>
        /// Creates a new member Query{{T}} for this provider. Calls the generic CreateQuery{{T}}
        /// to actually create the new Query{{T}} member.
        /// </summary>
        /// <param name="expression">The query expression.</param>
        /// <returns>A new member of Query{{T}}.</returns>
        public virtual IQueryable CreateQuery(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            Type elementType = TypeHelper.GetElementType(expression.Type);
            try
            {
                Type queryableType = typeof (Query<>).MakeGenericType(elementType);
                return (IQueryable) Activator.CreateInstance(queryableType, new Object[] {this, expression});
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
            return (TResult) Execute(expression);
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

            var mi = GetType().GetMethod("Execute", BindingFlags.Instance | BindingFlags.NonPublic);
            var gmi = mi.MakeGenericMethod(TypeHelper.GetElementType(expression.Type));
            return gmi.Invoke(this, new Object[] {translatedQuery});
        }

        #endregion

        /// <summary>
        /// Builds the Pig query that will be sent when the LINQ query is executed.
        /// </summary>
        /// <typeparam name="T">The type of the objects being queried.</typeparam>
        /// <param name="query">The LINQ query.</param>
        /// <returns>The query text.</returns>
        public String BuildQueryText<T>(Query<T> query)
        {
            var translatedQuery = QueryTranslator.Translate(this, ((IQueryable) query).Expression);
            return (translatedQuery).CommandText;
        }

        // protected methods
        // Overridden in subclasses
        /// <summary>
        /// Executes a query.
        /// </summary>
        /// <param name="translatedQuery">The TranslatedQuery.</param>
        /// <exception cref="NotImplementedException">NotImplementedException.</exception>
        /// <returns>The query result.</returns>
        protected virtual Object Execute<TResult>(ITranslatedQuery translatedQuery)
        {
            throw new NotImplementedException();
        }
    }
}