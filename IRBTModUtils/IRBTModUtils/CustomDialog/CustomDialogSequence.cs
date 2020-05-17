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
            bool isCancelable = true) : base(Combat) {
            this.isCancelable = isCancelable;
            this.sideStack = sideStack;
            this.state = DialogState.None;
        }

        public void SetState(DialogState newState) {
            Mod.Log.Trace($"CDS::SetState - state changing to: {newState}");
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
        }

        private void PublishDialogMessages() {
            AudioEventManager.DialogSequencePlaying = true;
            this.PlayMessage();
        }

        public void PlayMessage() {
            AudioEventManager.InterruptPilotVOForTeam(base.Combat.LocalPlayerTeam, null);
            WwiseManager.PostEvent<AudioEventList_vo>(AudioEventList_vo.vo_stop_missions, WwiseManager.GlobalAudioObject, null, null);
            this.Play();
            this.SetState(DialogState.Finished);
        }

        private void Play() {
            Mod.Log.Debug($"CDS::Play - invoked");
           
            this.sideStack.PanelFrame.gameObject.SetActive(true);
            if (this.currentMessage.DialogueSource != null && this.currentMessage.DialogueSource.team.IsLocalPlayer) {
                Mod.Log.Debug($"  Displaying pilot portrait");
                this.sideStack.ShowPortrait(this.currentMessage.DialogueSource.GetPilot().GetPortraitSpriteThumb());
            } else {
                Mod.Log.Debug($"  Displaying castDef portrait");
                this.sideStack.ShowPortrait(this.currentMessage.DialogueContent.CastDef.defaultEmotePortrait.LoadPortrait(false));
            }

            try {
                Transform speakerNameFieldT = this.sideStack.gameObject.transform.Find("Representation/dialog-layout/Portrait/speakerNameField");
                if (speakerNameFieldT == null) { Mod.Log.Warn("COULD NOT FIND speakerNameFieldT!"); }
                speakerNameFieldT.gameObject.SetActive(true);

                Mod.Log.Debug($" Setting SpeakerName to: '{this.currentMessage.DialogueContent.SpeakerName}' with callsign: '{this.currentMessage.DialogueContent.CastDef.callsign}'");
                LocalizableText speakerNameLT = speakerNameFieldT.GetComponentInChildren<LocalizableText>();
                speakerNameLT.SetText(this.currentMessage.DialogueContent.SpeakerName);
                speakerNameLT.gameObject.SetActive(true);
                speakerNameLT.alignment = TMPro.TextAlignmentOptions.Bottom;

            } catch (Exception e) {
                Mod.Log.Error("Failed to set display name due to error!");
                Mod.Log.Error(e);
            }

            this.activeDialog = this.sideStack.GetNextItem();
            this.activeDialog.Init(this.currentMessage.ShowDuration, true, new Action(this.AfterDialogShow), new Action(this.AfterDialogHide));

            Mod.Log.Debug($"CDS - Showing dialog: words: '{this.currentMessage.DialogueContent.words}' color: '{this.currentMessage.DialogueContent.wordsColor}' speakerName: '{this.currentMessage.DialogueContent.SpeakerName}' timeout: {this.currentMessage.ShowDuration}");
            this.activeDialog.Show(this.currentMessage.DialogueContent.words, this.currentMessage.DialogueContent.wordsColor, this.currentMessage.DialogueContent.SpeakerName);
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

            if (ModState.DialogueQueue.Count > 0) {
                Mod.Log.Trace("  DialogQueue has additional messages, reading another instead of exiting.");
                this.currentMessage = (CustomDialogMessage)ModState.DialogueQueue.Dequeue();
                this.SetState(DialogState.Talking);
                return;
            }
        }

        public void UserRequestHide() {
            this.sideStack.HideAll();
        }

        public override void OnAdded() {
            base.OnAdded();
            this.currentMessage = (CustomDialogMessage)ModState.DialogueQueue.Dequeue();
            this.SetState(DialogState.Talking);
        }

        public override void OnComplete() {
            Mod.Log.Trace("CDS:OC - entered.");

            base.OnComplete();
            AudioEventManager.DialogSequencePlaying = false;
            //this.SendCompleteMessage();
            ModState.IsDialogStackActive = false;
            Mod.Log.Trace("CDS:OC - Setting IsDialogStackActive = false");
        }

        public void SendCompleteMessage() {
            base.Combat.MessageCenter.PublishMessage(new DialogComplete(this.currentMessage.DialogueSourceGUID));
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
                if (this.currentMessage.DialogueContent != null) {
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
            this.currentMessage = null;
            this.SetState(DialogState.Finished);
            this.SendCompleteMessage();
        }

        private bool isCancelable;

        private DialogState state;

        private readonly CombatHUDDialogSideStack sideStack;

        public List<DialogueContent> pendingMessages = new List<DialogueContent>();

        private CombatHUDDialogItem activeDialog;

        private CustomDialogMessage currentMessage;

    }
}
