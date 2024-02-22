using HarmonyLib;
using LethalConfig.ConfigItems;
using LethalConfig;
using System;
using UnityEngine;

namespace CustomMoonPrices.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    public class MoonPricePatches
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
                CustomMoonPricesMain.CMPLogger.LogDebug("Terminal not found!");
            }
            else
            {
                CustomMoonPricesMain.CMPLogger.LogDebug("Terminal found!");
            }

            foreach (SelectableLevel level in StartOfRound.Instance.levels)
            {

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

                    CustomMoonPricesMain.CMPLogger.LogDebug("ConfigName: " + configname);

                    var ConfigEntry = CustomMoonPricesMain.LethalConfigSettings.Bind(configname, "Enable", false, "Setting if custom Price for: " + configname + " should be applied.");

                    var ConfigEntryPrice = CustomMoonPricesMain.LethalConfigSettings.Bind(configname, "Price", 0, "Setting the custom Price for: " + configname + ".");

                    ConfigEntry.SettingChanged += (sender, e) =>
                    {
                        bool ConfigEnabledMoons = CustomMoonPricesMain.LethalConfigSettings.Bind<bool>(configname, "Enable", false, "Setting if custom Price for: " + configname + " should be applied.").Value;

                        Config.Instance.updateMoonEnabled(configname, ConfigEnabledMoons);
                    };

                    ConfigEntryPrice.SettingChanged += (sender, e) =>
                    {
                        int ConfigPriceMoons = CustomMoonPricesMain.LethalConfigSettings.Bind<int>(configname, "Price", 0, "Setting the custom Price for: " + configname + ".").Value;

                        Config.Instance.updateMoonPrice(configname, ConfigPriceMoons);
                    };

                    CustomMoonPricesMain.LethalConfigSettings.Save();

                    Config.Instance.moonData[configname] = new moonData(ConfigEntry.Value, ConfigEntryPrice.Value);

                    var ConfigEntryCheckbox = new BoolCheckBoxConfigItem(ConfigEntry);

                    var ConfigEntryPriceInt = new IntInputFieldConfigItem(ConfigEntryPrice);

                    LethalConfigManager.AddConfigItem(ConfigEntryCheckbox);
                    LethalConfigManager.AddConfigItem(ConfigEntryPriceInt);

                }

            }

            foreach(TerminalNode node in MoonPricePatches.terminal.terminalNodes.terminalNodes)
            {
                if (node.buyRerouteToMoon != -2)
                {
                    
                }
            }

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

            if (moonname.Contains("Route") || moonname.Contains("Confirm"))
            {
                moonname = moonname.Replace("Route", "");
                moonname = moonname.Replace("Confirm", "");
            }

            moonname = GetMoonName(moonname);

            if (moonname == null)
            {
                CustomMoonPricesMain.CMPLogger.LogDebug("Moon not found (Just a Reminder when not a moon node)!");
                return;
            }

            if (Config.Instance.moonData[moonname].getEnabled())
            {
                totalCostOfItems = (int)terminalTraverse.GetValue();

                terminalTraverse.SetValue(Config.Instance.moonData[moonname].getPrice());
            }

        }

        [HarmonyPatch("LoadNewNode")]
        [HarmonyPostfix]
        private static void LoadNewNodePatchAfter(ref TerminalNode node)
        {

            if(MoonPricePatches.totalCostOfItems == -5) return;

            String moonname = node.name;

            if (moonname.Contains("route"))
            {
                moonname = moonname.Replace("route", "");
            }

            if (moonname.Contains("Route") || moonname.Contains("Confirm"))
            {
                moonname = moonname.Replace("Route", "");
                moonname = moonname.Replace("Confirm", "");
            }

            moonname = GetMoonName(moonname);

            if (moonname == null)
            {
                CustomMoonPricesMain.CMPLogger.LogDebug("Moon not found (Just a Reminder when not a moon node)!");
                return;
            }

            if (Config.Instance.moonData[moonname].getEnabled())
            {

                Traverse terminalTraverse = Traverse.Create(MoonPricePatches.terminal).Field("totalCostOfItems");

                terminalTraverse.SetValue(Config.Instance.moonData[moonname].getPrice());

            }

            totalCostOfItems = -5;
        }


        [HarmonyPatch("LoadNewNodeIfAffordable")]
        [HarmonyPrefix]
        private static void LoadNewNodeIfAffordablePatch(ref TerminalNode node)
        {
            if (node == null || node.buyRerouteToMoon == -1)
                return;

            String moonname = node.name;

            if (moonname.Contains("route"))
            {
                moonname = moonname.Replace("route", "");
            }

            if (moonname.Contains("Route") || moonname.Contains("Confirm"))
            {
                moonname = moonname.Replace("Route", "");
                moonname = moonname.Replace("Confirm", "");
            }

            moonname = GetMoonName(moonname);

            if (Config.Instance.moonData[moonname].getEnabled())
            {
                node.itemCost = Config.Instance.moonData[moonname].getPrice();
            }

        }

    }
}
