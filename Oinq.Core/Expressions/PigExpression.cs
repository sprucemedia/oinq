using System;
using System.Linq.Expressions;

namespace Oinq.Expressions
{
    /// <summary>
    /// Abstract class representing a Pig query.
    /// </summary>
    internal abstract class PigExpression : Expression
    {
        // private fields
        private readonly ExpressionType _nodeType;
        private readonly Type _type;

        // constructors
        protected PigExpression(PigExpressionType eType, Type type)
        {
            _nodeType = ((ExpressionType)eType);
            _type = type;
        }

        // internal override properties
        public override ExpressionType NodeType
        {
            get { return _nodeType; }
        }

        public override Type Type
        {
            get { return _type; }
        }
    }
}