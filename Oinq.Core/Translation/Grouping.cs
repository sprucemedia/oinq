using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Oinq.Translation
{
    /// <summary>
    /// Represents a collection of objects that have a common key.
    /// </summary>
    /// <typeparam name="TKey">Key.</typeparam>
    /// <typeparam name="TElement">Element.</typeparam>
    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        // private fields
        private readonly IEnumerable<TElement> _group;
        private readonly TKey _key;

        // constructors
        /// <summary>
        /// Initializes a new _instance of Grouping{{TKey, TElement}}.
        /// </summary>
        /// <param name="key">TKey.</param>
        /// <param name="group">TElement.</param>
        public Grouping(TKey key, IEnumerable<TElement> group)
        {
            _key = key;
            _group = group;
        }

        // public properties

        #region IGrouping<TKey,TElement> Members

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

        #endregion
    }
}