using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    public enum PigExpressionType
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

    public static class PigExpressionExtensions
    {
        public static Boolean IsPigExpression(this ExpressionType et)
        {
            return ((Int32)et) >= 1000;
        }
    }

    public enum JoinType
    {
        CrossJoin,
        InnerJoin,
        CrossApply,
    }

    public enum OrderType
    {
        Ascending,
        Descending
    }
}
