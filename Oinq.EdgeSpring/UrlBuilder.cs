using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Oinq.EdgeSpring
{
    /// <summary>
    /// Represents URL style connection strings to an EdgeSpring server.
    /// </summary>
    public class UrlBuilder
    {
        // constructors
        /// <summary>
        /// Initializes a new UrlBuilder.
        /// </summary>
        public UrlBuilder()
        {
            ResetValues();
        }

        /// <summary>
        /// Initializes a new UrlBuilder.
        /// </summary>
        /// <param name="url">A string containing a URL.</param>
        public UrlBuilder(String url)
        {
            Parse(url); // Parse calls ResetValues
        }

        // public properties
        /// <summary>
        /// Gets or sets the optional database name.
        /// </summary>
        public String EdgeMartName { get; set; }

        /// <summary>
        /// Gets or sets the server name.
        /// </summary>
        public ServerAddress Server { get; set; }

        // public methods
        /// <summary>
        /// Parses a URL and sets all settings to match the URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        public void Parse(String url)
        {
            ResetValues();
            const String serverPattern = @"((\[[^]]+?\]|[^:,/]+)(:\d+)?)";
            const String pattern = @"^http://" + @"(?<server>" + serverPattern + ")" + @"(/remote)";

            Match match = Regex.Match(url, pattern);
            if (match.Success)
            {
                String server = match.Groups["server"].Value;

                if (!String.IsNullOrEmpty(server))
                {
                    Server = ServerAddress.Parse(server);
                }
                else
                {
                    throw new FormatException("Invalid connection string. Server missing.");
                }

                var parameters = ParseQueryString(url).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                String edgeMartName;
                parameters.TryGetValue("edgemart", out edgeMartName);
                if (edgeMartName != null)
                {
                    EdgeMartName = edgeMartName;
                }
            }
            else
            {
                throw new FormatException(String.Format("Invalid connection string '{0}'.", url));
            }
        }

        /// <summary>
        /// Creates a new member of Url based on the settings in this UrlBuilder.
        /// </summary>
        /// <returns>A new member of Url.</returns>
        public Url ToUrl()
        {
            return Url.Create(ToString());
        }

        /// <summary>
        /// Returns the canonical URL based on the settings in this EsUrlBuilder.
        /// </summary>
        /// <returns>The canonical URL.</returns>
        public override String ToString()
        {
            StringBuilder url = new StringBuilder();
            url.Append("http://");

            if (Server != null)
            {
                url.AppendFormat("{0}:{1}", Server.Host, Server.Port);
                url.Append("/remote");
            }

            if (EdgeMartName != null)
            {
                url.Append("?edgemart=");
                url.Append(EdgeMartName);
            }
            return url.ToString();
        }

        // private methods
        private void ResetValues()
        {
            EdgeMartName = null;
            Server = null;
        }

        private static IEnumerable<KeyValuePair<String, String>> ParseQueryString(String url)
        {
            var queryStringPattern = new Regex(@"[\?&](?<name>[^&=]+)=(?<node>[^&=]+)");
            var matches = queryStringPattern.Matches(url);
            for (Int32 i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                yield return new KeyValuePair<String, String>
                    (match.Groups["name"].Value, match.Groups["node"].Value);
            }
        }
    }
}