using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Oinq.Pig;

namespace Oinq.Tests
{
    public static class FakeExtensions
    {
        [PigExtension(typeof(FakeExtensions))]
        public static Int32 AddUp(this IEnumerable<IAddable> source)
        {
            return 0;
        }

        [PigExtension(typeof(FakeExtensions))]
        public static Int32 AggOp(this IEnumerable<IAddable> source)
        {
            return 0;
        }

        public static LambdaExpression BindAddUp(Expression source)
        {
            ParameterExpression x = source as ParameterExpression;
            String sourceName = x.Name;
            return System.Linq.Dynamic.DynamicExpression.ParseLambda(
                new ParameterExpression[] { x }, null, String.Format("{0}.Sum(Mea1)", sourceName));
        }

        public static LambdaExpression BindAggOp(Expression source)
        {
            ParameterExpression x = source as ParameterExpression;
            String sourceName = x.Name;
            return System.Linq.Dynamic.DynamicExpression.ParseLambda(
                new ParameterExpression[] { x }, null, String.Format("{0}.Sum(Mea1) + {0}.Sum(Mea1) + {0}.Sum(Mea1)", sourceName));
        }
    }

    [PigExtension(typeof(FakeClassAttributeExtensions))]
    public static class FakeClassAttributeExtensions
    {
        public static Int32 MultiplyIt(this IEnumerable<IAddable> source)
        {
            return 0;
        }

        public static LambdaExpression BindMultiplyIt(Expression source)
        {
            ParameterExpression x = source as ParameterExpression;
            String sourceName = x.Name;
            return System.Linq.Dynamic.DynamicExpression.ParseLambda(
                new [] { x }, null, String.Format("{0}.Sum(Mea1) * 2", sourceName));
        }
    }
}