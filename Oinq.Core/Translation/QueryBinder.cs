using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Oinq.Core
{
    /// <summary>
    /// QueryBinder is a visitor that converts method calls to LINQ operations into
    /// custom PigExpression nodes and references to class members into references to columns.
    /// </summary>
    internal class QueryBinder : ExpressionVisitor
    {
        // private fields
        private Dictionary<ParameterExpression, Expression> _map;
        private IQueryProvider _provider;
        private Expression _root;
        private List<OrderByExpression> _thenBys;
        private Dictionary<Expression, GroupByInfo> _groupByMap;
        private Expression _currentGroupElement;

        // constructors
        private QueryBinder(IQueryProvider provider, Expression root)
        {
            _provider = provider;
            _map = new Dictionary<ParameterExpression, Expression>();
            _groupByMap = new Dictionary<Expression, GroupByInfo>();
            _root = root;
        }

        // internal static methods
        internal static Expression Bind(IQueryProvider provider, Expression node)
        {
            return new QueryBinder(provider, node).Visit(node);
        }

        // protected override methods
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (IsTable(node))
            {
                return GetTableProjection(TypeHelper.GetElementType(node.Type));
            }
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Expression source = Visit(node.Expression);
            switch (source.NodeType)
            {
                case ExpressionType.MemberInit:
                    MemberInitExpression min = (MemberInitExpression)source;
                    for (int i = 0, n = min.Bindings.Count; i < n; i++)
                    {
                        MemberAssignment assign = min.Bindings[i] as MemberAssignment;
                        if (assign != null && MembersMatch(assign.Member, node.Member))
                        {
                            return assign.Expression;
                        }
                    }
                    break;
                case ExpressionType.New:
                    NewExpression nex = (NewExpression)source;
                    if (nex.Members != null)
                    {
                        for (int i = 0, n = nex.Members.Count; i < n; i++)
                        {
                            if (MembersMatch(nex.Members[i], node.Member))
                            {
                                return nex.Arguments[i];
                            }
                        }
                    }
                    break;
            }
            if (source == node.Expression)
            {
                return node;
            }
            return MakeMember(source, node.Member);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable) || node.Method.DeclaringType == typeof(Enumerable))
            {
                switch (node.Method.Name)
                {
                    case "Where":
                        return BindWhere(node.Type, node.Arguments[0], GetLambda(node.Arguments[1]));
                    case "Select":
                        return BindSelect(node.Type, node.Arguments[0], GetLambda(node.Arguments[1]));
                    case "OrderBy":
                        return BindOrderBy(node.Type, node.Arguments[0], GetLambda(node.Arguments[1]), 
                            OrderByDirection.Ascending);
                    case "OrderByDescending":
                        return BindOrderBy(node.Type, node.Arguments[0], GetLambda(node.Arguments[1]),
                            OrderByDirection.Descending);
                    case "ThenBy":
                        return BindThenBy(node.Arguments[0], GetLambda(node.Arguments[1]),
                            OrderByDirection.Ascending);
                    case "ThenByDescending":
                        return BindThenBy(node.Arguments[0], GetLambda(node.Arguments[1]),
                            OrderByDirection.Descending);
                    case "Take":
                        if (node.Arguments.Count == 2)
                        {
                            return BindTake(node.Arguments[0], node.Arguments[1]);
                        }
                        break;
                    case "GroupBy":
                        if (node.Arguments.Count == 2)
                        {
                            return BindGroupBy(node.Arguments[0], GetLambda(node.Arguments[1]), null, null);
                        }
                        else if (node.Arguments.Count == 3)
                        {
                            LambdaExpression lambda1 = GetLambda(node.Arguments[1]);
                            LambdaExpression lambda2 = GetLambda(node.Arguments[2]);
                            if (lambda2.Parameters.Count == 1)
                            {
                                // second lambda is element selector
                                return BindGroupBy(node.Arguments[0], lambda1, lambda2, null);
                            }
                            else if (lambda2.Parameters.Count == 2)
                            {
                                // second lambda is result selector
                                return BindGroupBy(node.Arguments[0], lambda1, null, lambda2);
                            }
                        }
                        else if (node.Arguments.Count == 4)
                        {
                            return BindGroupBy(node.Arguments[0], GetLambda(node.Arguments[1]),
                                GetLambda(node.Arguments[2]), GetLambda(node.Arguments[3]));
                        }
                        break;
                    case "Count":
                    case "Min":
                    case "Max":
                    case "Sum":
                    case "Average":
                        if (node.Arguments.Count == 1)
                        {
                            return BindAggregate(node.Arguments[0], node.Method.Name, node.Method, null, node == _root);
                        }
                        else if (node.Arguments.Count == 2)
                        {
                            LambdaExpression selector = GetLambda(node.Arguments[1]);
                            return BindAggregate(node.Arguments[0], node.Method.Name, node.Method, selector, node == _root);
                        }
                        break;
                }
                throw new NotSupportedException(String.Format("The method '{0}' is not supported", node.Method.Name));
            }
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Expression e;
            if (_map.TryGetValue(node, out e))
            {
                return e;
            }
            return node;
        }

        // private methods
        private Expression BindAggregate(Expression source, String aggName, MethodInfo method, LambdaExpression argument, Boolean isRoot)
        {
            Type returnType = method.ReturnType;
            Boolean hasPredicateArg = HasPredicateArg(aggName);
            Boolean isDistinct = false;
            Boolean argumentWasPredicate = false;

            // check for distinct
            MethodCallExpression mcs = source as MethodCallExpression;
            if (mcs != null && !hasPredicateArg && argument == null)
            {
                if (mcs.Method.Name == "Distinct" && mcs.Arguments.Count == 1 &&
                    (mcs.Method.DeclaringType == typeof(Queryable) || mcs.Method.DeclaringType == typeof(Enumerable)))
                {
                    source = mcs.Arguments[0];
                    isDistinct = true;
                }
            }

            if (argument != null && hasPredicateArg)
            {
                // convert query.Count(predicate) into query.Where(predicate).Count()
                source = Expression.Call(typeof(Queryable), "Where", method.GetGenericArguments(), source, argument);
                argument = null;
                argumentWasPredicate = true;
            }

            ProjectionExpression projection = VisitSequence(source);

            Expression argExpr = null;
            if (argument != null)
            {
                _map[argument.Parameters[0]] = projection.Projector;
                argExpr = Visit(argument.Body);
            }
            else if (!hasPredicateArg)
            {
                argExpr = projection.Projector;
            }

            var alias = GetNextAlias();
            var pc = ProjectColumns(projection.Projector, alias, projection.Source.Alias);
            Expression aggExpr = new AggregateExpression(returnType, aggName, argExpr, isDistinct);
            SelectExpression select = new SelectExpression(alias, new ColumnDeclaration[] { new ColumnDeclaration("", aggExpr) }, projection.Source, null);

            if (isRoot)
            {
                ParameterExpression p = Expression.Parameter(typeof(IEnumerable<>).MakeGenericType(aggExpr.Type), "node");
                LambdaExpression gator = Expression.Lambda(Expression.Call(typeof(Enumerable), "Single", new Type[] { returnType }, p), p);
                return new ProjectionExpression(select, new ColumnExpression(returnType, alias, ""), gator);
            }

            ScalarExpression subquery = new ScalarExpression(returnType, select);

            // if we can find the corresponding group-info we can build a special AggregateSubquery node that will enable us to
            // optimize the node expression later using AggregateRewriter
            GroupByInfo info;
            if (!argumentWasPredicate && _groupByMap.TryGetValue(projection, out info))
            {
                // use the element expression from the group-by info to rebind the argument so the resulting expression is one that
                // would be legal to add to the columns in the select expression that has the corresponding group-by clause.
                if (argument != null)
                {
                    _map[argument.Parameters[0]] = info.Element;
                    argExpr = Visit(argument.Body);
                }
                else if (!hasPredicateArg)
                {
                    argExpr = info.Element;
                }
                aggExpr = new AggregateExpression(returnType, aggName, argExpr, isDistinct);

                // check for easy to optimize case. If the projection that our node is based on is really the 'group' argument from
                // the query.GroupBy(xxx, (key, group) => yyy) method then whatever expression we return here will automatically
                // become part of the select expression that has the group-by clause, so just return the simple node expression.
                if (projection == _currentGroupElement)
                {
                    return aggExpr;
                }
                return new AggregateSubqueryExpression(info.Alias, aggExpr, subquery);
            }
            return subquery;
        }

        private Expression BindGroupBy(Expression source, LambdaExpression keySelector, LambdaExpression elementSelector, LambdaExpression resultSelector)
        {
            ProjectionExpression projection = VisitSequence(source);

            _map[keySelector.Parameters[0]] = projection.Projector;
            Expression keyExpr = Visit(keySelector.Body);

            Expression elemExpr = projection.Projector;
            if (elementSelector != null)
            {
                _map[elementSelector.Parameters[0]] = projection.Projector;
                elemExpr = Visit(elementSelector.Body);
            }

            // Use ProjectColumns to get group-by expressions from key expression
            ProjectedColumns keyProjection = ProjectColumns(keyExpr, projection.Source.Alias, projection.Source.Alias);
            IEnumerable<Expression> groupExprs = keyProjection.Columns.Select(c => c.Expression);

            // make duplicate of source query as basis of element subquery by visiting the source again
            ProjectionExpression subqueryBasis = VisitSequence(source);

            // recompute key columns for group expressions relative to subquery (need these for doing the correlation predicate)
            _map[keySelector.Parameters[0]] = subqueryBasis.Projector;
            Expression subqueryKey = Visit(keySelector.Body);

            // use same projection trick to get group-by expressions based on subquery
            ProjectedColumns subqueryKeyPC = ProjectColumns(subqueryKey, subqueryBasis.Source.Alias, subqueryBasis.Source.Alias);
            IEnumerable<Expression> subqueryGroupExprs = subqueryKeyPC.Columns.Select(c => c.Expression);
            Expression subqueryCorrelation = BuildPredicateWithNullsEqual(subqueryGroupExprs, groupExprs);

            // compute element based on duplicated subquery
            Expression subqueryElemExpr = subqueryBasis.Projector;
            if (elementSelector != null)
            {
                _map[elementSelector.Parameters[0]] = subqueryBasis.Projector;
                subqueryElemExpr = Visit(elementSelector.Body);
            }

            // build subquery that projects the desired element
            var elementAlias = GetNextAlias();
            ProjectedColumns elementPC = ProjectColumns(subqueryElemExpr, elementAlias, subqueryBasis.Source.Alias);
            ProjectionExpression elementSubquery =
                new ProjectionExpression(
                    new SelectExpression(elementAlias, elementPC.Columns, subqueryBasis.Source, subqueryCorrelation),
                    elementPC.Projector
                    );
            var alias = GetNextAlias();

            // make it possible to tie _aggregates back to this group-by
            GroupByInfo info = new GroupByInfo(alias, elemExpr);
            _groupByMap.Add(elementSubquery, info);

            Expression resultExpr;
            if (resultSelector != null)
            {
                Expression saveGroupElement = _currentGroupElement;
                _currentGroupElement = elementSubquery;
                // compute result expression based on key & element-subquery
                _map[resultSelector.Parameters[0]] = keyProjection.Projector;
                _map[resultSelector.Parameters[1]] = elementSubquery;
                resultExpr = Visit(resultSelector.Body);
                _currentGroupElement = saveGroupElement;
            }
            else
            {
                // result must be IGrouping<K,E>
                resultExpr = Expression.New(
                    typeof(Grouping<,>).MakeGenericType(keyExpr.Type, subqueryElemExpr.Type).GetConstructors()[0],
                    new Expression[] { keyExpr, elementSubquery }
                    );
            }

            ProjectedColumns pc = ProjectColumns(resultExpr, alias, projection.Source.Alias);

            // make it possible to tie _aggregates back to this group-by
            Expression projectedElementSubquery = ((NewExpression)pc.Projector).Arguments[1];
            _groupByMap.Add(projectedElementSubquery, info);

            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Source, null, null, groupExprs),
                pc.Projector
                );
        }

        private Expression BindOrderBy(Type resultType, Expression source, LambdaExpression orderSelector, 
            OrderByDirection orderType)
        {
            List<OrderByExpression> myThenBys = _thenBys;
            _thenBys = null;
            ProjectionExpression projection = VisitSequence(source);

            _map[orderSelector.Parameters[0]] = projection.Projector;
            List<OrderByExpression> orderings = new List<OrderByExpression>();
            orderings.Add(new OrderByExpression(Visit(orderSelector.Body), orderType));

            if (myThenBys != null)
            {
                for (Int32 i = myThenBys.Count - 1; i >= 0; i--)
                {
                    OrderByExpression tb = myThenBys[i];
                    LambdaExpression lambda = (LambdaExpression)tb.Expression;
                    _map[lambda.Parameters[0]] = projection.Projector;
                    orderings.Add(new OrderByExpression(Visit(lambda.Body), tb.Direction));
                }
            }

            var alias = GetNextAlias();
            ProjectedColumns pc = ProjectColumns(projection.Projector, alias, projection.Source.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Source, null, orderings.AsReadOnly(), null),
                pc.Projector
                );
        }

        private Expression BindSelect(Type resultType, Expression source, LambdaExpression selector)
        {
            ProjectionExpression projection = VisitSequence(source);
            _map[selector.Parameters[0]] = projection.Projector;
            Expression expression = Visit(selector.Body);
            var alias = GetNextAlias();
            ProjectedColumns pc = ProjectColumns(expression, alias, projection.Source.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Source, null),
                pc.Projector);
        }

        private Expression BindTake(Expression source, Expression take)
        {
            ProjectionExpression projection = VisitSequence(source);
            take = Visit(take);
            SelectExpression select = projection.Source;
            var alias = GetNextAlias();
            ProjectedColumns pc = ProjectColumns(projection.Projector, alias, projection.Source.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Source, null, null, null, take),
                pc.Projector
                );
        }

        private Expression BindThenBy(Expression source, LambdaExpression orderSelector, OrderByDirection orderType)
        {
            if (_thenBys == null)
            {
                _thenBys = new List<OrderByExpression>();
            }
            _thenBys.Add(new OrderByExpression(orderSelector, orderType));
            return Visit(source);
        }



        private Expression BindWhere(Type resultType, Expression source, LambdaExpression predicate)
        {
            ProjectionExpression projection = (ProjectionExpression)Visit(source);
            _map[predicate.Parameters[0]] = projection.Projector;
            Expression where = Visit(predicate.Body);
            var alias = GetNextAlias();
            ProjectedColumns pc = ProjectColumns(projection.Projector, alias, projection.Source.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Source, where),
                pc.Projector);
        }

        private Expression BuildPredicateWithNullsEqual(IEnumerable<Expression> source1, IEnumerable<Expression> source2)
        {
            IEnumerator<Expression> en1 = source1.GetEnumerator();
            IEnumerator<Expression> en2 = source2.GetEnumerator();
            Expression result = null;
            while (en1.MoveNext() && en2.MoveNext())
            {
                Expression compare =
                    Expression.Or(
                        Expression.And(new IsNullExpression(en1.Current), new IsNullExpression(en2.Current)),
                        Expression.Equal(en1.Current, en2.Current)
                        );
                result = (result == null) ? compare : Expression.And(result, compare);
            }
            return result;
        }

        private Boolean CanBeColumn(Expression node)
        {
            switch (node.NodeType)
            {
                case (ExpressionType)PigExpressionType.Column:
                    return true;
                default:
                    return false;
            }
        }

        private ProjectionExpression ConvertToSequence(Expression node)
        {
            switch (node.NodeType)
            {
                case (ExpressionType)PigExpressionType.Projection:
                    return (ProjectionExpression)node;
                case ExpressionType.New:
                    NewExpression nex = (NewExpression)node;
                    if (node.Type.IsGenericType)
                    {
                        return (ProjectionExpression)nex.Arguments[1];
                    }
                    goto default;
                default:
                    throw new Exception(string.Format("The expression of type '{0}' is not a sequence", node.Type));
            }
        }

        private String GetColumnName(MemberInfo member)
        {
            return member.Name;
        }

        private Type GetColumnType(MemberInfo member)
        {
            FieldInfo fi = member as FieldInfo;
            if (fi != null)
            {
                return fi.FieldType;
            }
            PropertyInfo pi = (PropertyInfo)member;
            return pi.PropertyType;
        }

        private IEnumerable<MemberInfo> GetMappedMembers(Type rowType)
        {
            return rowType.GetProperties().Cast<MemberInfo>();
        }

        private SourceAlias GetNextAlias()
        {
            return new SourceAlias();
        }

        private String GetTableName(Type rowType)
        {
            return rowType.Name;
        }

        private ProjectionExpression GetTableProjection(Type rowType)
        {
            var sourceAlias = GetNextAlias();
            var selectAlias = GetNextAlias();
            List<MemberBinding> bindings = new List<MemberBinding>();
            List<ColumnDeclaration> columns = new List<ColumnDeclaration>();
            foreach (MemberInfo mi in GetMappedMembers(rowType))
            {
                String columnName = GetColumnName(mi);
                Type columnType = GetColumnType(mi);
                bindings.Add(Expression.Bind(mi, new ColumnExpression(columnType, selectAlias, columnName)));
                columns.Add(new ColumnDeclaration(columnName, new ColumnExpression(columnType, sourceAlias, columnName)));
            }

            Expression projector = Expression.MemberInit(Expression.New(rowType), bindings);
            Type resultType = typeof(IEnumerable<>).MakeGenericType(rowType);
            return new ProjectionExpression(
                new SelectExpression(
                    selectAlias,
                    columns,
                    new SourceExpression(sourceAlias, GetTableName(rowType)),
                    null),
                projector);
        }

        private Boolean HasPredicateArg(String aggregateType)
        {
            return aggregateType == "Count";
        }

        private Boolean IsTable(Expression expression)
        {
            return expression.Type.IsGenericType && expression.Type.GetGenericTypeDefinition() == typeof(Query<>);
        }

        private Expression MakeMember(Expression source, MemberInfo mi)
        {
            FieldInfo fi = mi as FieldInfo;
            if (fi != null)
            {
                return Expression.Field(source, fi);
            }
            PropertyInfo pi = (PropertyInfo)mi;
            return Expression.Property(source, pi);
        }

        private Boolean MembersMatch(MemberInfo a, MemberInfo b)
        {
            if (a == b)
            {
                return true;
            }
            if (a is MethodInfo && b is PropertyInfo)
            {
                return a == ((PropertyInfo)b).GetGetMethod();
            }
            else if (a is PropertyInfo && b is MethodInfo)
            {
                return ((PropertyInfo)a).GetGetMethod() == b;
            }
            return false;
        }

        private ProjectedColumns ProjectColumns(Expression expression, SourceAlias newAlias, params SourceAlias[] existingAliases)
        {
            return Projector.ProjectColumns(CanBeColumn, expression, newAlias, existingAliases);
        }

        private ProjectionExpression VisitSequence(Expression source)
        {
            // sure to call base.Visit in order to skip my override
            return ConvertToSequence(base.Visit(source));
        }

        // private static methods
        private static LambdaExpression GetLambda(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            if (e.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)e).Value as LambdaExpression;
            }
            return e as LambdaExpression;
        }
    }
}
