using GameNetcodeStuff;
using HarmonyLib;
using LCCustomMoonPrices;

namespace CustomMoonPrices.Patches
{
    public class ConfigSyncPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
        public static void InitializeLocalPlayer()
        {
            if (SyncedInstance<Config>.IsHost)
            {
                Config.MessageManager.RegisterNamedMessageHandler("CMP_ORS", Config.OnRequestSync);
                Config.MessageManager.RegisterNamedMessageHandler("CMP_SC", Config.OnReceivedSync);
                Config.Synced = true;
            }
            else
            {
                Config.Synced = false;
                Config.MessageManager.RegisterNamedMessageHandler("CMP_OSC", Config.OnSettingChange);
                Config.MessageManager.RegisterNamedMessageHandler("CMP_ORCS", Config.OnReceiveSync);
                Config.RequestSync();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
        public static void PlayerLeave() => SyncedInstance<Config>.RevertSync();
    }
}
