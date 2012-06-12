using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
        // IQueryProvider methods
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
            return (TResult)Execute(expression);
        }

        /// <summary>
        /// Executes a query. Calls the generic method Execute{{T}} to actually execute the query.
        /// </summary>
        /// <param name="expression">The query expression.</param>
        /// <returns>The result of the query.</returns>
        public virtual Object Execute(Expression expression)
        {
            // strip off lambda for now
            LambdaExpression lambda = expression as LambdaExpression;
            if (lambda != null)
            {
                expression = lambda.Body;
            }

            // translate query into component pieces
            ProjectionExpression projection = Translate(expression);

            String commandText = QueryFormatter.Format(projection.Source);
            ReadOnlyCollection<NamedValueExpression> namedValues = NamedValueGatherer.Gather(projection.Source);
            String[] names = namedValues.Select(v => v.Name).ToArray();

            Expression rootQueryable = RootQueryableFinder.Find(expression);
            Expression providerAccess = Expression.Convert(
                Expression.Property(rootQueryable, typeof(IQueryable).GetProperty("Provider")), typeof(QueryProvider));

            LambdaExpression projector = ProjectionBuilder.Build(this, projection, providerAccess);
            LambdaExpression eRead = GetReader(projector, projection.Aggregator, true);

            // if asked to execute a lambda, produce a function that will execute this query later.
            if (lambda != null)
            {
                // call low-level execute directly on the supplied QueryProvider
                Expression body = Expression.Call(
                    providerAccess, "Execute", null,
                    Expression.Constant(commandText),
                    Expression.Constant(names),
                    Expression.NewArrayInit(typeof(Object), namedValues.Select(v => Expression.Convert(v.Value, typeof(Object))).ToArray()),
                    eRead);
                body = Expression.Convert(body, expression.Type);
                LambdaExpression fn = Expression.Lambda(lambda.Type, body, lambda.Parameters);
                return fn.Compile();
            }
            else
            {
                // execute now!
                Object[] values = namedValues.Select(v => v.Value as ConstantExpression).Select(c => c != null ? c.Value : null).ToArray();
                var fnRead = (Func<IEnumerable<Object>, Object>)eRead.Compile();
                return Execute(commandText, names, values, fnRead);
            }
        }

        public virtual Object Execute(String commandText, String[] paramNames, Object[] paramValues, Func<IEnumerable<Object>, Object> fnRead)
        {
            throw new NotImplementedException();
        }

        // IQueryText implementation
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
            ProjectionExpression projection = Translate(expression);
            return QueryFormatter.Format(projection.Source);

        }

        // protected methods
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

        // create a lambda function that will convert a DbDataReader into a projected (and possibly aggregated) result
        protected static LambdaExpression GetReader(LambdaExpression fnProjector, LambdaExpression fnAggregator, Boolean boxReturn)
        {
            ParameterExpression reader = Expression.Parameter(typeof(IEnumerable<Object>), "reader");
            Expression body = Expression.New(typeof(ProjectionReader<>).MakeGenericType(fnProjector.Body.Type).GetConstructors()[0], reader, fnProjector);
            if (fnAggregator != null)
            {
                body = Expression.Invoke(fnAggregator, body);
            }
            if (boxReturn && body.Type != typeof(Object))
            {
                body = Expression.Convert(body, typeof(Object));
            }
            return Expression.Lambda(body, reader);
        }

        private ProjectionExpression Translate(Expression expression)
        {
            expression = PartialEvaluator.Evaluate(expression, CanBeEvaluatedLocally);
            expression = QueryBinder.Bind(this, expression);
            expression = AggregateRewriter.Rewrite(expression);
            expression = UnusedColumnRemover.Remove(expression);
            expression = RedundantSubqueryRemover.Remove(expression);
            return (ProjectionExpression)expression;
        }

        /// <summary>
        /// ProjectionBuilder is a visitor that converts an projector expression
        /// that constructs result objects out of ColumnExpressions into an actual
        /// LambdaExpression that constructs result objects out of accessing fields
        /// of a ProjectionRow
        /// </summary>
        class ProjectionBuilder : PigExpressionVisitor
        {
            // private fields
            private ParameterExpression _resultParam;
            private QueryProvider _provider;
            private ProjectionExpression _projection;
            private Expression _providerAccess;
            private Dictionary<String, Int32> _nameMap;

            // constructors
            private ProjectionBuilder(QueryProvider provider, ProjectionExpression projection, Expression providerAccess)
            {
                _provider = provider;
                _projection = projection;
                _providerAccess = providerAccess;
                _resultParam = Expression.Parameter(typeof(IEnumerable), "reader");
                _nameMap = projection.Source.Columns.Select((c, i) => new { c, i }).ToDictionary(x => x.c.Name, x => x.i);
            }

            // internal static methods
            internal static LambdaExpression Build(QueryProvider provider, ProjectionExpression projection, Expression providerAccess)
            {
                ProjectionBuilder builder = new ProjectionBuilder(provider, projection, providerAccess);
                Expression body = builder.Visit(projection.Projector);
                return Expression.Lambda(body, builder._resultParam);
            }

            // protected override methods
            protected override Expression VisitColumn(ColumnExpression column)
            {
                if (column.Alias == _projection.Source.Alias)
                {
                    Int32 iOrdinal = _nameMap[column.Name];

                    Expression defValue;
                    if (!column.Type.IsValueType || TypeHelper.IsNullableType(column.Type))
                    {
                        defValue = Expression.Constant(null, column.Type);
                    }
                    else
                    {
                        defValue = Expression.Constant(Activator.CreateInstance(column.Type), column.Type);
                    }

                    Expression value = Expression.Convert(
                        Expression.Call(typeof(System.Convert), "ChangeType", null,
                        Expression.Constant(TypeHelper.GetNonNullableType(column.Type))),
                        column.Type);

                    return Expression.Condition(
                        Expression.Call(_resultParam, "IsNull", null, Expression.Constant(iOrdinal)), defValue, value);
                }
                return column;
            }

            protected override Expression VisitProjection(ProjectionExpression projection) 
            {
                String commandText = QueryFormatter.Format(projection.Source);
                ReadOnlyCollection<NamedValueExpression> namedValues = NamedValueGatherer.Gather(projection.Source);
                String[] names = namedValues.Select(v => v.Name).ToArray();
                Expression[] values = namedValues.Select(v => Expression.Convert(Visit(v.Value), typeof(Object))).ToArray();

                LambdaExpression projector = ProjectionBuilder.Build(_provider, projection, _providerAccess);
                LambdaExpression eRead = GetReader(projector, projection.Aggregator, true);

                Type resultType = projection.Aggregator != null ? projection.Aggregator.Body.Type : typeof(IEnumerable<>).MakeGenericType(projection.Projector.Type);

                // return expression that will call Execute(...)
                return Expression.Convert(
                    Expression.Call(_providerAccess, "Execute", null, Expression.Constant(commandText), Expression.Constant(names),
                        Expression.NewArrayInit(typeof(Object), values), eRead),
                    resultType);
            }

            /// <summary>
            /// columns referencing the outer alias are turned into special named-node parameters
            /// </summary>
            class OuterParameterizer : PigExpressionVisitor
            {
                private Int32 _param;
                private String _outerAlias;

                internal static Expression Parameterize(String outerAlias, Expression expr)
                {
                    OuterParameterizer op = new OuterParameterizer();
                    op._outerAlias = outerAlias;
                    return op.Visit(expr);
                }

                protected override Expression VisitProjection(ProjectionExpression proj)
                {
                    SelectExpression select = (SelectExpression)Visit(proj.Source);
                    if (select != proj.Source)
                    {
                        return new ProjectionExpression(select, proj.Projector, proj.Aggregator);
                    }
                    return proj;
                }

                protected override Expression VisitColumn(ColumnExpression column)
                {
                    if (column.Alias.ToString() == _outerAlias)
                    {
                        return new NamedValueExpression("n" + (_param++), column);
                    }
                    return column;
                }
            }
        }

        // attempt to isolate a sub-expression that accesses a Query<T> object
        class RootQueryableFinder : PigExpressionVisitor
        {
            Expression root;
            internal static Expression Find(Expression expression)
            {
                RootQueryableFinder finder = new RootQueryableFinder();
                finder.Visit(expression);
                return finder.root;
            }

            protected override Expression Visit(Expression exp)
            {
                Expression result = base.Visit(exp);

                // remember the first sub-expression that produces an IQueryable
                if (this.root == null && result != null && typeof(IQueryable).IsAssignableFrom(result.Type))
                {
                    this.root = result;
                }

                return result;
            }
        }
    }
}
