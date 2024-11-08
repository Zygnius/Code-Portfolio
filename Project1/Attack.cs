using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class Attack : Area2D, IDamage
{
    [Export]
    private float damageMultiplier = 1f;

    [Export]
    private bool _singleHit = true; //Whether to allow the attack to hit the same entity multiple times

    [Export]
    private float _delayTime = 0f;

    [Export]
    private PackedScene _spawnOnDeath;

    [Export]
    private bool _updateCollisionShape
    {
        get { return false; }
        set
        {
            if (value) UpdateCollisionShape();
        }
    }

    private float _damage;
    private float _critRate;
    private float _critDamage;

    private Telegraph _telegraph;
    private CollisionShape2D _collisionShape;
    private CollisionPolygon2D _collisionPoly;

    private float _timer = 0f;

    private int _tickCounter = 0;
    private List<Entity> _intersectedEntities = new List<Entity>();
    


    public override void _Ready()
    {
        _telegraph = GetNode<Telegraph>("Telegraph");
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        _collisionPoly = GetNode<CollisionPolygon2D>("CollisionPolygon2D");
        if (!Engine.IsEditorHint())
        {
            _telegraph.TelegraphFinished += Activate;
            _telegraph.TreeExited += OnAttackFinish;
        }
        else
            UpdateCollisionShape();
    }

    public override void _Process(double delta)
    {
        if (_timer >= _delayTime)
        {
            _telegraph.Paused = false;
        }
        else
        {
            _timer += (float)delta;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        //Handle damaging of same entity multiple times if singleHit is false
        if (!_singleHit && _intersectedEntities.Count > 0)
        {
            _tickCounter++;
            //Tick set to 1/10 of a second, subject to change
            if (_tickCounter >= 6)
            {
                _tickCounter = 0;
                foreach (Entity e in _intersectedEntities)
                {
                    bool isCrit = Entity.RollCrit(_critRate);
                    e.TakeDamage(Entity.CalculateDamage(_damage, isCrit, _critDamage) * damageMultiplier);
                }
            }
        }
    }

    public void SetDamage(float damage, float critRate, float critDamage)
    {
        _damage = damage;
        _critRate = critRate;
        _critDamage = critDamage;
    }

    private void Activate()
    {
        Monitoring = true;
    }

    private void OnAttackFinish()
    {
        if(_spawnOnDeath != null)
        {
            Node2D obj = _spawnOnDeath.Instantiate<Node2D>();
            obj.GlobalPosition = GlobalPosition;
            obj.GlobalRotation = GlobalRotation;
            GetTree().CurrentScene.AddChild(obj);
            if(obj is IDamage) ((IDamage)obj).SetDamage(_damage, _critRate, _critDamage);
        }

        QueueFree();
    }

    //Updates the collider shape based on the visual telegraph shape
    public void UpdateCollisionShape()
    {
        if(_telegraph == null) _telegraph = GetNode<Telegraph>("Telegraph");
        if (_collisionShape == null) _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        if (_collisionPoly == null) _collisionPoly = GetNode<CollisionPolygon2D>("CollisionPolygon2D");

        if (_telegraph.Shape == Telegraph.TelegraphShape.Cone || _telegraph.Shape == Telegraph.TelegraphShape.Arc || _telegraph.Shape == Telegraph.TelegraphShape.Poly)
        {
            _collisionPoly.Polygon = _telegraph.CreateTelegraphPoly();
            _collisionShape.Position = _telegraph.GetTelegraphLocation();
            _collisionShape.Disabled = true;
            _collisionPoly.Disabled = false;
        }
        else
        {
            _collisionShape.Shape = _telegraph.CreateTelegraphShape();
            _collisionShape.Position = _telegraph.GetTelegraphLocation();
            _collisionPoly.Disabled = true;
            _collisionShape.Disabled = false;
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("player") || body.IsInGroup("enemy"))
        {
            bool isCrit = Entity.RollCrit(_critRate);
            ((Entity)body).TakeDamage(Entity.CalculateDamage(_damage, isCrit, _critDamage) * damageMultiplier);

            if (!_singleHit)
            {
                _intersectedEntities.Add(((Entity)body));
            }
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (!_singleHit)
        {
            _intersectedEntities.Remove(((Entity)body));
            if (_intersectedEntities.Count == 0) _tickCounter = 0;
        }
    }
}
