using System;
using System.IO;
using System.Net;
using System.Text;

namespace Oinq.EdgeSpring.Web
{
    /// <summary>
    /// Represents a query request made against the EdgeSpring EdgeMart.
    /// </summary>
    public class QueryRequest
    {
        // private fields
        private Uri _address;
        private Query _query;

        // constructors
        private QueryRequest(Uri address, Query query)
        {
            _address = address;
            _query = query;
        }

        // public static methods
        /// <summary>
        /// Sends the Pig query to the EdgeMart via REST API.
        /// </summary>
        /// <param name="address">Uri of the EdgeSpring server.</param>
        /// <param name="query">Query object to send.</param>
        /// <returns>A QueryResponse.</returns>
        public static QueryResponse SendQuery(Uri address, Query query)
        {
            QueryRequest queryRequest = new QueryRequest(address, query);
            return queryRequest.ExecuteQuery();
        }

        // private methods
        private QueryResponse ExecuteQuery()
        {
            WebRequest request = WebRequest.Create(_address);
            request.Method = "POST";
            request.ContentType = "application/json";

            Byte[] byteData = UTF8Encoding.UTF8.GetBytes(_query.ToString());

            // Write data
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(byteData, 0, byteData.Length);
            }

            return GetQueryResponse(request);
        }

        private QueryResponse GetQueryResponse(WebRequest request)
        {
            WebResponse response = null;
            StreamReader reader = null;
            try
            {
                // Get response
                response = request.GetResponse();
                // Get the response stream
                reader = new StreamReader(response.GetResponseStream());
            }
            catch (WebException wex)
            {
                // This exception will be raised if the server didn't return 200 - OK  
                // Try to retrieve more information about the network error  
                if (wex.Response != null)
                {
                    using (HttpWebResponse errorResponse = (HttpWebResponse)wex.Response)
                    {
                        throw new WebException(String.Format(
                            "The server returned '{0}' with the status code {1} ({2:d}).",
                            errorResponse.StatusDescription, errorResponse.StatusCode,
                            errorResponse.StatusCode), wex);
                    }
                }
            }
            finally
            {
                if (response != null) { response.Close(); }
            }

            return new QueryResponse(ResponseParser.BuildQueryResponseResult(reader.ReadToEnd())); 
        }
    }
}
