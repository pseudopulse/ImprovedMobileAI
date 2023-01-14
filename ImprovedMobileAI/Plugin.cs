using BepInEx;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using BepInEx.Configuration;
using System.Reflection;

namespace ImprovedMobileAI {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    
    public class ImprovedMobileAI : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "pseudopulse";
        public const string PluginName = "ImprovedMobileAI";
        public const string PluginVersion = "1.0.0";

        public static BepInEx.Logging.ManualLogSource ModLogger;
        public static ConfigFile config;

        public void Awake() {
            // set logger and config
            ModLogger = Logger;
            config = Config;


            // run stuff
            Tweaks.Drivers.Setup();
        }
    }
}