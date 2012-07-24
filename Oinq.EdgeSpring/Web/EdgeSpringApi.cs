using System;
using Newtonsoft.Json;
using RestSharp;

namespace Oinq.EdgeSpring.Web
{
    /// <summary>
    /// Proxy class for the EdgeSpring REST API.
    /// </summary>
    public static class EdgeSpringApi
    {
        /// <summary>
        /// Gets the results for a query against an EdgeSpring EdgeMart.
        /// </summary>
        /// <param name="query">The Query.</param>
        /// <param name="requestUri">The URI of the request.</param>
        /// <returns>The Query Response.</returns>
        public static QueryResponse<T> GetQueryResponse<T>(Query query, Uri requestUri)
        {
            var request = new RestRequest(requestUri.PathAndQuery, Method.POST);
            request.RootElement = "results";
            request.RequestFormat = DataFormat.Json;
            request.AddBody(query);

            return Execute<QueryResponse<T>>(request, requestUri.Authority);
        }

        /// <summary>
        /// Gets the results for an update to an EdgeSpring EdgeMart.
        /// </summary>
        /// <param name="requestUri">The URI of the request.</param>
        /// <returns>The Update Response.</returns>
        public static UpdateResponse GetUpdateResponse(Uri requestUri)
        {
            var request = new RestRequest(requestUri.PathAndQuery);

            return Execute<UpdateResponse>(request, requestUri.Authority);
        }

        // private static methods
        private static T Execute<T>(IRestRequest request, String baseUrl) where T : new()
        {
            var client = new RestClient("http://" + baseUrl);
            var temp = client.Execute(request);

            return JsonConvert.DeserializeObject<T>(temp.Content);
        }
    }
}
