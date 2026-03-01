using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HSHPlus.Patches;

namespace HSHPlus;

[BepInPlugin(GUID, NAME, VERSION)]
public class Plugin : BaseUnityPlugin
{
    private const string GUID = "com.mrkingpenguin.HSHPlus";
    private const string NAME = "HSH Plus";
    private const string VERSION = "1.0.0";

    internal static Harmony harmony = new("com.mrkingpenguin.HSHPlus");
    internal static new ManualLogSource Logger;
        
    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {NAME} is loaded!");
        harmony.PatchAll(typeof(TimerPatches));
        harmony.PatchAll(typeof(LevelPatches));
    }
}
