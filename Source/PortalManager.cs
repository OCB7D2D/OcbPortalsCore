using System.Collections.Generic;
using System.IO;
using System.Linq;

public class PortalManager : PersistedData<PortalManager>
{


    // ####################################################################
    // Configuration for persitant data implementation
    // ####################################################################

    public static byte FileVersion = 1;

    public override string GetStoragePath() => string.Format(
        "{0}/ocb-portal-manager.dat", GameIO.GetSaveGameDir());
    public override string GetBackupPath() => string.Format(
        "{0}/ocb-portal-manager.dat.bak", GameIO.GetSaveGameDir());
    public override string GetThreadKey() => "silent_ocbPortalManagerDataSave";

    // ####################################################################
    // ####################################################################

    public static void Cleanup()
    {
        if (instance == null) return;
        instance.CleanupInstance();
    }

    protected override void CleanupInstance()
    {
        // Save out state first
        base.CleanupInstance();
        Portals.Clear();
        instance = null;
    }

    // ####################################################################
    // ####################################################################

    // Use a dumb and unoptimized structure for now
    // Makes it easier to keep everything in sync
    // May start to regress performance around 100 portals
    private readonly List<PortalBlockData> Portals = new List<PortalBlockData>();

    // ####################################################################
    // ####################################################################

    public override void Read(BinaryReader br)
    {
        var version = br.ReadByte();
        int count = br.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            var data = new PortalBlockData();
            data.Read(br);
            Log.Out("LOADED PORTAL at {0} => {1}",
                data.Position, data.Group);
            Portals.Add(data);
        }
    }

    public override void Write(BinaryWriter bw)
    {
        bw.Write(FileVersion);
        bw.Write(Portals.Count);
        foreach (var portal in Portals)
            portal.Write(bw);
    }

    // ####################################################################
    // ####################################################################

    public void AddPortal(Vector3i position, string group = null)
    {
        // var block = bv.Block as BlockPortal;
        if (Portals.Any(x => x.Position == position))
            Log.Error("Portal Manager has duplicate entry");
        else Portals.Add(new PortalBlockData(position, group));
        Log.Out("Added portal {0}", position);
    }

    public void RemovePortal(Vector3i position, BlockValue bv)
        => Portals.RemoveAll(x => x.Position == position);

    // ####################################################################
    // Get next portal position for portal at `position`
    // ####################################################################

    public PortalBlockData GetNextPosition(Vector3i position)
    {
        Log.Out("Search for {0} in {1}", position, Portals.Count);
        // Find the current portal at `position` in the list
        for (int i = 0; i < Portals.Count; i++)
        {
            Log.Out("Has {0}", Portals[i].Position);
            if (Portals[i].Position == position)
            {
                Log.Out("Search Next Teleport for {0}", Portals[i].Group);
                // Search for next portal with the same group
                for (int n = i + 1; n < Portals.Count; n++)
                {
                    if (Portals[i].Group == Portals[n].Group) return Portals[n];
                }
                // Re-start search from begining
                for (int n = 0; n < i; n++)
                {
                    if (Portals[i].Group == Portals[n].Group) return Portals[n];
                }
            }
        }
        // Source not found
        return null;
    }

    public PortalBlockData GetPortalAt(Vector3i position)
        => Portals.Find(x => x.Position == position);
 
    // ####################################################################
    // ####################################################################

}

