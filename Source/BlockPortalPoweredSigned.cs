// ####################################################################
// ####################################################################

using Platform;

public class BlockPortalPoweredSigned : BlockPortalPowered
{

    // ####################################################################
    // Handle adding and removing of portals in the world
    // ####################################################################

    // Only called on the server side (and only for the main multidim block)
    public override void OnBlockAdded(WorldBase world, Chunk chunk, Vector3i position, BlockValue bv)
    {
        base.OnBlockAdded(world, chunk, position, bv);
        PortalManager.Instance.AddPortal(position, CFG.PortalGroup);
        // Add the TileEntitySign one block up from current position
        // This only works 100% reliably for 2x2x2 multi-dim blocks!
        // And for 1x2x1 block if placed upright and not on the side ;)
        HelperBlockSigned.OnBlockAdded(this, world, chunk, position + Vector3i.up, bv);
    }

    // Only called on the server side (and only for the main multidim block)
    public override void OnBlockRemoved(WorldBase world, Chunk chunk, Vector3i position, BlockValue bv)
    {
        base.OnBlockRemoved(world, chunk, position, bv);
        PortalManager.Instance.RemovePortal(position, bv);
        // Remove the TileEntitySign one block up from current position
        HelperBlockSigned.OnBlockRemoved(this, world, chunk, position + Vector3i.up, bv);
    }

    // Set owner once block is placed down
    public override void PlaceBlock(WorldBase world, BlockPlacement.Result result, EntityAlive player)
    {
        base.PlaceBlock(world, result, player);
        if (HelperBlockPortal.GetTileEntity(world, result.clrIdx,
            result.blockPos, result.blockValue, out TileEntitySign te))
                te.SetOwner(PlatformManager.InternalLocalUserIdentifier);
    }

    // ####################################################################
    // Handle additional commands (preserve base class commands)
    // ####################################################################

    private readonly BlockActivationCommand[] cmds = new BlockActivationCommand[] {
        new BlockActivationCommand("teleport", "map", true, true),
        // Need to copy these from `BlockPlayerSign.cmds`
        new BlockActivationCommand("edit", "pen", false),
        new BlockActivationCommand("lock", "lock", false),
        new BlockActivationCommand("unlock", "unlock", false),
        new BlockActivationCommand("keypad", "keypad", false)
    };

    public BlockPortalPoweredSigned()
    {
        OcbBlockHelper.AppendActivationCommands(
            this, typeof(BlockPowered), ref cmds);
    }

    // ####################################################################
    // ####################################################################

    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase world,
        BlockValue bv, int clrIdx, Vector3i position, EntityAlive player)
    {
        bv = OcbBlockHelper.FetchMasterBlock(world, bv, ref position);
        var CMDS =  OcbBlockHelper.HandlePrependedCommands(world, bv, cAirId, position, player,
             base.GetBlockActivationCommands(world, bv, clrIdx, position, player), cmds);
        CMDS[1].enabled = CMDS[2].enabled = CMDS[3].enabled = CMDS[4].enabled = false;
        if (HelperBlockPortal.GetTileEntity(world, clrIdx, position, bv, out TileEntitySign te))
        {
            PlatformUserIdentifierAbs userIdentifier = PlatformManager.InternalLocalUserIdentifier;
            var owner = world.GetGameManager().GetPersistentPlayerList().GetPlayerData(te.GetOwner());
            bool isOwner = te.LocalPlayerIsOwner();
            bool isAlly = !isOwner && owner?.ACL != null
                && owner.ACL.Contains(userIdentifier);
            bool hasCode = te.IsUserAllowed(userIdentifier);
            // ToDo: verify access-control and permissions are correctly checked
            // Log.Out("Is Owner {0}, Ally {1}, Code {2}", isOwner, isAlly, hasCode);
            CMDS[1].enabled = HelperBlockSigned.HasEditAccess(world, te);
            CMDS[2].enabled = CMDS[1].enabled && !te.IsLocked();
            CMDS[3].enabled = CMDS[1].enabled && te.IsLocked();
            CMDS[4].enabled = ((hasCode || !te.HasPassword() ? 0 : (te.IsLocked() ? 1 : 0)) | (isOwner ? 1 : 0)) != 0;

        }
        return CMDS;
    }

    // ####################################################################
    // Dispatch commands through different partial implementations
    // ####################################################################

    public override string GetActivationText(WorldBase world, BlockValue bv, int clrIdx, Vector3i position, EntityAlive player)
        => HelperBlockPortal.GetActivationText(this, world, bv, clrIdx, position, player);

    public override bool HasBlockActivationCommands(WorldBase world,
        BlockValue bv, int clrIdx, Vector3i position, EntityAlive player)
    {
        return HelperBlockPortal.HasBlockActivationCommands(this, world, bv, clrIdx, position, player)
               | HelperBlockSigned.HasBlockActivationCommands(this, world, bv, clrIdx, position, player)
               | base.HasBlockActivationCommands(world, bv, clrIdx, position, player);
    }

    public override bool OnBlockActivated(
        string command, WorldBase world, int cIdx,
        Vector3i position, BlockValue bv, EntityAlive player)
    {
        return HelperBlockPortal.OnBlockActivated(this, command, world, cIdx, position, bv, player)
               | HelperBlockSigned.OnBlockActivated(this, command, world, cIdx, position, bv, player)
               | base.OnBlockActivated(command, world, cIdx, position, bv, player);
    }

    // ####################################################################
    // ####################################################################

}