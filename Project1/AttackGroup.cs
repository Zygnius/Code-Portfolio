using Godot;
using System;

[GlobalClass]
public partial class AttackGroup : Node2D, IDamage
{
    [Export]
    private bool _trackPlayer = false;

    [Export]
    public float Speed = 0f;

    [Export]
    public float RotationSpeed = 0f;

    [Export]
    public float MovementDelay = 0f;

    [Export]
    public float MovementTime = 0f; //Unlimited movement time if negative

    private float _timer = 0f;
    private bool _canMove = false;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    public override void _PhysicsProcess(double delta)
    {
        _timer += (float)delta;
        if (!_canMove)
        {
            if (_timer >= MovementDelay)
            {
                _canMove = true;
                _timer = 0f;
            }
        }
        else if((Speed != 0 || RotationSpeed != 0) && (MovementTime < 0 || _timer < MovementTime))
        {
            if (_trackPlayer)
            {
                LookAt(GameManager.Player.GlobalPosition);
                GlobalPosition = GlobalPosition.MoveToward(GameManager.Player.GlobalPosition, Speed * (float)delta);
            }
            else
            {
                GlobalPosition = GlobalPosition.MoveToward(Vector2.Right.Rotated(GlobalRotation) * Speed, Speed * (float)delta);
                RotationDegrees += RotationSpeed * (float)delta;
            }
        }

        if(GetChildCount() == 0) QueueFree();
        
    }

    public void SetDamage(float damage, float critRate, float critDamage)
    {
        Godot.Collections.Array<Node> children = GetChildren();
        foreach(Node node in children)
        {
            if (node is IDamage) ((IDamage)node).SetDamage(damage, critRate, critDamage);
        }
    }
}
