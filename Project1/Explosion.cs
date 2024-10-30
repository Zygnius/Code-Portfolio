using Godot;
using System;

public partial class Explosion : Area2D, IDamage
{
    [Export]
    public float damageMultiplier = 1f;

    [Export]
    public float duration = 1f;

    [Export]
    public float maxSize;

    [Export]
    private bool _isWave = false;

    private float _lifetime = 0;

    private float _damage = 0;
    private float _critRate = 0;
    private float _critDamage = 0;

    private Sprite2D _solid;
    private Sprite2D _wave;
    private CollisionShape2D _collisionShape;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _solid = GetNode<Sprite2D>("Solid");
        _wave = GetNode<Sprite2D>("Wave");
        SetIsWave(_isWave);
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        Tween tween = CreateTween().SetParallel(true).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(_solid, "scale", new Vector2(maxSize, maxSize), duration);
        tween.TweenProperty(_wave, "scale", new Vector2(maxSize, maxSize), duration);
        tween.TweenProperty(_collisionShape, "shape:radius", maxSize * 100, duration);
        tween.Chain().SetTrans(Tween.TransitionType.Linear).TweenProperty(_solid, "self_modulate:a", 0, 0.1);
        tween.SetTrans(Tween.TransitionType.Linear).TweenProperty(_wave, "self_modulate:a", 0, 0.1);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        _lifetime += (float)delta;
        if (_lifetime > duration + 0.1) OnDeath();
    }
    public void SetDamage(float damage, float critRate, float critDamage)
    {
        _damage = damage;
        _critRate = critRate;
        _critDamage = critDamage;
    }

    public void SetAlpha(float alpha)
    {
        _solid.SelfModulate = new Color(_solid.SelfModulate, alpha);
        _wave.SelfModulate = new Color(_wave.SelfModulate, alpha);
    }

    public void SetIsWave(bool isWave)
    {
        _isWave = isWave;

        _wave.Visible = _isWave;
        _solid.Visible = !_isWave;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("enemy"))
        {
            bool isCrit = Entity.RollCrit(_critRate);
            ((Entity)body).TakeDamage(Entity.CalculateDamage(_damage, isCrit, _critDamage) * damageMultiplier);
        }
    }

    public void OnDeath()
    {
        QueueFree();
    }
}
