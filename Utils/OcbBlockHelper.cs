using HarmonyLib;

static class OcbBlockHelper
{

    //########################################################
    //########################################################
    public static BlockValue FetchMasterBlock(WorldBase world, BlockValue bv, ref Vector3i position)
    {
        if (bv.ischild == false) return bv;
        position = bv.Block.multiBlockPos.GetParentPos(position, bv);
        return world.GetBlock(position);
    }

    //########################################################
    // Helpers for block activation commands
    //########################################################

    public static void PrependActivationCommands(
        Block current, System.Type ptype,
        ref BlockActivationCommand[] cmds,
        string name = "cmds")
    {
        var field = AccessTools.Field(ptype, name);
        if (field?.GetValue(current) is
            BlockActivationCommand[] pcmds)
        {
            int size = cmds.Length;
            int psize = pcmds.Length;
            System.Array.Resize(ref cmds, size + psize);
            // Move existing commands to the end
            for (int i = size - 1; i != -1; i -= 1)
                cmds[psize + i] = cmds[i];
            // Insert parent cmds on the front
            for (int i = 0; i < psize; i += 1)
                cmds[i] = pcmds[i];
        }
    }

    public static void AppendActivationCommands(
        Block current, System.Type ptype,
        ref BlockActivationCommand[] cmds,
        string name = "cmds")
    {
        var field = AccessTools.Field(ptype, name);
        if (field?.GetValue(current) is
            BlockActivationCommand[] pcmds)
        {
            int size = cmds.Length;
            int psize = pcmds.Length;
            System.Array.Resize(ref cmds, size + psize);
            // Move parent commands to the end
            for (int i = 0; i < psize; i += 1)
                cmds[size + i] = pcmds[i];
        }
    }

    // ####################################################################
    // ####################################################################

    public static BlockActivationCommand[] HandlePrependedCommands(WorldBase world,
        BlockValue bv, int clrIdx, Vector3i position, EntityAlive player,
        BlockActivationCommand[] pcmds, BlockActivationCommand[] cmds)
    {
        // Copy state from old to new array
        for (int i = 0; i < pcmds.Length; i += 1)
            cmds[i + 1].enabled = pcmds[i].enabled;
        return cmds;
    }

    public static BlockActivationCommand[] HandleAppendedCommands(WorldBase world,
        BlockValue bv, int clrIdx, Vector3i position, EntityAlive player,
        BlockActivationCommand[] pcmds, BlockActivationCommand[] cmds)
    {
        // Copy state from old to new array
        for (int i = 0; i < pcmds.Length; i += 1)
            cmds[i + 1].enabled = pcmds[i].enabled;
        return cmds;
    }

    //########################################################
    // Helpers for generic states
    //########################################################

    public static bool GetEnabled(BlockValue bv)
    {
        return (bv.meta & 0b_0000_0010) > 0;
    }
    public static bool GetEnabled2(BlockValue bv)
    {
        return (bv.meta & 0b_0000_0001) > 0;
    }

    public static void SetEnabled(ref BlockValue bv, bool enabled)
    {
        bv.meta &= 0b_1111_1101;
        if (enabled == false) return;
        bv.meta |= 0b_0000_0010;
    }

    public static void SetEnabled2(ref BlockValue bv, bool enabled)
    {
        bv.meta &= 0b_1111_1110;
        if (enabled == false) return;
        bv.meta |= 0b_0000_0001;
    }

    //########################################################
    //########################################################

}
