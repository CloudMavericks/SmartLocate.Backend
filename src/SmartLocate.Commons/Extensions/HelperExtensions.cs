using System.Reflection;
using SmartLocate.Commons.Attributes;

namespace SmartLocate.Commons.Extensions;

public static class HelperExtensions
{
    public static string GetCollectionName(this Type type)
    {
        var attribute = type.GetCustomAttribute<CollectionAttribute>();
        return attribute?.Name ?? type.Name;
    }
}