using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class UpgradeManager : Node
{
    public static UpgradeManager Instance;

    [Export]
    private Godot.Collections.Array<Upgrade> _upgradeList; //All upgrades in the game

    [Export]
    private bool _updateUpgradeList
    {
        get { return false; }
        set
        {
            if (value) UpdateUpgradeList();
        }
    }

    private List<Upgrade> _mainUpgradePool; //Only upgrades currently available to be rolled
    private List<Upgrade> _lockedUpgradePool; //Upgrades that are not currently available to be rolled

    public UpgradeManager()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            QueueFree();
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    //Updates upgrade list based off of Upgrade Resources in "res://Resources/Upgrades"
    private void UpdateUpgradeList()
    {
        if (Engine.IsEditorHint())
        {
            _upgradeList = new Godot.Collections.Array<Upgrade>();
            using DirAccess dir = DirAccess.Open("res://Resources/Upgrades");
            if (dir != null)
            {
                dir.ListDirBegin();
                string fileName = dir.GetNext();
                while (fileName != "")
                {
                    if (!dir.CurrentIsDir())
                    {
                        Resource temp = GD.Load<Upgrade>("res://Resources/Upgrades/" + fileName);
                        _upgradeList.Add(temp as Upgrade);
                    }
                    fileName = dir.GetNext();
                }
            }
            else GD.PushError("Error occurred while trying to access path \"Resources/Upgrades\"");
        }
    }

    private void InitializePool()
    {
        //Copy enabled upgrades to _lockedUpgradePool
        foreach (Upgrade item in _upgradeList)
        {
            if (item.Enabled) _lockedUpgradePool.Add(item);
        }

        UpdatePool();
    }

    private void UpdatePool()
    {
        //Iterate through _lockedUpgradePool and add upgrades to pool if they meet certain conditions
        foreach (Upgrade item in _lockedUpgradePool)
        {
            if (GameManager.Player.Level >= item.MinPlayerLevel) AddToPool(item);
        }
    }

    public void RemoveFromPool(Upgrade upgrade)
    {
        foreach (Upgrade item in _mainUpgradePool)
        {
            if (item.Id == upgrade.Id)
            {
                _mainUpgradePool.Remove(item);
            }
        }
        foreach (Upgrade item in _lockedUpgradePool)
        {
            if (item.Id == upgrade.Id)
            {
                _lockedUpgradePool.Remove(item);
            }
        }
        _mainUpgradePool.Remove(upgrade);
    }

    public void AddToPool(Upgrade upgrade)
    {
        _mainUpgradePool.Add(upgrade);
        _lockedUpgradePool.Remove(upgrade);
    }

    public List<Upgrade> RollUpgradePool(int amount)
    {
        if (amount > _mainUpgradePool.Count) amount = _mainUpgradePool.Count; //Prevents rolling more upgrades than available

        List<Upgrade> output = new List<Upgrade>();
        List<int> previousRolls = new List<int>();
        for(int i = 0; i < amount; i++)
        {
            int rng = GD.RandRange(0, _mainUpgradePool.Count);
            while (previousRolls.Contains(rng))
            {
                rng = GD.RandRange(0, _mainUpgradePool.Count);
            }
            output.Add(_mainUpgradePool[rng]);
            previousRolls.Add(rng);
        }
        return output;
    }

    public void ApplyUpgrade(Upgrade upgrade)
    {
        if (!upgrade.IsMaxLevel())
        {
            GameManager.Player.PlayerData.AddStatUpgrade(new KeyValuePair<StatName, float>(upgrade.Effect, upgrade.EffectAmount), upgrade.IsMult);
            upgrade.IncreaseLevel();
            if (upgrade.IsMaxLevel()) RemoveFromPool(upgrade); //Remove upgrade from pool
            else
            {
                foreach (Upgrade item in _mainUpgradePool)
                {
                    if (item.Id == upgrade.Id)
                    {
                        item.SyncLevel(upgrade);
                    }
                }
                foreach (Upgrade item in _lockedUpgradePool)
                {
                    if (item.Id == upgrade.Id)
                    {
                        item.SyncLevel(upgrade);
                    }
                }
            }
        }
        else GD.PrintErr("Attempted to apply upgrade at max level, this should not be possible");
    }
}
