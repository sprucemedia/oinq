using System;
using Oinq.Core;

namespace Oinq.EdgeSpring
{
    /// <summary>
    /// Represents an EdgeSpring EdgeMart and the settings used to access it.
    /// </summary>
    public class EdgeMart : ISource
    {
        // private fields
        private Url _url;

        // constructors
        /// <summary>
        /// Creates a new instance of EdgeMart.
        /// </summary>
        /// <param name="url">Server and EdgeMart settings in the form of a Url.</param>
        public EdgeMart(Url url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("server");
            }
            if (String.IsNullOrEmpty(url.EdgeMartName))
            {
                throw new ArgumentException("The name of the EdgeMart is required.");
            }
            _url = url;
        }

        /// <summary>
        /// Creates a new instance of EdgeMart.
        /// </summary>
        /// <param name="connectionString">Server and EdgeMart settings in the form of a connection string.</param>
        public EdgeMart (String connectionString)
            : this(Url.Create(connectionString))
        {
        }

        /// <summary>
        /// Creates a new instance of EdgeMart.
        /// </summary>
        /// <param name="uri">Server and EdgeMart settings in the form of a Uri.</param>
        public EdgeMart (Uri uri)
            : this (Url.Create(uri.ToString()))
        {
        }

        // public properties
        /// <summary>
        /// Gets the name of this EdgeMart.
        /// </summary>
        public virtual String Name
        {
            get { return _url.EdgeMartName; }
        }

        /// <summary>
        /// Gets the path to the EdgeMart.
        /// </summary>
        public virtual string Path
        {
            get { return _url.EdgeMartName; }
        }

        /// <summary>
        /// Gets the url string for this EdgeMart.
        /// </summary>
        public virtual ServerAddress Server
        {
            get { return _url.Server; }
        }

        /// <summary>
        /// Gets the Uri for the server hosting the EdgeMart.
        /// </summary>
        public virtual Uri ServerUrl
        {
            get { return new Uri(String.Format("http://{0}:{1}/remote", Server.Host, Server.Port)); }
        }

        /// <summary>
        /// Gets the url string for this EdgeMart.
        /// </summary>
        public virtual String UrlString
        {
            get { return _url.ToString(); }
        }
    }
}