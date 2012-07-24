using System;
using System.Text.RegularExpressions;
using System.Xml;

namespace Oinq.EdgeSpring
{
    /// <summary>
    /// Represents an EdgeSpring server.
    /// </summary>
    public class ServerAddress : IEquatable<ServerAddress>
    {
        // private fields
        private const int DEFAULT_PORT = 8000;

        // constructors
        /// <summary>
        /// Initializes a new member of ServerAddress.
        /// </summary>
        /// <param name="host">The server's host name.</param>
        public ServerAddress(String host)
        {
            Host = host;
            Port = DEFAULT_PORT;
        }

        /// <summary>
        /// Initializes a new member of ServerAddress.
        /// </summary>
        /// <param name="host">The server's host name.</param>
        /// <param name="port">The server's port number.</param>
        public ServerAddress(String host, Int32 port)
        {
            Host = host;
            Port = port;
        }

        // factory methods
        /// <summary>
        /// Parses a string representation of a server address.
        /// </summary>
        /// <param name="value">The string representation of a server address.</param>
        /// <returns>A new member of ServerAddress initialized with values parsed from the string.</returns>
        public static ServerAddress Parse(String value)
        {
            ServerAddress address;
            if (TryParse(value, out address))
            {
                return address;
            }
            else
            {
                throw new FormatException(String.Format("'{0}' is not a valid server address.", value));
            }
        }

        /// <summary>
        /// Tries to parse a string representation of a server address.
        /// </summary>
        /// <param name="value">The string representation of a server address.</param>
        /// <param name="address">The server address (set to null if TryParse fails).</param>
        /// <returns>True if the string is parsed succesfully.</returns>
        public static Boolean TryParse(String value, out ServerAddress address)
        {
            // don't throw ArgumentNullException if node is null
            if (value != null)
            {
                Match match = Regex.Match(value, @"^(?<host>(\[[^]]+?\]|[^:]+))(:(?<port>\d+))?$");
                if (match.Success)
                {
                    String host = match.Groups["host"].Value;
                    String portString = match.Groups["port"].Value;
                    Int32 port = (String.IsNullOrEmpty(portString)) ? DEFAULT_PORT : XmlConvert.ToInt32(portString);
                    address = new ServerAddress(host, port);
                    return true;
                }
            }
            address = null;
            return false;
        }

        // public properties
        /// <summary>
        /// Gets the server's host name.
        /// </summary>
        public String Host { get; private set; }

        /// <summary>
        /// Gets the server's port number.
        /// </summary>
        public Int32 Port { get; private set; }

        // public operators
        /// <summary>
        /// Compares two server addresses.
        /// </summary>
        /// <param name="lhs">The first address.</param>
        /// <param name="rhs">The other address.</param>
        /// <returns>True if the two addresses are equal (or both are null).</returns>
        public static Boolean operator ==(ServerAddress lhs, ServerAddress rhs)
        {
            return Object.Equals(lhs, rhs);
        }

        /// <summary>
        /// Compares two server addresses.
        /// </summary>
        /// <param name="lhs">The first address.</param>
        /// <param name="rhs">The other address.</param>
        /// <returns>True if the two addresses are not equal (or one is null and the other is not).</returns>
        public static Boolean operator !=(ServerAddress lhs, ServerAddress rhs)
        {
            return !(lhs == rhs);
        }

        // public methods
        /// <summary>
        /// Compares two server addresses.
        /// </summary>
        /// <param name="rhs">The other server address.</param>
        /// <returns>True if the two server addresses are equal.</returns>
        public Boolean Equals(ServerAddress rhs)
        {
            if (Object.ReferenceEquals(rhs, null) || GetType() != rhs.GetType()) { return false; }
            return Host.Equals(rhs.Host, StringComparison.OrdinalIgnoreCase) && Port == rhs.Port;
        }

        /// <summary>
        /// Compares two server addresses.
        /// </summary>
        /// <param name="obj">The other server address.</param>
        /// <returns>True if the two server addresses are equal.</returns>
        public override Boolean Equals(Object obj)
        {
            return Equals(obj as ServerAddress); // works even if obj is null or of a different type
        }

        /// <summary>
        /// Gets the hash code for this object.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override Int32 GetHashCode()
        {
            // see Effective Java by Joshua Bloch
            Int32 hash = 17;
            hash = 37 * hash + Host.ToLower().GetHashCode();
            hash = 37 * hash + Port.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Returns a string representation of the server address.
        /// </summary>
        /// <returns>A string representation of the server address.</returns>
        public override String ToString()
        {
            return String.Format("{0}:{1}", Host, Port);
        }
    }
}