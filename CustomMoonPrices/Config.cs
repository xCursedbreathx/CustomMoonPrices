using LCCustomMoonPrices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
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

            using (FastBufferWriter messageStream = new FastBufferWriter(30000, Allocator.Temp))

                MessageManager.SendNamedMessageToAll("CMP_OSC", messageStream);

        }

        public void updateMoonPrice(string moonname, int price)
        {

            if (!IsHost) return;

            Config.Instance.moonData[moonname].setPrice(price);

            using (FastBufferWriter messageStream = new FastBufferWriter(30000, Allocator.Temp))

                MessageManager.SendNamedMessageToAll("CMP_OSC", messageStream);

        }

        internal static void RequestSync()
        {
            if (!IsClient) return;

            using (FastBufferWriter message = new FastBufferWriter(30000, Allocator.Temp))
            {

                MessageManager.SendNamedMessage("CMP_ORS", 0UL, message);

                // Method `OnRequestSync` will then get called on host.
            }

        }

        internal static void OnRequestSync(ulong clientId, FastBufferReader _)
        {
            if (!IsHost) return;

            CustomMoonPricesMain.CMPLogger.LogDebug($"Config sync request received from client: {clientId}");

            byte[] array = SerializeToBytes(Config.Instance);
            int value = array.Length;
            int MaxSize = value + IntSize;

            using (FastBufferWriter message = new FastBufferWriter(MaxSize, Allocator.Temp))
            {

                try
                {

                    bool fragment = message.Capacity > 1000;
                    NetworkDelivery delivery = fragment ? NetworkDelivery.ReliableFragmentedSequenced : NetworkDelivery.Reliable;

                    if (fragment) CustomMoonPricesMain.CMPLogger.LogDebug(
                        $"Size of stream ({message.Capacity}) was past the max buffer size.\n" +
                        "Config instance will be sent in fragments to avoid overflowing the buffer."
                    );

                    message.WriteValueSafe(in value, default);
                    message.WriteBytesSafe(array, value);

                    MessageManager.SendNamedMessage("CMP_ORCS", clientId, message, delivery);

                }
                catch (Exception e)
                {
                    CustomMoonPricesMain.CMPLogger.LogDebug($"Error occurred syncing config with client: {clientId}\n{e}");
                }

            }

        }

        internal static void OnReceiveSync(ulong _, FastBufferReader reader)
        {
            CustomMoonPricesMain.CMPLogger.LogDebug("Config sync received from host.");

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

                using (FastBufferWriter messageStream = new FastBufferWriter(30000, Allocator.Temp))

                    MessageManager.SendNamedMessage("CMP_SC", 0UL, messageStream);
            }
            catch (Exception e)
            {
                CustomMoonPricesMain.CMPLogger.LogError($"Error syncing config instance!\n{e}");
            }
        }

        internal static void OnSettingChange(ulong _, FastBufferReader reader)
        {
            if(!IsClient) return;

            Config.RequestSync();
        }

        internal static void OnReceivedSync(ulong senderClientId, FastBufferReader messagePayload)
        {
            if (!IsHost) return;

            CustomMoonPricesMain.CMPLogger.LogDebug("Client " + senderClientId + " Succesfully Synced.");
        }
    }
}
