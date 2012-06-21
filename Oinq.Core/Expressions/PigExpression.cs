using System;
using System.Linq.Expressions;

namespace Oinq
{
    /// <summary>
    /// Abstract class representing a Pig query.
    /// </summary>
    internal abstract class PigExpression : Expression
    {
        // private fields
        private ExpressionType _nodeType;
        private Type _type;

        // constructors
        protected PigExpression(PigExpressionType eType, Type type)
            : base()
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