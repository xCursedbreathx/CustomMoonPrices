using CustomMoonPrices;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Netcode;

namespace LCCustomMoonPrices
{
    [Serializable]
    public class SyncedInstance<T>
    {
        [NonSerialized]
        protected static int IntSize = 4;

        internal static CustomMessagingManager MessageManager => NetworkManager.Singleton.CustomMessagingManager;

        internal static bool IsClient => NetworkManager.Singleton.IsClient;

        internal static bool IsHost => NetworkManager.Singleton.IsHost;

        public static T Default { get; set; }

        public static T Instance { get; set; }

        public static bool Synced { get; internal set; }

        protected void InitInstance(T instance)
        {
            SyncedInstance<T>.Default = instance;
            SyncedInstance<T>.Instance = instance;
        }

        internal static void SyncInstance(byte[] data)
        {
            SyncedInstance<T>.Instance = SyncedInstance<T>.DeserializeFromBytes(data);
            SyncedInstance<T>.Synced = true;
        }

        internal static void RevertSync()
        {
            SyncedInstance<T>.Instance = SyncedInstance<T>.Default;
            SyncedInstance<T>.Synced = false;
        }

        public static byte[] SerializeToBytes(T val)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream serializationStream = new MemoryStream())
            {
                try
                {
                    binaryFormatter.Serialize(serializationStream, val);
                    return serializationStream.ToArray();
                }
                catch (Exception ex)
                {
                    CustomMoonPricesMain.CMPLogger.LogError("Error serializing instance: " + ex);
                    return (byte[])null;
                }
            }
        }

        public static T DeserializeFromBytes(byte[] data)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream serializationStream = new MemoryStream(data))
            {
                try
                {
                    CustomMoonPricesMain.CMPLogger.LogMessage("Deserializing instance...");
                    return (T)binaryFormatter.Deserialize(serializationStream);
                }
                catch (Exception ex)
                {
                    CustomMoonPricesMain.CMPLogger.LogError("Error deserializing instance: " + ex);
                    return default(T);
                }
            }
        }
    }
}
