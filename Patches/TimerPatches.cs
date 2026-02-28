using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HSHPlus.Patches
{
    internal class TimerPatches
    {
        static IEnumerable<CodeInstruction> AddMilliseconds(IEnumerable<CodeInstruction> instructions)
        {
            var stringIndex = -1;

            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                var strOperand = codes[i].operand as string;
                if (strOperand == "{0:D2}:{1:D2}")
                {
                    stringIndex = i;
                    break;
                }
            }
            if (stringIndex > -1)
            {
                codes[stringIndex].operand = "{0:D2}:{1:D2}.{2:D3}";
                codes[stringIndex + 7] = CodeInstruction.Call(typeof(string), "Format", [typeof(string), typeof(object), typeof(object), typeof(object)]);
                codes.InsertRange(stringIndex + 7, codes.GetRange(stringIndex + 1, 3));
                codes[stringIndex + 8] = CodeInstruction.Call(typeof(TimeSpan), "get_Milliseconds");
            }
            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(Story_LevelCompletePopup), "UpdateTime")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UpdateTime_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return AddMilliseconds(instructions);
        }

        [HarmonyPatch(typeof(Story_LevelCompletePopup), "SetupTimings")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> SetupTimings_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return AddMilliseconds(instructions);
        }

        [HarmonyPatch(typeof(Story_LevelCompletePopup), "TryStopTweening")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> TryStopTweening_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return AddMilliseconds(instructions);
        }

        [HarmonyPatch(typeof(StoryLevelSelectPanel), "SetBestTimeText")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> SetBestTimeText_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return AddMilliseconds(instructions);
        }

        [HarmonyPatch(typeof(StoryLevelSelectPanel), "SetupPanel")]
        [HarmonyPostfix]
        static void SetupPanel_Postfix(StoryLevelSelectPanel __instance)
        {
            if (!__instance.bestTimeTitleText.IsActive() && !__instance.bestTimeText.IsActive())
            {
                __instance.bestTimeTitleText.gameObject.SetActive(true);
                __instance.bestTimeText.gameObject.SetActive(true);
            }
        }
    }
}
