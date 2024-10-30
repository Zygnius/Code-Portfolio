using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerData : EntityData
{
    [Export]
    private AbilityData[] _abilityData;
    public AbilityData[] AbilityData => _abilityData;

    private List<KeyValuePair<StatName, float>> _upgradesFlat = new List<KeyValuePair<StatName, float>>();
    private List<KeyValuePair<StatName, float>> _upgradesMult = new List<KeyValuePair<StatName, float>>();

    public override float GetStat(StatName name)
    {
        float flatupgrades = 0;
        float multupgrades = 0;
        foreach (KeyValuePair<StatName, float> item in _upgradesFlat)
        {
            if (item.Key == name) flatupgrades += item.Value;
        }
        foreach (KeyValuePair<StatName, float> item in _upgradesMult)
        {
            if (item.Key == name) multupgrades += item.Value;
        }

        float flatbuffs = 0;
        float multbuffs = 0;
        foreach (KeyValuePair<StatName, float> item in _buffsFlat)
        {
            if (item.Key == name) flatbuffs += item.Value;
        }
        foreach (KeyValuePair<StatName, float> item in _buffsMult)
        {
            if (item.Key == name) multbuffs += item.Value;
        }

        return (GetBaseStat(name) + flatbuffs + flatupgrades) * (multbuffs + multupgrades);

    }

    public void AddStatUpgrade(KeyValuePair<StatName, float> buff, bool isMult = false)
    {
        if (isMult)
            _upgradesMult.Add(buff);
        else
            _upgradesFlat.Add(buff);
    }

    public void RemoveStatUpgrade(KeyValuePair<StatName, float> buff, bool isMult = false)
    {
        if (isMult)
            _upgradesMult.Remove(buff);
        else
            _upgradesFlat.Remove(buff);
    }
}
