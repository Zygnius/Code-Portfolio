using Godot;
using System;
using Godot.Collections;
using System.Collections.Generic;

[GlobalClass]
[Tool]
public partial class Pattern : Resource
{
    [Export]
    private string _name;
    public string Name => _name;

    [Export]
    private float _moveTime;
    public float MoveTime => _moveTime;

    [Export]
    private float _windupTime;
    public float WindupTime => _windupTime;

    [Export]
    private float _castTime = -1f; //if < 0 behavior should wait until the Pattern is fully finished
    public float CastTime => _castTime;

    [Export]
    private float _recoveryTime;
    public float RecoveryTime => _recoveryTime;

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

    [Export]
    private bool _moveDuringWindup;
    public bool MoveDuringWindup => _moveDuringWindup;

    private AimTypeData _aimType = new AimTypeData();
    [Export]
    public AimTypeData AimData
    {
        get => _aimType;
        set
        {
            _aimType = value;
            if (_aimType == null)
            {
                _aimType = new AimTypeData();
            }
        }
    }

    private TargetTypeData _targetType = new TargetTypeData();
    [Export]
    public TargetTypeData TargetData
    {
        get =>_targetType;
        set
        {
            _targetType = value;
            if (_targetType == null)
            {
                _targetType = new TargetTypeData();
            }
        }
    }

    private Godot.Collections.Array<PatternSequence> _sequence;
    [Export]
    public Godot.Collections.Array<PatternSequence> Sequence
    {
        get { return _sequence; }
        set
        {
            _sequence = value;
            while (_sequence.Contains(null))
            { 
                _sequence.Remove(null);
                _sequence.Add(new PatternSequence());
            }
        }
    }

    private int _currSequence = -1;
    public PatternSequence CurrSequence => _sequence[_currSequence];
    public bool IsLastSequence => _currSequence >= _sequence.Count - 1;

    private float _currWeight;
    public float CurrWeight => _currWeight;

    [Export]
    private Godot.Collections.Array<Vector2> _hpThresholds; //(HP Threshold for random selection weight to be active, rng selection weight)
    public Godot.Collections.Array<Vector2> HpThresholds => _hpThresholds;

    private int _currHpThreshold = 0;

    [Export]
    private int _repeats = 0;
    private int _currRepeat = 0;

    public bool DoRepeat => _currRepeat < _repeats;

    [Export]
    private Pattern _nextPattern;
    public Pattern NextPattern => _nextPattern;

    private Enemy _enemy;

    public Pattern()
    {

    }

    public void Initialize(Enemy enemy)
    {
        _enemy = enemy;
    }

    public void Reset()
    {
        _movementType.Reset();
        _targetType.Reset();
        foreach (PatternSequence sequence in _sequence)
        {
            sequence.MovementData.Reset();
            sequence.TargetData.Reset();
        }
        _currRepeat = 0;
        _currSequence = -1;
    }

    public void Repeat()
    {
        _currRepeat++;
        _movementType.Reset();
    }

    /// <summary>
    /// Moves onto the next PatternSequence returning false if there is none
    /// </summary>
    /// <returns></returns>
    public void NextSequence()
    {
        _currSequence++;
    }

    public void UpdateWeight(float currHPPercentage)
    {
        if (_currHpThreshold >= _hpThresholds.Count) return;
        while(_currHpThreshold < _hpThresholds.Count - 1 && currHPPercentage < _hpThresholds[_currHpThreshold + 1].X)
        {
            _currHpThreshold++;
        }
        _currWeight = _hpThresholds[_currHpThreshold].Y;
    }

}
