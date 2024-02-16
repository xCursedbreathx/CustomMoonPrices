using GameNetcodeStuff;
using HarmonyLib;
using LCCustomMoonPrices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;

namespace CustomMoonPrices
{
    [Serializable]
    public class Config : SyncedInstance<Config>
    {
        
        public Dictionary<string, bool> moonPriceEnabled;

        public Dictionary<string, int> moonPrice;

        public Config()
        {
            InitInstance(this);
            moonPriceEnabled = new Dictionary<string, bool>();
            moonPrice = new Dictionary<string, int>();
        }

        public void updateMoonEnabled(string moonname, bool enabled)
        {

            if (!IsHost) return;

            moonPriceEnabled[moonname] = enabled;

            InitInstance(this);

            using (FastBufferWriter messageStream = new FastBufferWriter(SyncedInstance<Config>.IntSize, Allocator.Temp))

                MessageManager.SendNamedMessageToAll("CustomMoonPrices_ResyncConfigOnChange", messageStream);

        }

        public void updateMoonPrice(string moonname, int price)
        {

            if (!IsHost) return;

            moonPrice[moonname] = price;

            InitInstance(this);

            using (FastBufferWriter messageStream = new FastBufferWriter(SyncedInstance<Config>.IntSize, Allocator.Temp))

                MessageManager.SendNamedMessageToAll("CustomMoonPrices_ResyncConfigOnChange", messageStream);

        }

        public static void RequestSync()
        {
            CustomMoonPricesMain.CMPLogger.LogMessage("Requesting sync");

            if (!SyncedInstance<Config>.IsClient) return;

            using (FastBufferWriter messageStream = new FastBufferWriter(SyncedInstance<Config>.IntSize, Allocator.Temp))
                SyncedInstance<Config>.MessageManager.SendNamedMessage("CustomMoonPrices_OnRequestConfigSync", 0UL, messageStream);
        }

        public static void OnRequestSync(ulong clientId, FastBufferReader _)
        {
            CustomMoonPricesMain.CMPLogger.LogMessage("Incoming Sync Request by: " + clientId);
            if (!SyncedInstance<Config>.IsHost) return;
            byte[] bytes = SyncedInstance<Config>.SerializeToBytes(SyncedInstance<Config>.Instance);
            int length = bytes.Length;
            using (FastBufferWriter messageStream = new FastBufferWriter(length + SyncedInstance<Config>.IntSize, Allocator.Temp))
            {
                try
                {
                    messageStream.WriteValueSafe<int>(in length, new FastBufferWriter.ForPrimitives());
                    messageStream.WriteBytesSafe(bytes);
                    SyncedInstance<Config>.MessageManager.SendNamedMessage("CustomMoonPrices_OnReceiveConfigSync", clientId, messageStream);
                }
                catch (Exception ex)
                {
                    CustomMoonPricesMain.CMPLogger.LogDebug((object)string.Format("Error occurred syncing config with client: {0}\n{1}", (object)clientId, (object)ex));
                }
            }
        }

        public static void OnReceiveSync(ulong _, FastBufferReader reader)
        {
            CustomMoonPricesMain.CMPLogger.LogMessage("Incoming Sync from: " + _);

            if (!reader.TryBeginRead(SyncedInstance<Config>.IntSize))
            {
                CustomMoonPricesMain.CMPLogger.LogError("Config sync error: Could not begin reading buffer.");
            }
            else
            {
                int length;
                reader.ReadValueSafe<int>(out length, new FastBufferWriter.ForPrimitives());
                if (!reader.TryBeginRead(length))
                {
                    CustomMoonPricesMain.CMPLogger.LogError("Config sync error: Host could not sync.");
                }
                else
                {
                    byte[] data = new byte[length];
                    reader.ReadBytesSafe(ref data, length);
                    SyncedInstance<Config>.SyncInstance(data);
                    CustomMoonPricesMain.CMPLogger.LogMessage("Successfully synced config with host.");
                }
            }
        }

        public static void OnSettingChange(ulong _, FastBufferReader reader)
        {
            Config.RequestSync();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
        public static void InitializeLocalPlayer()
        {
            if (SyncedInstance<Config>.IsHost)
            {
                // ISSUE: reference to a compiler-generated field
                // ISSUE: reference to a compiler-generated field
                SyncedInstance<Config>.MessageManager.RegisterNamedMessageHandler("CustomMoonPrices_OnRequestConfigSync", Config.OnRequestSync);
                SyncedInstance<Config>.Synced = true;
                Config.RequestSync();
            }
            else
            {
                SyncedInstance<Config>.Synced = false;
                // ISSUE: reference to a compiler-generated field
                // ISSUE: reference to a compiler-generated field
                SyncedInstance<Config>.MessageManager.RegisterNamedMessageHandler("CustomMoonPrices_OnReceiveConfigSync", Config.OnReceiveSync);
                SyncedInstance<Config>.MessageManager.RegisterNamedMessageHandler("CustomMoonPrices_ResyncConfigOnChange", Config.OnSettingChange);
                Config.RequestSync();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
        public static void PlayerLeave() => SyncedInstance<Config>.RevertSync();

    }
}
