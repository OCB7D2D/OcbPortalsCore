public class BlockPortal : Block
{

    // ####################################################################
    // ####################################################################

    public virtual string GetPortalGroup()
    {
        return Properties.GetString("PortalGroup");
    }

    // ####################################################################
    // ####################################################################

    // Only called on the server side (and only for the main multidim block)
    public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
        PortalManager.Instance.AddPortal(_blockPos, _blockValue);
    }

    // Only called on the server side (and only for the main multidim block)
    public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
        PortalManager.Instance.RemovePortal(_blockPos, _blockValue);
    }

    // ####################################################################
    // ####################################################################

    private BlockActivationCommand[] cmds = new BlockActivationCommand[]
    {
        new BlockActivationCommand("teleport", "pen", true, true),
        //new BlockActivationCommand("edit", "pen", false, false),
        //new BlockActivationCommand("lock", "lock", false, false),
        //new BlockActivationCommand("unlock", "unlock", false, false),
        //new BlockActivationCommand("keypad", "keypad", false, false)
    };

    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        // var tileEntity = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityPoweredPortalOld;
        // if (tileEntity == null) { return new BlockActivationCommand[0]; }
        //PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
        //PersistentPlayerData playerData = _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(tileEntity.GetOwner());
        // bool flag = tileEntity.LocalPlayerIsOwner();
        // bool flag2 = !tileEntity.LocalPlayerIsOwner() && (playerData != null && playerData.ACL != null) && playerData.ACL.Contains(internalLocalUserIdentifier);
        this.cmds[0].enabled = true;
        // this.cmds[1].enabled = string.IsNullOrEmpty(location);
        // this.cmds[2].enabled = (!tileEntity.IsLocked() && (flag || flag2));
        // this.cmds[3].enabled = (tileEntity.IsLocked() && flag);
        // this.cmds[4].enabled = ((!tileEntity.IsUserAllowed(internalLocalUserIdentifier) && tileEntity.HasPassword() && tileEntity.IsLocked()) || flag);
        return this.cmds;
    }

    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        
        // if (display == false) return "";

        if (_blockValue.ischild)
        {
            Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
            return GetActivationText(_world, _world.GetBlock(parentPos), _clrIdx, parentPos, _entityFocusing);
        }

        // if (requiredPower > 0)
        // {
        //     var tileEntity = GameManager.Instance.World.GetTileEntity(0, _blockPos) as TileEntityPoweredPortalOld;
        //     if (tileEntity == null) return "";
        //     if (tileEntity.IsPowered == false)
        //     {
        //         ToggleAnimator(_blockPos, false);
        //         return $"{Localization.Get("teleporttoNeedPower")}...";
        //     }
        // }

        // if (!string.IsNullOrEmpty(displayBuff))
        // {
        //     if (_entityFocusing.Buffs.HasBuff(displayBuff) == false) return $"{Localization.Get("teleportto")}...";
        // }

        return string.Format(Localization.Get("teleportto"), GetPortalGroup());

        // if (PortalManager.Instance.TryGetDestination(_blockPos, out string dst))
        // return $"{Localization.Get("teleportto")} {dst}";
        // return $"{Localization.Get("portal_configure")} {text}";
    }





    public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        if (_blockValue.ischild)
        {
            Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
            BlockValue block = _world.GetBlock(parentPos);
            return this.OnBlockActivated(_world, _cIdx, parentPos, block, _player);
        }
        // var tileEntitySign = (TileEntityPoweredPortal)_world.GetTileEntity(_cIdx, _blockPos);
        // if (tileEntitySign == null)
        // {
        //     return false;
        // }
        // _player.AimingGun = false;
        // Vector3i blockPos = tileEntitySign.ToWorldPos();
        // _world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntitySign.entityId, _player.entityId, null);
        return true;
    }

    public override bool OnBlockActivated(string commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        if (_blockValue.ischild)
        {
            Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
            BlockValue block = _world.GetBlock(parentPos);
            return this.OnBlockActivated(commandName, _world, _cIdx, parentPos, block, _player);
        }

        switch (commandName)
        {
            case "teleport":
                TeleportPlayer(_player, _blockPos);
                return true;
        }

        return false;
/*
        var tileEntity = _world.GetTileEntity(_cIdx, _blockPos) as TileEntityPoweredPortal;
        if (tileEntity == null)
        {
            return false;
        }
        switch (commandName)
        {
            case "portalActivate":
                if (GameManager.Instance.IsEditMode() || !tileEntity.IsLocked() || tileEntity.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                {
                    if (requiredPower <= 0)
                        TeleportPlayer(_player, _blockPos);

                    if (requiredPower > 0 && tileEntity.IsPowered)
                        TeleportPlayer(_player, _blockPos);


                }
                return false;
            case "edit":
                if (GameManager.Instance.IsEditMode() || !tileEntity.IsLocked() || tileEntity.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                {
                    if (string.IsNullOrEmpty(location))
                        return this.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
                }
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
                return false;
            case "lock":
                tileEntity.SetLocked(true);
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locking");
                GameManager.ShowTooltip(_player as EntityPlayerLocal, "containerLocked");
                return true;
            case "unlock":
                tileEntity.SetLocked(false);
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
                GameManager.ShowTooltip(_player as EntityPlayerLocal, "containerUnlocked");
                return true;
            case "keypad":
                if (string.IsNullOrEmpty(location))
                    XUiC_KeypadWindow.Open(LocalPlayerUI.GetUIForPlayer(_player as EntityPlayerLocal), tileEntity);
                return true;
            default:
                return false;
        }
*/
    }


    // Always executed client side from block activation
    public void TeleportPlayer(EntityAlive player, Vector3i position)
    {
        // var tileEntity = GameManager.Instance.World.GetTileEntity(0, _blockPos) as TileEntityPoweredPortal;
        // if (tileEntity == null) return;
        // if (requiredPower > 0 && !tileEntity.IsPowered) return;

        const string buffCooldown = "buffTeleportCooldown";
        if (player.Buffs.HasBuff(buffCooldown)) return;
        player.Buffs.AddBuff(buffCooldown);

        int delay = 0;
        var gm = GameManager.Instance;
        var cm = ConnectionManager.Instance;
        var pkg = NetPackageManager.GetPackage<PkgRequestTeleport>();
        if (!cm.IsServer) cm.SendToServer(pkg.Setup(position, delay));
        else pkg.Setup(position, delay).ProcessPackage(player.world, gm);
    }

    private void Teleport(EntityAlive player, Vector3i position)
    {
        // var destination = PortalManager.Instance.GetDestination(_blockPos);
        // if (destination != Vector3i.zero)
        {
            // Check if the destination is powered.
            // var tileEntity = GameManager.Instance.World.GetTileEntity(0, destination) as TileEntityPoweredPortal;
            // if (tileEntity != null)
            // {
            //     if (requiredPower > 0 && !tileEntity.IsPowered) return;
            // }
        }
    }

}