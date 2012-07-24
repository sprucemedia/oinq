using System;
using System.Diagnostics;
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
            Stopwatch timer = new Stopwatch();
            timer.Start();
            String commandText = ((SelectQuery)translatedQuery).CommandText;
            timer.Stop();

            Debug.WriteLine(String.Format("ES query: {0}", commandText));
            Debug.WriteLine(String.Format("ES query generated: {0}ms", timer.ElapsedMilliseconds));

            Query query = new Query(commandText);
            QueryResponse<TResult> response = EdgeSpringApi.GetQueryResponse<TResult>(query, ((EdgeMart)Source).AbsoluteUri);

            Debug.WriteLine(String.Format("ES query execution time: {0}", response.query_time));

            return response.results.records;
        }
    }
}
