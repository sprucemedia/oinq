using System.Linq;
using Oinq;

namespace Oinq.EdgeSpring
{
    /// <summary>
    /// Static class that contains the EdgeSpring Linq extension methods.
    /// </summary>
    public static class LinqExtensionMethods
    {
        /// <summary>
        /// Returns an isntance of IQueryable{{T}} for an EdgeMart.
        /// </summary>
        /// <typeparam name="T">The type of records in an EdgeMart.</typeparam>
        /// <param name="edgeMart">The EdgeMart.</param>
        /// <returns>An instance of IQueryable{{T}} for an EdgeMart.</returns>
        public static IQueryable<T> AsQueryable<T>(this EdgeMart edgeMart)
        {
            var provider = new EdgeSpringQueryProvider(edgeMart);
            return new Query<T>(provider);
        }

        /// <summary>
        /// Returns an isntance of IQueryable{{T}} for an EdgeMart.
        /// </summary>
        /// <typeparam name="T">The type of records in an EdgeMart.</typeparam>
        /// <param name="edgeMart">The EdgeMart.</param>
        /// <returns>An instance of IQueryable{{T}} for an EdgeMart.</returns>
        public static IQueryable<T> AsQueryable<T>(this EdgeMart<T> edgeMart)
        {
            var provider = new EdgeSpringQueryProvider(edgeMart);
            return new Query<T>(provider);
        }
    }
}
