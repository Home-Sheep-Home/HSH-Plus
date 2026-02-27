using HarmonyLib;
using UnityEngine;

namespace HSHPlus.Patches
{
    internal class LevelPatches
    {
        static bool sheepStatusDisplayFixed = false;

        [HarmonyPatch(typeof(Level), "Update")]
        [HarmonyPostfix]
        static void Level_Postfix()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
            {
                LevelLoader.instance.ReloadLevel(false);
            }
        }

        [HarmonyPatch(typeof(SheepManager), "Update")]
        [HarmonyPostfix]
        static void SheepManager_Postfix(SheepManager __instance)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Traverse.Create(__instance).Method("ChangeSheep", [typeof(Sheep)]).GetValue([__instance.shirley]);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Traverse.Create(__instance).Method("ChangeSheep", typeof(Sheep)).GetValue(__instance.shaun);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Traverse.Create(__instance).Method("ChangeSheep", typeof(Sheep)).GetValue(__instance.timmy);
            }
        }

        [HarmonyPatch(typeof(StorySheepStatusDisplay), "Update")]
        [HarmonyPrefix]
        static bool StorySheepStatusDisplay_Update_Prefix(StorySheepStatusDisplay __instance)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (!sheepStatusDisplayFixed)
                {
                    sheepStatusDisplayFixed = true;
                    Traverse.Create(__instance).Method("AnimateOnScreen").GetValue();
                }
                else
                {
                    sheepStatusDisplayFixed = false;
                    Traverse.Create(__instance).Method("AnimateOffScreen").GetValue();
                }
            }
            if (sheepStatusDisplayFixed)
            {
                return false;
            }
            return true;
        }
    }
}
