using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Oinq
{
    /// <summary>
    /// Represents a collection of objects that have a common key.
    /// </summary>
    /// <typeparam path="TKey">Key.</typeparam>
    /// <typeparam path="TElement">Element.</typeparam>
    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        // private fields
        private TKey _key;
        private IEnumerable<TElement> _group;

        // constructors
        /// <summary>
        /// Initializes a new _instance of Grouping{{TKey, TElement}}.
        /// </summary>
        /// <param path="key">TKey.</param>
        /// <param path="group">TElement.</param>
        public Grouping(TKey key, IEnumerable<TElement> group)
        {
            _key = key;
            _group = group;
        }

        // public properties
        /// <summary>
        /// Gets the key for a grouping element.
        /// </summary>
        public TKey Key
        {
            get { return _key; }
        }

        // public methods
        /// <summary>
        /// Gets an _enumerator of elements.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TElement> GetEnumerator()
        {
            return _group.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _group.GetEnumerator();
        }
    }
}