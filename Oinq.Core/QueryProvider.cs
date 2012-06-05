using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;

namespace Oinq.Core
{
    /// <summary>
    /// Optional interface for IQueryProvider to implement Query{{T}}'s QueryText property.
    /// </summary>
    public interface IQueryText
    {
        String GetQueryText(Expression expression);
    }

    /// <summary>
    /// An implementation of IQueryProvider for querying an EdgeSpring EdgeMart.
    /// </summary>
    public class QueryProvider : IQueryProvider, IQueryText
    {
        // private fields
        private ISource _source;

        // constructors
        /// <summary>
        /// Initializes a new instance of the QueryProvider class.
        /// </summary>
        /// <param name="source">The data source being queried.</param>
        public QueryProvider(ISource source)
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
        public ISource Source
        {
            get { return _source; }
        }

        // public methods
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
                return (IQueryable)Activator.CreateInstance(queryableType, new object[] { this, expression });
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes a query. Calls the generic method Execute{{T}} to actually execute the query.
        /// </summary>
        /// <param name="expression">The query expression.</param>
        /// <returns>The result of the query.</returns>
        public virtual Object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Translates and returns an expression tree as query text.
        /// </summary>
        /// <param name="expression">The query expression.</param>
        /// <returns>The query text.</returns>
        public virtual String GetQueryText(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            return Translate(expression).CommandText;

        }

        // private methods
        protected TranslatedQuery Translate(Expression expression)
        {
            ProjectionExpression projection = expression as ProjectionExpression;
            if (projection == null)
            {
                expression = PartialEvaluator.Evaluate(expression, CanBeEvaluatedLocally);
                expression = QueryBinder.Bind(this, expression);
                expression = AggregateRewriter.Rewrite(expression);
                expression = UnusedColumnRemover.Remove(expression);
                expression = RedundantSubqueryRemover.Remove(expression);
                projection = (ProjectionExpression)expression;
            }
            string commandText = QueryFormatter.Format(projection.Source);
            string[] columns = projection.Source.Columns.Select(c => c.Name).ToArray();
            LambdaExpression projector = ProjectionBuilder.Build(projection.Projector, projection.Source.Alias, columns);
            return new TranslatedQuery(commandText, projector, projection.Aggregator);
        }

        protected Boolean CanBeEvaluatedLocally(Expression expression)
        {
            // any operation on a query can't be done locally
            ConstantExpression cex = expression as ConstantExpression;
            if (cex != null)
            {
                IQueryable query = cex.Value as IQueryable;
                if (query != null && query.Provider == this)
                    return false;
            }
            return expression.NodeType != ExpressionType.Parameter &&
                   expression.NodeType != ExpressionType.Lambda;
        }
    }
}
