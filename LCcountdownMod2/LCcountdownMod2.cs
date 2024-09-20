using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BepInEx.Configuration;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;

namespace LCcountdownMod2
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("ainavt.lc.lethalconfig", BepInDependency.DependencyFlags.SoftDependency)]
    public class LCcountdownMod2 : BaseUnityPlugin
    {
        public static LCcountdownMod2 Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        public static bool DebugMode = false, SpawnCountdownInfrountOfUI, StartCountdownWhenShipLeaveEarly, StopCountdownAfter12Am, StopCountdownAfterDeath, StopCountdownAfterShipLeaves, StopCountdownAfterTwelve;
        public static GameObject countdownPrefab;
        public static Countdowner CountDownInstace;
        public static string TextColor = "(214,98,41,255)", CircleColor = "(214,98,41,255)";
        public static string[] Txts = ["10", "9", "8", "7", "6", "5", "4", "3", "2", "1", "0"];
        public static int[] TxtSizes = [36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36];

        private ConfigEntry<string>[] txtConfigs;
        private ConfigEntry<int>[] txtSizeConfigs;
        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;
            InitalizeAssets();
            Patch();
            BindConfigs();
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }
        internal static void InitalizeAssets()
        {
            countdownPrefab = AssetLoader.LoadAsset<GameObject>("Assets/CountDownAssets/Countdown.prefab");
            Countdowner downscript = countdownPrefab.AddComponent<Countdowner>();
            downscript.anim = downscript.GetComponent<Animator>();
            downscript.SFX = countdownPrefab.transform.Find("SFX").GetComponent<AudioSource>();
            downscript.TXT = countdownPrefab.transform.Find("Image/Number").GetComponent<TMP_Text>();
            downscript.Sprite = countdownPrefab.transform.Find("Image").GetComponent<Image>();
        }

        public void BindConfigs()
        {
            var textColorConfig = Config.Bind("Colors", "Text Color", "(214,98,41,255)", "Color for the text. Format: (R,G,B,A)");
            var circleColorConfig = Config.Bind("Colors", "Circle Color", "(214,98,41,255)", "Color for the circle. Format: (R,G,B,A)");
            var startCountdownWhenShipLeaveEarlyConfig = Config.Bind("Countdown", "Start Countdown When Ship Leaves Early", true, "Start the countdown when the ship leaves early");
            var stopCountdownAfter12AmConfig = Config.Bind("Countdown", "Stop Countdown After 12 AM", false, "Stop the countdown after 12 AM");
            var stopCountdownAfterDeathConfig = Config.Bind("Countdown", "Stop Countdown After Death", true, "Stop the countdown after death");
            var stopCountdownAfterShipLeavesConfig = Config.Bind("Countdown", "Stop Countdown After Ship Leaves", true, "Stop the countdown after the ship leaves");
            var stopCountdownAfterTwelveConfig = Config.Bind("Countdown", "Stop Countdown After Twelve", true, "Stop the countdown after twelve");
            var SpawnCountdownInfrountOfUIConfig = Config.Bind("Countdown", "Show coundown in front", false, "Spawns the countdown infront of the main UI");
            txtConfigs = new ConfigEntry<string>[Txts.Length];
            for (int i = 0; i < Txts.Length; i++)
            {
                int configIndex = Txts.Length - 1 - i;  // Reverse the index
                txtConfigs[i] = Config.Bind("Countdown Text", $"Text {configIndex}", Txts[i], $"Text to display for countdown step {configIndex}");
                int index = i;
                txtConfigs[i].SettingChanged += (_, _) =>
                {
                    Txts[index] = txtConfigs[index].Value;
                };
            }

            // Configure TxtSizes
            txtSizeConfigs = new ConfigEntry<int>[TxtSizes.Length];
            for (int i = 0; i < TxtSizes.Length; i++)
            {
                int configIndex = TxtSizes.Length - 1 - i;  // Reverse the index
                txtSizeConfigs[i] = Config.Bind("Countdown Text Sizes", $"Size {configIndex}", TxtSizes[i], $"Font size for countdown step {configIndex}");
                int index = i;
                txtSizeConfigs[i].SettingChanged += (_, _) =>
                {
                    TxtSizes[index] = txtSizeConfigs[index].Value;
                };
            }

            // Update arrays with config values
            for (int i = 0; i < Txts.Length; i++)
            {
                Txts[i] = txtConfigs[i].Value;
                TxtSizes[i] = txtSizeConfigs[i].Value;
            }


            TextColor = textColorConfig.Value;
            CircleColor = circleColorConfig.Value;
            StartCountdownWhenShipLeaveEarly = startCountdownWhenShipLeaveEarlyConfig.Value;
            StopCountdownAfter12Am = stopCountdownAfter12AmConfig.Value;
            StopCountdownAfterDeath = stopCountdownAfterDeathConfig.Value;
            StopCountdownAfterShipLeaves = stopCountdownAfterShipLeavesConfig.Value;
            StopCountdownAfterTwelve = stopCountdownAfterTwelveConfig.Value;
            SpawnCountdownInfrountOfUI = SpawnCountdownInfrountOfUIConfig.Value;
            textColorConfig.SettingChanged += (_, _) =>
   {
       TextColor = textColorConfig.Value;
       if (CountDownInstace != null)
           CountDownInstace.SetColors();
   };

            circleColorConfig.SettingChanged += (_, _) =>
            {
                CircleColor = circleColorConfig.Value;
                CountDownInstace.SetColors();
            };

            // Add SettingChanged events for each config
            if (IsDependencyLoaded("ainavt.lc.lethalconfig"))
            {
                BindLethalConfigs(textColorConfig, circleColorConfig, startCountdownWhenShipLeaveEarlyConfig,
                    stopCountdownAfter12AmConfig, stopCountdownAfterDeathConfig, stopCountdownAfterShipLeavesConfig,
                    stopCountdownAfterTwelveConfig, SpawnCountdownInfrountOfUIConfig);
            }
        }
        public void BindLethalConfigs(ConfigEntry<string> textColorConfig, ConfigEntry<string> circleColorConfig,
    ConfigEntry<bool> startCountdownWhenShipLeaveEarlyConfig, ConfigEntry<bool> stopCountdownAfter12AmConfig,
    ConfigEntry<bool> stopCountdownAfterDeathConfig, ConfigEntry<bool> stopCountdownAfterShipLeavesConfig,
    ConfigEntry<bool> stopCountdownAfterTwelveConfig, ConfigEntry<bool> SCDIFOUI)
        {
            var textColorItem = new TextInputFieldConfigItem(textColorConfig, new TextInputFieldOptions
            {
                RequiresRestart = false,
            });
            LethalConfigManager.AddConfigItem(textColorItem);

            var circleColorItem = new TextInputFieldConfigItem(circleColorConfig, new TextInputFieldOptions
            {
                RequiresRestart = false,
            });
            LethalConfigManager.AddConfigItem(circleColorItem);

            var startCountdownWhenShipLeaveEarlyItem = new BoolCheckBoxConfigItem(startCountdownWhenShipLeaveEarlyConfig, new BoolCheckBoxOptions
            {
                RequiresRestart = false,
            });
            LethalConfigManager.AddConfigItem(startCountdownWhenShipLeaveEarlyItem);

            var stopCountdownAfter12AmItem = new BoolCheckBoxConfigItem(stopCountdownAfter12AmConfig, new BoolCheckBoxOptions
            {
                RequiresRestart = false,
            });
            LethalConfigManager.AddConfigItem(stopCountdownAfter12AmItem);

            var stopCountdownAfterDeathItem = new BoolCheckBoxConfigItem(stopCountdownAfterDeathConfig, new BoolCheckBoxOptions
            {
                RequiresRestart = false,
            });
            LethalConfigManager.AddConfigItem(stopCountdownAfterDeathItem);

            var stopCountdownAfterShipLeavesItem = new BoolCheckBoxConfigItem(stopCountdownAfterShipLeavesConfig, new BoolCheckBoxOptions
            {
                RequiresRestart = false,
            });
            LethalConfigManager.AddConfigItem(stopCountdownAfterShipLeavesItem);

            var stopCountdownAfterTwelveItem = new BoolCheckBoxConfigItem(stopCountdownAfterTwelveConfig, new BoolCheckBoxOptions
            {
                RequiresRestart = false,
            });
            LethalConfigManager.AddConfigItem(stopCountdownAfterTwelveItem);

            var SpawnCountdownIFOUI = new BoolCheckBoxConfigItem(SCDIFOUI, new BoolCheckBoxOptions
            {
                RequiresRestart = true,
            });
            LethalConfigManager.AddConfigItem(SpawnCountdownIFOUI);

            // Add Txt configs
            for (int i = 0; i < txtConfigs.Length; i++)
            {
                int configIndex = txtConfigs.Length - 1 - i;  // Reverse the index
                var txtItem = new TextInputFieldConfigItem(txtConfigs[i], new TextInputFieldOptions
                {
                    RequiresRestart = false,
                    Name = $"Text {configIndex}"  // Set a custom name for the config item
                });
                LethalConfigManager.AddConfigItem(txtItem);
            }

            // Add TxtSize configs
            for (int i = 0; i < txtSizeConfigs.Length; i++)
            {
                int configIndex = txtSizeConfigs.Length - 1 - i;  // Reverse the index
                var txtSizeItem = new IntInputFieldConfigItem(txtSizeConfigs[i], new IntInputFieldOptions
                {
                    RequiresRestart = false,
                    Min = 1,
                    Max = 100,
                    Name = $"Size {configIndex}"  // Set a custom name for the config item
                });
                LethalConfigManager.AddConfigItem(txtSizeItem);
            }
        }

        public static bool IsDependencyLoaded(string pluginGUID)
        {
            return Chainloader.PluginInfos.ContainsKey(pluginGUID);
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            Harmony.PatchAll();

            Logger.LogDebug("Finished patching!");
        }

        internal static void Unpatch()
        {
            Logger.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            Logger.LogDebug("Finished unpatching!");
        }
    }
}
