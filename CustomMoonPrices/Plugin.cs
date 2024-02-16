using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CustomMoonPrices.Patches;
using HarmonyLib;
using System;

namespace CustomMoonPrices
{

    [BepInPlugin(UMID, NAME, VERSION)]
    [BepInDependency("ainavt.lc.lethalconfig")]
    [BepInProcess("Lethal Company.exe")]
    public class CustomMoonPricesMain : BaseUnityPlugin
    {
        public const String UMID = "zz.cursedbreath.custommoonprices";
        public const String NAME = "Custom Moon Prices";
        public const String VERSION = "1.0.0";

        private Harmony harmony;

        public static CustomMoonPricesMain CustomMoonPricesMainInstance;

        public static ConfigFile LethalConfigSettings;



        public static Config SyncedCofig;

        public static ManualLogSource CMPLogger;

        private void Awake()
        {
            CMPLogger = Logger;
            harmony = new Harmony("zz.cursedbreath.custommoonprices");

            LethalConfigSettings = this.Config;

            harmony.PatchAll(typeof (MoonPricePatches));
            harmony.PatchAll(typeof (Config));
        }

    }
}
