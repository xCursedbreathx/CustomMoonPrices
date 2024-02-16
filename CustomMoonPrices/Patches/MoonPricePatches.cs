using HarmonyLib;
using LethalConfig.ConfigItems;
using LethalConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomMoonPrices.Patches
{
    internal class MoonPricePatches
    {

        public static Terminal terminal;

        private static int totalCostOfItems = -5;

        private static String GetMoonName(String id)
        {

            switch (id)
            {
                case "8": return "titan";

                case "7": return "dine";

                case "85": return "rend";

                case "61": return "march";

                case "56": return "vow";

                case "220": return "assurance";

                case "41": return "experimentation";

                case "21": return "offense";

                case null: return null;
            }

            return null;
        }



        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void FindTerminal()
        {

            MoonPricePatches.terminal = GameObject.FindObjectOfType<Terminal>();

            if (MoonPricePatches.terminal == null)
            {
                CustomMoonPricesMain.CMPLogger.LogError("Terminal not found!");
            }
            else
            {
                CustomMoonPricesMain.CMPLogger.LogInfo("Terminal found!");
            }

            foreach (SelectableLevel level in StartOfRound.Instance.levels)
            {

                CustomMoonPricesMain.CMPLogger.LogInfo("PlanetName: " + level);

                if (!level.name.Contains("Company"))
                {

                    String configname;

                    String[] levelname = level.name.Split(' ');

                    if (levelname.Length >= 2)
                    {
                        configname = levelname[1].ToLower();
                    }
                    else
                    {
                        configname = level.name.Replace("Level", "").Trim().ToLower();
                    }

                    CustomMoonPricesMain.CMPLogger.LogInfo("ConfigName: " + configname);

                    var ConfigEntry = CustomMoonPricesMain.LethalConfigSettings.Bind(configname, "Enable", false, "Setting if custom Price for: " + configname + " should be applied.");

                    var ConfigEntryPrice = CustomMoonPricesMain.LethalConfigSettings.Bind(configname, "Price", 0, "Setting the custom Price for: " + configname + ".");

                    ConfigEntry.SettingChanged += (sender, e) =>
                    {
                        bool ConfigEnabledMoons = CustomMoonPricesMain.LethalConfigSettings.Bind<bool>(configname, "Enable", false, "Setting if custom Price for: " + configname + " should be applied.").Value;

                        int ConfigPriceMoons = CustomMoonPricesMain.LethalConfigSettings.Bind<int>(configname, "Price", 0, "Setting the custom Price for: " + configname + ".").Value;

                        CustomMoonPricesMain.Config.updateMoon(configname, ConfigEnabledMoons, ConfigPriceMoons);
                    };

                    ConfigEntryPrice.SettingChanged += (sender, e) =>
                    {
                        bool ConfigEnabledMoons = CustomMoonPricesMain.LethalConfigSettings.Bind<bool>(configname, "Enable", false, "Setting if custom Price for: " + configname + " should be applied.").Value;

                        int ConfigPriceMoons = CustomMoonPricesMain.LethalConfigSettings.Bind<int>(configname, "Price", 0, "Setting the custom Price for: " + configname + ".").Value;

                        CustomMoonPricesMain.Config.updateMoon(configname, ConfigEnabledMoons, ConfigPriceMoons);
                    };

                    CustomMoonPricesMain.LethalConfigSettings.Save();

                    Config.moonPriceEnabled[configname] = ConfigEntry.Value;

                    Config.moonPrice[configname] = ConfigEntryPrice.Value;

                    var ConfigEntryCheckbox = new BoolCheckBoxConfigItem(ConfigEntry);

                    var ConfigEntryPriceInt = new IntInputFieldConfigItem(ConfigEntryPrice);

                    LethalConfigManager.AddConfigItem(ConfigEntryCheckbox);
                    LethalConfigManager.AddConfigItem(ConfigEntryPriceInt);

                }

            }

            CustomMoonPricesMain.Config = new Config();

        }

        [HarmonyPatch("LoadNewNode")]
        [HarmonyPrefix]
        private static void LoadNewNodePatchBefore(ref TerminalNode node)
        {
            if (node.buyRerouteToMoon != -2) return;

            Traverse terminalTraverse = Traverse.Create(MoonPricePatches.terminal).Field("totalCostOfItems");

            String moonname = node.name;

            if (moonname.Contains("route"))
            {
                moonname = moonname.Replace("route", "");
            }

            if (moonname.Contains("Route") && moonname.Contains("Confirm"))
            {
                moonname = moonname.Replace("Route", "");
                moonname = moonname.Replace("Confirm", "");
            }

            CustomMoonPricesMain.CMPLogger.LogMessage("MoonName: " + moonname);

            moonname = GetMoonName(moonname);

            if (moonname == null)
            {
                CustomMoonPricesMain.CMPLogger.LogDebug("Moon not found (Just a Reminder when not a moon node)!");
                return;
            }

            bool isCustomPrice = Config.moonPriceEnabled[moonname];

            int customPrice = Config.moonPrice[moonname];

            if (isCustomPrice)
            {
                totalCostOfItems = (int)terminalTraverse.GetValue();

                terminalTraverse.SetValue(customPrice);

                node.itemCost = customPrice;
            }

        }

        [HarmonyPatch("LoadNewNode")]
        [HarmonyPostfix]
        private static void LoadNewNodePatchAfter(ref TerminalNode node)
        {
            Traverse terminalTraverse = Traverse.Create(MoonPricePatches.terminal).Field("totalCostOfItems");

            terminalTraverse.SetValue(totalCostOfItems);

            CustomMoonPricesMain.CMPLogger.LogError("traverseValueFinish: " + terminalTraverse.GetValue());

            totalCostOfItems = -5;
        }

        [HarmonyPatch("LoadNewNodeIfAffordable")]
        [HarmonyPrefix]
        private static void LoadNewNodeIfAffordablePatch(ref TerminalNode node)
        {
            if (node.buyRerouteToMoon == -1) return;

            String moonname = node.name.Replace("Route", "");

            if (moonname.Contains("route"))
            {
                moonname = moonname.Replace("route", "");
            }

            if (moonname.Contains("Confirm"))
            {
                moonname = moonname.Replace("Confirm", "");
            }

            moonname = GetMoonName(moonname);

            if (moonname == null)
            {
                CustomMoonPricesMain.CMPLogger.LogDebug("Moon not found (Just a Reminder when not a moon node)!");
                return;
            }

            bool isCustomPrice = Config.moonPriceEnabled[moonname];

            int customPrice = Config.moonPrice[moonname];

            CustomMoonPricesMain.CMPLogger.LogMessage("isCustomPrice for "+ moonname +" enabled: " + isCustomPrice);

            CustomMoonPricesMain.CMPLogger.LogMessage("customPrice " + moonname + " price: " + customPrice);

            if (isCustomPrice)
            {
                node.itemCost = customPrice;
            }

        }

    }
}
