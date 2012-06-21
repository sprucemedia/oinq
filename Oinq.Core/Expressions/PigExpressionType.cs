namespace Oinq
{
    internal enum PigExpressionType
    {
        Source = 1000,
        Column,
        Select,
        Projection,
        IsNull,
        Scalar,
        Aggregate,
        AggregateSubquery,
        Subquery
    }
}
