using Godot;
using System;

[GlobalClass]
public partial class AttackSpawner : Node2D, IDamage
{
    [Export]
    private Godot.Collections.Array<PackedScene> _attacks;

    [Export]
    private Vector2 _initialOffset;

    [Export]
    private float _initialRotation;

    [Export]
    private bool _useGlobalRotation;

    [Export]
    private Vector2 _repeatOffset;

    [Export]
    private int _repeats;

    [Export]
    private float _rotation;

    [Export]
    private float _initialDelay;

    [Export]
    private float _delay;

    private float _damage;
    private float _critRate;
    private float _critDamage;

    private int _repeatCounter = 0;
    private float _rotateCounter => _rotation * _repeatCounter;
    private float _timer = 0f;

    public override void _Ready()
    {
        
    }

    public override void _Process(double delta)
    {
        if (_attacks == null) return;

        _timer += (float)delta;

        if (_initialDelay > 0f)
        {
            if (_timer >= _initialDelay)
            {
                _timer = 0f;
                _initialDelay = 0f;
            }
            else return;
        }

        while (_repeatCounter < _repeats)
        {
            if (_repeatCounter == 0 && _timer <= (float)delta)
            {
                foreach (PackedScene attack in _attacks)
                {
                    Node2D node = attack.Instantiate<Node2D>();
                    AddChild(node);
                    node.Position = _initialOffset;
                    if (_useGlobalRotation) node.GlobalRotationDegrees = _initialRotation;
                    else node.RotationDegrees = _initialRotation;
                    if (node is IDamage) ((IDamage)node).SetDamage(_damage, _critRate, _critDamage);
                }
            }
            if (_timer >= _delay)
            {
                _timer = 0f;
                _repeatCounter++;
                foreach (PackedScene attack in _attacks)
                {
                    Node2D node = attack.Instantiate<Node2D>();
                    AddChild(node);
                    node.Position = (_initialOffset + (_repeatOffset * _repeatCounter)).Rotated(Mathf.DegToRad(_rotateCounter));
                    if (_useGlobalRotation) node.GlobalRotationDegrees = _initialRotation + _rotateCounter;
                    else node.RotationDegrees = _initialRotation + _rotateCounter;
                    if (node is IDamage) ((IDamage)node).SetDamage(_damage, _critRate, _critDamage);
                }
            }
            else break;
        }
        if (_repeatCounter >= _repeats && GetChildCount() == 0) QueueFree();
    }

    public void SetDamage(float damage, float critRate, float critDamage)
    {
        _damage = damage;
        _critRate = critRate;
        _critDamage = critDamage;
    }
}
