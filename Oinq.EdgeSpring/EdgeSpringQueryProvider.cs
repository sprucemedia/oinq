using System;
using System.Collections.Generic;
using Oinq.EdgeSpring.Web;

namespace Oinq.EdgeSpring
{
    /// <summary>
    /// Custom inmplementation of the Oinq query provider.
    /// </summary>
    public class EdgeSpringQueryProvider : QueryProvider
    {
        /// <summary>
        /// Initializes a new member of EdgeSpringQueryProvider.
        /// </summary>
        /// <param name="source">IDataFile of type EdgeMart.</param>
        public EdgeSpringQueryProvider(EdgeMart source)
            : base(source)
        {          
        }

        /// <summary>
        /// Provides custom functionality on the Pig Query Provider.
        /// </summary>
        /// <param name="translatedQuery">An actionable query.</param>
        /// <returns>A query result.</returns>
        protected override Object Execute<TResult>(ITranslatedQuery translatedQuery)
        {
            Query query = new Query(((SelectQuery)translatedQuery).CommandText);
            QueryResponse<TResult> response = EdgeSpringApi.GetQueryResponse<TResult>(query, ((EdgeMart)Source).AbsoluteUri);

            return response.results.records;
        }
    }
}
