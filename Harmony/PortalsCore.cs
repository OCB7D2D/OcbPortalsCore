using HarmonyLib;
using System.Reflection;

public class PortalsCore : IModApi
{

    // ####################################################################
    // ####################################################################

    public void InitMod(Mod mod)
    {
        Log.Out("OCB Harmony Patch: " + GetType().ToString());
        Harmony harmony = new Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    // ####################################################################
    // ####################################################################

    // Hook when `PowerManger` is loaded
    [HarmonyPatch(typeof(PowerManager), "LoadPowerManager")]
    public class PowerManager_LoadPowerManager
    {
        public static void Prefix()
        {
            PortalManager.Instance.LoadPersistedData();
        }
    }

    // Hook when `PowerManger` is cleaned up
    [HarmonyPatch(typeof(VehicleManager), "Cleanup")]
    public class VehicleManager_Cleanup
    {
        public static void Prefix()
        {
            PortalManager.Cleanup();
        }
    }

    // ####################################################################
    // ####################################################################

    // Hook when `PowerManger` is cleaned up
    [HarmonyPatch(typeof(TileEntitySign), "SetText")]
    public class TileEntitySignSetTextPatch
    {
        public static void Postfix(TileEntitySign __instance, string _text, bool _syncData)
        {
            if (!ConnectionManager.Instance.IsServer) return;
            var position = __instance.ToWorldPos();
            PortalBlockData portal = PortalManager.Instance.GetPortalAt(position + Vector3i.down);
            if (portal == null) portal = PortalManager.Instance.GetPortalAt(position);
            Log.Out("Update portal group at {0} to {1} (sync: {2})", portal, _text, _syncData);
            if (portal != null) portal.Group = _text;
        }
    }

    // ####################################################################
    // ####################################################################

}
