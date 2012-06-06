using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Oinq.Core;
using Oinq.EdgeSpring.Web;

namespace Oinq.EdgeSpring
{
    public class EdgeSpringQueryProvider : QueryProvider
    {
        public EdgeSpringQueryProvider(EdgeMart source)
            : base(source)
        {          
        }

        public override Object Execute(Expression expression)
        {
            return Execute(Translate(expression));   
        }

        private Object Execute(TranslatedQuery query)
        {
            Delegate projector = query.Projector.Compile();
            Type elementType = TypeHelper.GetElementType(query.Projector.Body.Type);

            Query esQuery = new Query(query.CommandText);
            QueryResponse response = QueryRequest.SendQuery(Source.ServerUrl, esQuery);

            IEnumerable sequence = (IEnumerable)Activator.CreateInstance(
                typeof(ProjectionReader<>).MakeGenericType(elementType),
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                new object[] { response.Result.Records, projector, this },
                null
                );

            if (query.Aggregator != null)
            {
                Delegate aggregator = query.Aggregator.Compile();
                AggregateReader aggReader = (AggregateReader)Activator.CreateInstance(
                    typeof(AggregateReader<,>).MakeGenericType(elementType, query.Aggregator.Body.Type),
                    BindingFlags.Instance | BindingFlags.NonPublic, null,
                    new object[] { aggregator },
                    null
                    );
                return aggReader.Read(sequence);
            }
            else
            {
                return sequence;
            }
        }
    }
}
