using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Represents a collection of projected columns.
    /// </summary>
    internal sealed class ProjectedColumns
    {
        // constructors
        internal ProjectedColumns(Expression projector, ReadOnlyCollection<ColumnDeclaration> columns)
        {
            Projector = projector;
            Columns = columns;
        }

        // internal properties
        internal Expression Projector { get; private set; }
        internal ReadOnlyCollection<ColumnDeclaration> Columns { get; private set; }
    }
}
