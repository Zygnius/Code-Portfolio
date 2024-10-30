using Godot;
using System;
using Godot.Collections;
using System.Collections.Generic;

[GlobalClass]
[Tool]
public partial class Upgrade : Resource
{
    [Export]
    private string _id; //Used to identify mutually exclusive upgrades
    public string Id => _id;

    [Export]
    private string _name;
    public string Name => _name;

    [Export]
    private string _description;
    public string Description => _description;

    [Export]
    private int _maxLevel = 1;
    public int MaxLevel => _maxLevel;

    [Export]
    private StatName _effect;
    public StatName Effect => _effect;

    [Export]
    private bool _isMult;
    public bool IsMult => _isMult;

    [Export]
    private float _effectAmount;
    public float EffectAmount => _effectAmount;

    [ExportGroup("Unlock Conditions")]
    [Export]
    private bool _enabled = true;
    public bool Enabled => _enabled;

    [Export]
    private float _minPlayerLevel = 0;
    public float MinPlayerLevel => _minPlayerLevel;

    private int _currLevel = 0;
    public int CurrLevel => _currLevel;

    public Upgrade()
    {
    }

    public bool ApplyUpgrade(PlayerData data)
    {
        if (_currLevel < _maxLevel)
        {

            data.AddStatUpgrade(new KeyValuePair<StatName, float>(_effect, _effectAmount));
            _currLevel++;
            if (_currLevel >= _maxLevel) UpgradeManager.Instance.RemoveFromPool(this); //Remove upgrade from pool
            return true;
        }
        else return false;
    }

    //Increments upgrade level if not at max level, returning true if level incremented
    public bool IncreaseLevel()
    {
        if (IsMaxLevel()) return false;
        else
        {
            _currLevel++;
            return true;
        }
    }

    public void SyncLevel(Upgrade upgrade)
    {
        _currLevel = upgrade.CurrLevel;
    }

    public bool IsMaxLevel()
    {
        return _currLevel >= _maxLevel;
    }
}
