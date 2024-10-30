using Godot;
using System;

public partial class ExpBar : TextureProgressBar
{
    private Player _player;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Value = 0;
        _player = GameManager.Player;
        _player.ExpChanged += OnPlayerExpChanged;
        _player.LevelChanged += OnPlayerExpChanged;
        OnPlayerExpChanged();
    }

    private void OnPlayerExpChanged()
    {
        Vector2 Exp = _player.GetExp();
        Value = (Exp.X / Exp.Y) * 100;
    }
}
