using UnityEngine;

public class PkgExecuteTeleport : NetPackageTeleportPlayer
{

    // ####################################################################
    // ####################################################################

    protected Vector3i SourcePosition = Vector3i.invalid;

    // ####################################################################
    // ####################################################################

    public PkgExecuteTeleport Setup(Vector3i src, Vector3 dst,
        Vector3? viewDirection = null, bool onlyIfNotFlying = false)
    {
        Setup(dst, viewDirection, onlyIfNotFlying);
        SourcePosition = src;
        return this;
    }

    // ####################################################################
    // ####################################################################

    // Must only be executed client side
    public override void ProcessPackage(World world, GameManager gm)
    {
        base.ProcessPackage(world, gm);
        // Disable potential animators on block entity
        HelperBlockPortal.OnAfterTeleport(SourcePosition);
    }

    // ####################################################################
    // ####################################################################

    public override void read(PooledBinaryReader br)
    {
        base.read(br);
        SourcePosition = StreamUtils.ReadVector3i(br);
    }

    public override void write(PooledBinaryWriter bw)
    {
        base.write(bw);
        StreamUtils.Write(bw, SourcePosition);
    }

    // ####################################################################
    // ####################################################################

}

