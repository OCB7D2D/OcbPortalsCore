using HarmonyLib;
using System.Reflection;

public class PortalsCore : IModApi
{

    public void InitMod(Mod mod)
    {
        Log.Out("OCB Harmony Patch: " + GetType().ToString());
        Harmony harmony = new Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

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


    // Register event handlers when game starts
    /*[HarmonyPatch(typeof(GameStateManager))]
    [HarmonyPatch("StartGame")]
    public class GameStateManager_StartGame
    {
        static void Postfix()
        {
            XUi xui = LocalPlayerUI.GetUIForPrimaryPlayer()?.xui;
            // Force instance; player wouldn't be known otherwise
            PortalsCoreManager.Instance.AttachPlayerAndInventory(xui);
        }
    }*/

}
