using System;
using System.Collections.Generic;

namespace Oinq.EdgeSpring
{
    public class Url : IEquatable<Url>
    {
        // private static fields
        private static object __staticLock = new Object();
        private static Dictionary<String, Url> __cache = new Dictionary<string, Url>();

        // private fields
        private String _url;

        // constructor
        public Url(String url)
        {
            var builder = new UrlBuilder(url);
            EdgeMartName = builder.EdgeMartName;
            Server = builder.Server;
            _url = builder.ToString();
        }

        /// <summary>
        /// Gets the name of the EdgeMart.
        /// </summary>
        public String EdgeMartName { get; private set; }

        /// <summary>
        /// Gets the address of the server.
        /// </summary>
        public ServerAddress Server { get; private set; }

        // public operators
        /// <summary>
        /// Compares two Urls.
        /// </summary>
        /// <param name="lhs">The first URL.</param>
        /// <param name="rhs">The other URL.</param>
        /// <returns>True if the two URLs are equal (or both null).</returns>
        public static Boolean operator ==(Url lhs, Url rhs)
        {
            return object.Equals(lhs, rhs);
        }

        /// <summary>
        /// Compares two Urls.
        /// </summary>
        /// <param name="lhs">The first URL.</param>
        /// <param name="rhs">The other URL.</param>
        /// <returns>True if the two URLs are not equal (or one is null and the other is not).</returns>
        public static Boolean operator !=(Url lhs, Url rhs)
        {
            return !(lhs == rhs);
        }

        // public static methods
        /// <summary>
        /// Clears the URL cache. When a URL is parsed it is stored in the cache so that it doesn't have to be
        /// parsed again. There is rarely a need to call this method.
        /// </summary>
        public static void ClearCache()
        {
            __cache.Clear();
        }

        /// <summary>
        /// Creates an instance of Url (might be an existing instance if the same URL has been used before).
        /// </summary>
        /// <param name="url">The URL containing the settings.</param>
        /// <returns>An instance of Url.</returns>
        public static Url Create(String url)
        {
            // cache previously seen urls to avoid repeated parsing
            lock (__staticLock)
            {
                Url esUrl;
                if (!__cache.TryGetValue(url, out esUrl))
                {
                    esUrl = new Url(url);
                    var canonicalUrl = esUrl.ToString();
                    if (canonicalUrl != url)
                    {
                        if (__cache.ContainsKey(canonicalUrl))
                        {
                            esUrl = __cache[canonicalUrl]; // use existing Url
                        }
                        else
                        {
                            __cache[canonicalUrl] = esUrl; // cache under canonicalUrl also
                        }
                    }
                    __cache[url] = esUrl;
                }
                return esUrl;
            }
        }

        // public methods
        /// <summary>
        /// Compares two Urls.
        /// </summary>
        /// <param name="rhs">The other URL.</param>
        /// <returns>True if the two URLs are equal.</returns>
        public Boolean Equals(Url rhs)
        {
            if (object.ReferenceEquals(rhs, null) || GetType() != rhs.GetType()) { return false; }
            return _url == rhs._url; // this works because URL is in canonical form
        }

        /// <summary>
        /// Compares two Urls.
        /// </summary>
        /// <param name="obj">The other URL.</param>
        /// <returns>True if the two URLs are equal.</returns>
        public override Boolean Equals(Object obj)
        {
            return Equals(obj as Url); // works even if obj is null or of a different type
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override Int32 GetHashCode()
        {
            return _url.GetHashCode(); // this works because URL is in canonical form
        }

        /// <summary>
        /// Returns the canonical URL based on the settings in this UrlBuilder.
        /// </summary>
        /// <returns>The canonical URL.</returns>
        public override String ToString()
        {
            return _url;
        }
    }
}