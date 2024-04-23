using HarmonyLib;
using LethalConfig.ConfigItems;
using LethalConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomMoonPrices.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    public class Start_Round_Patch
    {

        internal static string GetNumberlessPlanetName(SelectableLevel selectableLevel)
        {
            if (selectableLevel != null)
            {
                return new string(selectableLevel.PlanetName.SkipWhile((char c) => !char.IsLetter(c)).ToArray());
            }

            return string.Empty;
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void Start()
        {

            CustomMoonPricesMain.CMPLogger.LogError("Starting to generate Config!");

            try
            {
                foreach (LethalLevelLoader.ExtendedLevel extendedLevel in LethalLevelLoader.PatchedContent.CustomExtendedLevels)
                {
                    String configname = extendedLevel.NumberlessPlanetName;
                    configname = configname.Replace(" ", "").ToLower();

                    if (configname.Contains("gordion")) continue;

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

                    if (Config.Instance.moonData.ContainsKey(configname))
                    {

                        bool ConfigEnabledMoons = CustomMoonPricesMain.LethalConfigSettings.Bind<bool>(configname, "Enable", false, "Setting if custom Price for: " + configname + " should be applied.").Value;

                        int ConfigPriceMoons = CustomMoonPricesMain.LethalConfigSettings.Bind<int>(configname, "Price", 0, "Setting the custom Price for: " + configname + ".").Value;

                        Config.Instance.updateMoonEnabled(configname, ConfigEnabledMoons);
                        Config.Instance.updateMoonPrice(configname, ConfigPriceMoons);

                    }
                    else
                    {

                        Config.Instance.moonData[configname] = new moonData(ConfigEntry.Value, ConfigEntryPrice.Value);

                    }

                    var ConfigEntryCheckbox = new BoolCheckBoxConfigItem(ConfigEntry);

                    var ConfigEntryPriceInt = new IntInputFieldConfigItem(ConfigEntryPrice);

                    LethalConfigManager.AddConfigItem(ConfigEntryCheckbox);
                    LethalConfigManager.AddConfigItem(ConfigEntryPriceInt);

                    if (!MoonPricePatches.defaultMoonPrices.ContainsKey(configname)) MoonPricePatches.defaultMoonPrices.Add(configname, extendedLevel.RoutePrice);

                    CustomMoonPricesMain.LethalConfigSettings.Save();

                }

            }
            catch (Exception e)
            {
                CustomMoonPricesMain.CMPLogger.LogError("Error in finding Adding Moons to Dictionary: " + e);
            }

            try
            {

                foreach (SelectableLevel level in StartOfRound.Instance.levels)
                {

                    if (!level.name.Contains("Company"))
                    {

                        CustomMoonPricesMain.CMPLogger.LogError("Trying to add moon: " + level.name + " to Config.");

                        String configname;

                        configname = GetNumberlessPlanetName(level).ToLower();

                        CustomMoonPricesMain.CMPLogger.LogError("Moon ConfigName: " + configname);

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

                        if (Config.Instance.moonData.ContainsKey(configname))
                        {

                            bool ConfigEnabledMoons = CustomMoonPricesMain.LethalConfigSettings.Bind<bool>(configname, "Enable", false, "Setting if custom Price for: " + configname + " should be applied.").Value;

                            int ConfigPriceMoons = CustomMoonPricesMain.LethalConfigSettings.Bind<int>(configname, "Price", 0, "Setting the custom Price for: " + configname + ".").Value;

                            Config.Instance.updateMoonEnabled(configname, ConfigEnabledMoons);
                            Config.Instance.updateMoonPrice(configname, ConfigPriceMoons);

                        }
                        else
                        {

                            Config.Instance.moonData[configname] = new moonData(ConfigEntry.Value, ConfigEntryPrice.Value);

                        }

                        var ConfigEntryCheckbox = new BoolCheckBoxConfigItem(ConfigEntry);

                        var ConfigEntryPriceInt = new IntInputFieldConfigItem(ConfigEntryPrice);

                        LethalConfigManager.AddConfigItem(ConfigEntryCheckbox);
                        LethalConfigManager.AddConfigItem(ConfigEntryPriceInt);

                        if (!MoonPricePatches.defaultMoonPrices.ContainsKey(configname)) MoonPricePatches.defaultMoonPrices.Add(configname, 0);

                        if (Config.Instance.moonData.ContainsKey(configname)) continue;

                        Config.Instance.moonData[configname] = new moonData(ConfigEntry.Value, ConfigEntryPrice.Value);

                        CustomMoonPricesMain.LethalConfigSettings.Save();

                    }

                }

            }
            catch (Exception e)
            {
                CustomMoonPricesMain.CMPLogger.LogError("Error in finding Adding Moons to Dictionary: " + e);
            }

        }

    }
}
