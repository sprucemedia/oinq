using System.Linq;

namespace Oinq
{
    /// <summary>
    /// Static class that contains the Pig Linq extension methods.
    /// </summary>
    public static class LinqExtensionMethods
    {
        /// <summary>
        /// Returns an isntance of IQueryable{{T}} for an IDataFile.
        /// </summary>
        /// <typeparam name="T">The type of records in an IDataFile.</typeparam>
        /// <param name="dataFile">The IDataFile.</param>
        /// <returns>An instance of IQueryable{{T}} for an IDataFile.</returns>
        public static IQueryable<T> AsQueryable<T>(this IDataFile dataFile)
        {
            var provider = new QueryProvider(dataFile);
            return new Query<T>(provider);
        }
    }
}