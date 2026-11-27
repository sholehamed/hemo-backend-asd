namespace Domain.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class PropConfig : Attribute
{
    public PropConfig(string Name, int MaxLenght = 256, int MinLenght = 0, long MaxNumber = long.MaxValue,
        long MinNumber = 0, bool IsUnique = false, bool HasIndex = false, bool AddToDTO = true)
    {
        this.Name = Name;
        this.MaxLenght = MaxLenght;
        this.MinLenght = MinLenght;
        this.MaxNumber = MaxNumber;
        this.MinNumber = MinNumber;
        this.IsUnique = IsUnique;
        this.HasIndex = HasIndex;
        this.AddToDTO = AddToDTO;
    }

    public string Name { get; }
    public int MaxLenght { get; }
    public int MinLenght { get; }
    public long MaxNumber { get; }
    public long MinNumber { get; }
    public bool IsUnique { get; set; }
    public bool HasIndex { get; set; }
    public bool AddToDTO { get; set; }
}