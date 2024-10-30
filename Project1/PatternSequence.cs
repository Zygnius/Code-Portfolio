using Godot;
using System;
using Godot.Collections;

[Tool]
public partial class PatternSequence : Resource
{
    [Export]
    private float _waitTime;
    public float WaitTime => _waitTime; //Time to wait after previous PatternSequence starts. Will run after previous finishes if < 0

    [Export]
    private PackedScene _attack;
    public PackedScene Attack => _attack;

    [Export]
    private bool _parentToEnemy;
    public bool ParentToEnemy => _parentToEnemy;

    private MovementTypeData _movementType = new MovementTypeData();
    [Export]
    public MovementTypeData MovementData
    {
        get => _movementType;
        set
        {
            _movementType = value;
            if (_movementType == null)
            {
                _movementType = new MovementTypeData();
            }
        }
    }

    private TargetTypeData _targetType = new TargetTypeData();
    [Export]
    public TargetTypeData TargetData
    {
        get => _targetType;
        set
        {
            _targetType = value;
            if (_targetType == null)
            {
                _targetType = new TargetTypeData();
            }
        }
    }


    public PatternSequence()
    {
        
    }

    public AttackGroup StartAttack(Enemy enemy)
    {
        AttackGroup atk = _attack.Instantiate<AttackGroup>();
        if (_parentToEnemy) enemy.AddChild(atk);
        else enemy.GetTree().CurrentScene.AddChild(atk);

        if (_waitTime < 0)
            atk.TreeExited += enemy.NextPatternSequence;
        return atk;
    }
}
