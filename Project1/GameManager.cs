using Godot;
using System;
using static System.Net.Mime.MediaTypeNames;

public partial class GameManager : Node
{
    public static GameManager Instance;

    public static Player Player;

    public Node ExpDrops;

    private PackedScene expOrbTemplate;

    public GameManager()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            QueueFree();
        }

        ExpDrops = new Node();
        ExpDrops.Name = "ExperienceDrops";

        expOrbTemplate = GD.Load<PackedScene>("res://Scenes/ExperienceOrb.tscn");

    }
    public override void _Ready()
    {

        GetTree().CurrentScene.AddChild(ExpDrops);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        
    }

    /// <summary>
    /// Spawns an experience orb at specified GlobalPosition.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public ExperienceOrb SpawnExpOrb(Vector2 position)
    {
        ExperienceOrb expOrb = expOrbTemplate.Instantiate<ExperienceOrb>();
        ExpDrops.AddChild(expOrb);
        expOrb.GlobalPosition = position;
        return expOrb;
    }
}
