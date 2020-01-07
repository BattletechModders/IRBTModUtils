using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using IRBTModUtils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace us.frostraptor.modUtils.CustomDialog {
    public class CustomDialogSequence : MultiSequence {

        public CustomDialogSequence(CombatGameState Combat, CombatHUDDialogSideStack sideStack, 
            DialogueContent content, AbstractActor dialogueSource, float showDuration, bool isCancelable = true) : base(Combat) {
            this.isCancelable = isCancelable;
            this.sideStack = sideStack;
            this.state = DialogState.None;
            this.dialogueSource = dialogueSource;
            this.content = content;
            this.showDuration = showDuration;
        }

        public void SetState(DialogState newState) {
            Mod.Log.Debug($"CDS::SetState - {CombatantUtils.Label(this.dialogueSource)} state changing to: {newState}");
            if (this.state == newState) {
                return;
            }

            this.state = newState;
            DialogState dialogState = this.state;
            if (dialogState != DialogState.Talking) {
                return;
            }
            this.PublishDialogMessages();
        }

        public override void OnUpdate() {
            base.OnUpdate();
            if (this.state == DialogState.None && !ModState.IsDialogueSequencePlaying) {
                this.SetState(DialogState.Talking);
                ModState.IsDialogueSequencePlaying = true;
            }
        }

        private void PublishDialogMessages() {
            AudioEventManager.DialogSequencePlaying = true;
            if (this.dialogueSource == null || this.content.words.Length < 1) {
                this.SetState(DialogState.Finished);
                return;
            }
            this.PlayMessage();
        }

        public void PlayMessage() {
            AudioEventManager.InterruptPilotVOForTeam(base.Combat.LocalPlayerTeam, null);
            WwiseManager.PostEvent<AudioEventList_vo>(AudioEventList_vo.vo_stop_missions, WwiseManager.GlobalAudioObject, null, null);
            this.Play();
            this.SetState(DialogState.Finished);
        }

        private void Play() {
            Mod.Log.Debug($"CDS::Play - invoked {CombatantUtils.Label(this.dialogueSource)}");
           
            this.sideStack.PanelFrame.gameObject.SetActive(true);
            if (this.dialogueSource.team.IsLocalPlayer) {
                Mod.Log.Debug($"  Displaying pilot portrait");
                this.sideStack.ShowPortrait(this.dialogueSource.GetPilot().GetPortraitSpriteThumb());
            } else {
                Mod.Log.Debug($"  Displaying castDef portrait");
                this.sideStack.ShowPortrait(this.content.CastDef.defaultEmotePortrait.LoadPortrait(false));
            }

            try {
                Transform speakerNameFieldT = this.sideStack.gameObject.transform.Find("Representation/dialog-layout/Portrait/speakerNameField");
                if (speakerNameFieldT == null) { Mod.Log.Warn("COULD NOT FIND speakerNameFieldT!"); }
                speakerNameFieldT.gameObject.SetActive(true);

                Mod.Log.Debug($" Setting SpeakerName to: '{content.SpeakerName}' with callsign: '{content.CastDef.callsign}'");
                LocalizableText speakerNameLT = speakerNameFieldT.GetComponentInChildren<LocalizableText>();
                speakerNameLT.SetText(content.SpeakerName);
                speakerNameLT.gameObject.SetActive(true);
                speakerNameLT.alignment = TMPro.TextAlignmentOptions.Bottom;

            } catch (Exception e) {
                Mod.Log.Error("Failed to set display name due to error!");
                Mod.Log.Error(e);
            }

            this.activeDialog = this.sideStack.GetNextItem();
            this.activeDialog.Init(this.showDuration, true, new Action(this.AfterDialogShow), new Action(this.AfterDialogHide));

            Mod.Log.Debug($"CDS - Showing dialog: words:{this.content.words} color:{this.content.wordsColor} speakerName:{this.content.SpeakerName} timeout: {this.showDuration}");
            this.activeDialog.Show(this.content.words, this.content.wordsColor, this.content.SpeakerName);
            Mod.Log.Trace("CDS::Play - DONE");
        }

        public void AfterDialogShow() {
            Mod.Log.Trace("CDS:ADS - entered.");
            this.sideStack.AfterDialogShow();
        }

        public void AfterDialogHide() {
            Mod.Log.Trace("CDS:ADH - entered.");
            this.sideStack.AfterDialogHide();

            Transform speakerNameFieldT = this.sideStack.gameObject.transform.Find("Representation/dialog-layout/Portrait/speakerNameField");
            if (speakerNameFieldT == null) { Mod.Log.Warn("COULD NOT FIND speakerNameFieldT!"); }
            speakerNameFieldT.gameObject.SetActive(false);

            ModState.IsDialogueSequencePlaying = false;
        }

        public void UserRequestHide() {
            this.sideStack.HideAll();
        }

        public override void OnAdded() {
            base.OnAdded();
            if (!ModState.IsDialogueSequencePlaying) {
                this.SetState(DialogState.Talking);
                ModState.IsDialogueSequencePlaying = true;
            }
        }

        public override void OnComplete() {
            base.OnComplete();
            AudioEventManager.DialogSequencePlaying = false;
            this.SendCompleteMessage();
        }

        public void SendCompleteMessage() {
            base.Combat.MessageCenter.PublishMessage(new DialogComplete(this.dialogueSource.GUID));
        }

        public void SetIsCancelable(bool isCancelable) {
            this.isCancelable = isCancelable;
        }

        public override bool IsParallelInterruptable {
            get {
                return true;
            }
        }

        public override bool IsCancelable {
            get {
                return this.isCancelable;
            }
        }

        public override bool IsComplete {
            get {
                return this.state == DialogState.Finished && this.IsCameraFinished;
            }
        }

        public bool IsCameraFinished {
            get {
                return base.cameraSequence == null || base.cameraSequence.IsFinished;
            }
        }

        public override void OnSuspend() {
            base.OnSuspend();
            this.UserRequestHide();
        }

        public override void OnResume() {
            base.OnResume();
            if (this.activeDialog != null) {
                if (this.content != null) {
                    this.Play();
                    return;
                }
            } else {
                this.PlayMessage();
            }
        }

        public override void OnCanceled() {
            base.OnCanceled();
            this.UserRequestHide();
            this.pendingMessages.Clear();
            this.content = null;
            this.SetState(DialogState.Finished);
            this.SendCompleteMessage();
        }

        private bool isCancelable;

        private DialogState state;

        private readonly AbstractActor dialogueSource;

        private readonly CombatHUDDialogSideStack sideStack;

        public List<DialogueContent> pendingMessages = new List<DialogueContent>();

        private DialogueContent content;

        private CombatHUDDialogItem activeDialog;

        private readonly float showDuration;
    }
}
