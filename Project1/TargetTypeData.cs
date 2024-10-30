using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class TargetTypeData : Resource
{
    private TargetType _targetType;
    [Export]
    public TargetType Target
    {
        get => _targetType;
        set
        {
            _targetType = value;
            NotifyPropertyListChanged();
        }
    }

    [ExportGroup("Target Data")]
    [Export]
    private float _rotationSpeed;
    public float RotationSpeed => _rotationSpeed;
    [Export]
    private float _duration;
    public float Duration => _duration;
    [Export]
    private bool _spawnOnTarget;
    public bool SpawnOnTarget => _spawnOnTarget;
    [Export]
    private Vector2 _areaSize;
    public Vector2 AreaSize => _areaSize;
    [Export]
    private Vector2 _areaPos; //Center of areaSize
    public Vector2 AreaPos => _areaPos;
    [Export]
    private Vector2[] _discretePos; //Array of discrete positions to randomly pick from. Gets added to the random position from areaSize and areaOffset.
    public Vector2[] DiscretePos => _discretePos;
    [Export]
    private bool _faceTarget;
    public bool FaceTarget => _faceTarget;
    [Export]
    private Vector2 _rotationRange;
    public Vector2 RotationRange => _rotationRange;
    [Export]
    private float[] _discreteRot; //Array of discrete rotations to randomly pick from. Gets added to the random rotation from rotationRange.
    public float[] DiscreteRot => _discreteRot;
    [Export]
    private bool _isRelative;
    public bool IsRelative => _isRelative;

    private string[] _rotateProperties = { "_rotationSpeed", "_duration" };
    private string[] _trackTargetProperties = { "_rotationSpeed", "_duration", "_spawnOnTarget" };
    private string[] _randomAreaProperties = { "_areaSize", "_areaPos", "_discretePos", "_faceTarget", "_isRelative" };
    private string[] _randomDirProperties = { "_rotationRange", "_discreteRot", "_isRelative" };

    public float DurationTimer = 0f; //For use with duration

    public override void _ValidateProperty(Dictionary property)
    {
        if (System.Array.IndexOf(_rotateProperties, (string)property["name"]) < 0
            && System.Array.IndexOf(_trackTargetProperties, (string)property["name"]) < 0
            && System.Array.IndexOf(_randomAreaProperties, (string)property["name"]) < 0
            && System.Array.IndexOf(_randomDirProperties, (string)property["name"]) < 0)
        {
            property["usage"] = (long)property["usage"] | (long)PropertyUsageFlags.Editor;
            return;
        }

        switch (_targetType)
        {
            case TargetType.None:
                property["usage"] = (long)property["usage"] & (long)~PropertyUsageFlags.Editor;
                break;
            case TargetType.Rotate:
                if (System.Array.IndexOf(_rotateProperties, (string)property["name"]) > -1)
                    property["usage"] = (long)property["usage"] | (long)PropertyUsageFlags.Editor;
                else property["usage"] = (long)property["usage"] & (long)~PropertyUsageFlags.Editor;
                break;
            case TargetType.TrackTarget:
                if (System.Array.IndexOf(_trackTargetProperties, (string)property["name"]) > -1)
                    property["usage"] = (long)property["usage"] | (long)PropertyUsageFlags.Editor;
                else property["usage"] = (long)property["usage"] & (long)~PropertyUsageFlags.Editor;
                break;
            case TargetType.RandomArea:
                if (System.Array.IndexOf(_randomAreaProperties, (string)property["name"]) > -1)
                    property["usage"] = (long)property["usage"] | (long)PropertyUsageFlags.Editor;
                else property["usage"] = (long)property["usage"] & (long)~PropertyUsageFlags.Editor;
                break;
            case TargetType.RandomDir:
                if (System.Array.IndexOf(_randomDirProperties, (string)property["name"]) > -1)
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
        DurationTimer = 0f;
    }
}



public enum TargetType
{
    None,
    Rotate,
    TrackTarget,
    RandomArea,
    RandomDir
}
