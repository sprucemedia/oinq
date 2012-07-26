using System.Collections.ObjectModel;
using System.Reflection;

namespace Oinq.EdgeSpring.Entity
{
    public interface IEntityInfo
    {
        ReadOnlyCollection<PropertyInfo> Dimensions { get; }
        ReadOnlyCollection<PropertyInfo> Keys { get; }
        ReadOnlyCollection<PropertyInfo> Measures { get; }
    }
}