using Godot;
using System;
using Godot.Collections;
using System.Collections.Generic;

[GlobalClass]
public partial class EntityData : Resource
{
    [Export]
    private float _damage = 0f;
    public float Damage => GetStat(StatName.Damage);
    [Export]
    private float _maxHP = 0f;
    public float MaxHP => GetStat(StatName.MaxHP);
    [Export]
    private float _speed = 0f;
    public float Speed => GetStat(StatName.Speed);
    [Export]
    private float _critRate = 0f; //Should be between 0 and 1
    public float CritRate => GetStat(StatName.CritRate);
    [Export]
    private float _critDamage = 0f;
    public float CritDamage => GetStat(StatName.CritDamage);
    [Export]
    private float _armor = 0f;
    public float Armor => GetStat(StatName.Armor);
    [Export]
    private float _damageReduction = 0f; //Should be between 0 and 1
    public float DamageReduction => GetStat(StatName.DamageReduction);
    [Export]
    private float _onHitIFrameTime = 0f;
    public float OnHitIFrameTime => GetStat(StatName.OnHitIFrameTime);

    //Other stat ideas: Range/Area, Cooldown Reduction, Pickup Radius, Duration, Experience, Projectile, Penetration/Pierce, Channel Time, Cast Time, HP Regen, Dodge, Revives, Luck, Rerolls/Banishes/etc, Tenacity


    protected List<KeyValuePair<StatName, float>> _buffsFlat = new List<KeyValuePair<StatName, float>>();
    protected List<KeyValuePair<StatName, float>> _buffsMult = new List<KeyValuePair<StatName, float>>();


    public EntityData()
    {
        
    }

    public virtual float GetStat(StatName name)
    {
        float flatbuffs = 0;
        float multbuffs = 0;
        foreach (KeyValuePair<StatName,float> item in _buffsFlat)
        {
            if (item.Key == name) flatbuffs += item.Value;
        }
        foreach (KeyValuePair<StatName, float> item in _buffsMult)
        {
            if (item.Key == name) multbuffs += item.Value;
        }

        return (GetBaseStat(name) + flatbuffs) * multbuffs;

    }

    public float GetBaseStat(StatName name)
    {
        switch (name)
        {
            case StatName.Damage:
                return _damage;
            case StatName.MaxHP:
                return _maxHP;
            case StatName.Speed:
                return _speed;
            case StatName.CritRate:
                return _critRate;
            case StatName.CritDamage:
                return _critDamage;
            case StatName.Armor:
                return _armor;
            case StatName.DamageReduction:
                return _damageReduction;
            case StatName.OnHitIFrameTime:
                return _onHitIFrameTime;
            default:
                return 0;
        }
    }

    public void AddBuff(KeyValuePair<StatName, float> buff, bool isMult = false)
    {
        if (isMult)
            _buffsMult.Add(buff);
        else
            _buffsFlat.Add(buff);
    }

    public void RemoveBuff(KeyValuePair<StatName, float> buff, bool isMult = false)
    {
        if (isMult)
            _buffsMult.Remove(buff);
        else
            _buffsFlat.Remove(buff);
    }

    public static StatName ParseStatString(string name)
    {
        switch (name.ToLower())
        {
            case "damage":
                return StatName.Damage;
            case "maxhp":
                return StatName.MaxHP;
            case "speed":
                return StatName.Speed;
            case "critrate":
                return StatName.CritRate;
            case "critdamage":
                return StatName.CritDamage;
            case "armor":
                return StatName.Armor;
            case "damagereduction":
                return StatName.DamageReduction;
            case "onhitiframetime":
                return StatName.OnHitIFrameTime;
            default:
                return StatName.None;
        }
    }
    
}

public enum StatName
{
    None,
    Damage,
    MaxHP,
    Speed,
    CritRate,
    CritDamage,
    Armor,
    DamageReduction,
    OnHitIFrameTime
}
