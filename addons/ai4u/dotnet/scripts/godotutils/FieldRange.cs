using Godot;
namespace ai4u;
[Tool]
[GlobalClass]
public partial class FieldRange : Resource
{
    [Export] public int Min { get; set; } = 0;
    [Export] public int Max { get; set; } = 1;
    
    public FieldRange()
    {
    }

    public FieldRange(int min, int max)
    {
        this.Min = min; this.Max = max;
    }

    public override string ToString()
    {
        return $"Range({Min}, {Max})";
    }

    public override bool Equals(object obj)
    {
        if (obj is FieldRange other)
        {
            return Min == other.Min && Max == other.Max;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return (Min, Max).GetHashCode();
    }
}
