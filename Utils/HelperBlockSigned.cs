using Audio;
using Platform;
using UnityEngine;

// ####################################################################
// Static helpers copied from BlockPayerSign for shared implementation
// ####################################################################

public static class HelperBlockSigned
{

    // ####################################################################
    // ####################################################################

    public static bool HasAccess(WorldBase world, TileEntitySign te)
    {
        if (te == null) return false;
        if (GameManager.Instance.IsEditMode()) return true;
        if (te.LocalPlayerIsOwner()) return true;
        var players = world.GetGameManager().GetPersistentPlayerList();
        var identifier = PlatformManager.InternalLocalUserIdentifier;
        var owner = players.GetPlayerData(te.GetOwner());
        return owner?.ACL?.Contains(identifier) ?? false;
    }

    public static bool HasEditAccess(WorldBase world, TileEntitySign te)
    {
        if (!HasAccess(world, te))
        {
            return false;
        }
        // No edit if TE is locked?
        // if (te.IsLocked())
        // {
        //     return false;
        // }
        // Can edit if not denied yet
        return true;
    }

    // ####################################################################
    // ####################################################################

    public static bool OnBlockActivated(
        IBlockPortal blk, string command, WorldBase world, int cIdx,
        Vector3i position, BlockValue bv, EntityAlive player)
    {
        bv = OcbBlockHelper.FetchMasterBlock(world, bv, ref position);
        // Try to get the TileEntitySign for access control and other stuff
        if (!HelperBlockPortal.GetTileEntity(world, cIdx, position, bv, out TileEntitySign te)) return false;
        // Can't do anyting if we have no edit access to TileEntitySign
        if (HasEditAccess(world, te) == false) return false;
        switch (command)
        {
            // ToDo: verify access-control and permissions are correctly checked
            // Basically re-implement whatever we had to check if command is enabled
            case "edit":
                OnBlockActivated(blk, world, cIdx, position, bv, player);
                Manager.BroadcastPlayByLocalPlayer(position.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
                return false;
            case "lock":
                te.SetLocked(true);
                Manager.BroadcastPlayByLocalPlayer(position.ToVector3() + Vector3.one * 0.5f, "Misc/locking");
                GameManager.ShowTooltip(player as EntityPlayerLocal, "containerLocked");
                return true;
            case "unlock":
                te.SetLocked(false);
                Manager.BroadcastPlayByLocalPlayer(position.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
                GameManager.ShowTooltip(player as EntityPlayerLocal, "containerUnlocked");
                return true;
            case "keypad":
                XUiC_KeypadWindow.Open(LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal), (ILockable)te);
                return true;
            default:
                return false;
        }
    }

    // ####################################################################
    // ####################################################################

    public static bool OnBlockActivated(IBlockPortal blk, WorldBase world,
        int cIdx, Vector3i position, BlockValue bv, EntityAlive player)
    {
        
        bv = OcbBlockHelper.FetchMasterBlock(world, bv, ref position);
        if (!HelperBlockPortal.GetTileEntity(world, cIdx, position, bv, out TileEntitySign te)) return false;
        player.AimingGun = false;
        world.GetGameManager().TELockServer(cIdx,
            te.ToWorldPos(), te.entityId, player.entityId);
        return true;
    }

    // ####################################################################
    // ####################################################################

    public static bool HasBlockActivationCommands(Block blk, WorldBase world,
        BlockValue bv, int clrIdx, Vector3i position, EntityAlive player)
    {
        return HelperBlockPortal.GetTileEntity(world, clrIdx, position, bv, out TileEntitySign te);
    }

    // ####################################################################
    // ####################################################################

    public static void OnBlockAdded(IBlockPortal blk, WorldBase world, Chunk chunk, Vector3i position, BlockValue bv)
    {
        if (!bv.ischild) GetOrCreateTileEntitySign(blk, world, position, bv, chunk.ClrIdx);
    }

    public static void OnBlockRemoved(IBlockPortal blk, WorldBase world, Chunk chunk, Vector3i position, BlockValue bv)
    {
        if (!bv.ischild) RemoveTileEntitySign(blk, world, position, chunk.ClrIdx);
    }

    // ####################################################################
    // ####################################################################

    public static TileEntitySign GetOrCreateTileEntitySign(IBlockPortal blk, WorldBase world, Vector3i position, BlockValue bv, int cIdx)
    {
        Chunk chunk = (Chunk)world.GetChunkFromWorldPos(position);
        // ToDo: could fetch it directly from the chunk object!?
        if (HelperBlockPortal.GetTileEntity(world, cIdx, position, bv, out TileEntitySign te)) return te;
        te = new TileEntitySign(chunk);
        te.localChunkPos = World.toBlock(position);
        te.SetText(blk.GetPortalGroup(position));
        chunk.AddTileEntity(te);
        return te;
    }

    public static void RemoveTileEntitySign(IBlockPortal blk, WorldBase world, Vector3i position, int clrIdx)
    {
        Chunk chunk = (Chunk)world.GetChunkFromWorldPos(position);
        chunk.RemoveTileEntityAt<TileEntitySign>(world as World, World.toBlock(position));
    }

    // ####################################################################
    // ####################################################################

}
