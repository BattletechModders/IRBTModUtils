using BattleTech;
using BattleTech.UI;
using HBS.Data;
using IRBTModUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                Mod.Log.Debug("No existing dialog sequence, publishing a new one.");
                ModState.IsDialogStackActive = true;
                MessageCenter.PublishMessage(
                    new AddParallelSequenceToStackMessage(new CustomDialogSequence(Combat, SideStack, false))
                    );
            } else {
                Mod.Log.Debug("Existing dialog sequence exists, skipping creation.");
            }
        }

        public static void OnCombatHUDInit(CombatGameState combat, CombatHUD combatHUD) {
            Mod.Log.Trace("Coordinator::OCHUDI - entered.");

            Coordinator.Combat = combat;
            Coordinator.MessageCenter = combat.MessageCenter;
            Coordinator.SideStack = combatHUD.DialogSideStack;

            if (Coordinator.CallSigns == null) {
                string filePath = Path.Combine(Mod.ModDir, Mod.Config.Dialogue.CallsignsPath);
                Mod.Log.Debug($"Reading files from {filePath}");
                try {
                    Coordinator.CallSigns = File.ReadAllLines(filePath).ToList();
                } catch (Exception e) {
                    Mod.Log.Error("Failed to read callsigns from BT directory!");
                    Mod.Log.Error(e);
                    Coordinator.CallSigns = new List<string> { "Alpha", "Beta", "Gamma" };
                }
                Mod.Log.Debug($"Callsign count is: {Coordinator.CallSigns.Count}");
            }

            Mod.Log.Trace("Coordinator::OCHUDI - exiting.");
        }

        public static void OnCombatGameDestroyed() {
            Mod.Log.Trace("Coordinator::OCGD - entered.");

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
            bool factionExists = actorFaction.Name != "INVALID_UNSET" && actorFaction.Name != "NoFaction" && 
                actorFaction.FactionDefID != null && actorFaction.FactionDefID.Length != 0 ? true : false;

            string employerFactionName = "Military Support";
            if (factionExists) {
                Mod.Log.Debug($"Found factionDef for id:{actorFaction}");
                string factionId = actorFaction?.FactionDefID;
                FactionDef employerFactionDef = UnityGameInstance.Instance.Game.DataManager.Factions.Get(factionId);
                if (employerFactionDef == null) { Mod.Log.Error($"Error finding FactionDef for faction with id '{factionId}'"); }
                else { employerFactionName = employerFactionDef.Name.ToUpper(); }
            } else {
                Mod.Log.Debug($"FactionDefID does not exist for faction: {actorFaction}");
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
                Mod.Log.Debug("Actor has a pilot, using pilot values.");
                Pilot pilot = actor.GetPilot();
                newCastDef.callsign = pilot.Callsign;

                // Hide the faction name if it's the player's mech
                if (actor.team.IsLocalPlayer) { newCastDef.showFirstName = false; }
            } else {
                Mod.Log.Debug("Actor is not piloted, generating castDef.");
                newCastDef.callsign = GetRandomCallsign();
            }
            Mod.Log.Debug($" Generated cast with callsign: {newCastDef.callsign} and DisplayName: {newCastDef.DisplayName()} using portrait: '{portraitPath}'");

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
                Mod.Log.Debug($"Found factionDef for id:{actorFaction}");
                string factionId = actorFaction?.FactionDefID;
                FactionDef employerFactionDef = UnityGameInstance.Instance.Game.DataManager.Factions.Get(factionId);
                if (employerFactionDef == null) { Mod.Log.Error($"Error finding FactionDef for faction with id '{factionId}'"); }
                else { employerFactionName = employerFactionDef.Name.ToUpper(); }
            }
            else
            {
                Mod.Log.Debug($"FactionDefID does not exist for faction: {actorFaction}");
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
            newCastDef.defaultEmotePortrait.portraitAssetPath = portraitPath;
            Mod.Log.Debug("Actor is not piloted, generating castDef.");
            newCastDef.callsign = GetRandomCallsign();
            Mod.Log.Debug($" Generated cast with callsign: {newCastDef.callsign} and DisplayName: {newCastDef.DisplayName()} using portrait: '{portraitPath}'");

            ((DictionaryStore<CastDef>)UnityGameInstance.BattleTechGame.DataManager.CastDefs).Add(newCastDef.id, newCastDef);

            return newCastDef;
        }

        private static string GetRandomCallsign() {
            return Coordinator.CallSigns[UnityEngine.Random.Range(0, Coordinator.CallSigns.Count)];
        }
        private static string GetRandomPortraitPath() {
            return Mod.Config.Dialogue.Portraits[UnityEngine.Random.Range(0, Mod.Config.Dialogue.Portraits.Length)];
        }

    }
}
