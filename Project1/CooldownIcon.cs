using Godot;
using System;

public partial class CooldownIcon : PanelContainer
{
    private TextureProgressBar _progressBar;

    [Export]
    private AbilityData _abilityData;

    [Export]
    private bool _tickUp = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _progressBar = GetNode<TextureProgressBar>("TextureProgressBar");

        if (_abilityData != null)
        {
            _abilityData.CooldownChanged += OnAbilityDataCooldownChanged;
        }
    }

    private void OnAbilityDataCooldownChanged()
    {
        if(_tickUp) _progressBar.Value = ((_abilityData.cooldown - _abilityData.currCooldown) / _abilityData.cooldown) * _progressBar.MaxValue;
        else _progressBar.Value = (_abilityData.currCooldown / _abilityData.cooldown) * _progressBar.MaxValue;
    }
}
