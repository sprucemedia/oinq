using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;

namespace Oinq.Core
{
    /// <summary>
    /// Formats the query text for consumption by EdgeSpring.
    /// </summary>
    internal class QueryFormatter : PigExpressionVisitor
    {
        // private fields
        private StringBuilder _sb;
        private Int32 _aliasCount;
        private Int32 _levelCount;

        // constructors
        private QueryFormatter()
        {
            _sb = new StringBuilder();
        }

        // internal methods
        internal static String Format(Expression expression)
        {
            QueryFormatter formatter = new QueryFormatter();
            formatter.Visit(expression);
            return formatter._sb.ToString();
        }

        // protected methods
        protected virtual void AddAlias()
        {
            _aliasCount += 1;
        }

        protected virtual String GetLastAliasName()
        {
            return "t" + (_aliasCount - 1);
        }
        
        protected virtual String GetNextAliasName()
        {
            return "t" + _aliasCount;
        }

        protected virtual string GetOperator(String methodName)
        {
            switch (methodName)
            {
                case "Add": return "+";
                case "Subtract": return "-";
                case "Multiply": return "*";
                case "Divide": return "/";
                case "Negate": return "-";
                case "Remainder": return "%";
                default: return null;
            }
        }

        protected virtual string GetOperator(BinaryExpression b)
        {
            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return (IsBoolean(b.Left.Type)) ? "AND" : "&";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return (IsBoolean(b.Left.Type) ? "OR" : "|");
                case ExpressionType.Equal:
                    return "==";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.ExclusiveOr:
                    return "^";
                case ExpressionType.LeftShift:
                    return "<<";
                case ExpressionType.RightShift:
                    return ">>";
                default:
                    return "";
            }
        }

        protected virtual Boolean IsBoolean(Type type)
        {
            return type == typeof(Boolean) || type == typeof(Boolean?);
        }

        protected virtual Boolean IsInteger(Type type)
        {
            return TypeHelper.IsInteger(type);
        }

        protected virtual Boolean IsPredicate(Expression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return IsBoolean(((BinaryExpression)expr).Type);
                case ExpressionType.Not:
                    return IsBoolean(((UnaryExpression)expr).Type);
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case (ExpressionType)PigExpressionType.IsNull:
                    return true;
                case ExpressionType.Call:
                    return IsBoolean(((MethodCallExpression)expr).Type);
                default:
                    return false;
            }
        }

        protected virtual Boolean RequiresAsteriskWhenNoArgument(String aggregateName)
        {
            return aggregateName == "Count";
        }

        protected virtual Expression VisitPredicate(Expression node)
        {
            Visit(node);
            if (!IsPredicate(node))
            {
                Write(" <> 0");
            }
            return node;
        }

        protected virtual Expression VisitValue(Expression node)
        {
            return Visit(node);
        }

        protected void Write(Object value)
        {
            _sb.Append(value);
        }

        protected virtual void WriteAggregateName(String aggregateName)
        {
            switch (aggregateName)
            {
                case "Average":
                    this.Write("AVG");
                    break;
                default:
                    this.Write(aggregateName.ToLower());
                    break;
            }
        }

        protected void WriteAliasName(String aliasName)
        {
            Write(aliasName);
        }

        protected void WriteAsColumnName(String columnName)
        {
            Write("AS ");
            Write(columnName);
        }

        protected void WriteAsAliasName(String aliasName)
        {
            Write("AS ");
            WriteAliasName(aliasName);
        }

        protected void WriteColumnName(String columnName)
        {
            Write(columnName);
        }

        protected virtual void WriteColumns(ReadOnlyCollection<ColumnDeclaration> columns)
        {
            if (columns.Count > 0)
            {
                for (Int32 i = 0, n = columns.Count; i < n; i++)
                {
                    ColumnDeclaration column = columns[i];
                    if (i > 0)
                    {
                        Write(", ");
                    }
                    ColumnExpression c = VisitValue(column.Expression) as ColumnExpression;
                    if (!String.IsNullOrEmpty(column.Name))
                    {
                        this.Write(" ");
                        this.WriteAsColumnName(column.Name);
                    }
                }
            }
        }

        protected void WriteSourceName(String sourceName)
        {
            Write(sourceName);
        }

        protected virtual void WriteValue(Object value)
        {
            if (value == null)
            {
                this.Write("NULL");
            }
            else if (value.GetType().IsEnum)
            {
                this.Write(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())));
            }
            else
            {
                switch (Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.Boolean:
                        this.Write(((Boolean)value) ? 1 : 0);
                        break;
                    case TypeCode.String:
                        this.Write("'");
                        this.Write(value);
                        this.Write("'");
                        break;
                    case TypeCode.Object:
                        throw new NotSupportedException(String.Format("The constant for '{0}' is not supported", value));
                    case TypeCode.Single:
                    case TypeCode.Double:
                        String str = value.ToString();
                        if (!str.Contains("."))
                        {
                            str += ".0";
                        }
                        this.Write(str);
                        break;
                    default:
                        this.Write(value);
                        break;
                }
            }
        }

        // protected override methods
        protected override Expression Visit(Expression node)
        {
            if (node == null) return null;

            // check for supported node types first 
            // non-supported ones should not be visited (as they would produce bad Pig)
            switch (node.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.UnaryPlus:
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.Power:
                case ExpressionType.Conditional:
                case ExpressionType.Constant:
                case ExpressionType.MemberAccess:
                case ExpressionType.Call:
                case ExpressionType.New:
                case (ExpressionType)PigExpressionType.Source:
                case (ExpressionType)PigExpressionType.Column:
                case (ExpressionType)PigExpressionType.Select:
                case (ExpressionType)PigExpressionType.Join:
                case (ExpressionType)PigExpressionType.Aggregate:
                case (ExpressionType)PigExpressionType.Scalar:
                case (ExpressionType)PigExpressionType.AggregateSubquery:
                case (ExpressionType)PigExpressionType.IsNull:
                case (ExpressionType)PigExpressionType.Projection:
                    return base.Visit(node);

                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                case ExpressionType.ArrayIndex:
                case ExpressionType.TypeIs:
                case ExpressionType.Parameter:
                case ExpressionType.Lambda:
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                case ExpressionType.Invoke:
                case ExpressionType.MemberInit:
                case ExpressionType.ListInit:
                default:
                    throw new NotSupportedException(string.Format("The LINQ expression node of type {0} is not supported", node.NodeType));
            }
        }


        protected override Expression VisitAggregate(AggregateExpression node)
        {
            WriteAggregateName(node.AggregateName);
            Write("(");
            if (node.Argument != null)
            {
                VisitValue(node.Argument);
            }
            else if (RequiresAsteriskWhenNoArgument(node.AggregateName))
            {
                this.Write("*");
            }
            Write(")");
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            String op = GetOperator(node);
            Expression left = node.Left;
            Expression right = node.Right;
            
            Write("(");
            switch (node.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    if (IsBoolean(left.Type))
                    {
                        VisitPredicate(left);
                        Write(" ");
                        Write(op);
                        Write(" ");
                        VisitPredicate(right);
                    }
                    else
                    {
                        VisitValue(left);
                        Write(" ");
                        Write(op);
                        Write(" ");
                        VisitValue(right);
                    }
                    break;
                case ExpressionType.Equal:
                    if (right.NodeType == ExpressionType.Constant)
                    {
                        ConstantExpression ce = (ConstantExpression)right;
                        if (ce.Value == null)
                        {
                            Visit(left);
                            Write(" IS NULL");
                            break;
                        }
                    }
                    else if (right.NodeType == ExpressionType.Constant)
                    {
                        ConstantExpression ce = (ConstantExpression)left;
                        if (ce.Value == null)
                        {
                            Visit(right);
                            Write(" IS NULL");
                            break;
                        }
                    }
                    goto case ExpressionType.LessThan;
                case ExpressionType.NotEqual:
                    if (right.NodeType == ExpressionType.Constant)
                    {
                        ConstantExpression ce = (ConstantExpression)right;
                        if (ce.Value == null)
                        {
                            Visit(left);
                            Write(" IS NOT NULL");
                            break;
                        }
                    }
                    else if (right.NodeType == ExpressionType.Constant)
                    {
                        ConstantExpression ce = (ConstantExpression)left;
                        if (ce.Value == null)
                        {
                            Visit(right);
                            Write(" IS NOT NULL");
                            break;
                        }
                    }
                    goto case ExpressionType.LessThan;
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    // check for special x.CompareTo(y) && type.Compare(x,y)
                    if (left.NodeType == ExpressionType.Call && right.NodeType == ExpressionType.Constant)
                    {
                        MethodCallExpression mc = (MethodCallExpression)left;
                        ConstantExpression ce = (ConstantExpression)right;
                        if (ce.Value != null && ce.Value.GetType() == typeof(Int32) && ((Int32)ce.Value) == 0)
                        {
                            if (mc.Method.Name == "CompareTo" && !mc.Method.IsStatic && mc.Arguments.Count == 1)
                            {
                                left = mc.Object;
                                right = mc.Arguments[0];
                            }
                            else if (
                                (mc.Method.DeclaringType == typeof(String) || mc.Method.DeclaringType == typeof(Decimal))
                                  && mc.Method.Name == "Compare" && mc.Method.IsStatic && mc.Arguments.Count == 2)
                            {
                                left = mc.Arguments[0];
                                right = mc.Arguments[1];
                            }
                        }
                    }
                    goto case ExpressionType.Add;
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.LeftShift:
                case ExpressionType.RightShift:
                    VisitValue(left);
                    Write(" ");
                    Write(op);
                    Write(" ");
                    VisitValue(right);
                    break;
                default:
                    throw new NotSupportedException(String.Format("The binary operator '{0}' is not supported", node.NodeType));
            }
            Write(")");
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            WriteValue(node.Value);
            return node;
        }

        protected override Expression VisitColumn(ColumnExpression node)
        {
            WriteColumnName(node.Name);
            return node;
        }

        protected override Expression VisitIsNull(IsNullExpression node)
        {
            VisitValue(node.Expression);
            Write(" IS NULL");
            return node;
        }

        protected override Expression VisitJoin(JoinExpression join)
        {
            VisitSource(join.Left);
            switch (join.Join)
            {
                case JoinType.CrossJoin:
                    _sb.Append("CROSS JOIN ");
                    break;
                case JoinType.InnerJoin:
                    _sb.Append("INNER JOIN ");
                    break;
                case JoinType.CrossApply:
                    _sb.Append("CROSS APPLY ");
                    break;
            }
            VisitSource(join.Right);
            if (join.Condition != null)
            {
                _sb.Append("ON ");
                Visit(join.Condition);
            }
            return join;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            throw new NotSupportedException(String.Format("The member access '{0}' is not supported", node.Member));
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Decimal))
            {
                switch (node.Method.Name)
                {
                    case "Add":
                    case "Subtract":
                    case "Multiply":
                    case "Divide":
                    case "Remainder":
                        this.Write("(");
                        this.VisitValue(node.Arguments[0]);
                        this.Write(" ");
                        this.Write(GetOperator(node.Method.Name));
                        this.Write(" ");
                        this.VisitValue(node.Arguments[1]);
                        this.Write(")");
                        return node;
                    case "Negate":
                        this.Write("-");
                        this.Visit(node.Arguments[0]);
                        this.Write("");
                        return node;
                    case "Compare":
                        this.Visit(Expression.Condition(
                            Expression.Equal(node.Arguments[0], node.Arguments[1]),
                            Expression.Constant(0),
                            Expression.Condition(
                                Expression.LessThan(node.Arguments[0], node.Arguments[1]),
                                Expression.Constant(-1),
                                Expression.Constant(1)
                                )));
                        return node;
                }
            }
            else if (node.Method.Name == "ToString" && node.Object.Type == typeof(string))
            {
                return this.Visit(node.Object);  // no op
            }
            else if (node.Method.Name == "Equals")
            {
                if (node.Method.IsStatic && node.Method.DeclaringType == typeof(object))
                {
                    this.Write("(");
                    this.Visit(node.Arguments[0]);
                    this.Write(" = ");
                    this.Visit(node.Arguments[1]);
                    this.Write(")");
                    return node;
                }
                else if (!node.Method.IsStatic && node.Arguments.Count == 1 && node.Arguments[0].Type == node.Object.Type)
                {
                    this.Write("(");
                    this.Visit(node.Object);
                    this.Write(" = ");
                    this.Visit(node.Arguments[0]);
                    this.Write(")");
                    return node;
                }
            }
            throw new NotSupportedException(String.Format("The method '{0}' is not supported", node.Method.Name));
        }

        protected override NewExpression VisitNew(NewExpression node)
        {    
            throw new NotSupportedException(String.Format("The construtor for '{0}' is not supported", node.Constructor.DeclaringType));
        }

        protected override Expression VisitProjection(ProjectionExpression node)
        {
            // treat these like scalar subqueries
            if (node.Projector is ColumnExpression)
            {
                Write("(");
                Visit(node.Source);
                this.Write(")");
            }
            else
            {
                throw new NotSupportedException("Non-scalar projections cannot be translated to Pig.");
            }
            return node;
        }

        protected override Expression VisitScalar(ScalarExpression node)
        {
            Write("(");
            Visit(node.Select);
            Write(")");
            return node;
        }
        protected override Expression VisitSelect(SelectExpression node)
        {
            // from
            VisitSource(node.From);
            
            if ((PigExpressionType)node.From.NodeType == PigExpressionType.Select || _levelCount == 0)
            {
                if (node.Where != null)
                {
                    Write(String.Format("{0} = FILTER {1} BY ", GetNextAliasName(), GetLastAliasName()));
                    VisitPredicate(node.Where);
                    Write("; ");
                    AddAlias();
                }

                if (node.GroupBy != null && node.GroupBy.Count > 0)
                {
                    _sb.Append(String.Format("{0} = GROUP {1} BY ", GetNextAliasName(), GetLastAliasName()));
                    for (Int32 i = 0, n = node.GroupBy.Count; i < n; i++)
                    {
                        if (i > 0)
                        {
                            _sb.Append(", ");
                        }
                        Visit(node.GroupBy[i]);
                    }
                    _sb.Append("; ");
                    AddAlias();
                }

                if (_levelCount == 0)
                {
                    Write(String.Format("{0} = FOREACH {1} GENERATE ", GetNextAliasName(), GetLastAliasName()));
                    WriteColumns(node.Columns);
                    Write("; ");
                }
            }

            return node;
        }

        protected override Expression VisitSource(Expression node)
        {
            switch ((PigExpressionType)node.NodeType)
            {
                case PigExpressionType.Source:
                    SourceExpression source = (SourceExpression)node;
                    Write(String.Format("{0} = LOAD '{1}'; ", GetNextAliasName(), source.Name));
                    AddAlias();
                    break;
                case PigExpressionType.Select:
                    SelectExpression select = (SelectExpression)node;
                    _levelCount += 1;
                    Visit(select);
                    _levelCount -= 1;
                    break;
                default:
                    throw new InvalidOperationException("Select source is not valid type");
            }
            return node;
        }

        protected override Expression VisitSubquery(SubqueryExpression node)
        {
            Visit(node.Select.Columns[0].Expression);
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    _sb.Append(" NOT ");
                    Visit(node.Operand);
                    break;
                default:
                    throw new NotSupportedException(String.Format("The unary operator '{0}' is not supported", node.NodeType));
            }
            return node;
        }

        // private methods
        private SourceExpression GetSourceExpression(Expression node)
        {
            var loadExpression = node as SourceExpression;
            if (loadExpression != null)
            {
                return loadExpression;
            }

            var selectExpression = node as SelectExpression;
            if (selectExpression != null && selectExpression.From != null)
            {
                return GetSourceExpression(selectExpression.From);
            }

            throw new ArgumentOutOfRangeException(String.Format("Unable to find source of expression: {0}.", node.ToString()));
        }

        private Expression GetWhereExpression(Expression node)
        {
            var expression = node as SelectExpression;
            if (expression != null && expression.Where != null)
            {
                return expression;
            }
            if (expression != null && expression.From != null)
            {
                return GetWhereExpression(expression.From);
            }
            return null;
        }     
    }
}
