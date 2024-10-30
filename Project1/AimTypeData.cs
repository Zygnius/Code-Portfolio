using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class AimTypeData : Resource
{
    private AimType _aimType;
    [Export]
    public AimType Aim
    {
        get => _aimType;
        set
        {
            _aimType = value;
            NotifyPropertyListChanged();
        }
    }

    [ExportGroup("Aim Data")]
    [Export]
    private float _rotation;
    public float Rotation => _rotation;
    [Export]
    private Vector2 _rotationRange;
    public Vector2 RotationRange => _rotationRange;
    [Export]
    private float[] _discreteRot; //Array of discrete rotations to randomly pick from. Gets added to the random rotation from rotationRange.
    public float[] DiscreteRot => _discreteRot;
    [Export]
    private bool _isRelative;
    public bool IsRelative => _isRelative;

    private string[] _setDirProperties = { "_rotation", "_isRelative" };
    private string[] _faceTargetProperties = { };
    private string[] _randomDirProperties = { "_rotationRange", "_discreteRot", "_isRelative" };

    public override void _ValidateProperty(Dictionary property)
    {
        if (System.Array.IndexOf(_setDirProperties, (string)property["name"]) < 0
            && System.Array.IndexOf(_faceTargetProperties, (string)property["name"]) < 0
            && System.Array.IndexOf(_randomDirProperties, (string)property["name"]) < 0)
        {
            property["usage"] = (long)property["usage"] | (long)PropertyUsageFlags.Editor;
            return;
        }

        switch (_aimType)
        {
            case AimType.None:
                property["usage"] = (long)property["usage"] & (long)~PropertyUsageFlags.Editor;
                break;
            case AimType.SetDir:
                if (System.Array.IndexOf(_setDirProperties, (string)property["name"]) > -1)
                    property["usage"] = (long)property["usage"] | (long)PropertyUsageFlags.Editor;
                else property["usage"] = (long)property["usage"] & (long)~PropertyUsageFlags.Editor;
                break;
            case AimType.FaceTarget:
                if (System.Array.IndexOf(_faceTargetProperties, (string)property["name"]) > -1)
                    property["usage"] = (long)property["usage"] | (long)PropertyUsageFlags.Editor;
                else property["usage"] = (long)property["usage"] & (long)~PropertyUsageFlags.Editor;
                break;
            case AimType.RandomDir:
                if (System.Array.IndexOf(_randomDirProperties, (string)property["name"]) > -1)
                    property["usage"] = (long)property["usage"] | (long)PropertyUsageFlags.Editor;
                else property["usage"] = (long)property["usage"] & (long)~PropertyUsageFlags.Editor;
                break;
            default:
                property["usage"] = (long)property["usage"] & (long)~PropertyUsageFlags.Editor;
                break;
        }
    }
}

public enum AimType
{
    None,
    SetDir,
    FaceTarget,
    RandomDir
}
