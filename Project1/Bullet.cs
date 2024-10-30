using Godot;
using System;
using System.Collections.Generic;

public partial class Bullet : StaticBody2D, IDamage
{
    [Export]
    public float damageMultiplier = 1f;

    [Export]
    public float speed = 200f;

    [Export]
    public float speedMult = 1f;

    [Export]
    public float duration = 5f;

    [Export]
    public bool isAnimated = true;

    [Export]
    public bool doesPenetrate = false;

    [Export]
    public float knockbackMult; //This should be negative and be -1 most of the time unless a single hit needs a lot of knockback

    [Export]
    public float rootDuration;

    [Export]
    public PackedScene spawnOnDeath;

    private float _lifetime = 0;
    private Vector2 _direction => Vector2.Right.Rotated(GlobalRotation);

    private float _damage = 0;
    private float _critRate = 0;
    private float _critDamage = 0;

    private Sprite2D _sprite;

    private List<Node> _alreadyHit; //List of objects already hit once before to prevent multiple hits against the same object

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Sprite2D");

        _alreadyHit = new List<Node>();

        if (isAnimated)
        {
            //Spawn with increased speed and ease back into normal speed + basic squash and stretch animation
            speedMult = 2f;
            Vector2 origSpriteScale = _sprite.Scale;
            _sprite.Scale = new Vector2(_sprite.Scale.X * 1.3f, _sprite.Scale.Y * 0.8f);

            Tween tween = CreateTween().SetParallel(true).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
            tween.TweenProperty(this, "speedMult", 1f, 0.6);
            tween.TweenProperty(_sprite, "scale", origSpriteScale, 0.5);
        }
        
    }

    public override void _PhysicsProcess(double delta)
    {
        KinematicCollision2D collisionInfo = MoveAndCollide(_direction * speed * speedMult * (float)delta);

        if(collisionInfo != null)
        {
            OnCollision(collisionInfo);
        }

    }

    public override void _Process(double delta)
    {
        _lifetime += (float)delta;
        if (_lifetime > duration) OnDeath();
    }

    public void SetDamage(float damage, float critRate, float critDamage)
    {
        _damage = damage;
        _critRate = critRate;
        _critDamage = critDamage;
    }

    private void OnCollision(KinematicCollision2D collision)
    {
        Node other = collision.GetCollider() as Node;
        if (other.IsInGroup("enemy")) 
        {
            if (!_alreadyHit.Contains(other))
            {
                bool isCrit = Entity.RollCrit(_critRate);
                ((Entity)other).TakeDamage(Entity.CalculateDamage(_damage, isCrit, _critDamage) * damageMultiplier);
                if (rootDuration > 0) ((Enemy)other).Root(rootDuration);
                _alreadyHit.Add(other);
            }
            if(knockbackMult != 0)
            {
                ((CharacterBody2D)other).Velocity = collision.GetNormal() * knockbackMult * speed * speedMult;
                ((CharacterBody2D)other).MoveAndSlide();
            }
                
        }
        if(!doesPenetrate) OnDeath();
    }

    public void OnDeath()
    {
        if(spawnOnDeath != null)
        {
            Node2D temp = spawnOnDeath.Instantiate<Node2D>();
            GetTree().CurrentScene.AddChild(temp);
            temp.GlobalPosition = GlobalPosition;
            temp.GlobalRotation = GlobalRotation;
            if (temp is IDamage) ((IDamage)temp).SetDamage(_damage, _critRate, _critDamage);
        }
        QueueFree();
    }
}
