namespace SmartLocate.Commons.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CollectionAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}