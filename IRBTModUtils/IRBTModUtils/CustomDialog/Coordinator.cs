using BattleTech.Framework;
using BattleTech.UI;
using HBS.Data;
using IRBTModUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace us.frostraptor.modUtils.CustomDialog {
    // This classes liberally borrows CWolf's amazing MissionControl mod, in particular 
    //  https://github.com/CWolfs/MissionControl/blob/master/src/Core/DataManager.cs

    // A command control class that coordinates between the messages and the generated sequences
    public static class Coordinator {

        private static CombatGameState Combat;
        private static MessageCenter MessageCenter;
        private static CombatHUDDialogSideStack SideStack;
        public static List<string> CallSigns;

        public static bool CombatIsActive {
            get { return Coordinator.Combat != null && Coordinator.SideStack != null; }
        }

        public static void OnCustomDialogMessage(MessageCenterMessage message) {
            CustomDialogMessage msg = (CustomDialogMessage)message;
            if (msg == null) { return; }

            ModState.DialogueQueue.Enqueue(msg);
            if (!ModState.IsDialogStackActive) {
                Mod.Log.Debug?.Write("No existing dialog sequence, publishing a new one.");
                ModState.IsDialogStackActive = true;
                MessageCenter.PublishMessage(
                    new AddParallelSequenceToStackMessage(new CustomDialogSequence(Combat, SideStack, false))
                    );
            } else {
                Mod.Log.Debug?.Write("Existing dialog sequence exists, skipping creation.");
            }
        }

        public static void OnCombatHUDInit(CombatGameState combat, CombatHUD combatHUD) {
            Mod.Log.Trace?.Write("Coordinator::OCHUDI - entered.");

            Coordinator.Combat = combat;
            Coordinator.MessageCenter = combat.MessageCenter;
            Coordinator.SideStack = combatHUD.DialogSideStack;

            if (Coordinator.CallSigns == null) {
                string filePath = Path.Combine(Mod.ModDir, Mod.Config.Dialogue.CallsignsPath);
                Mod.Log.Debug?.Write($"Reading files from {filePath}");
                try {
                    Coordinator.CallSigns = File.ReadAllLines(filePath).ToList();
                } catch (Exception e) {
                    Mod.Log.Error?.Write("Failed to read callsigns from BT directory!");
                    Mod.Log.Error?.Write(e);
                    Coordinator.CallSigns = new List<string> { "Alpha", "Beta", "Gamma" };
                }
                Mod.Log.Debug?.Write($"Callsign count is: {Coordinator.CallSigns.Count}");
            }

            Mod.Log.Trace?.Write("Coordinator::OCHUDI - exiting.");
        }

        public static void OnCombatGameDestroyed() {
            Mod.Log.Trace?.Write("Coordinator::OCGD - entered.");

            Combat = null;
            MessageCenter = null;
            SideStack = null;
        }

        public static CastDef CreateCast(AbstractActor actor) {
            string castDefId = $"castDef_{actor.GUID}";
            if (actor.Combat.DataManager.CastDefs.Exists(castDefId)) {
                return actor.Combat.DataManager.CastDefs.Get(castDefId);
            }

            FactionValue actorFaction = actor?.team?.FactionValue;
            bool factionExists = !"INVALID_UNSET".Equals(actorFaction.Name, StringComparison.InvariantCultureIgnoreCase) &&
                !"NoFaction".Equals(actorFaction.Name, StringComparison.InvariantCultureIgnoreCase) &&
                !String.IsNullOrEmpty(actorFaction.FactionDefID);

            string employerFactionName = "Military Support";
            if (factionExists) {
                Mod.Log.Debug?.Write($"Found factionDef for id:{actorFaction}");
                string factionId = actorFaction?.FactionDefID;
                FactionDef employerFactionDef = UnityGameInstance.Instance.Game.DataManager.Factions.Get(factionId);
                if (employerFactionDef == null) { Mod.Log.Error?.Write($"Error finding FactionDef for faction with id '{factionId}'"); }
                else { employerFactionName = employerFactionDef.Name.ToUpper(); }
            } else {
                Mod.Log.Debug?.Write($"FactionDefID does not exist for faction: {actorFaction}");
            }

            CastDef newCastDef = new CastDef {
                // Temp test data
                FactionValue = actorFaction,
                firstName = $"{employerFactionName} -",
                showRank = false,
                showCallsign = true,
                showFirstName = true,
                showLastName = false
            };
            // DisplayName order is first, callsign, lastname

            newCastDef.id = castDefId;
            string portraitPath = GetRandomPortraitPath();
            newCastDef.defaultEmotePortrait.portraitAssetPath = portraitPath;
            if (actor.GetPilot() != null) {
                Mod.Log.Debug?.Write("Actor has a pilot, using pilot values.");
                Pilot pilot = actor.GetPilot();
                newCastDef.callsign = pilot.Callsign;

                // Hide the faction name if it's the player's mech
                if (actor.team.IsLocalPlayer) { newCastDef.showFirstName = false; }
            } else {
                Mod.Log.Debug?.Write("Actor is not piloted, generating castDef.");
                newCastDef.callsign = GetRandomCallsign();
            }
            Mod.Log.Debug?.Write($" Generated cast with callsign: {newCastDef.callsign} and DisplayName: {newCastDef.DisplayName()} using portrait: '{portraitPath}'");

            ((DictionaryStore<CastDef>)UnityGameInstance.BattleTechGame.DataManager.CastDefs).Add(newCastDef.id, newCastDef);

            return newCastDef;
        }

        public static CastDef CreateCast(CombatGameState combat, string sourceGUID, Team team, string employerFactionName = "Support")
        {
            string castDefId = $"castDef_{sourceGUID}";
            if (combat.DataManager.CastDefs.Exists(castDefId))
            {
                return combat.DataManager.CastDefs.Get(castDefId);
            }

            FactionValue actorFaction = team?.FactionValue;
            bool factionExists = actorFaction.Name != "INVALID_UNSET" && actorFaction.Name != "NoFaction" &&
                actorFaction.FactionDefID != null && actorFaction.FactionDefID.Length != 0 ? true : false;

            if (factionExists)
            {
                Mod.Log.Debug?.Write($"Found factionDef for id:{actorFaction}");
                string factionId = actorFaction?.FactionDefID;
                FactionDef employerFactionDef = UnityGameInstance.Instance.Game.DataManager.Factions.Get(factionId);
                if (employerFactionDef == null) { Mod.Log.Error?.Write($"Error finding FactionDef for faction with id '{factionId}'"); }
                else { employerFactionName = employerFactionDef.Name.ToUpper(); }
            }
            else
            {
                Mod.Log.Debug?.Write($"FactionDefID does not exist for faction: {actorFaction}");
            }

            CastDef newCastDef = new CastDef
            {
                // Temp test data
                FactionValue = actorFaction,
                firstName = $"{employerFactionName} -",
                showRank = false,
                showCallsign = true,
                showFirstName = true,
                showLastName = false
            };
            // DisplayName order is first, callsign, lastname

            newCastDef.id = castDefId;
            string portraitPath = GetRandomPortraitPath();
            newCastDef.callsign = GetRandomCallsign();
            Mod.Log.Debug?.Write($" Generated cast with callsign: {newCastDef.callsign} and DisplayName: {newCastDef.DisplayName()} using portrait: '{portraitPath}'");

            // Load the associated portrait
            newCastDef.defaultEmotePortrait.portraitAssetPath = portraitPath;
            Mod.Log.Debug?.Write($"Generated random portrait: {portraitPath}.");

            ((DictionaryStore<CastDef>)UnityGameInstance.BattleTechGame.DataManager.CastDefs).Add(newCastDef.id, newCastDef);

            return newCastDef;
        }

        public static DialogueContent BuildDialogueContent(CastDef castDef, string dialogue, Color dialogueColor)
        {

            if (castDef == null || String.IsNullOrEmpty(castDef.id) || castDef.defaultEmotePortrait == null || String.IsNullOrEmpty(castDef.defaultEmotePortrait.portraitAssetPath))
            {
                Mod.Log.Warn?.Write("Was passed a castDef with an empty ID - we can't handle this!");
                return null;
            }

            Mod.Log.Info?.Write($"Creating dialogueContent for castDef: {castDef.id}");

            DialogueContent content = new DialogueContent(dialogue, dialogueColor, castDef.id, null, null, 
                DialogCameraDistance.Medium, DialogCameraHeight.Default, 0);
            
            // ContractInitialize normally sets the castDef on the content... no need, since we have the actual ref
            Traverse castDefT = Traverse.Create(content).Field("castDef");
            castDefT.SetValue(castDef);

            // Initialize the active contract's team settings
            ApplyCastDef(content);
            
            // Load the default emote portrait
            Traverse dialogueSpriteCacheT = Traverse.Create(content).Field("dialogueSpriteCache");
            Dictionary<string, Sprite> dialogueSpriteCache = dialogueSpriteCacheT.GetValue<Dictionary<string, Sprite>>();
            Mod.Log.Debug?.Write($"Populating dialogueContent with sprite from path: {castDef.defaultEmotePortrait.portraitAssetPath}");
            dialogueSpriteCache[castDef.defaultEmotePortrait.portraitAssetPath] = ModState.Portraits[castDef.defaultEmotePortrait.portraitAssetPath];

            return content;
        }

        // Clone of DialogueContent::ApplyCastDef
        private static void ApplyCastDef(DialogueContent dialogueContent)
        {
            Contract contract = SharedState.Combat.ActiveContract;

            if (dialogueContent.selectedCastDefId == CastDef.castDef_TeamLeader_Employer)
            {
                TeamOverride teamOverride = contract.GameContext.GetObject(GameContextObjectTagEnum.TeamEmployer) as TeamOverride;
                dialogueContent.selectedCastDefId = teamOverride.teamLeaderCastDefId;
            }
            else if (dialogueContent.selectedCastDefId == CastDef.castDef_TeamLeader_EmployersAlly)
            {
                TeamOverride teamOverride2 = contract.GameContext.GetObject(GameContextObjectTagEnum.TeamEmployersAlly) as TeamOverride;
                dialogueContent.selectedCastDefId = teamOverride2.teamLeaderCastDefId;
            }
            else if (dialogueContent.selectedCastDefId == CastDef.castDef_TeamLeader_Target)
            {
                TeamOverride teamOverride3 = contract.GameContext.GetObject(GameContextObjectTagEnum.TeamTarget) as TeamOverride;
                dialogueContent.selectedCastDefId = teamOverride3.teamLeaderCastDefId;
            }
            else if (dialogueContent.selectedCastDefId == CastDef.castDef_TeamLeader_TargetsAlly)
            {
                TeamOverride teamOverride4 = contract.GameContext.GetObject(GameContextObjectTagEnum.TeamTargetsAlly) as TeamOverride;
                dialogueContent.selectedCastDefId = teamOverride4.teamLeaderCastDefId;
            }
            else if (dialogueContent.selectedCastDefId == CastDef.castDef_TeamLeader_Neutral)
            {
                TeamOverride teamOverride5 = contract.GameContext.GetObject(GameContextObjectTagEnum.TeamNeutralToAll) as TeamOverride;
                dialogueContent.selectedCastDefId = teamOverride5.teamLeaderCastDefId;
            }
            else if (dialogueContent.selectedCastDefId == CastDef.castDef_TeamLeader_Hostile)
            {
                TeamOverride teamOverride6 = contract.GameContext.GetObject(GameContextObjectTagEnum.TeamHostileToAll) as TeamOverride;
                dialogueContent.selectedCastDefId = teamOverride6.teamLeaderCastDefId;
            }
        }

        private static string GetRandomCallsign() {
            return Coordinator.CallSigns[UnityEngine.Random.Range(0, Coordinator.CallSigns.Count)];
        }
        private static string GetRandomPortraitPath() {
            return Mod.Config.Dialogue.Portraits[UnityEngine.Random.Range(0, Mod.Config.Dialogue.Portraits.Length)];
        }

    }
}
