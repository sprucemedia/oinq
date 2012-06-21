using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Oinq
{
    /// <summary>
    /// Formats the query text for consumption by Pig.
    /// </summary>
    internal class PigFormatter : PigExpressionVisitor
    {
        // private fields
        private StringBuilder _sb;
        private Int32 _aliasCount;
        private Dictionary<String, String> _mappings;
        private List<String> _ignores;

        // constructors
        private PigFormatter()
        {
            _sb = new StringBuilder();
        }

        // internal methods
        internal static String Format(TranslatedQuery query)
        {
            SelectQuery selectQuery = query as SelectQuery;
            if (selectQuery == null)
            {
                throw new ArgumentException("The query parameter must be assignable to SelectQuery.", "query");
            }

            PigFormatter formatter = new PigFormatter();
            formatter.SetAttributes(query.SourceType);
            formatter.WriteLoad(query.Source);
            foreach (SelectExpression ex in selectQuery.CommandStack)
            {
                if (ex.Where != null)
                {
                    formatter.WriteFilter(selectQuery.Where);
                }
                if (ex.GroupBy != null)
                {
                    formatter.WriteGroupBy(selectQuery.GroupBy);
                }
                if (ex.OrderBy != null)
                {
                    formatter.WriteOrderBy(selectQuery.OrderBy);
                }
            }
            formatter.WriteGenerate(selectQuery.Columns);
            formatter.WriteOutput(selectQuery.Take, formatter.GetLastAliasName());
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

        protected virtual String GetOperator(String methodName)
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

        protected virtual String GetOperator(BinaryExpression b)
        {
            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return (IsBoolean(b.Left.Type)) ? " AND " : " & ";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return (IsBoolean(b.Left.Type)) ? " OR " : " | ";
                case ExpressionType.Equal:
                    return " == ";
                case ExpressionType.NotEqual:
                    return " != ";
                case ExpressionType.LessThan:
                    return " < ";
                case ExpressionType.LessThanOrEqual:
                    return " <= ";
                case ExpressionType.GreaterThan:
                    return " > ";
                case ExpressionType.GreaterThanOrEqual:
                    return " >= ";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return " + ";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return " - ";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return " * ";
                case ExpressionType.Divide:
                    return " / ";
                case ExpressionType.Modulo:
                    return " % ";
                default:
                    return "";
            }
        }

        protected virtual Boolean IsBoolean(Type type)
        {
            return type == typeof(Boolean) || type == typeof(Boolean?);
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

        protected override Expression VisitColumn(ColumnExpression node)
        {
            WriteColumnName(node.Name);
            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Write(node.Name);
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
                    Write("AVG");
                    break;
                default:
                    Write(aggregateName.ToLower());
                    break;
            }
        }

        protected void WriteAsColumnName(String columnName)
        {
            Write(" AS ");
            Write(columnName);
        }

        protected virtual void WriteColumns(ReadOnlyCollection<ColumnDeclaration> columns)
        {
            if (columns.Count > 0)
            {
                for (Int32 i = 0, n = columns.Count; i < n; i++)
                {
                    ColumnDeclaration column = columns[i];
                    if (!_ignores.Contains(column.Name))
                    {
                        if (i > 0)
                        {
                            Write(", ");
                        }
                        VisitValue(column.Expression);
                        if (!String.IsNullOrEmpty(column.Name))
                        {
                            WriteAsColumnName(column.Name);
                        }
                    }
                }
            }
        }

        protected void WriteColumnName(String columnName)
        {
            String mappedName;
            if (_mappings.TryGetValue(columnName, out mappedName))
            {
                Write(mappedName);
            }
            else
            {
                Write(columnName);
            }
        }

        protected void WriteFilter(Expression where)
        {
            Write(String.Format("{0} = FILTER {1} BY ", GetNextAliasName(), GetLastAliasName()));
            Visit(where);
            Write("; ");
            AddAlias();
        }

        protected void WriteGenerate(ReadOnlyCollection<ColumnDeclaration> columns)
        {
            Write(String.Format("{0} = FOREACH {1} GENERATE ", GetNextAliasName(), GetLastAliasName()));
            WriteColumns(columns);
            Write("; ");
            AddAlias();
        }

        protected void WriteGroupBy(ReadOnlyCollection<Expression> groupBys)
        {
            if (groupBys.Count > 0)
            {
                Write(String.Format("{0} = GROUP {1} BY ", GetNextAliasName(), GetLastAliasName()));

                for (Int32 i = 0, n = groupBys.Count; i < n; i++)
                {
                    if (i > 0)
                    {
                        Write(", ");
                    }
                    Visit(groupBys[i]);
                }
                Write("; ");
                AddAlias();
            }
        }

        protected void WriteLoad(IDataFile source)
        {
            Write(String.Format("{0} = LOAD '{1}'; ", GetNextAliasName(), source.AbsolutePath));
            AddAlias();
        }

        protected void WriteOrderBy(ReadOnlyCollection<OrderByExpression> orderBys)
        {
            if (orderBys.Count > 0)
            {
                Write(String.Format("{0} = ORDER {1} BY ", GetNextAliasName(), GetLastAliasName()));

                for (Int32 i = 0, n = orderBys.Count; i < n; i++)
                {
                    OrderByExpression orderBy = orderBys[i];
                    if (i > 0)
                    {
                        Write(", ");
                    }
                    VisitValue(orderBy.Expression);
                    WriteOrderByDirection(orderBy.Direction);
                }
                Write("; ");
                AddAlias();
            }
        }

        protected void WriteOrderByDirection(OrderByDirection direction)
        {
            switch (direction)
            {
                case OrderByDirection.Descending:
                    Write(" DESC");
                    break;
                default:
                    Write(" ASC");
                    break;
            }
        }

        protected void WriteOutput(Expression take, String lastAlias)
        {
            if (take != null)
            {
                Write(String.Format("limit {0} ", lastAlias));
                Visit(take);
                Write("; ");
            }
            else
            {
                Write(String.Format("dump {0}; ", lastAlias));
            }
        }

        protected virtual void WriteValue(Object value)
        {
            if (value == null)
            {
                Write("NULL");
            }
            else if (value.GetType().IsEnum)
            {
                Write(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())));
            }
            else
            {
                switch (Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.Boolean:
                        Write(((Boolean)value) ? 1 : 0);
                        break;
                    case TypeCode.String:
                        Write("'");
                        Write(value);
                        Write("'");
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
                        Write(str);
                        break;
                    default:
                        Write(value);
                        break;
                }
            }
        }

        // protected override methods
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
                Write("*");
            }
            Write(")");
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.ArrayIndex)
            {
                Visit(node.Left);
                _sb.Append("[");
                Visit(node.Right);
                _sb.Append("]");
                return node;
            }

            _sb.Append("(");
            Visit(node.Left);
            Write(GetOperator(node));
            Visit(node.Right);
            _sb.Append(")");
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            WriteValue(node.Value);
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Write(node.Member.Name);
            return node;
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
                        Write("(");
                        VisitValue(node.Arguments[0]);
                        Write(" ");
                        Write(GetOperator(node.Method.Name));
                        Write(" ");
                        VisitValue(node.Arguments[1]);
                        Write(")");
                        return node;
                    case "Negate":
                        Write("-");
                        Visit(node.Arguments[0]);
                        Write("");
                        return node;
                    case "Compare":
                        Visit(Expression.Condition(
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
            else if (node.Method.Name == "ToString" && node.Object.Type == typeof(String))
            {
                return Visit(node.Object);  // no op
            }
            else if (node.Method.Name == "Equals")
            {
                if (node.Method.IsStatic && node.Method.DeclaringType == typeof(Object))
                {
                    Write("(");
                    Visit(node.Arguments[0]);
                    Write(" = ");
                    Visit(node.Arguments[1]);
                    Write(")");
                    return node;
                }
                else if (!node.Method.IsStatic && node.Arguments.Count == 1 && node.Arguments[0].Type == node.Object.Type)
                {
                    Write("(");
                    Visit(node.Object);
                    Write(" = ");
                    Visit(node.Arguments[0]);
                    Write(")");
                    return node;
                }
            }
            throw new NotSupportedException(String.Format("The method '{0}' is not supported", node.Method.Name));
        }

        protected override NewExpression VisitNew(NewExpression node)
        {
            throw new NotSupportedException(String.Format("The constructor for '{0}' is not supported", node.Constructor.DeclaringType));
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
        private void SetAttributes(Type sourceType)
        {
            // Get all properties that are in the source type.
            PropertyInfo[] sourceProperties = sourceType.GetProperties();
            SetMappings(sourceProperties);
            SetIgnores(sourceProperties);
        }

        private void SetMappings(PropertyInfo[] sourceProperties)
        {
            _mappings = new Dictionary<String, String>();

            foreach (PropertyInfo property in sourceProperties)
            {
                Object[] mappingAttributes = property.GetCustomAttributes(typeof(PigMapping), true);
                foreach (Object attribute in mappingAttributes)
                {
                    _mappings.Add(property.Name, ((PigMapping)attribute).Name);
                }
            }
        }

        private void SetIgnores(PropertyInfo[] sourceProperties)
        {
            _ignores = new List<String>();

            foreach (PropertyInfo property in sourceProperties)
            {
                Object[] mappingAttributes = property.GetCustomAttributes(typeof(PigIgnore), true);
                if (mappingAttributes != null && mappingAttributes.Length > 0)
                {
                    _ignores.Add(property.Name);
                }
            }
        }
    }
}
