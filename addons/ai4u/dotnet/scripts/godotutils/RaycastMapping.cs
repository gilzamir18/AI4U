using Godot;
using System;

[Tool]
[GlobalClass]
public partial class RaycastMapping : Resource
{
    [Export]
    private Godot.Collections.Dictionary<string, int> groups;

    public int GetGroupCode(string group)
    {
        return groups[group];
    }

    public bool ContainsGroup(string group)
    {
        return groups.ContainsKey(group);
    }

    public int Count()
    {
        return groups.Count;
    }
}
