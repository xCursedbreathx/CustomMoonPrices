using LCCustomMoonPrices;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

namespace CustomMoonPrices
{
    [Serializable]
    public class Config : SyncedInstance<Config>
    {
        
        public Dictionary<string, moonData> moonData;

        public Config()
        {
            InitInstance(this);
            moonData = new Dictionary<string, moonData>();
        }

        public void updateMoonEnabled(string moonname, bool enabled)
        {

            if (!IsHost) return;

            Config.Instance.moonData[moonname].setEnabled(enabled);

            using (FastBufferWriter messageStream = new FastBufferWriter(IntSize, Allocator.Temp))

                MessageManager.SendNamedMessageToAll("CMP_SC", messageStream);

        }

        public void updateMoonPrice(string moonname, int price)
        {

            if (!IsHost) return;

            Config.Instance.moonData[moonname].setPrice(price);

            using (FastBufferWriter messageStream = new FastBufferWriter(IntSize, Allocator.Temp))

                MessageManager.SendNamedMessageToAll("CMP_SC", messageStream);

        }

        public static void RequestSync()
        {
            CustomMoonPricesMain.CMPLogger.LogMessage("Requesting config sync...");

            if (!IsClient) return;

            CustomMoonPricesMain.CMPLogger.LogMessage("Sending request to host...");

            using (FastBufferWriter messageStream = new FastBufferWriter(IntSize, Allocator.Temp))
                MessageManager.SendNamedMessage("CMP_ORCS", 0UL, messageStream);

            CustomMoonPricesMain.CMPLogger.LogMessage("Sending request to host completed");
        }

        public static void OnRequestSync(ulong clientId, FastBufferReader _)
        {

            if (!IsHost) return;

            byte[] bytes = SerializeToBytes(Instance);
            int length = bytes.Length;
            using (FastBufferWriter messageStream = new FastBufferWriter(length + IntSize, Allocator.Temp))
            {

                CustomMoonPricesMain.CMPLogger.LogMessage("Trying to Write Config into Stream");

                try
                {
                    messageStream.WriteValueSafe<int>(in length, new FastBufferWriter.ForPrimitives());
                    messageStream.WriteBytesSafe(bytes);

                    MessageManager.SendNamedMessage("CMP_ORS", clientId, messageStream);
                }
                catch (Exception ex)
                {
                    CustomMoonPricesMain.CMPLogger.LogDebug(string.Format("Error occurred syncing config with client: {0}\n{1}", clientId, ex));
                }
            }

        }

        public static void OnReceiveSync(ulong _, FastBufferReader reader)
        {
            if (!reader.TryBeginRead(IntSize))
            {
                CustomMoonPricesMain.CMPLogger.LogError("Config sync error: Could not begin reading buffer.");
                return;
            }

            reader.ReadValueSafe(out int val, default);
            if (!reader.TryBeginRead(val))
            {
                CustomMoonPricesMain.CMPLogger.LogError("Config sync error: Host could not sync.");
                return;
            }

            byte[] data = new byte[val];
            reader.ReadBytesSafe(ref data, val);

            try
            {
                SyncInstance(data);

                using (FastBufferWriter messageStream = new FastBufferWriter(IntSize, Allocator.Temp))
                    MessageManager.SendNamedMessage("CMP_SC", 0UL, messageStream);
            }
            catch (Exception e)
            {
                CustomMoonPricesMain.CMPLogger.LogError($"Error syncing config instance!\n{e}");
            }
        }

        public static void OnSettingChange(ulong _, FastBufferReader reader)
        {
            if(!IsClient) return;

            RequestSync();
        }

        public static void OnReceivedSync(ulong senderClientId, FastBufferReader messagePayload)
        {
            if (!IsHost) return;

            CustomMoonPricesMain.CMPLogger.LogMessage("Client Succesfully Synced.");
        }
    }
}
