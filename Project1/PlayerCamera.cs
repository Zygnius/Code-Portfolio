using Godot;
using System;

public partial class PlayerCamera : Camera2D
{
    private Player _player;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _player = GameManager.Player;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        GlobalPosition = GlobalPosition.MoveToward(_player.GlobalPosition, Mathf.Pow(GlobalPosition.DistanceTo(_player.GlobalPosition), 1.2f) * (float)delta);
    }
}
