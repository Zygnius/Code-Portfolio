using Godot;
using System;

[GlobalClass]
public partial class AbilityData : Resource
{
    [Export]
    public PackedScene abilityScene;

    [Export]
    public float cooldown;

    private float _currCooldown;
    public float currCooldown
    {
        get => _currCooldown;
        set
        {
            _currCooldown = value;
            EmitSignal(SignalName.CooldownChanged);
        }
    }

    [Export]
    public float channelTime;

    [Export]
    public float channelSpeedMult = 1f;

    [Export]
    public float castTime;

    [Export]
    public float castSpeedMult = 1f;

    [Export]
    public float recoveryTime;

    [Export]
    public float recoverySpeedMult = 1f;

    [Export]
    public TargettingType targettingType;

    public bool channeling = false;

    [Signal]
    public delegate void CooldownChangedEventHandler();

    public AbilityData() { }

    public void StartCooldown()
    {
        currCooldown = cooldown;
    }

    public bool IsOnCooldown()
    {
        return currCooldown > 0;
    }
}

public enum TargettingType
{
    Self,
    MousePos,
    MovementDir,
    Random
}
