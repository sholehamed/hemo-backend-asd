namespace Domain.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ClassName : Attribute
{
    public ClassName(string Name)
    {
        this.Name = Name;
    }

    public string Name { get; private set; }
}