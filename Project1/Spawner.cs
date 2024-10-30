using Godot;
using System;

public partial class Spawner : Node
{
    [Export]
    private PackedScene _enemyTemplate;

    [Export]
    private float _spawnRate;

    [Export]
    private float _spawnRange;

    private float timer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }
    
    public override void _PhysicsProcess(double delta)
    {
        timer += (float)delta;
        if(timer > (1 / _spawnRate))
        {
            Spawn();
            timer = 0;
        }
    }

    private void Spawn()
    {
        //spawn enemy in a random location between viewportGetViewport().GetVisibleRect().Size. size + margin and previous + _spawnRange
        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
        float margin = 50f;

        RandomNumberGenerator rng = new RandomNumberGenerator();
        Vector2 randomLoc = new Vector2(rng.RandfRange(-1 * _spawnRange, _spawnRange), rng.RandfRange(-1 * _spawnRange, _spawnRange));
        Vector2 spawnLoc = new Vector2();
        if(randomLoc.X < 0)
        {
            spawnLoc.X = (-1 * viewportSize.X) - margin + randomLoc.X;
        }
        else
        {
            spawnLoc.X = viewportSize.X + margin + randomLoc.X;
        }
        if (randomLoc.Y < 0)
        {
            spawnLoc.Y = (-1 * viewportSize.Y) - margin + randomLoc.Y;
        }
        else
        {
            spawnLoc.Y = viewportSize.Y + margin + randomLoc.Y;
        }

        Enemy enemy = _enemyTemplate.Instantiate<Enemy>();
        AddChild(enemy);
        enemy.GlobalPosition = spawnLoc;
    }
}
