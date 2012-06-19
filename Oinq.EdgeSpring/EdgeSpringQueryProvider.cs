using System;
using System.Linq;
using System.Collections.Generic;
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

        protected override Object Execute(TranslatedQuery translatedQuery)
        {
            Query esQuery = new Query(((SelectQuery)translatedQuery).CommandText);
            QueryResponse response = QueryRequest.SendQuery(((EdgeMart)Source).AbsoluteUri, esQuery);

            return response.Result.Records;
        }
    }
}
