namespace Oinq.Expressions
{
    internal enum PigExpressionType
    {
        Source = 1000,
        Column,
        Select,
        Projection,
        Join,
        IsNull,
        Scalar,
        Aggregate,
        AggregateSubquery,
        Subquery
    }
}
