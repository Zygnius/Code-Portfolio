using Godot;
using System;

public partial class Enemy : Entity
{
    [Export]
    private Godot.Collections.Array<Pattern> _patterns;

    private Pattern _currPattern;

    protected bool _canMove = true;

    private BehaviorState _behaviorState = BehaviorState.Idle;

    private float _timer = 0f;

    enum BehaviorState
    {
        Idle,
        Move,
        WindUp,
        Casting,
        Recovery,
        Stunned
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        Behavior((float)delta);
    }

    //Should be run in _PhysicsProcess
    protected virtual void Behavior(float delta)
    {
        if (_patterns != null)
        {
            switch (_behaviorState)
            {
                case BehaviorState.Idle:
                    ChoosePattern();
                    ChangeBehaviorState(BehaviorState.Move);
                    break;
                case BehaviorState.Move:
                    if (_timer == 0)
                    {
                        Aim();
                    }
                    else
                    {
                        Target();
                    }
                    _timer += delta;
                    Move();
                    if (_timer >= _currPattern.MoveTime && _currPattern.MoveTime >= 0)
                    {
                        ChangeBehaviorState(BehaviorState.WindUp);
                    }
                    break;
                case BehaviorState.WindUp:
                    Target();
                    if (_currPattern.MoveDuringWindup)
                    {
                        Move();
                    }
                    _timer += delta;
                    if (_timer >= _currPattern.WindupTime)
                    {
                        ChangeBehaviorState(BehaviorState.Casting);
                    }
                    break;
                case BehaviorState.Casting:
                    if (_timer == 0)
                        NextPatternSequence();
                    Target();
                    Move();
                    _timer += delta;
                    if (_currPattern.CastTime >= 0 && _timer >= _currPattern.CastTime)
                    {
                        EndPattern();
                    }
                    break;
                case BehaviorState.Recovery:
                    _timer += delta;
                    if (_timer >= _currPattern.RecoveryTime)
                    {
                        ChangeBehaviorState(BehaviorState.Idle);
                    }
                    break;
                case BehaviorState.Stunned:
                    break;
                default:
                    break;
            }
        }
    }

    private void ChangeBehaviorState(BehaviorState behaviorState)
    {
        _timer = 0f;
        _behaviorState = behaviorState;
    }

    private void Move()
    {
        if (_canMove)
        {
            MovementTypeData move = _currPattern.MovementData;
            if (_behaviorState == BehaviorState.Casting) move = _currPattern.CurrSequence.MovementData;
            GD.Print(move.Movement);
            switch (move.Movement)
            {
                case MovementType.None:
                    if(_behaviorState == BehaviorState.Move)
                    {
                        ChangeBehaviorState(BehaviorState.WindUp);
                        return;
                    }
                    break;
                case MovementType.SetPosition:
                    if(Position.DistanceTo(move.Position) <= Speed / 100)
                    {
                        Velocity = Vector2.Zero;
                        Position = move.Position;
                        if (_behaviorState == BehaviorState.Move)
                        {
                            ChangeBehaviorState(BehaviorState.WindUp);
                        }
                        return;
                    }
                    Velocity = Position.DirectionTo(move.Position) * Speed;
                    break;
                case MovementType.RelativePosition:
                    if (!move.HasStoredData)
                    {
                        Vector2 calc = Position + move.Position;
                        if (move.AffectedByRotation)
                            move.TargettedPosition = calc.Rotated(Rotation);
                        else
                            move.TargettedPosition = calc;
                        move.HasStoredData = true;
                    }
                    if (Position.DistanceTo(move.TargettedPosition) <= Speed / 100)
                    {
                        Position = move.TargettedPosition;
                        if (_behaviorState == BehaviorState.Move)
                        {
                            ChangeBehaviorState(BehaviorState.WindUp);
                        }
                        return;
                    }
                    Velocity = Position.DirectionTo(move.TargettedPosition) * Speed;
                    break;
                case MovementType.ChaseTarget:
                    if (_behaviorState == BehaviorState.Casting) return;
                    if (GlobalPosition.DistanceTo(GameManager.Player.GlobalPosition) <= move.Range)
                    {
                        if (_behaviorState == BehaviorState.Move && _timer >= move.MinMoveTime)
                        {
                            ChangeBehaviorState(BehaviorState.WindUp);
                        }
                        return;
                    }
                    Velocity = GlobalPosition.DirectionTo(GameManager.Player.GlobalPosition) * Speed;
                    break;
                default:
                    break;
            }
        }
        else
        {
            Velocity = Velocity.MoveToward(Vector2.Zero, Speed);
        }
        MoveAndSlide();
    }

    private void Aim()
    {
        switch (_currPattern.AimData.Aim)
        {
            case AimType.None:
                break;
            case AimType.SetDir:
                RotationDegrees = _currPattern.AimData.Rotation;
                break;
            case AimType.FaceTarget:
                GlobalRotation = GlobalPosition.AngleToPoint(GameManager.Player.GlobalPosition);
                break;
            case AimType.RandomDir:
                Vector2 rotRange = _currPattern.AimData.RotationRange;
                float rot = (float)GD.RandRange(rotRange.X, rotRange.Y);
                if (_currPattern.AimData.IsRelative) rot += RotationDegrees;

                float[] discrete = _currPattern.AimData.DiscreteRot;
                if (discrete.Length > 0)
                {
                    rot += discrete[GD.RandRange(0, discrete.Length - 1)];
                }

                RotationDegrees = rot;
                break;
            default:
                break;
        }
    }

    private void Target()
    {
        TargetTypeData target = _currPattern.TargetData;
        if (_behaviorState == BehaviorState.Casting) target = _currPattern.CurrSequence.TargetData;
        switch (target.Target)
        {
            case TargetType.None:
                break;
            case TargetType.Rotate:
                if (target.Duration < 0 || target.DurationTimer < target.Duration)
                {
                    target.DurationTimer += (float)GetPhysicsProcessDeltaTime();
                    if (target.RotationSpeed != 0)
                    {
                        RotationDegrees += target.RotationSpeed * (float)GetPhysicsProcessDeltaTime();
                    }
                }
                break;
            case TargetType.TrackTarget:
                if (target.Duration < 0 || target.DurationTimer < target.Duration)
                {
                    target.DurationTimer += (float)GetPhysicsProcessDeltaTime();
                    GlobalRotation = RotateToward(GlobalRotation, GlobalPosition.AngleToPoint(GameManager.Player.GlobalPosition), Mathf.DegToRad(target.RotationSpeed) * (float)GetPhysicsProcessDeltaTime());
                }
                break;
            case TargetType.RandomArea:
                break;
            case TargetType.RandomDir:
                Vector2 rotRange = target.RotationRange;
                float rot = (float)GD.RandRange(rotRange.X, rotRange.Y);
                if (target.IsRelative) rot += RotationDegrees;

                float[] discrete = target.DiscreteRot;
                if (discrete.Length > 0)
                {
                    rot += discrete[GD.RandRange(0, discrete.Length - 1)];
                }

                RotationDegrees = rot;
                break;
            default:
                break;
        }
    }

    private void Target(Vector2 position)
    {
        if (true)
        {
            GlobalRotation = GlobalPosition.AngleToPoint(position);
        }
    }

    private static float RotateToward(float from, float to, float delta)
    {
        float toInverse = to;
        if (to > 0) toInverse -= 2 * Mathf.Pi;
        else toInverse += 2 * Mathf.Pi;

        float dist1 = Mathf.Abs(to - from);
        float dist2 = Mathf.Abs(toInverse - from);
        if (dist1 <= dist2) return Mathf.MoveToward(from, to, delta);
        else return Mathf.MoveToward(from, toInverse, delta);
    }

    private void ChoosePattern()
    {
        if (_currPattern != null && _currPattern.DoRepeat)
        {
            _currPattern.Repeat();
            return;
        }
        foreach (Pattern pattern in _patterns)
        {
            pattern.UpdateWeight(currHP / maxHP);
        }
        if(_currPattern != null && _currPattern.NextPattern != null)
        {
            _currPattern = _currPattern.NextPattern;
        }
        else
        {
            float totalWeight = 0f;
            foreach (Pattern pattern in _patterns)
            {
                totalWeight += pattern.CurrWeight;
            }

            Pattern chosen = _currPattern;
            if (_currPattern == null || _patterns.Count <= 3)
            {
                float rng = (float)GD.RandRange(0, totalWeight);
                float cumulative = 0f;
                foreach (Pattern pattern in _patterns)
                {
                    cumulative += pattern.CurrWeight;
                    if (rng <= cumulative)
                    {
                        chosen = pattern;
                        break;
                    }
                }
            }
            else
            {
                while (chosen.Name == _currPattern.Name)
                {
                    float rng = (float)GD.RandRange(0, totalWeight);
                    float cumulative = 0f;
                    foreach (Pattern pattern in _patterns)
                    {
                        cumulative += pattern.CurrWeight;
                        if (rng <= cumulative)
                        {
                            chosen = pattern;
                            break;
                        }
                    }
                }
            }
            
            _currPattern = chosen;
        }
    }

    public async void NextPatternSequence()
    {
        if (!_currPattern.IsLastSequence)
        {
            _currPattern.NextSequence();
            if (_currPattern.CurrSequence.WaitTime > 0)
                await ToSignal(GetTree().CreateTimer(_currPattern.CurrSequence.WaitTime), SceneTreeTimer.SignalName.Timeout);
            AttackGroup atk = _currPattern.CurrSequence.StartAttack(this);
            atk.SetDamage(Damage, CritRate, CritDamage);
            //Set position
            if(_currPattern.TargetData.Target != TargetType.RandomArea)
            {
                if (_currPattern.TargetData.Target == TargetType.TrackTarget && _currPattern.TargetData.SpawnOnTarget) atk.GlobalPosition = GameManager.Player.GlobalPosition;
                else atk.GlobalPosition = GlobalPosition;
            }
            else
            {
                Vector2 areaSize = _currPattern.TargetData.AreaSize;
                Vector2 areaPos = _currPattern.TargetData.AreaPos;
                if (_currPattern.TargetData.IsRelative) areaPos = areaPos + GlobalPosition;
                Vector2 newPos = new Vector2((float)GD.RandRange(areaPos.X - (areaSize.X / 2), areaPos.X + (areaSize.X / 2)), (float)GD.RandRange(areaPos.Y - (areaSize.Y / 2), areaPos.Y + (areaSize.Y / 2)));
                atk.GlobalPosition = newPos;
                Target(newPos);
            }
            if (_currPattern.IsLastSequence && _currPattern.CastTime < 0)
            {
                atk.TreeExited += EndPattern;
                return;
            }
            if (_currPattern.CurrSequence.WaitTime >= 0) NextPatternSequence();
        }
    }

    public void EndPattern()
    {
        ChangeBehaviorState(BehaviorState.Recovery);
        _currPattern.Reset();
    }

    public async void Root(float duration)
    {
        _canMove = false;
        await ToSignal(GetTree().CreateTimer(duration), SceneTreeTimer.SignalName.Timeout);
        _canMove = true;
    }

    public override void Death()
    {
        GameManager.Instance.CallDeferred("SpawnExpOrb", GlobalPosition);
        GameManager.Player.OnEnemyDeath();
        
        base.Death();
    }
}
