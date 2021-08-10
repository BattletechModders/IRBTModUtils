using BattleTech;
using BattleTech.Framework;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using HBS;
using Harmony;
using SVGImporter;
using IRBTModUtils.Extension;
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
        public static void Postfix(AAR_ObjectiveListItem __instance, MissionObjectiveResult missionObjectiveResult)
        {
            if (missionObjectiveResult.status == ObjectiveStatus.Ignored)
            {
                Mod.Log.Info?.Write($"Objective found with status {missionObjectiveResult.status} ({missionObjectiveResult.title})");
                LocalizableText resultText = Traverse.Create(__instance).Field("ResultText").GetValue<LocalizableText>();

                UIColorRefTracker resultTextColor = Traverse.Create(__instance).Field("ResultTextColor").GetValue<UIColorRefTracker>();
                if (resultTextColor != null)
                {
                    Mod.Log.Info?.Write($"Setting Gold");
                    resultTextColor.SetUIColor(UIColor.Gold);
                }
                SVGImage objectiveFrame = Traverse.Create(__instance).Field("ObjectiveFrame").GetValue<SVGImage>();
                SVGImage objectiveIcon = Traverse.Create(__instance).Field("ObjectiveIcon").GetValue<SVGImage>();

                resultText.SetText("RESULT");
                objectiveFrame.color = GOLD;
                objectiveIcon.color = GOLD;
            }
        }
    }

}
