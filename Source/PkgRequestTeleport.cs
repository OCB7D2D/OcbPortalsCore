using System.Collections;
using UnityEngine;

public class PkgRequestTeleport : NetPackage
{

    // Make sure we teleport in the center of the destination block
    private Vector3 TeleportOffset = new Vector3(0.5f, 0f, 0.5f);

    // Block position to teleport player to
    private Vector3i Position = Vector3i.invalid;

    // Minimum delay to wait in seconds
    private float Delay = 0;

    // Observer is only active before teleport action
    private ChunkManager.ChunkObserver Observer;

    // ####################################################################
    // ####################################################################

    public PkgRequestTeleport Setup(
      Vector3i position, float delay)
    {
        Position = position;
        Delay = delay;
        return this;
    }

    // ####################################################################
    // ####################################################################

    private void ClearChunkObserver(GameManager gm)
    {
        if (Observer == null) return;
        gm.RemoveChunkObserver(Observer);
        Observer = null;
    }

    // ####################################################################
    // ####################################################################

    // Might be executed client or server side
    public override void ProcessPackage(World world, GameManager gm)
    {
        // Try to find next portal in the list of same group as ourself
        if (PortalManager.Instance.GetNextPosition(Position) is PortalBlockData next)
        {
            var dst = next.Position + TeleportOffset;
            // Start coroutine to wait for delay and chunk load
            gm.StartCoroutine(TeleportWhenLoaded(
                world, gm, Position, dst, Delay));
        }
    }

    // ####################################################################
    // ####################################################################

    // Wait for chunk to load via chunk observer if required
    private IEnumerator TeleportWhenLoaded(World world,
        GameManager gm, Vector3i src, Vector3 dst, float delay)
    {
        if (Observer != null) Log.Error("Pending Observer?");
        // Check if the chunk needs to be loaded first
        if (world.GetChunkFromWorldPos(World.worldToBlockPos(dst)) == null)
        {
            // Add a chunk observer if the chunk is not loaded
            Observer = gm.AddChunkObserver(dst, false, 3, -1);
            Log.Out("Initial wait for chunk to load for teleport");
        }
        // Wait for the initial amount of time
        yield return new WaitForSeconds(delay);
        // Endless loop until chunk is loaded
        for (int i = 0; i < 250; i++)
        {
            // Check if the chunk needs to be loaded first
            if (world.GetChunkFromWorldPos(World.worldToBlockPos(dst)) != null)
            {
                // Create package to inform user to teleport to new position
                var pkg = NetPackageManager.GetPackage<PkgExecuteTeleport>();
                pkg.Setup(src, dst, null, false);
                // Process right away if there is not sender (executed in SP)
                if (Sender != null) Sender.SendPackage(pkg);
                else pkg.ProcessPackage(world, gm);
                ClearChunkObserver(gm);
                // Stop coroutine
                yield break;
            }
            Log.Out("Initial wait for chunk to load for teleport");
            // Maybe chunk was loaded first but unloaded during delay?
            Observer ??= gm.AddChunkObserver(dst, false, 3, -1);
            yield return new WaitForSeconds(0.25f);
        }
        // We gave up, chunk failed to load?
        Log.Error("Chunk failed to load, give up");
        ClearChunkObserver(gm);
    }

    // ####################################################################
    // ####################################################################

    public override void read(PooledBinaryReader br)
    {
        Position = StreamUtils.ReadVector3i(br);
        Delay = br.ReadSingle();
    }

    public override void write(PooledBinaryWriter bw)
    {
        base.write(bw);
        StreamUtils.Write(bw, Position);
        bw.Write(Delay);
    }

    public override int GetLength() => 50;

    // ####################################################################
    // ####################################################################

}

