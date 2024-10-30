using Godot;
using System;

public partial class Entity : CharacterBody2D
{
    public float maxHP => _entityData.MaxHP;

    protected float currHP;

    public float Damage => _entityData.Damage;
    public float CritRate => _entityData.CritRate;
    public float CritDamage => _entityData.CritDamage;
    public float Speed => _entityData.Speed;

    [Export]
    protected EntityData _entityData;

    private bool _isInvincible = false;
    private float _timer = 0f;
    private bool _isDead = false;

    [Signal]
    public delegate void HealthChangedEventHandler();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        Reset();
    }

    public virtual void Reset()
    {
        currHP = maxHP;
    }


    public void ChangeHP(float amount)
    {
        if (_isDead) return;

        if (currHP + amount <= 0)
        {
            currHP = 0;
            Death();
        }
        else if (currHP + amount > maxHP)
        {
            currHP = maxHP;
        }
        else
        {
            currHP += amount;
        }
        EmitSignal(SignalName.HealthChanged);
    }

    public void TakeDamage(float amount)
    {
        if (_isInvincible) return;

        float damage = amount - _entityData.Armor;
        if (damage < 0) damage = 1;
        damage = damage * (1 - _entityData.DamageReduction);
        ChangeHP(-1 * damage);
        GameManager.Player.OnDamageDealt(damage);
        if (_entityData.OnHitIFrameTime > 0)
        {
            _isInvincible = true;
            SceneTreeTimer timer = GetTree().CreateTimer(_entityData.OnHitIFrameTime);
            timer.Timeout += EndInvincibility;
        }
    }

    private void EndInvincibility()
    {
        _isInvincible = false;
    }

    /// <summary>
    /// Returns HP values as a Vector2(currHP, maxHP)
    /// </summary>
    /// <returns></returns>
    public Vector2 GetHP()
    {
        return new Vector2(currHP, maxHP);
    }

    /// <summary>
    /// Randomly returns a boolean based off of crit rate
    /// </summary>
    /// <returns></returns>
    public static bool RollCrit(float critRate)
    {
        return GD.Randf() <= critRate;
    }

    /// <summary>
    /// Calculates a damage value based off of offensive stats. Randomly determines crit based off of crit stats if isCrit is not provided.
    /// </summary>
    /// <returns></returns>
    public static float CalculateDamage(float damage, bool isCrit = false, float critDamage = 0f)
    {
        if (isCrit) return damage * (1 + critDamage);
        else return damage;
    }

    public static float CalculateDamage(float damage, float critRate, float critDamage)
    {
        return CalculateDamage(damage, RollCrit(critRate), critDamage);
    }

    public virtual void Death()
    {
        _isDead = true;
        QueueFree();
    }
}
