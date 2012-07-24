using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Oinq
{
    /// <summary>
    /// An abstract base class for an Expression visitor.
    /// </summary>
    internal abstract class ExpressionVisitor
    {
        // constructors
        /// <summary>
        /// Initializes a new member of the ExpressionVisitor class.
        /// </summary>
        protected ExpressionVisitor()
        {
        }

        // protected methods
        /// <summary>
        /// Visits an Expression.
        /// </summary>
        /// <param path="node">The Expression.</param>
        /// <returns>The Expression (posibly modified).</returns>
        protected virtual Expression Visit(Expression node)
        {
            if (node == null)
            {
                return node;
            }
            switch (node.NodeType)
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
                    return VisitUnary((UnaryExpression)node);
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
                    return VisitBinary((BinaryExpression)node);
                case ExpressionType.TypeIs:
                    return VisitTypeBinary((TypeBinaryExpression)node);
                case ExpressionType.Conditional:
                    return VisitConditional((ConditionalExpression)node);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)node);
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)node);
                case ExpressionType.MemberAccess:
                    return VisitMember((MemberExpression)node);
                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)node);
                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression)node);
                case ExpressionType.New:
                    return VisitNew((NewExpression)node);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray((NewArrayExpression)node);
                case ExpressionType.Invoke:
                    return VisitInvocation((InvocationExpression)node);
                case ExpressionType.MemberInit:
                    return VisitMemberInit((MemberInitExpression)node);
                case ExpressionType.ListInit:
                    return VisitListInit((ListInitExpression)node);
                default:
                    return VisitUnknown(node);
            }
        }

        /// <summary>
        /// Visits a BinaryExpression.
        /// </summary>
        /// <param path="node">The BinaryExpression.</param>
        /// <returns>The BinaryExpression (possibly modified).</returns>
        protected virtual Expression VisitBinary(BinaryExpression node)
        {
            Expression left = Visit(node.Left);
            Expression right = Visit(node.Right);
            Expression conversion = Visit(node.Conversion);
            return UpdateBinary(node, left, right, conversion, node.IsLiftedToNull, node.Method);
        }

        /// <summary>
        /// Visits a ConditionalExpression.
        /// </summary>
        /// <param path="node">The ConditionalExpression.</param>
        /// <returns>The ConditionalExpression (possibly modified).</returns>
        protected virtual Expression VisitConditional(ConditionalExpression node)
        {
            Expression test = Visit(node.Test);
            Expression ifTrue = Visit(node.IfTrue);
            Expression ifFalse = Visit(node.IfFalse);
            return UpdateConditional(node, test, ifTrue, ifFalse);
        }

        /// <summary>
        /// Visits a ConstantExpression.
        /// </summary>
        /// <param path="node">The ConstantExpression.</param>
        /// <returns>The ConstantExpression (possibly modified).</returns>
        protected virtual Expression VisitConstant(ConstantExpression node)
        {
            return node;
        }

        /// <summary>
        /// Visits an ElementInit.
        /// </summary>
        /// <param path="node">The ElementInit.</param>
        /// <returns>The ElementInit (possibly modified).</returns>
        protected virtual ElementInit VisitElementInit(ElementInit node)
        {
            ReadOnlyCollection<Expression> arguments = VisitExpressionList(node.Arguments);
            if (arguments != node.Arguments)
            {
                return Expression.ElementInit(node.AddMethod, arguments);
            }
            return node;
        }

        /// <summary>
        /// Visits an ElementInit list.
        /// </summary>
        /// <param path="nodes">The ElementInit list.</param>
        /// <returns>The ElementInit list (possibly modified).</returns>
        protected virtual IEnumerable<ElementInit> VisitElementInitList(
            ReadOnlyCollection<ElementInit> nodes)
        {
            List<ElementInit> list = null;
            for (Int32 i = 0, n = nodes.Count; i < n; i++)
            {
                ElementInit node = VisitElementInit(nodes[i]);
                if (list != null)
                {
                    list.Add(node);
                }
                else if (node != nodes[i])
                {
                    list = new List<ElementInit>(n);
                    for (Int32 j = 0; j < i; j++)
                    {
                        list.Add(nodes[j]);
                    }
                    list.Add(node);
                }
            }
            if (list != null)
            {
                return list;
            }
            return nodes;
        }

        /// <summary>
        /// Visits an Expression list.
        /// </summary>
        /// <param path="nodes">The Expression list.</param>
        /// <returns>The Expression list (possibly modified).</returns>
        protected ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> nodes)
        {
            if (nodes != null)
            {
                List<Expression> list = null;
                for (Int32 i = 0, n = nodes.Count; i < n; i++)
                {
                    Expression node = Visit(nodes[i]);
                    if (list != null)
                    {
                        list.Add(node);
                    }
                    else if (node != nodes[i])
                    {
                        list = new List<Expression>(n);
                        for (Int32 j = 0; j < i; j++)
                        {
                            list.Add(nodes[j]);
                        }
                        list.Add(node);
                    }
                }
                if (list != null)
                {
                    return list.AsReadOnly();
                }
            }
            return nodes;
        }

        /// <summary>
        /// Visits an InvocationExpression.
        /// </summary>
        /// <param path="node">The InvocationExpression.</param>
        /// <returns>The InvocationExpression (possibly modified).</returns>
        protected virtual Expression VisitInvocation(InvocationExpression node)
        {
            IEnumerable<Expression> args = VisitExpressionList(node.Arguments);
            Expression expr = Visit(node.Expression);
            return UpdateInvocation(node, expr, args);
        }

        /// <summary>
        /// Visits a LambdaExpression.
        /// </summary>
        /// <param path="node">The LambdaExpression.</param>
        /// <returns>The LambdaExpression (possibly modified).</returns>
        protected virtual Expression VisitLambda(LambdaExpression node)
        {
            Expression body = Visit(node.Body);
            return UpdateLambda(node, node.Type, body, node.Parameters);
        }

        /// <summary>
        /// Visits a ListInitExpression.
        /// </summary>
        /// <param path="node">The ListInitExpression.</param>
        /// <returns>The ListInitExpression (possibly modified).</returns>
        protected virtual Expression VisitListInit(ListInitExpression node)
        {
            NewExpression n = VisitNew(node.NewExpression);
            IEnumerable<ElementInit> initializers = VisitElementInitList(node.Initializers);
            return UpdateListInit(node, n, initializers);
        }

        /// <summary>
        /// Visits a MemberExpression.
        /// </summary>
        /// <param path="node">The MemberExpression.</param>
        /// <returns>The MemberExpression (possibly modified).</returns>
        protected virtual Expression VisitMember(MemberExpression node)
        {
            Expression exp = Visit(node.Expression);
            return UpdateMember(node, exp, node.Member);
        }

        /// <summary>
        /// Visits a MemberAssignment.
        /// </summary>
        /// <param path="node">The MemberAssignment.</param>
        /// <returns>The MemberAssignment (possibly modified).</returns>
        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            Expression e = Visit(node.Expression);
            return UpdateMemberAssignment(node, node.Member, e);
        }

        /// <summary>
        /// Visits a MemberBinding.
        /// </summary>
        /// <param path="node">The MemberBinding.</param>
        /// <returns>The MemberBinding (possibly modified).</returns>
        protected virtual MemberBinding VisitMemberBinding(MemberBinding node)
        {
            switch (node.BindingType)
            {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment)node);
                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding)node);
                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding)node);
                default:
                    throw new Exception(String.Format("Unhandled binding type '{0}'", node.BindingType));
            }
        }

        /// <summary>
        /// Visits a MemberBinding list.
        /// </summary>
        /// <param path="nodes">The MemberBinding list.</param>
        /// <returns>The MemberBinding list (possibly modified).</returns>
        protected virtual IEnumerable<MemberBinding> VisitMemberBindingList(ReadOnlyCollection<MemberBinding> nodes)
        {
            List<MemberBinding> list = null;
            for (Int32 i = 0, n = nodes.Count; i < n; i++)
            {
                MemberBinding node = VisitMemberBinding(nodes[i]);
                if (list != null)
                {
                    list.Add(node);
                }
                else if (node != nodes[i])
                {
                    list = new List<MemberBinding>(n);
                    for (Int32 j = 0; j < i; j++)
                    {
                        list.Add(nodes[j]);
                    }
                    list.Add(node);
                }
            }
            if (list != null)
            {
                return list;
            }
            return nodes;
        }

        /// <summary>
        /// Visits a MemberInitExpression.
        /// </summary>
        /// <param path="node">The MemberInitExpression.</param>
        /// <returns>The MemberInitExpression (possibly modified).</returns>
        protected virtual Expression VisitMemberInit(MemberInitExpression node)
        {
            NewExpression n = VisitNew(node.NewExpression);
            IEnumerable<MemberBinding> bindings = VisitMemberBindingList(node.Bindings);
            return UpdateMemberInit(node, n, bindings);
        }

        /// <summary>
        /// Visits a MemberListBinding.
        /// </summary>
        /// <param path="node">The MemberListBinding.</param>
        /// <returns>The MemberListBinding (possibly modified).</returns>
        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            IEnumerable<ElementInit> initializers = VisitElementInitList(node.Initializers);
            return UpdateMemberListBinding(node, node.Member, initializers);
        }

        /// <summary>
        /// Visits a MemberMemberBinding.
        /// </summary>
        /// <param path="node">The MemberMemberBinding.</param>
        /// <returns>The MemberMemberBinding (possibly modified).</returns>
        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            IEnumerable<MemberBinding> bindings = VisitMemberBindingList(node.Bindings);
            return UpdateMemberMemberBinding(node, node.Member, bindings);
        }

        /// <summary>
        /// Visits a MethodCallExpression.
        /// </summary>
        /// <param path="node">The MethodCallExpression.</param>
        /// <returns>The MethodCallExpression (possibly modified).</returns>
        protected virtual Expression VisitMethodCall(MethodCallExpression node)
        {
            Expression obj = Visit(node.Object);
            IEnumerable<Expression> args = VisitExpressionList(node.Arguments);
            return UpdateMethodCall(node, obj, node.Method, args);
        }

        /// <summary>
        /// Visits a NewExpression.
        /// </summary>
        /// <param path="node">The NewExpression.</param>
        /// <returns>The NewExpression (possibly modified).</returns>
        protected virtual NewExpression VisitNew(NewExpression node)
        {
            IEnumerable<Expression> args = VisitExpressionList(node.Arguments);
            return UpdateNew(node, node.Constructor, args, node.Members);
        }

        /// <summary>
        /// Visits a NewArrayExpression.
        /// </summary>
        /// <param path="node">The NewArrayExpression.</param>
        /// <returns>The NewArrayExpression (possibly modified).</returns>
        protected virtual Expression VisitNewArray(NewArrayExpression node)
        {
            IEnumerable<Expression> exprs = VisitExpressionList(node.Expressions);
            return UpdateNewArray(node, node.Type, exprs);
        }

        /// <summary>
        /// Visits a ParameterExpression.
        /// </summary>
        /// <param path="node">The ParameterExpression.</param>
        /// <returns>The ParameterExpression (possibly modified).</returns>
        protected virtual Expression VisitParameter(ParameterExpression node)
        {
            return node;
        }

        /// <summary>
        /// Visits a TypeBinaryExpression.
        /// </summary>
        /// <param path="node">The TypeBinaryExpression.</param>
        /// <returns>The TypeBinaryExpression (possibly modified).</returns>
        protected virtual Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            Expression expr = Visit(node.Expression);
            return UpdateTypeBinary(node, expr, node.TypeOperand);
        }

        /// <summary>
        /// Visits a UnaryExpression.
        /// </summary>
        /// <param path="node">The UnaryExpression.</param>
        /// <returns>The UnaryExpression (possibly modified).</returns>
        protected virtual Expression VisitUnary(UnaryExpression node)
        {
            Expression operand = Visit(node.Operand);
            return UpdateUnary(node, operand, node.Type, node.Method);
        }

        protected virtual Expression VisitUnknown(Expression node)
        {
            throw new Exception(String.Format("Unhandled expression type: '{0}'", node.NodeType));
        }

        // protected methods
        protected static BinaryExpression UpdateBinary(BinaryExpression node, Expression left, Expression right, 
            Expression conversion, Boolean isLiftedToNull, MethodInfo method)
        {
            if (node.Left != left || node.Right != right || node.Conversion != conversion || 
                node.Method != method || node.IsLiftedToNull != isLiftedToNull)
            {
                if (node.NodeType == ExpressionType.Coalesce && node.Conversion != null)
                {
                    return Expression.Coalesce(left, right, conversion as LambdaExpression);
                }
                else
                {
                    return Expression.MakeBinary(node.NodeType, left, right, isLiftedToNull, method);
                }
            }
            return node;
        }

        protected static ConditionalExpression UpdateConditional(ConditionalExpression node, Expression test, 
            Expression ifTrue, Expression ifFalse)
        {
            if (node.Test != test || node.IfTrue != ifTrue || node.IfFalse != ifFalse)
            {
                return Expression.Condition(test, ifTrue, ifFalse);
            }
            return node;
        }

        protected static InvocationExpression UpdateInvocation(InvocationExpression node, Expression expression, IEnumerable<Expression> args)
        {
            if (node.Arguments != args || node.Expression != expression)
            {
                return Expression.Invoke(expression, args);
            }
            return node;
        }

        protected static LambdaExpression UpdateLambda(LambdaExpression node, Type delegateType, Expression body, 
            IEnumerable<ParameterExpression> parameters)
        {
            if (node.Body != body || node.Parameters != parameters || node.Type != delegateType)
            {
                return Expression.Lambda(delegateType, body, parameters);
            }
            return node;
        }

        protected static ListInitExpression UpdateListInit(ListInitExpression node, NewExpression nex, IEnumerable<ElementInit> initializers)
        {
            if (node.NewExpression != nex || node.Initializers != initializers)
            {
                return Expression.ListInit(nex, initializers);
            }
            return node;
        }

        protected static Expression UpdateMember(MemberExpression node, Expression expression, MemberInfo member)
        {
            if (node.Expression != expression || node.Member != member)
            {
                return Expression.MakeMemberAccess(expression, member);
            }
            return node;
        }

        protected static MemberAssignment UpdateMemberAssignment(MemberAssignment assignment, MemberInfo member, Expression expression)
        {
            if (assignment.Expression != expression || member != assignment.Member)
            {
                return Expression.Bind(member, expression);
            }
            return assignment;
        }

        protected static MemberInitExpression UpdateMemberInit(MemberInitExpression node, NewExpression nex, 
            IEnumerable<MemberBinding> bindings)
        {
            if (node.NewExpression != nex || node.Bindings != bindings)
            {
                return Expression.MemberInit(nex, bindings);
            }
            return node;
        }

        protected static MemberListBinding UpdateMemberListBinding(MemberListBinding binding, MemberInfo member, 
            IEnumerable<ElementInit> initializers)
        {
            if (binding.Initializers != initializers || binding.Member != member)
            {
                Expression.ListBind(member, initializers);
            }
            return binding;
        }

        protected static MemberMemberBinding UpdateMemberMemberBinding(MemberMemberBinding binding, MemberInfo member, 
            IEnumerable<MemberBinding> bindings)
        {
            if (binding.Bindings != bindings || binding.Member != member)
            {
                return Expression.MemberBind(member, bindings);
            }
            return binding;
        }

        protected static MethodCallExpression UpdateMethodCall(MethodCallExpression node, Expression obj, 
            MethodInfo method, IEnumerable<Expression> args)
        {
            if (node.Object != obj || node.Method != method || node.Arguments != args)
            {
                return Expression.Call(obj, method, args);
            }
            return node;
        }

        protected static NewExpression UpdateNew(NewExpression node, ConstructorInfo constructor, IEnumerable<Expression> args, 
            IEnumerable<MemberInfo> members)
        {
            if (node.Arguments != args || node.Constructor != constructor || node.Members != members)
            {
                if (node.Members != null)
                {
                    return Expression.New(constructor, args, members);
                }
                else
                {
                    return Expression.New(constructor, args);
                }
            }
            return node;
        }

        protected static NewArrayExpression UpdateNewArray(NewArrayExpression node, Type arrayType, IEnumerable<Expression> expressions)
        {
            if (node.Expressions != expressions || node.Type != arrayType)
            {
                if (node.NodeType == ExpressionType.NewArrayInit)
                {
                    return Expression.NewArrayInit(arrayType.GetElementType(), expressions);
                }
                else
                {
                    return Expression.NewArrayBounds(arrayType.GetElementType(), expressions);
                }
            }
            return node;
        }

        protected static Expression UpdateTypeBinary(TypeBinaryExpression node, Expression expression, Type typeOperand)
        {
            if (node.Expression != expression || node.TypeOperand != typeOperand)
            {
                return Expression.TypeIs(expression, typeOperand);
            }
            return node;
        }

        protected static UnaryExpression UpdateUnary(UnaryExpression node, Expression operand, Type resultType, MethodInfo method)
        {
            if (node.Operand != operand || node.Type != resultType || node.Method != method)
            {
                return Expression.MakeUnary(node.NodeType, operand, resultType, method);
            }
            return node;
        }
    }
}
