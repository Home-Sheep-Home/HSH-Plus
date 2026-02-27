using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HSHPlus.Patches;

namespace HSHPlus;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static Harmony harmony = new("com.mrkingpenguin.HSHPlus");
    internal static new ManualLogSource Logger;
        
    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        harmony.PatchAll(typeof(TimerPatches));
        harmony.PatchAll(typeof(LevelPatches));
    }
}
