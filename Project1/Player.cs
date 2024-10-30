using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

public partial class Player : Entity
{
    private PlayerData _playerData => (PlayerData)_entityData;
    public PlayerData PlayerData => _playerData;
    private AbilityData[] _abilityData => _playerData.AbilityData; //{Ability1, Ability2, Ability3, Ability4, AbilityMovement, AbilityUlt}

    private Vector2 _experience; //(currEXP, totalEXP)

    private int _level;
    public int Level => _level;

    private bool _canMove = true;

    private float _speedMult = 1f;

    private int _currAbility = 0;

    private CastState _castState = CastState.Idle;

    private Vector2 _channelStatus;
    /// <summary>
    /// Status of ability channeling in a Vector2 of (remaining channel time, total channel time)
    /// </summary>
    public Vector2 channelStatus => _channelStatus;


    [Signal]
    public delegate void ExpChangedEventHandler();

    [Signal]
    public delegate void LevelChangedEventHandler();

    [Signal]
    public delegate void AbilityChannelStartedEventHandler();

    enum CastState
    {
        Idle,
        Channel,
        Cast,
        Recovery
    }

    public Player()
    {
        GameManager.Player = this;

        
    }

    public override void _Ready()
    {
        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        _level = 1;
        _experience = Vector2.Zero;
    }

    public override void _PhysicsProcess(double delta)
    {
        
        
    }

    public override void _Process(double delta)
    {
        if (_canMove)
        {
            Vector2 velocity = Velocity;

            // Get the input direction and handle the movement/deceleration.
            Vector2 direction = Input.GetVector("player_left", "player_right", "player_up", "player_down");
            if (direction != Vector2.Zero)
            {
                velocity = direction * Speed * _speedMult;
            }
            else
            {
                velocity = Velocity.MoveToward(Vector2.Zero, Speed);
            }

            Velocity = velocity;
            
        }
        MoveAndSlide();

        //Rotate character to face mouse
        Rotation = GlobalPosition.AngleToPoint(GetGlobalMousePosition());

        if (Input.IsActionPressed("player_ability1"))
        {
            CastAbility(0);
        }

        //tick cooldowns
        for (int i = 0; i < _abilityData.Length; i++)
        {
            TickCooldown(i, (float)delta);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("player_ability2"))
        {
            CastAbility(1);
        }
        if (@event.IsActionPressed("player_ability3"))
        {
            CastAbility(2);
        }
        if (@event.IsActionPressed("player_ability4"))
        {
            CastAbility(3);
        }
        if (@event.IsActionPressed("player_abilityMovement"))
        {
            CastAbility(4);
        }
        if (@event.IsActionPressed("player_abilityUlt"))
        {
            CastAbility(5);
        }
    }

    private void TickCooldown(int abilityIndex, float amount)
    {
        if (_abilityData[abilityIndex].currCooldown > 0)
        {
            _abilityData[abilityIndex].currCooldown -= amount;
            if (_abilityData[abilityIndex].currCooldown < 0)
            {
                _abilityData[abilityIndex].currCooldown = 0;
            }
        }
    }

    private Vector2 GetTarget(TargettingType targettingType)
    {
        switch (targettingType)
        {
            case TargettingType.Self:
                return GlobalPosition;
            case TargettingType.MousePos:
                return GetGlobalMousePosition();
            case TargettingType.MovementDir:
                return Input.GetVector("player_left", "player_right", "player_up", "player_down");
            default:
                return GlobalPosition;
        }
    }

    private void CancelChannel()
    {
        for (int i = 0; i < _abilityData.Length; i++)
        {
            if (_abilityData[i].channeling) _abilityData[i].channeling = false;
        }
        SetSpeedMult(1f);
        _castState = CastState.Idle;
        _channelStatus = Vector2.Zero;
    }

    private void SetSpeedMult(float percent)
    {
        _speedMult = percent;

        //If _canMove is false, that means something else is affecting Velocity so it shouldn't be changed
        if(_canMove) Velocity *= _speedMult;
    }

    private async void CastAbility(int abilityIndex)
    {
        //Check if player can cast
        if (_abilityData[abilityIndex].IsOnCooldown() || _castState == CastState.Cast)
        {
            return;
        }

        //Check if player is channeling something else
        if(_castState == CastState.Channel)
        {
            if (_abilityData[abilityIndex].channeling) return;
            else
            {
                CancelChannel();
            }
        }

        //Channel
        if (_abilityData[abilityIndex].channelTime > 0)
        {
            _castState = CastState.Channel;
            _abilityData[abilityIndex].channeling = true;
            SetSpeedMult(_abilityData[abilityIndex].channelSpeedMult);
            float channelTimer = _abilityData[abilityIndex].channelTime;
            _channelStatus = new Vector2(channelTimer, channelTimer);
            EmitSignal(SignalName.AbilityChannelStarted);
            while (channelTimer > 0)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                if (_abilityData[abilityIndex].channeling)
                {
                    channelTimer -= (float)GetProcessDeltaTime();
                    _channelStatus.X = channelTimer;
                }
                else break;
            }
            if (!_abilityData[abilityIndex].channeling) return;
            else _abilityData[abilityIndex].channeling = false;
        }

        //Cast
        Vector2 target = GetTarget(_abilityData[abilityIndex].targettingType);
        switch (abilityIndex)
        {
            case 0:
                Ability1(target);
                break;
            case 1:
                Ability2(target);
                break;
            case 2:
                Ability3(target);
                break;
            case 3:
                Ability4(target);
                break;
            case 4:
                AbilityMovement(target);
                break;
            case 5:
                AbilityUlt(target);
                break;
            default:
                Ability1(target);
                break;
        }
        if(_abilityData[abilityIndex].castTime > 0)
        {
            _castState = CastState.Cast;
            SetSpeedMult(_abilityData[abilityIndex].castSpeedMult);
            float castTimer = _abilityData[abilityIndex].castTime;
            while ( castTimer > 0)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                if (_castState == CastState.Cast) castTimer -= (float)GetProcessDeltaTime();
                else break;
            }
            if (_castState != CastState.Cast) return;
        }

        //Recovery
        if (_abilityData[abilityIndex].recoveryTime > 0)
        {
            _castState = CastState.Recovery;
            SetSpeedMult(_abilityData[abilityIndex].recoverySpeedMult);
            float recoveryTimer = _abilityData[abilityIndex].recoveryTime;
            while (recoveryTimer > 0)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                if (_castState == CastState.Recovery) recoveryTimer -= (float)GetProcessDeltaTime();
                else break;
            }
            if(_castState != CastState.Recovery) return;
        }

        //Return to Idle
        _castState = CastState.Idle;
        SetSpeedMult(1f);
    }

    private void Ability1(Vector2 target)
    {
        if (!_abilityData[0].IsOnCooldown())
        {
            _abilityData[0].StartCooldown();
            Bullet instance = _abilityData[0].abilityScene.Instantiate<Bullet>();
            Vector2 direction = (target - GlobalPosition).Normalized();
            instance.GlobalRotation = direction.Angle();
            instance.Position = Position;
            instance.SetDamage(Damage, CritRate, CritDamage);
            GetTree().CurrentScene.AddChild(instance);
        }

    }
    private void Ability2(Vector2 target)
    {
        _abilityData[1].StartCooldown();
        Bullet instance = _abilityData[1].abilityScene.Instantiate<Bullet>();
        Vector2 direction = (target - GlobalPosition).Normalized();
        instance.GlobalRotation = direction.Angle();
        instance.Position = Position;
        instance.SetDamage(Damage, CritRate, CritDamage);
        GetTree().CurrentScene.AddChild(instance);
    }

    private void Ability3(Vector2 target)
    {
        _abilityData[2].StartCooldown();
        Attack instance = _abilityData[2].abilityScene.Instantiate<Attack>();
        instance.GlobalPosition = target;
        instance.GlobalRotation = GlobalRotation;
        instance.SetDamage(Damage, CritRate, CritDamage);
        GetTree().CurrentScene.AddChild(instance);
    }

    private void Ability4(Vector2 target)
    {
        _abilityData[3].StartCooldown();
        Bullet instance = _abilityData[3].abilityScene.Instantiate<Bullet>();
        Vector2 direction = (target - GlobalPosition).Normalized();
        instance.GlobalRotation = direction.Angle();
        instance.Position = Position;
        instance.SetDamage(Damage, CritRate, CritDamage);
        GetTree().CurrentScene.AddChild(instance);
    }

    private void AbilityMovement(Vector2 direction)
    {
        if (_canMove && !_abilityData[4].IsOnCooldown())
        {
            _abilityData[4].StartCooldown();
            _canMove = false;
            Velocity = direction * 500f;
            EndAbilityMovement();
        }
        
        
    }

    private async Task EndAbilityMovement()
    {
        await ToSignal(GetTree().CreateTimer(_abilityData[4].castTime), SceneTreeTimer.SignalName.Timeout);
        Velocity = Vector2.Zero;
        _canMove = true;
    }

    private void AbilityUlt(Vector2 target)
    {
        _abilityData[5].StartCooldown();
        Explosion screenwide = _abilityData[5].abilityScene.Instantiate<Explosion>();
        screenwide.Position = target;
        screenwide.Rotation = Rotation;
        screenwide.damageMultiplier = 5f;
        screenwide.maxSize = GetViewportRect().Size.X / 150;
        screenwide.CallDeferred("SetIsWave", true);
        screenwide.CallDeferred("SetAlpha", 0.4f);
        screenwide.SetDamage(Damage, CritRate, CritDamage);
        GetTree().CurrentScene.AddChild(screenwide);
        Explosion instance = _abilityData[5].abilityScene.Instantiate<Explosion>();
        instance.Position = target;
        instance.Rotation = Rotation;
        instance.damageMultiplier = 5f;
        instance.maxSize = 3f;
        instance.SetDamage(Damage, CritRate, CritDamage);
        GetTree().CurrentScene.AddChild(instance);
    }

    public void OnDamageDealt(float amount)
    {
        //Reduce Ult cooldown based on damage dealt
        TickCooldown(5, 0.01f * amount);
    }

    public void OnEnemyDeath()
    {
        //Reduce Ult cooldown by a certain amount when the player kills an enemy
        TickCooldown(5, 0.5f);
    }

    public override void Death()
    {
        GD.Print("You Died");
        //TODO
    }

    private int CalcExpForLevel()
    {
        return 5 * Mathf.RoundToInt(Mathf.Pow(1.2f, _level));
    }

    public void AddExperience(int amount)
    {
        _experience.X += amount;
        _experience.Y += amount;
        EmitSignal(SignalName.ExpChanged);
        while(_experience.X >= CalcExpForLevel())
        {
            _experience.X -= CalcExpForLevel();
            _level++;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        GD.Print("Level Up! Current LV: " + _level);
        //TODO
        EmitSignal(SignalName.LevelChanged);
    }

    /// <summary>
    /// Returns current experience and total experience needed for the next level in a Vector2
    /// </summary>
    /// <returns></returns>
    public Vector2 GetExp()
    {
        return new Vector2(_experience.X, CalcExpForLevel());
    }
}
