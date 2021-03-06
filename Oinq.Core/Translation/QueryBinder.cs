﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Oinq.Expressions;
using ExpressionVisitor = Oinq.Expressions.ExpressionVisitor;

namespace Oinq.Translation
{
    /// <summary>
    /// QueryBinder is a visitor that converts method calls to LINQ operations into
    /// custom PigExpression nodes and references to class members into references to _columns.
    /// </summary>
    internal class QueryBinder : ExpressionVisitor
    {
        // private fields
        private readonly Dictionary<Expression, GroupByInfo> _groupByMap;
        private readonly Dictionary<ParameterExpression, Expression> _map;
        private readonly Expression _root;
        private Expression _currentGroupElement;
        private List<OrderByExpression> _thenBys;

        // constructors
        private QueryBinder(Expression root)
        {
            _map = new Dictionary<ParameterExpression, Expression>();
            _groupByMap = new Dictionary<Expression, GroupByInfo>();
            _root = root;
        }

        // internal static methods
        internal static Expression Bind(IQueryProvider provider, Expression node)
        {
            return new QueryBinder(node).Visit(node);
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
                    var min = (MemberInitExpression) source;
                    for (Int32 i = 0, n = min.Bindings.Count; i < n; i++)
                    {
                        var assign = min.Bindings[i] as MemberAssignment;
                        if (assign != null && MembersMatch(assign.Member, node.Member))
                        {
                            return assign.Expression;
                        }
                    }
                    break;
                case ExpressionType.New:
                    var nex = (NewExpression) source;
                    // ReSharper thinks that nex.Members can't be null, so it thinks that the "else"
                    // can't be reached.  That isn't true.
                    if (nex.Members != null)
                    {
                        for (Int32 i = 0, n = nex.Members.Count; i < n; i++)
                        {
                            if (MembersMatch(nex.Members[i], node.Member))
                            {
                                return nex.Arguments[i];
                            }
                        }
                    }
                    else
                    {
                        return nex.Arguments[1];
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
            if (node.Method.DeclaringType == typeof (Queryable) || node.Method.DeclaringType == typeof (Enumerable))
            {
                switch (node.Method.Name)
                {
                    case "Where":
                        return BindWhere(node.Arguments[0], GetLambda(node.Arguments[1]));
                    case "Select":
                        return BindSelect(node.Arguments[0], GetLambda(node.Arguments[1]));
                    case "Join":
                        return BindJoin(
                            node.Type, node.Arguments[0], node.Arguments[1],
                            GetLambda(node.Arguments[2]), GetLambda(node.Arguments[3]), GetLambda(node.Arguments[4]));
                    case "OrderBy":
                        return BindOrderBy(node.Arguments[0], GetLambda(node.Arguments[1]),
                                           OrderByDirection.Ascending);
                    case "OrderByDescending":
                        return BindOrderBy(node.Arguments[0], GetLambda(node.Arguments[1]),
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
                        if (node.Arguments.Count == 3)
                        {
                            LambdaExpression lambda1 = GetLambda(node.Arguments[1]);
                            LambdaExpression lambda2 = GetLambda(node.Arguments[2]);
                            if (lambda2.Parameters.Count == 1)
                            {
                                // second lambda is element selector
                                return BindGroupBy(node.Arguments[0], lambda1, lambda2, null);
                            }
                            if (lambda2.Parameters.Count == 2)
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
                    case "First":
                    case "FirstOrDefault":
                        {
                            return BindTake(node.Arguments[0], Expression.Constant(1));
                        }
                    case "Count":
                    case "Min":
                    case "Max":
                    case "Sum":
                    case "Average":
                        if (node.Arguments.Count == 1)
                        {
                            return BindAggregate(node.Arguments[0], node.Method.Name, node.Method, null, node == _root);
                        }
                        if (node.Arguments.Count == 2)
                        {
                            LambdaExpression selector = GetLambda(node.Arguments[1]);
                            return BindAggregate(node.Arguments[0], node.Method.Name, node.Method, selector,
                                                 node == _root);
                        }
                        break;
                }
                throw new NotSupportedException(String.Format("The method '{0}' is not supported", node.Method.Name));
            }
            // custom extensions
            var extensions = node.Method.GetCustomAttributes(typeof(PigExtensionAttribute), true);
            if (extensions.Length == 0 && node.Method.DeclaringType != null)
            {
                extensions = node.Method.DeclaringType.GetCustomAttributes(typeof(PigExtensionAttribute), true);
            }
            if (extensions.Length > 0)
            {
                MethodInfo method = node.Method;
                MethodInfo mi = ((PigExtensionAttribute)extensions[0]).BinderType.GetMethod(String.Format("Bind{0}", method.Name));
                var exp = (LambdaExpression) mi.Invoke(null, node.Arguments.ToArray<Object>());
                return Visit(exp.Body);
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
        private Expression BindAggregate(Expression source, String aggName, MethodInfo method, LambdaExpression argument,
                                         Boolean isRoot)
        {
            Type returnType = method.ReturnType;
            Boolean hasPredicateArg = HasPredicateArg(aggName);
            Boolean isDistinct = false;
            Boolean argumentWasPredicate = false;

            // check for distinct
            var mcs = source as MethodCallExpression;
            if (mcs != null && !hasPredicateArg && argument == null)
            {
                if (mcs.Method.Name == "Distinct" && mcs.Arguments.Count == 1 &&
                    (mcs.Method.DeclaringType == typeof (Queryable) || mcs.Method.DeclaringType == typeof (Enumerable)))
                {
                    source = mcs.Arguments[0];
                    isDistinct = true;
                }
            }

            if (argument != null && hasPredicateArg)
            {
                // convert query.Count(predicate) into query.Where(predicate).Count()
                source = Expression.Call(typeof (Queryable), "Where", method.GetGenericArguments(), source, argument);
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

            SourceAlias alias = GetNextAlias();
            Expression aggExpr = new AggregateExpression(returnType, aggName, argExpr, isDistinct);
            var select = new SelectExpression(alias, new[] {new ColumnDeclaration("", aggExpr)}, projection.Source, null);

            if (isRoot)
            {
                ParameterExpression p = Expression.Parameter(typeof (IEnumerable<>).MakeGenericType(aggExpr.Type),
                                                             "node");
                LambdaExpression gator =
                    Expression.Lambda(Expression.Call(typeof (Enumerable), "Single", new[] {returnType}, p), p);
                return new ProjectionExpression(select, new ColumnExpression(returnType, alias, ""), gator);
            }

            var subquery = new ScalarExpression(returnType, select);

            // if we can find the corresponding group-info we can build a special AggregateSubquery node that will enable us to
            // optimize the node expression later using AggregateRewriter
            GroupByInfo info;
            if (!argumentWasPredicate && _groupByMap.TryGetValue(projection, out info))
            {
                // use the element expression from the group-by info to rebind the argument so the resulting expression is one that
                // would be legal to add to the _columns in the select expression that has the corresponding group-by clause.
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

        private Expression BindGroupBy(Expression source, LambdaExpression keySelector, LambdaExpression elementSelector,
                                       LambdaExpression resultSelector)
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

            // recompute key _columns for group expressions relative to subquery (need these for doing the correlation predicate)
            _map[keySelector.Parameters[0]] = subqueryBasis.Projector;
            Expression subqueryKey = Visit(keySelector.Body);

            // use same projection trick to get group-by expressions based on subquery
            var subqueryKeyPc = ProjectColumns(subqueryKey, subqueryBasis.Source.Alias,
                                                            subqueryBasis.Source.Alias);
            IEnumerable<Expression> subqueryGroupExprs = subqueryKeyPc.Columns.Select(c => c.Expression);
            Expression subqueryCorrelation = BuildPredicateWithNullsEqual(subqueryGroupExprs, groupExprs);

            // compute element based on duplicated subquery
            Expression subqueryElemExpr = subqueryBasis.Projector;
            if (elementSelector != null)
            {
                _map[elementSelector.Parameters[0]] = subqueryBasis.Projector;
                subqueryElemExpr = Visit(elementSelector.Body);
            }

            // build subquery that projects the desired element
            SourceAlias elementAlias = GetNextAlias();
            ProjectedColumns elementPc = ProjectColumns(subqueryElemExpr, elementAlias, subqueryBasis.Source.Alias);
            var elementSubquery =
                new ProjectionExpression(
                    new SelectExpression(elementAlias, elementPc.Columns, subqueryBasis.Source, subqueryCorrelation),
                    elementPc.Projector
                    );
            SourceAlias alias = GetNextAlias();

            // make it possible to tie _aggregates back to this group-by
            var info = new GroupByInfo(alias, elemExpr);
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
                    typeof (Grouping<,>).MakeGenericType(keyExpr.Type, subqueryElemExpr.Type).GetConstructors()[0],
                    new[] {keyExpr, elementSubquery}
                    );
            }

            ProjectedColumns pc = ProjectColumns(resultExpr, alias, projection.Source.Alias);

            // make it possible to tie _aggregates back to this group-by
            //Expression projectedElementSubquery = ((NewExpression)pc.Projector).Arguments[1];
            //_groupByMap.Add(projectedElementSubquery, info);

            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Source, null, null, groupExprs),
                pc.Projector
                );
        }

        private Expression BindJoin(Type resultType, Expression outerSource, Expression innerSource,
                                    LambdaExpression outerKey, LambdaExpression innerKey,
                                    LambdaExpression resultSelector)
        {
            ProjectionExpression outerProjection = VisitSequence(outerSource);
            ProjectionExpression innerProjection = VisitSequence(innerSource);
            _map[outerKey.Parameters[0]] = outerProjection.Projector;
            Expression outerKeyExpr = Visit(outerKey.Body);
            _map[innerKey.Parameters[0]] = innerProjection.Projector;
            Expression innerKeyExpr = Visit(innerKey.Body);
            _map[resultSelector.Parameters[0]] = outerProjection.Projector;
            _map[resultSelector.Parameters[1]] = innerProjection.Projector;
            Expression resultExpr = Visit(resultSelector.Body);
            var join = new JoinExpression(resultType, outerProjection.Source, innerProjection.Source,
                                          Expression.Equal(outerKeyExpr, innerKeyExpr));
            SourceAlias alias = GetNextAlias();
            ProjectedColumns pc = ProjectColumns(resultExpr, alias, outerProjection.Source.Alias,
                                                 innerProjection.Source.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, join, null),
                pc.Projector
                );
        }

        private Expression BindOrderBy(Expression source, LambdaExpression orderSelector,
                                       OrderByDirection orderType)
        {
            List<OrderByExpression> myThenBys = _thenBys;
            _thenBys = null;
            ProjectionExpression projection = VisitSequence(source);

            _map[orderSelector.Parameters[0]] = projection.Projector;
            var orderings = new List<OrderByExpression>();
            orderings.Add(new OrderByExpression(orderSelector.Body, orderType));

            if (myThenBys != null)
            {
                for (Int32 i = myThenBys.Count - 1; i >= 0; i--)
                {
                    OrderByExpression tb = myThenBys[i];
                    var lambda = (LambdaExpression) tb.Expression;
                    _map[lambda.Parameters[0]] = projection.Projector;
                    orderings.Add(new OrderByExpression(Visit(lambda.Body), tb.Direction));
                }
            }

            SourceAlias alias = GetNextAlias();
            ProjectedColumns pc = ProjectColumns(projection.Projector, alias, projection.Source.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Source, null, orderings.AsReadOnly(), null),
                pc.Projector
                );
        }

        private Expression BindSelect(Expression source, LambdaExpression selector)
        {
            ProjectionExpression projection = VisitSequence(source);
            _map[selector.Parameters[0]] = projection.Projector;
            Expression expression = Visit(selector.Body);
            SourceAlias alias = GetNextAlias();
            ProjectedColumns pc = ProjectColumns(expression, alias, projection.Source.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Source, null),
                pc.Projector);
        }

        private Expression BindTake(Expression source, Expression take)
        {
            ProjectionExpression projection = VisitSequence(source);
            take = Visit(take);
            SourceAlias alias = GetNextAlias();
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

        private Expression BindWhere(Expression source, LambdaExpression predicate)
        {
            var projection = (ProjectionExpression) Visit(source);
            _map[predicate.Parameters[0]] = projection.Projector;
            Expression where = Visit(predicate.Body);
            SourceAlias alias = GetNextAlias();
            ProjectedColumns pc = ProjectColumns(projection.Projector, alias, projection.Source.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Source, where),
                pc.Projector);
        }

        private static Expression BuildPredicateWithNullsEqual(IEnumerable<Expression> source1,
                                                               IEnumerable<Expression> source2)
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
                case (ExpressionType) PigExpressionType.Column:
                    return true;
                default:
                    return false;
            }
        }

        private static ProjectionExpression ConvertToSequence(Expression node)
        {
            switch (node.NodeType)
            {
                case (ExpressionType) PigExpressionType.Projection:
                    return (ProjectionExpression) node;
                case ExpressionType.New:
                    var nex = (NewExpression) node;
                    if (node.Type.IsGenericType)
                    {
                        return (ProjectionExpression) nex.Arguments[1];
                    }
                    goto default;
                default:
                    throw new Exception(String.Format("The expression of type '{0}' is not a sequence", node.Type));
            }
        }

        private static String GetColumnName(MemberInfo member)
        {
            return member.Name;
        }

        private static Type GetColumnType(MemberInfo member)
        {
            var fi = member as FieldInfo;
            if (fi != null)
            {
                return fi.FieldType;
            }
            var pi = (PropertyInfo) member;
            return pi.PropertyType;
        }

        private static IEnumerable<MemberInfo> GetMappedMembers(Type rowType)
        {
            return rowType.GetProperties().Where(pi => pi.CanWrite);
        }

        private static SourceAlias GetNextAlias()
        {
            return new SourceAlias();
        }

        private static String GetTableName(Type rowType)
        {
            return rowType.Name;
        }

        private static ProjectionExpression GetTableProjection(Type rowType)
        {
            SourceAlias sourceAlias = GetNextAlias();
            SourceAlias selectAlias = GetNextAlias();
            var bindings = new List<MemberBinding>();
            var columns = new List<ColumnDeclaration>();
            foreach (MemberInfo mi in GetMappedMembers(rowType))
            {
                String columnName = GetColumnName(mi);
                Type columnType = GetColumnType(mi);
                bindings.Add(Expression.Bind(mi, new ColumnExpression(columnType, selectAlias, columnName)));
                columns.Add(new ColumnDeclaration(columnName, new ColumnExpression(columnType, sourceAlias, columnName)));
            }

            Expression projector = Expression.MemberInit(Expression.New(rowType), bindings);
            return new ProjectionExpression(
                new SelectExpression(
                    selectAlias,
                    columns,
                    new SourceExpression(rowType, sourceAlias, GetTableName(rowType)),
                    null),
                projector);
        }

        private static Boolean HasPredicateArg(String aggregateType)
        {
            return aggregateType == "Count";
        }

        private static Boolean IsTable(Expression expression)
        {
            return expression.Type.IsGenericType && expression.Type.GetGenericTypeDefinition() == typeof (Query<>);
        }

        private static Expression MakeMember(Expression source, MemberInfo mi)
        {
            var fi = mi as FieldInfo;
            if (fi != null)
            {
                return Expression.Field(source, fi);
            }
            var pi = (PropertyInfo) mi;
            return Expression.Property(source, pi);
        }

        private static Boolean MembersMatch(MemberInfo a, MemberInfo b)
        {
            if (a == b)
            {
                return true;
            }
            if (a is MethodInfo && b is PropertyInfo)
            {
                return a == ((PropertyInfo) b).GetGetMethod();
            }
            if (a is PropertyInfo && b is MethodInfo)
            {
                return ((PropertyInfo) a).GetGetMethod() == b;
            }
            return false;
        }

        private ProjectedColumns ProjectColumns(Expression expression, SourceAlias newAlias,
                                                params SourceAlias[] existingAliases)
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
                e = ((UnaryExpression) e).Operand;
            }
            if (e.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression) e).Value as LambdaExpression;
            }
            return e as LambdaExpression;
        }
    }
}