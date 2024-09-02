using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Signals;
using Kingmaker.UI.Common;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("9b492d9943834d39ac4333d13cdb42a2")]
public class CommandBarkEntity : CommandBase
{
	private class Data
	{
		internal bool Finished;

		internal IBarkHandle BarkHandle;

		internal SignalWrapper StopPlaySignal;
	}

	public string Text;

	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark, GetNameFromAsset = true)]
	[ValidateNotNull]
	public SharedStringAsset SharedText;

	[SerializeReference]
	public EntityEvaluator Entity;

	[Tooltip("Bark duration depends on text length")]
	public bool BarkDurationByText;

	[Tooltip("Wait until bark disappears before starting next command")]
	public bool AwaitFinish;

	[Tooltip("Allow set exact playback time")]
	public bool OverrideBarkDuration;

	[Tooltip("If true, override bark and broadcast it to subtitle")]
	public bool IsSubText;

	[SerializeField]
	[ShowIf("IsSubText")]
	[CanBeNull]
	private LocalizedString m_SpeakerName;

	[Tooltip("Exact playback time")]
	[ShowIf("OverrideBarkDuration")]
	public float BarkDuration;

	[Tooltip("Allow override speaker name in combat log")]
	public bool OverrideNameInLog;

	[SerializeField]
	[ShowIf("OverrideNameInLog")]
	private LocalizedString m_NameInLog;

	[SerializeField]
	[ShowIf("OverrideNameInLog")]
	private Color m_NameColorInLog = new Color(0.15f, 0.15f, 0.15f, 1f);

	protected override bool StopPlaySignalIsReady(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		return SignalService.Instance.CheckReadyOrSend(ref commandData.StopPlaySignal);
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		player.GetCommandData<Data>(this).BarkHandle?.InterruptBark();
	}

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		float duration = UIUtility.DefaultBarkTime;
		if (BarkDurationByText)
		{
			duration = UIUtility.GetBarkDuration(SharedText ? ((string)SharedText.String) : Text);
		}
		if (OverrideBarkDuration)
		{
			duration = BarkDuration;
		}
		if (IsSubText)
		{
			commandData.BarkHandle = (SharedText ? BarkPlayer.BarkSubtitle(SharedText.String, duration, m_SpeakerName) : BarkPlayer.BarkSubtitle(Text, duration, m_SpeakerName));
		}
		else
		{
			Entity entity = Entity?.GetValue();
			if ((bool)SharedText)
			{
				commandData.BarkHandle = (OverrideNameInLog ? BarkPlayer.Bark(entity, SharedText.String, duration, BarkDurationByText, null, synced: true, m_NameInLog, m_NameColorInLog) : BarkPlayer.Bark(entity, SharedText.String, duration, BarkDurationByText));
			}
			else
			{
				commandData.BarkHandle = (OverrideNameInLog ? BarkPlayer.Bark(entity, Text, duration, null, null, synced: true, m_NameInLog, m_NameColorInLog) : BarkPlayer.Bark(entity, Text, duration));
			}
		}
		commandData.StopPlaySignal = SignalService.Instance.RegisterNext();
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return player.GetCommandData<Data>(this).Finished;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData.BarkHandle == null)
		{
			commandData.Finished = true;
		}
		else if (!AwaitFinish || !commandData.BarkHandle.IsPlayingBark())
		{
			commandData.Finished = true;
		}
	}

	public override string GetCaption()
	{
		return Entity?.ToString() + "<b> bark</b> " + (SharedText ? ((string)SharedText.String) : Text);
	}

	public override string GetWarning()
	{
		if (Entity == null)
		{
			return "No entity";
		}
		if (!Entity.CanEvaluate())
		{
			return "No entity";
		}
		return null;
	}
}
