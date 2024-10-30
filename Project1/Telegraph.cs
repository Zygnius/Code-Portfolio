using Godot;
using System;
using Godot.Collections;
using System.Reflection.Metadata;

[Tool]
public partial class Telegraph : Node2D
{
    
    private TelegraphShape _shape;
    [Export]
    private TelegraphShape shape
    {
        get => _shape;
        set
        {
            if (_shape != value)
            {
                _shape = value;
                ChangeShape();
            }
        }
    }
    public TelegraphShape Shape => _shape;

    [Export]
    private float _outlineWidth;

    [Export]
    private Color _outlineColor;

    [Export]
    private Color _unfilledColor;

    [Export]
    private Color _fillColor;

    [Export]
    private Godot.Collections.Dictionary _shapeParameters;

    [Export]
    private float _duration;

    [Export]
    private float _activeDuration = 0.1f;

    [Export]
    private bool _isStatic = false;

    [Export]
    private bool _antialiased = true;

    private bool _previewInEditor;
    [Export]
    private bool previewInEditor
    {
        get => _previewInEditor;
        set
        {
            if (_previewInEditor != value)
            {
                _previewInEditor = value;
                if (!value) _timer = 0f;
            }
        }
    }

    [Export]
    private bool _startUnpaused = false; //Should default to false unless Telegraph is not activated by another class

    [Export]
    private bool _showWhilePaused = false;

    public bool Paused = true;
    private float _timer;

    [Signal]
    public delegate void TelegraphFinishedEventHandler();

    public enum TelegraphShape
    {
        Circle,
        Rect,
        Cone,
        Arc,
        Poly
    }

    public Telegraph()
    {
        _shapeParameters = new Godot.Collections.Dictionary();
        _timer = 0;
    }

    private void ChangeShape()
    {
        _shapeParameters = new Godot.Collections.Dictionary();
        switch (_shape)
        {
            case TelegraphShape.Circle:
                _shapeParameters["radius"] = 50f;
                break;
            case TelegraphShape.Rect:
                _shapeParameters["width"] = 50f;
                _shapeParameters["length"] = 100f;
                break;
            case TelegraphShape.Cone:
                _shapeParameters["radius"] = 80f;
                _shapeParameters["angle"] = 60f;
                break;
            case TelegraphShape.Arc:
                _shapeParameters["inner-radius"] = 30f;
                _shapeParameters["outer-radius"] = 80f;
                _shapeParameters["angle"] = 60f;
                break;
            case TelegraphShape.Poly:
                _shapeParameters["points"] = new Godot.Collections.Array<Vector2>();
                break;
            default:
                break;
        }
        QueueRedraw();
    }

    private void DrawTelegraph()
    {
        if(_shapeParameters == null || _shapeParameters.Count == 0) return;
        switch (_shape)
        {
            case TelegraphShape.Circle:
                DrawCircleTelegraph();
                break;
            case TelegraphShape.Rect:
                DrawRectTelegraph();
                break;
            case TelegraphShape.Cone:
                DrawConeTelegraph();
                break;
            case TelegraphShape.Arc:
                DrawArcTelegraph();
                break;
            case TelegraphShape.Poly:
                DrawPolyTelegraph();
                break;
            default:
                break;
        }
    }

    private void DrawCircleTelegraph()
    {
        if(_timer <= _duration) 
        {
            DrawCircle(Position, (float)_shapeParameters["radius"], _unfilledColor, true, -1, _antialiased);
            if(!_isStatic)
                DrawCircle(Position, (float)_shapeParameters["radius"] * (_timer / _duration), _fillColor, true, -1, _antialiased);
            DrawCircle(Position, (float)_shapeParameters["radius"], _outlineColor, false, _outlineWidth, _antialiased);
        }
        else
        {
            DrawCircle(Position, (float)_shapeParameters["radius"], _outlineColor, true, -1, _antialiased);
        }
    }

    private void DrawRectTelegraph()
    {
        Rect2 rect = new Rect2(0, (float)_shapeParameters["width"] / -2, (float)_shapeParameters["length"], (float)_shapeParameters["width"]);
        if (_timer <= _duration)
        {
            DrawRect(rect, _unfilledColor, true, -1, _antialiased);
            if (!_isStatic)
            {
                DrawRect(new Rect2(rect.Position, new Vector2((float)_shapeParameters["length"] * (_timer / _duration), rect.Size.Y)), _fillColor, true, -1, _antialiased);
            }
            DrawRect(rect, _outlineColor, false, _outlineWidth, _antialiased);
        }
        else
        {
            DrawRect(rect, _outlineColor, true);
        }
    }

    private void DrawConeTelegraph()
    {
        if (_timer <= _duration)
        {
            DrawCone((float)_shapeParameters["radius"], (float)_shapeParameters["angle"], _unfilledColor, true, -1, _antialiased);
            if (!_isStatic)
                DrawCone((float)_shapeParameters["radius"] * (_timer / _duration), (float)_shapeParameters["angle"], _fillColor, true, -1, _antialiased);
            DrawCone((float)_shapeParameters["radius"], (float)_shapeParameters["angle"], _outlineColor, false, _outlineWidth, _antialiased);
        }
        else
        {
            DrawCone((float)_shapeParameters["radius"], (float)_shapeParameters["angle"], _outlineColor, true, -1, _antialiased);
        }
    }

    private void DrawArcTelegraph()
    {
        if (_timer <= _duration)
        {
            DrawArc((float)_shapeParameters["inner-radius"], (float)_shapeParameters["outer-radius"], (float)_shapeParameters["angle"], _unfilledColor, true, -1, _antialiased);
            if (!_isStatic)
            {
                float expandingRadius = (((float)_shapeParameters["outer-radius"] - (float)_shapeParameters["inner-radius"]) * (_timer / _duration)) + (float)_shapeParameters["inner-radius"];
                DrawArc((float)_shapeParameters["inner-radius"], expandingRadius, (float)_shapeParameters["angle"], _fillColor, true, -1, _antialiased);
            }
            DrawArc((float)_shapeParameters["inner-radius"], (float)_shapeParameters["outer-radius"], (float)_shapeParameters["angle"], _outlineColor, false, _outlineWidth, _antialiased);
        }
        else
        {
            DrawArc((float)_shapeParameters["inner-radius"], (float)_shapeParameters["outer-radius"], (float)_shapeParameters["angle"], _outlineColor, true, -1, _antialiased);
        }
    }

    private void DrawPolyTelegraph()
    {
        if (_timer <= _duration)
        {
            if (!_isStatic)
            {

            }
        }
        else
        {

        }
    }

    public override void _Draw()
    {
        if (_shapeParameters.Count > 0 && (!Paused || _showWhilePaused))
            DrawTelegraph();
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (_startUnpaused)
            Paused = false;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint() || _previewInEditor)
        {
            if (!Paused || _previewInEditor)
            {
                _timer += (float)delta;
                QueueRedraw();

                if (_timer > _duration) EmitSignal(SignalName.TelegraphFinished);
                if (_timer > _duration + _activeDuration) FinishTelegraph();
            }
            else if (_showWhilePaused)
            {
                QueueRedraw();
            }
        }
    }

    private void FinishTelegraph()
    {
        if (_previewInEditor)
        {
            _timer = 0;
            _previewInEditor = false;
        }
        else
        {
            
            QueueFree();
        }
    }

    public Shape2D CreateTelegraphShape()
    {
        if (_shapeParameters == null || _shapeParameters.Count == 0) return null;
        switch (_shape)
        {
            case TelegraphShape.Circle:
                CircleShape2D circle = new CircleShape2D();
                circle.Radius = (float)_shapeParameters["radius"];
                return circle;
            case TelegraphShape.Rect:
                RectangleShape2D rect = new RectangleShape2D();
                rect.Size = new Vector2((float)_shapeParameters["length"], (float)_shapeParameters["width"]);
                return rect;
            case TelegraphShape.Cone:
                ConvexPolygonShape2D cone = new ConvexPolygonShape2D();
                cone.Points = CreateConeShape((float)_shapeParameters["radius"], (float)_shapeParameters["angle"], true);
                return cone;
            case TelegraphShape.Arc:
                ConvexPolygonShape2D arc = new ConvexPolygonShape2D();
                arc.Points = CreateArcShape((float)_shapeParameters["inner-radius"], (float)_shapeParameters["outer-radius"], (float)_shapeParameters["angle"], true);
                return arc;
            case TelegraphShape.Poly:
                return null;
            default:
                return null;
        }
    }

    public Vector2[] CreateTelegraphPoly()
    {
        if (_shapeParameters == null || _shapeParameters.Count == 0) return null;
        switch (_shape)
        {
            case TelegraphShape.Circle:
                return null;
            case TelegraphShape.Rect:
                return null;
            case TelegraphShape.Cone:
                return CreateConeShape((float)_shapeParameters["radius"], (float)_shapeParameters["angle"], true);
            case TelegraphShape.Arc:
                return CreateArcShape((float)_shapeParameters["inner-radius"], (float)_shapeParameters["outer-radius"], (float)_shapeParameters["angle"], true);
            case TelegraphShape.Poly:
                return null;
            default:
                return null;
        }
    }

    public Vector2 GetTelegraphLocation()
    {
        if (_shapeParameters == null || _shapeParameters.Count == 0) return Vector2.Zero;
        switch (_shape)
        {
            case TelegraphShape.Circle:
                return Vector2.Zero;
            case TelegraphShape.Rect:
                return new Vector2((float)_shapeParameters["length"] / 2, 0);
            case TelegraphShape.Cone:
                return Vector2.Zero;
            case TelegraphShape.Arc:
                return Vector2.Zero;
            case TelegraphShape.Poly:
                return Vector2.Zero;
            default:
                return Vector2.Zero;
        }
    }

    //Consider using facing section of https://docs.godotengine.org/en/3.1/tutorials/math/vector_math.html to detect collisions using circle collision shape for cones (and 2 circles for arcs)

    //draw functions taken and modified from https://docs.godotengine.org/en/4.0/tutorials/2d/custom_drawing_in_2d.html
    private void DrawCone(float radius, float angle, Color color, bool filled = true, float width = -1, bool antialiased = false)
    {
        Vector2[] pointsArc = CreateConeShape(radius, angle);
        if (filled) DrawColoredPolygon(pointsArc, color);
        else DrawPolyline(pointsArc, color, width, antialiased);
    }

    private Vector2[] CreateConeShape(float radius, float angle, bool isCollisionShape = false)
    {
        int nbPoints = 24;
        if (!isCollisionShape && angle > 50) nbPoints += (Mathf.FloorToInt(angle) - 50) / 5;

        float angleTo = angle / 2;
        float angleFrom = angleTo * -1;
        angleTo += 90f;
        angleFrom += 90f;
        Vector2[] pointsArc = new Vector2[nbPoints + 3];
        pointsArc[0] = Vector2.Zero;
        pointsArc[nbPoints + 2] = Vector2.Zero;

        for (int i = 0; i <= nbPoints; i++)
        {
            float anglePoint = Mathf.DegToRad(angleFrom + i * (angleTo - angleFrom) / nbPoints - 90);
            pointsArc[i + 1] = Vector2.Zero + new Vector2(Mathf.Cos(anglePoint), Mathf.Sin(anglePoint)) * radius;
        }

        return pointsArc;
    }

    private void DrawArc(float innerRadius, float outerRadius, float angle, Color color, bool filled = true, float width = -1, bool antialiased = false)
    {
        if (angle < 360)
        {
            Vector2[] pointsArc = CreateArcShape(innerRadius, outerRadius, angle);
            if (filled) DrawColoredPolygon(pointsArc, color);
            else DrawPolyline(pointsArc, color, width, antialiased);
        }
        else
        {
            if (filled)
            {
                Vector2[] pointsArc = CreateArcShape(innerRadius, outerRadius, 180);
                DrawColoredPolygon(pointsArc, color);
                for (int i = 0; i < pointsArc.Length; i++)
                {
                    pointsArc[i] *= -1;
                }
                DrawColoredPolygon(pointsArc, color);
            }
            else
            {
                DrawCircle(Position, innerRadius, color, filled, width, antialiased);
                DrawCircle(Position, outerRadius, color, filled, width, antialiased);
            }
        }
    }

    private Vector2[] CreateArcShape(float innerRadius, float outerRadius, float angle, bool isCollisionShape = false)
    {
        int nbPoints = 24;
        if (!isCollisionShape && angle > 50) nbPoints += (Mathf.FloorToInt(angle) - 50) / 5;

        //To prevent polygon triangulation error
        if (outerRadius == innerRadius) outerRadius += 0.0001f;
        if (angle >= 360) angle = 359.99f;

        float angleTo = angle / 2;
        float angleFrom = angleTo * -1;
        angleTo += 90f;
        angleFrom += 90f;
        Vector2[] pointsArc = new Vector2[nbPoints * 2 + 3];

        for (int i = 0; i <= nbPoints; i++)
        {
            float anglePoint = Mathf.DegToRad(angleFrom + i * (angleTo - angleFrom) / nbPoints - 90);
            pointsArc[i] = Vector2.Zero + new Vector2(Mathf.Cos(anglePoint), Mathf.Sin(anglePoint)) * innerRadius;
        }
        for (int i = nbPoints; i >= 0; i--)
        {
            float anglePoint = Mathf.DegToRad(angleFrom + i * (angleTo - angleFrom) / nbPoints - 90);
            pointsArc[(nbPoints - i) + nbPoints + 1] = Vector2.Zero + new Vector2(Mathf.Cos(anglePoint), Mathf.Sin(anglePoint)) * outerRadius;
        }
        pointsArc[pointsArc.Length - 1] = pointsArc[0];

        return pointsArc;
    }



}
