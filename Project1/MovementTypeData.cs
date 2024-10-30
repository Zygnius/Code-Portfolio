using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class MovementTypeData : Resource
{
    private MovementType _MovementType;
    [Export]
    public MovementType Movement
    {
        get => _MovementType;
        set
        {
            _MovementType = value;
            NotifyPropertyListChanged();
        }
    }

    [ExportGroup("Movement Data")]
    [Export]
    private Vector2 _position;
    public Vector2 Position => _position;
    [Export]
    private bool _affectedByRotation;
    public bool AffectedByRotation => _affectedByRotation;
    [Export]
    private float _distance;
    public float Distance => _distance;
    [Export]
    private float _minMoveTime;
    public float MinMoveTime => _minMoveTime;
    [Export]
    private float _range;
    public float Range => _range;

    private string[] _setPositionProperties = { "_position" };
    private string[] _relativePositionProperties = { "_position", "_affectedByRotation" };
    private string[] _towardTargetProperties = { "_distance" };
    private string[] _chaseTargetProperties = { "_minMoveTime", "_range" };

    public bool HasStoredData = false; //Toggle this on if outside data (ie OrigPosition) is stored and needs to be reset later
    public Vector2 TargettedPosition = Vector2.Zero;

    public override void _ValidateProperty(Dictionary property)
    {
        if (System.Array.IndexOf(_setPositionProperties, (string)property["name"]) < 0
            && System.Array.IndexOf(_relativePositionProperties, (string)property["name"]) < 0
            && System.Array.IndexOf(_towardTargetProperties, (string)property["name"]) < 0
            && System.Array.IndexOf(_chaseTargetProperties, (string)property["name"]) < 0)
        {
            property["usage"] = (long)property["usage"] | (long)PropertyUsageFlags.Editor;
            return;
        }

        switch (_MovementType)
        {
            case MovementType.None:
                property["usage"] = (long)property["usage"] & (long)~PropertyUsageFlags.Editor;
                break;
            case MovementType.SetPosition:
                if (System.Array.IndexOf(_setPositionProperties, (string)property["name"]) > -1)
                    property["usage"] = (long)property["usage"] | (long)PropertyUsageFlags.Editor;
                else property["usage"] = (long)property["usage"] & (long)~PropertyUsageFlags.Editor;
                break;
            case MovementType.RelativePosition:
                if (System.Array.IndexOf(_relativePositionProperties, (string)property["name"]) > -1)
                    property["usage"] = (long)property["usage"] | (long)PropertyUsageFlags.Editor;
                else property["usage"] = (long)property["usage"] & (long)~PropertyUsageFlags.Editor;
                break;
            case MovementType.TowardTarget:
                if (System.Array.IndexOf(_towardTargetProperties, (string)property["name"]) > -1)
                    property["usage"] = (long)property["usage"] | (long)PropertyUsageFlags.Editor;
                else property["usage"] = (long)property["usage"] & (long)~PropertyUsageFlags.Editor;
                break;
            case MovementType.ChaseTarget:
                if (System.Array.IndexOf(_chaseTargetProperties, (string)property["name"]) > -1)
                    property["usage"] = (long)property["usage"] | (long)PropertyUsageFlags.Editor;
                else property["usage"] = (long)property["usage"] & (long)~PropertyUsageFlags.Editor;
                break;
            default:
                property["usage"] = (long)property["usage"] & (long)~PropertyUsageFlags.Editor;
                break;
        }
    }

    public void Reset()
    {
        if (HasStoredData)
        {
            HasStoredData = false;
            TargettedPosition = Vector2.Zero;
        }
    }
}

public enum MovementType
{
    None,
    SetPosition,
    RelativePosition,
    TowardTarget,
    ChaseTarget
}
