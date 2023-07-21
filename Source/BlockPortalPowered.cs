// ####################################################################
// ####################################################################

public class BlockPortalPowered : BlockPowered, IBlockPortal
{

    // ####################################################################
    // ####################################################################

    protected PortalConfig CFG = new PortalConfig();
    public PortalConfig GetPortalConfig() => CFG;
    public bool RequiresPower => true;

    // ####################################################################
    // ####################################################################

    public override void Init()
    {
        base.Init();
        CFG.Parse(Properties);
    }

    // ####################################################################
    // ####################################################################

    public string GetPortalGroup(Vector3i position)
    {
        var portal = PortalManager.Instance.GetPortalAt(position);
        return portal?.Group ?? CFG.PortalGroup;
    }

    // ####################################################################
    // Handle adding and removing of portals in the world
    // ####################################################################

    // Only called on the server side (and only for the main multidim block)
    public override void OnBlockAdded(WorldBase world, Chunk chunk, Vector3i position, BlockValue bv)
    {
        base.OnBlockAdded(world, chunk, position, bv);
        PortalManager.Instance.AddPortal(position, CFG.PortalGroup);
    }

    // Only called on the server side (and only for the main multidim block)
    public override void OnBlockRemoved(WorldBase world, Chunk chunk, Vector3i position, BlockValue bv)
    {
        base.OnBlockRemoved(world, chunk, position, bv);
        PortalManager.Instance.RemovePortal(position, bv);
    }

    // ####################################################################
    // Handle additional commands (preserve base class commands)
    // ####################################################################

    private readonly BlockActivationCommand[] cmds = new BlockActivationCommand[] {
        new BlockActivationCommand("teleport", "map", true, true),
    };

    public BlockPortalPowered()
    {
        // Prepends `cmds` from parent to our `cmds`
        OcbBlockHelper.AppendActivationCommands(
            this, typeof(BlockPowered), ref cmds);
    }

    // ####################################################################
    // ####################################################################

    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase world,
        BlockValue bv, int clrIdx, Vector3i position, EntityAlive player)
    {
        bv = OcbBlockHelper.FetchMasterBlock(world, bv, ref position);
        return OcbBlockHelper.HandlePrependedCommands(world, bv, cAirId, position, player,
             base.GetBlockActivationCommands(world, bv, clrIdx, position, player), cmds);
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
               | base.HasBlockActivationCommands(world, bv, clrIdx, position, player);
    }

    public override bool OnBlockActivated(
        string command, WorldBase world, int cIdx,
        Vector3i position, BlockValue bv, EntityAlive player)
    {
        return HelperBlockPortal.OnBlockActivated(this, command, world, cIdx, position, bv, player)
               | base.OnBlockActivated(command, world, cIdx, position, bv, player);
    }

    // ####################################################################
    // ####################################################################

}