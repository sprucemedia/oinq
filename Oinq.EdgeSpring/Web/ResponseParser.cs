using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Oinq.EdgeSpring.Web
{
    public static class ResponseParser
    {
        public static QueryResponseResult BuildQueryResponseResult(String response)
        {
            JObject fullResults = JObject.Parse(response);
            var records = fullResults["results"]["records"].ToString();

            return new QueryResponseResult(JsonConvert.DeserializeObject<List<Object>>(records));
        }
    }
}
