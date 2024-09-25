using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Serializable]
[TypeId("dd8667f2c38e5804fba853885780b39a")]
public class CommandStartDialog : CommandBase, IDialogReference
{
	private class Finisher : IDialogFinishHandler, ISubscriber
	{
		private BlueprintDialog m_Dialog;

		private Cutscene m_Cutscene;

		public bool Finished { get; set; }

		public void Subscribe(BlueprintDialog dialog, Cutscene cutscene)
		{
			m_Dialog = dialog;
			m_Cutscene = cutscene;
			Finished = false;
			EventBus.Subscribe(this);
		}

		void IDialogFinishHandler.HandleDialogFinished(BlueprintDialog dialog, bool success)
		{
			Finished = true;
			if (!success)
			{
				PFLog.Default.Error($"Dialog {m_Dialog} failed to start in cutscene {m_Cutscene}");
				QAModeExceptionReporter.MaybeShowError($"Dialog {m_Dialog} failed to start in cutscene {m_Cutscene}");
			}
			EventBus.Unsubscribe(this);
		}
	}

	[SerializeReference]
	public AbstractUnitEvaluator Speaker;

	[SerializeField]
	[FormerlySerializedAs("Dialog")]
	private BlueprintDialogReference m_Dialog;

	[SerializeReference]
	public BlueprintEvaluator DialogEvaluator;

	public LocalizedString SpeakerName;

	public BlueprintDialog Dialog => m_Dialog?.Get();

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		BlueprintDialog blueprintDialog = (Dialog ? Dialog : (DialogEvaluator ? (DialogEvaluator.GetValue() as BlueprintDialog) : null));
		if (blueprintDialog == null)
		{
			OnStop(player);
			player.GetCommandData<Finisher>(this).Finished = true;
			PFLog.Default.Error(this, $"Cutscene command {this} in {player.Cutscene} unable to start dialog: no dialog found");
			QAModeExceptionReporter.MaybeShowError($"Cutscene command {this} in {player.Cutscene} unable to start dialog: no dialog found");
			return;
		}
		if (Speaker != null)
		{
			if (!(Speaker.GetValue() is BaseUnitEntity unit))
			{
				string message = $"[IS NOT BASE UNIT ENTITY] Cutscene command {this}, {Speaker} is not BaseUnitEntity";
				if (!QAModeExceptionReporter.MaybeShowError(message))
				{
					UberDebug.LogError(message);
				}
				return;
			}
			DialogData data = DialogController.SetupDialogWithUnit(blueprintDialog, unit);
			Game.Instance.DialogController.StartDialog(data);
		}
		else
		{
			DialogData data2 = DialogController.SetupDialogWithoutTarget(blueprintDialog, SpeakerName);
			Game.Instance.DialogController.StartDialog(data2);
		}
		player.GetCommandData<Finisher>(this).Subscribe(blueprintDialog, player.Cutscene);
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return player.GetCommandData<Finisher>(this).Finished;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	public override string GetCaption()
	{
		return "<b>Dialog</b> " + (Dialog ? Dialog.name : "???");
	}

	public DialogReferenceType GetUsagesFor(BlueprintDialog dialog)
	{
		if (dialog != Dialog)
		{
			return DialogReferenceType.None;
		}
		return DialogReferenceType.Start;
	}
}
