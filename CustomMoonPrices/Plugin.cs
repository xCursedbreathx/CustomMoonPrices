using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using CustomMoonPrices.Patches;
using HarmonyLib;
using System;
using System.Linq;
using Unity.Netcode;

namespace CustomMoonPrices
{

    [BepInPlugin(UMID, NAME, VERSION)]
    [BepInDependency("ainavt.lc.lethalconfig")]
    [BepInProcess("Lethal Company.exe")]
    public class CustomMoonPricesMain : BaseUnityPlugin
    {
        public const String UMID = "zz.cursedbreath.custommoonprices";
        public const String NAME = "Custom Moon Prices";
        public const String VERSION = "1.1.0";

        private Harmony harmony;

        public static CustomMoonPricesMain CustomMoonPricesMainInstance;

        public static ConfigFile LethalConfigSettings;

        public static bool existLethalLevelLoader = false;



        //public static Config SyncedConfig;

        public static ManualLogSource CMPLogger;

        private void Awake()
        {
            CMPLogger = Logger;
            harmony = new Harmony("zz.cursedbreath.custommoonprices");

            LethalConfigSettings = this.Config;

            new Config();
            //SyncedConfig = new Config();

            harmony.PatchAll(typeof (MoonPricePatches));
            harmony.PatchAll(typeof (ConfigSyncPatch));

            foreach (PluginInfo pluginInfo in Chainloader.PluginInfos.Values.ToList<PluginInfo>())
            {
                CustomMoonPricesMain.CMPLogger.LogMessage("PluginGUID: " + pluginInfo.Metadata.GUID);
                if (pluginInfo.Metadata.GUID == "imabatby.lethallevelloader")
                {
                    existLethalLevelLoader = true;
                }
            }


        }

    }

    [Serializable]
    public class moonData
    {


        private bool enabled;
        private int price;

        public moonData(bool enabled, int price)
        {
            this.enabled = enabled;
            this.price = price;
        }

        public bool getEnabled()
        {
            return enabled;
        }

        public int getPrice()
        {
            return price;
        }

        public void setEnabled(bool enabled)
        {
            this.enabled = enabled;
        }

        public void setPrice(int price)
        {
            this.price = price;
        }

    }
}
