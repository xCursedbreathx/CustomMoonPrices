using CustomMoonPrices;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Netcode;

namespace LCCustomMoonPrices
{
    [Serializable]
    public class SyncedInstance<T>
    {
        static readonly DataContractSerializer serializer = new DataContractSerializer(typeof(T));
        internal static CustomMessagingManager MessageManager => NetworkManager.Singleton.CustomMessagingManager;
        internal static bool IsClient => NetworkManager.Singleton.IsClient;
        internal static bool IsHost => NetworkManager.Singleton.IsHost;

        [NonSerialized]
        protected static int IntSize = 4;

        public static T Default { get; private set; }
        public static T Instance { get; private set; }

        public static bool Synced { get; internal set; }

        protected void InitInstance(T instance)
        {
            SyncedInstance<T>.Default = instance;
            SyncedInstance<T>.Instance = instance;

            IntSize = sizeof(int);
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
            using (MemoryStream serializationStream = new MemoryStream())
            {
                try
                {
                    serializer.WriteObject(serializationStream, val);
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
                    return (T)serializer.ReadObject(serializationStream);
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
