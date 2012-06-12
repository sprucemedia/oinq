using System;
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

        public override Object  Execute(String commandText, String[] paramNames, Object[] paramValues, 
            Func<IEnumerable<Object>,Object> fnRead)
        {
            Query esQuery = new Query(commandText);
            QueryResponse response = QueryRequest.SendQuery(Source.ServerUrl, esQuery);

            return fnRead(response.Result.Records);
        }
    }
}
