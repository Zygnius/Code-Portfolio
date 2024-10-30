using Godot;
using System;

public partial class ChannelProgressBar : TextureProgressBar
{
    private Player _player;

    private bool _channeling = false;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _player = GetNode<Player>("../..");
        _player.AbilityChannelStarted += OnPlayerAbilityChannelStarted;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (_channeling)
        {
            Vector2 channelStatus = _player.channelStatus;
            if(channelStatus.X < 0) channelStatus.X = 0;
            Value = ((channelStatus.Y - channelStatus.X) / channelStatus.Y) * MaxValue;

            if(channelStatus.X == 0)
            {
                _channeling = false;
                Tween tween = CreateTween().SetTrans(Tween.TransitionType.Linear);
                tween.TweenProperty(this, "self_modulate:a", 0, 0.1);
            }
        }
    }

    private void OnPlayerAbilityChannelStarted()
    {
        _channeling = true;
        SelfModulate = new Color(1, 1, 1, 1);
    }
}
