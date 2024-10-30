using Godot;
using System;

public partial class PickupRadius : Area2D
{
    private Player _player;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _player = GetParent<Player>();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area.IsInGroup("exp"))
        {
            _player.AddExperience(((ExperienceOrb)area).Value);
            area.QueueFree();
        }
    }
}
