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

        public override Object Execute(String commandText, String[] paramNames, Object[] paramValues, 
            Func<EnumerableDataReader, Object> fnRead)
        {
            Query esQuery = new Query(commandText);
            QueryResponse response = QueryRequest.SendQuery(Source.ServerUrl, esQuery);

            return fnRead(new EnumerableDataReader(response.Result.Records));
        }
    }
}
