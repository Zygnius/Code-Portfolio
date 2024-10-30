using Godot;
using System;

public partial class Boss : Enemy
{
    [Export]
    private Godot.Collections.Array<float> _phases; //Maybe make phases into a resource that incorporates how much HP it has, the HP thresholds, and the patterns to run during thresholds

    [Export]
    private float _tenacity; //Implement later
}
