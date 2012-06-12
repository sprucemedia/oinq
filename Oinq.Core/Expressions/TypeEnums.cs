using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    internal enum PigExpressionType
    {
        Source = 1000, // make sure these don't overlap with ExpressionType
        Column,
        Select,
        Projection,
        Join,
        Aggregate,
        Subquery,
        Grouping,
        AggregateSubquery,
        IsNull,
        Scalar,
        NamedValue
    }

    internal static class PigExpressionExtensions
    {
        internal static Boolean IsPigExpression(this ExpressionType et)
        {
            return ((Int32)et) >= 1000;
        }
    }

    internal enum JoinType
    {
        CrossJoin,
        InnerJoin,
        CrossApply,
    }

    internal enum OrderType
    {
        Ascending,
        Descending
    }
}
