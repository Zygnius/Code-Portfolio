using Godot;
using System;

public partial interface IDamage
{
    public void SetDamage(float damage, float critRate, float critDamage);
}
