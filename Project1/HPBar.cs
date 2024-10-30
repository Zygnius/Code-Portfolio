using Godot;
using System;

public partial class HPBar : TextureProgressBar
{
    [Export]
    private bool _isWorldHPBar = false;

    private Entity _entity;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Value = 100;
        if(_isWorldHPBar)
            _entity = GetNode<Entity>("../..");
        else
        {
            _entity = GameManager.Player;
        }
        _entity.HealthChanged += OnEntityHealthChanged;
        UpdateAfterReady();
    }

    private async void UpdateAfterReady()
    {
        await ToSignal(_entity, "ready");
        OnEntityHealthChanged();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void OnEntityHealthChanged()
    {
        Vector2 HP = _entity.GetHP();
        Value = (HP.X / HP.Y) * MaxValue;
    }
}
