using System;
using System.IO;

public class PortalData
{

    // ####################################################################
    // ####################################################################

    public Vector3i Position = Vector3i.invalid;
    public string Group = String.Empty;

    // ####################################################################
    // ####################################################################

    public PortalData()
    {
    }

    public PortalData(Vector3i position, string group)
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
        bw.Write(Group);
    }

    // ####################################################################
    // ####################################################################

}
