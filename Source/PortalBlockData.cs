using System;
using System.IO;

// ####################################################################
// ####################################################################

public interface IBlockPortal
{
    public bool RequiresPower { get; }
    public string GetPortalGroup(Vector3i position);
    public PortalConfig GetPortalConfig();
}

// ####################################################################
// ####################################################################

public struct PortalConfig
{

    public string PortalGroup;
    public float PortalDelay;
    public string PortalLockBuff;
    public string PortalEffectBuff;

    // Default constructor with default value assignments
    public PortalConfig(string group = "portal", float delay = 0,
        string lockBuff = "buffPortalLock", string effectBuff = null)
    {
        PortalGroup = group;
        PortalDelay = delay;
        PortalLockBuff = lockBuff;
        PortalEffectBuff = effectBuff;
    }

    // Create new struct and parse from properties
    public PortalConfig(DynamicProperties props) : this() => Parse(props);

    // Copy/Parse configuration for dynamic properties
    public void Parse(DynamicProperties props)
    {
        props.ParseFloat("PortalDelay", ref PortalDelay);
        props.ParseString("PortalGroup", ref PortalGroup);
        props.ParseString("PortalLockBuff", ref PortalLockBuff);
        props.ParseString("PortalEffectBuff", ref PortalEffectBuff);
    }

}

// ####################################################################
// ####################################################################

public class PortalBlockData
{

    // ####################################################################
    // ####################################################################

    public Vector3i Position = Vector3i.invalid;
    public string Group = String.Empty;

    // ####################################################################
    // ####################################################################

    public PortalBlockData()
    {
    }

    public PortalBlockData(Vector3i position, string group)
    {
        Position = position;
        Group = group;
    }

    // ####################################################################
    // ####################################################################

    public void Read(BinaryReader br)
    {
        Position = StreamUtils.ReadVector3i(br);
        Group = br.ReadString();
    }

    public void Write(BinaryWriter bw)
    {
        StreamUtils.Write(bw, Position);
        bw.Write(Group ?? string.Empty);
    }

    // ####################################################################
    // ####################################################################

}
