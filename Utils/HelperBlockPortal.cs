// ####################################################################
// Static helpers to implement a Portal Block
// ####################################################################

using UnityEngine;

public static class HelperBlockPortal
{

    // ####################################################################
    // Return the specific TileEntity from master block or one above
    // This abuses an undefine behavior only possible on multidim blocks
    // We basically store a TileEntity in a child BlockValue position.
    // ####################################################################

    public static bool GetTileEntity<T>(WorldBase world, int cIdx,
        Vector3i position, BlockValue bv, out T te) where T : TileEntity
    {
        bv = OcbBlockHelper.FetchMasterBlock(world, bv, ref position);
        te = world.GetTileEntity(cIdx, position) as T;
        if (te != null) return true;
        if (bv.Block.isMultiBlock == false) return false;
        te = world.GetTileEntity(cIdx, position + Vector3i.up) as T;
        return te != null;
    }

    // ####################################################################
    // ####################################################################

    public static bool HasBlockActivationCommands(IBlockPortal blk, WorldBase world,
        BlockValue bv, int clrIdx, Vector3i position, EntityAlive player)
    {
        return true;
    }

    public static string GetActivationText(IBlockPortal blk, WorldBase world, BlockValue bv, int clrIdx, Vector3i position, EntityAlive player)
    {
        bv = OcbBlockHelper.FetchMasterBlock(world, bv, ref position);
        return string.Format(Localization.Get("portalTeleport"), blk.GetPortalGroup(position));
    }

    // ####################################################################
    // ####################################################################

    // Execute one of the available commands (dispatch to above for main command)
    public static bool OnBlockActivated(
        IBlockPortal blk, string command, WorldBase world, int cIdx,
        Vector3i position, BlockValue bv, EntityAlive player)
    {
        OcbBlockHelper.FetchMasterBlock(world, bv, ref position);
        // Handle special case for powered (or other) portals
        if (!IsReadyToTeleport(blk, world, cIdx, position, bv))
        {
            GameManager.ShowTooltip(player as EntityPlayerLocal, "ttPortalNotPowered");
            return false; // Can only activate if source portal is powered
        }
        // Handle the command
        if (command == "teleport")
        {
            TeleportPlayer(blk, player, position);
            return true;
        }
        // Not handles
        return false;
    }

    // ####################################################################
    // ####################################################################

    private static void TeleportPlayer(IBlockPortal blk, EntityAlive player, Vector3i position)
    {
        if (blk == null) return;
        var cfg = blk.GetPortalConfig();
        // Check if cooldown/lock buff is still there
        if (!string.IsNullOrEmpty(cfg.PortalLockBuff))
        {
            if (player.Buffs.HasBuff(cfg.PortalLockBuff)) return;
            player.Buffs.AddBuff(cfg.PortalLockBuff);
        }
        // Add buff to trigger portal effects
        if (!string.IsNullOrEmpty(cfg.PortalEffectBuff))
        {
            if (!player.Buffs.HasBuff(cfg.PortalEffectBuff))
                player.Buffs.AddBuff(cfg.PortalEffectBuff);
        }
        Log.Out("Request Teleport at {0}", position);
        // Setup package to send to server
        var gm = GameManager.Instance;
        var cm = ConnectionManager.Instance;
        var pkg = NetPackageManager.GetPackage<PkgRequestTeleport>();
        pkg.Setup(position, cfg.PortalDelay);
        if (!cm.IsServer) cm.SendToServer(pkg);
        else pkg.ProcessPackage(player.world, gm);
        SetAnimator(position, true);
    }

    // ####################################################################
    // ####################################################################

    // Must only be called client side to show animations
    public static void SetAnimator(Vector3i position, bool isOn)
    {
        if (GameManager.IsDedicatedServer) return;
        var world = GameManager.Instance.World;
        var chunk = world.GetChunkFromWorldPos(position);
        var ebcd = chunk?.GetBlockEntity(position);
        if (ebcd == null || ebcd.transform == null) return;
        foreach (var animator in ebcd.transform.GetComponentsInChildren<Animator>())
        {
            var state = animator.GetBool("portalOn");
            if (state != isOn)
            {
                animator.SetBool("portalOn", isOn);
                animator.SetBool("portalOff", !isOn);
            }
        }
    }

    // ####################################################################
    // ####################################################################

    public static void OnAfterTeleport(Vector3i position)
    {
        SetAnimator(position, false);
    }

    // ####################################################################
    // ####################################################################

    private static bool IsReadyToTeleport(IBlockPortal blk, WorldBase world,
        int cIdx, Vector3i position, BlockValue bv)
    {
        if (blk.RequiresPower)
        {
            if (!GetTileEntity(world, cIdx, position,
                bv, out TileEntityPowered te)) return false;
            return te.IsPowered;
        }
        return true;
    }

    // ####################################################################
    // ####################################################################

}
