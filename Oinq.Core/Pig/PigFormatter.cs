using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Oinq.Expressions;

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
        private Dictionary<String, String> _sources;
        private SourceAlias _alias;
        private Dictionary<SourceAlias, Int32> _aliases;
        private const Int32 RESULT_LIMIT = 1000;
        private List<String> _columnNames;

        // constructors
        private PigFormatter()
        {
            _sb = new StringBuilder();
            _sources = new Dictionary<String, String>();
            _aliases = new Dictionary<SourceAlias, Int32>();
            _columnNames = new List<String>();
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
            formatter.SetSourceMappings(selectQuery.Sources);
            foreach (SourceExpression s in selectQuery.Sources)
            {
                formatter._alias = s.Alias;
                formatter.AddAlias(s.Alias);
                formatter.WriteLoad(s.Type);
            }

            foreach (SelectExpression ex in selectQuery.CommandStack)
            {
                if (ex.Take == null && ex.OrderBy == null)
                {
                    formatter._alias = formatter.FindRootSource(ex.From);
                    if (ex.Where != null)
                    {
                        formatter.WriteFilter(selectQuery.Where);
                        formatter.AddAlias(formatter._alias, formatter._aliasCount);
                        formatter.AddAlias();
                    }
                    if (ex.GroupBy != null && selectQuery.Joins.Count == 0)
                    {
                        formatter.WriteGroupBy(selectQuery.GroupBy);
                        formatter.AddAlias(formatter._alias, formatter._aliasCount);
                        formatter.AddAlias();
                    }
                }
            }
            formatter.WriteJoins(selectQuery.Joins, selectQuery.Columns);
            formatter.WriteGenerate(selectQuery.Columns);
            formatter.WriteOrderBy(selectQuery.OrderBy, selectQuery.Columns);
            formatter.WriteOutput(selectQuery.Take);
            return formatter._sb.ToString();
        }

        // protected methods
        protected virtual void AddAlias()
        {
            _aliasCount += 1;
        }

        protected virtual void AddAlias(SourceAlias alias)
        {
            Int32 value;
            if (_aliases.TryGetValue(alias, out value))
            {
                _aliases[alias] = value += 1;
            }
            else
            {
                _aliases.Add(alias, _aliasCount);
            }
            AddAlias();
        }

        protected virtual void AddAlias(SourceAlias alias, Int32 aliasNumber)
        {
            Int32 value;
            if (_aliases.TryGetValue(alias, out value))
            {
                _aliases[alias] = aliasNumber;
            }
            else
            {
                AddAlias(alias);
            }
        }

        protected virtual String GetLastAliasName()
        {
            return "t" + (_aliasCount - 1);
        }

        protected virtual String GetLastAliasName(SourceAlias alias)
        {
            Int32 value;
            if (_aliases.TryGetValue(alias, out value))
            {
                return "t" + value;
            }
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
                    return (IsBoolean(b.Left.Type)) ? " and " : " & ";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return (IsBoolean(b.Left.Type)) ? " or " : " | ";
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
                    Write("avg");
                    break;
                case "Count":
                    Write("count");
                    break;
                default:
                    Write(aggregateName.ToLower());
                    break;
            }
        }

        protected void WriteAsColumnName(String columnName)
        {
            Write(" as ");
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
            Write(String.Format("{0} = filter {1} by ", GetNextAliasName(), GetLastAliasName(_alias)));
            Visit(where);
            Write("; ");
        }

        protected void WriteGenerate(ReadOnlyCollection<ColumnDeclaration> columns)
        {
            Write(String.Format("{0} = foreach {1} generate ", GetNextAliasName(), GetLastAliasName(_alias)));
            WriteColumns(columns);
            Write("; ");
            AddAlias(_alias, _aliasCount);
            AddAlias();
        }

        protected void WriteGroupBy(ReadOnlyCollection<Expression> groupBys)
        {
            if (groupBys.Count > 0)
            {
                Write(String.Format("{0} = group {1} by (", GetNextAliasName(), GetLastAliasName(_alias)));

                for (Int32 i = 0, n = groupBys.Count; i < n; i++)
                {
                    if (i > 0)
                    {
                        Write(", ");
                    }
                    Visit(groupBys[i]);
                }
                Write("); ");
            }
        }

        protected void WriteJoins(ReadOnlyCollection<JoinExpression> joins, ReadOnlyCollection<ColumnDeclaration> outputColumns)
        {
            if (joins.Count > 0)
            {
                foreach (ColumnDeclaration column in outputColumns)
                {
                    FindSourceColumnName(column.Expression);
                }
                for (Int32 i = 0, n = joins.Count; i < n; i++)
                {
                    JoinExpression join = joins[i];
                    BinaryExpression condition = (BinaryExpression)join.Condition;
                    SourceAlias left = FindRootSource(join.Left);
                    SourceAlias right = FindRootSource(join.Right);
                    
                    Write(String.Format("{0} = group {1} by (", GetNextAliasName(), GetLastAliasName(right)));
                    List<String> columns = ((SelectExpression)join.Right).Columns.Select(s => s.Name).ToList();
                    Boolean first = true;
                    foreach(String name in columns)
                    {
                        if (name != ((ColumnExpression)condition.Right).Name && _columnNames.Contains(name))
                        {
                            if (!first)
                            {
                                Write(", ");
                            }
                            WriteColumnName(name);
                            first = false;
                        }
                    }
                    Write("); ");
                    AddAlias(right, _aliasCount);
                    AddAlias();
                    
                    Write(String.Format("{0} = group {1} by ", GetNextAliasName(), GetLastAliasName(left)));
                    Visit(condition.Left);
                    Write(String.Format(", {0} by ", GetLastAliasName(right)));
                    Visit(condition.Right);
                    Write("; ");
                    Int32 aliasCount = _aliasCount;
                    AddAlias(left, aliasCount);
                    AddAlias(right, aliasCount);
                    AddAlias();
                }
            }
        }

        protected void WriteLoad(Type sourceType)
        {
            Write(String.Format("{0} = load '{1}'; ", GetLastAliasName(_alias), _sources[sourceType.Name]));
        }

        protected void WriteOrderBy(ReadOnlyCollection<OrderByExpression> orderBys, ReadOnlyCollection<ColumnDeclaration> outputColumns)
        {
            if (orderBys != null && orderBys.Count > 0)
            {
                List<ColumnDeclaration> columns = outputColumns.Where(p => (PigExpressionType)p.Expression.NodeType == PigExpressionType.Column).ToList();
                List<ColumnDeclaration> aggs = outputColumns.Where(p => (PigExpressionType)p.Expression.NodeType == PigExpressionType.Aggregate).ToList();
                Dictionary<String, String> columnMaps = new Dictionary<String, String>();
                foreach (ColumnDeclaration column in columns)
                {
                    columnMaps.Add(column.Name, ((ColumnExpression)column.Expression).Name);
                }
                Write(String.Format("{0} = order {1} by ", GetNextAliasName(), GetLastAliasName(_alias)));

                for (Int32 i = 0, n = orderBys.Count; i < n; i++)
                {
                    OrderByExpression orderBy = orderBys[i];
                    if (i > 0)
                    {
                        Write(", ");
                    }

                    var colExp = orderBy.Expression as ColumnExpression;
                    if (colExp != null)
                    {
                        WriteColumnName(columnMaps.FirstOrDefault(x => x.Value == colExp.Name).Key);
                    }
                    var aggExp = orderBy.Expression as AggregateExpression;
                    if (aggExp != null)
                    {
                        if (aggs.Count == 1)
                        {
                            WriteColumnName(aggs[0].Name);
                        }
                    }
                    var parseExp = orderBy.Expression as MethodCallExpression;
                    if (parseExp != null)
                    {
                        foreach(ColumnDeclaration col in outputColumns)
                        {
                            var mce = col.Expression as MethodCallExpression;
                            if (mce != null && parseExp.Method == mce.Method && parseExp.Method.DeclaringType == mce.Method.DeclaringType)
                            {
                                WriteColumnName(col.Name);
                            }
                        }
                    }

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
                    Write(" desc");
                    break;
                default:
                    Write(" asc");
                    break;
            }
        }

        protected void WriteOutput(Expression take)
        {
            String nextAlias = GetNextAliasName();
            String lastAlias = GetLastAliasName();
            if (take != null)
            {
                Write(String.Format("{0} = limit {1} ", nextAlias, lastAlias));
                Visit(take);
                Write("; ");
            }
            else
            {
                Write(String.Format("{0} = limit {1} {2}; ", nextAlias, lastAlias, RESULT_LIMIT.ToString()));
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
            else if (node.Method.Name == "Parse" || node.Method.DeclaringType == typeof(Convert))
            {
                Visit(node.Arguments[0]);
                // Do nothing else for now - should be replaces with new EdgeSpring functions once they become available.
                return node;
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
                    _sb.Append(" not ");
                    Visit(node.Operand);
                    break;
                case ExpressionType.Convert:
                    Visit(node.Operand);
                    break;
                default:
                    throw new NotSupportedException(String.Format("The unary operator '{0}' is not supported", node.NodeType));
            }
            return node;
        }

        // private methods
        private void InterpretNonPigExpression(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                case ExpressionType.UnaryPlus:
                    FindSourceColumnName(((UnaryExpression)expression).Operand);
                    break;
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
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.Power:
                    BinaryExpression binary = expression as BinaryExpression;
                    FindSourceColumnName(binary.Left);
                    FindSourceColumnName(binary.Right);
                    break;
                case ExpressionType.Call:
                    MethodCallExpression exp = expression as MethodCallExpression;
                    if (exp.Arguments.Count == 1)
                    {
                        ColumnExpression columnExpression = exp.Arguments.First() as ColumnExpression;
                        if (columnExpression != null)
                        {
                            _columnNames.Add(columnExpression.Name);
                            break;
                        }
                        FindSourceColumnName(exp.Arguments.First());
                        break;
                    }
                    break;
                case ExpressionType.New:
                    NewExpression expr = expression as NewExpression;
                    foreach (Expression argument in expr.Arguments)
                    {
                        FindSourceColumnName(argument);
                    }
                    break;
                case ExpressionType.MemberInit:
                    MemberInitExpression mie = expression as MemberInitExpression;
                    foreach (MemberBinding binding in mie.Bindings)
                    {
                        _columnNames.Add(binding.Member.Name);
                    }
                    break;
                default:
                    throw new NotSupportedException("Aggregates must fall on the left side of the join.");
            }
        }

        private void FindSourceColumnName(Expression expression)
        {
            switch ((PigExpressionType)expression.NodeType)
            {
                case PigExpressionType.Column:
                    _columnNames.Add(((ColumnExpression)expression).Name);
                    break;
                case PigExpressionType.Aggregate:
                    var arg = ((AggregateExpression)expression).Argument;
                    var col = arg as ColumnExpression;
                    if (col != null)
                    {
                        _columnNames.Add(col.Name);
                    }
                    break;
                case PigExpressionType.Join:
                    break;
                default:
                    InterpretNonPigExpression(expression);
                    break;
            }
        }

        private SourceAlias FindRootSource(Expression expression)
        {
            switch ((PigExpressionType)expression.NodeType)
            {
                case PigExpressionType.Source:
                    return ((SourceExpression)expression).Alias;
                case PigExpressionType.Select:
                    return FindRootSource(((SelectExpression)expression).From);
                default:
                    throw new InvalidOperationException("An invalid expression exists in the tree.");
            }
        }

        private void GetMappings(PropertyInfo[] sourceProperties)
        {
            foreach (PropertyInfo property in sourceProperties)
            {
                Object[] mappingAttributes = property.GetCustomAttributes(typeof(PigMapping), true);
                foreach (Object attribute in mappingAttributes)
                {
                    _mappings.Add(property.Name, ((PigMapping)attribute).Name);
                }
            }
        }

        private void SetSourceMappings(ReadOnlyCollection<SourceExpression> sources)
        {
            _ignores = new List<String>();
            _mappings = new Dictionary<String, String>();

            foreach (SourceExpression source in sources)
            {
                Type sourceType = source.Type;
                Object[] attributes = sourceType.GetCustomAttributes(typeof(PigSourceMapping), true);
                if (attributes != null && attributes.Length > 0)
                {
                    _sources.Add(source.Name, ((PigSourceMapping)attributes[0]).Path);
                }
                else
                {
                    _sources.Add(source.Name, source.Name);
                }

                // Get all properties that are in the source type.
                PropertyInfo[] sourceProperties = sourceType.GetProperties();
                GetIgnores(sourceProperties);
                GetMappings(sourceProperties);
            }     
        }

        private void GetIgnores(PropertyInfo[] sourceProperties)
        {
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
