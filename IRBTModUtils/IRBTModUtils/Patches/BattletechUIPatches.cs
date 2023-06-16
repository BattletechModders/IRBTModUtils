using BattleTech.Framework;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using SVGImporter;
using UnityEngine;
using System;

namespace IRBTModUtils.Patches
{
    // Allow additional missionObjectiveResult statuses
    [HarmonyPatch(typeof(BattleTech.UI.AAR_ObjectiveListItem), "Init")]
    [HarmonyPatch(new Type[] { typeof(MissionObjectiveResult), typeof(SimGameState), typeof(Contract) })]
    public static class AAR_ObjectiveListItem__Init
    {
        private static Color GOLD = new Color(0.969f, 0.608f, 0.145f, 1.0f);

        [HarmonyPostfix]
        public static void Postfix(AAR_ObjectiveListItem __instance, MissionObjectiveResult missionObjectiveResult)
        {
            if (missionObjectiveResult.status == ObjectiveStatus.Ignored)
            {
                Mod.Log.Info?.Write($"Objective found with status {missionObjectiveResult.status} ({missionObjectiveResult.title})");
                LocalizableText resultText = __instance.ResultText;
                UIColorRefTracker resultTextColor = __instance.ResultTextColor;
                if (resultTextColor != null)
                {
                    Mod.Log.Debug?.Write($"Setting Gold");
                    resultTextColor.SetUIColor(UIColor.Gold);
                }
                SVGImage objectiveFrame = __instance.ObjectiveFrame;
                SVGImage objectiveIcon = __instance.ObjectiveIcon;

                resultText.SetText("RESULT");
                objectiveFrame.color = GOLD;
                objectiveIcon.color = GOLD;
            }
        }
    }

}
