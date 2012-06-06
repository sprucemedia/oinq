using System;
using System.IO;
using System.Net;
using System.Text;

namespace Oinq.EdgeSpring.Web
{
    public class QueryRequest
    {
        private Uri _address;
        private Query _query;

        private QueryRequest(Uri address, Query query)
        {
            _address = address;
            _query = query;
        }

        public static QueryResponse SendQuery(Uri address, Query query)
        {
            QueryRequest queryRequest = new QueryRequest(address, query);
            return queryRequest.ExecuteQuery();
        }

        private QueryResponse ExecuteQuery()
        {
            HttpWebRequest request = WebRequest.Create(_address) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/json";

            Byte[] byteData = ASCIIEncoding.ASCII.GetBytes(_query.ToString());

            // Write data
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(byteData, 0, byteData.Length);
            }

            return GetQueryResponse(request);
        }

        private QueryResponse GetQueryResponse(HttpWebRequest request)
        {
            HttpWebResponse response = null;
            StreamReader reader = null;
            try
            {
                // Get response
                response = request.GetResponse() as HttpWebResponse;
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
