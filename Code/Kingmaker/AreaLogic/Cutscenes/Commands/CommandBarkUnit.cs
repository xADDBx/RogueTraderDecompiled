using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Signals;
using Kingmaker.UI.Common;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Obsolete]
[TypeId("de5b317dc48ef484193a356ca566cb7e")]
public class CommandBarkUnit : CommandBase
{
	private class Data
	{
		internal float Delay;

		internal bool Finished;

		internal IBarkHandle BarkHandle;

		internal SignalWrapper StopPlaySignal;
	}

	public string Text;

	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	[ValidateNotNull]
	public SharedStringAsset SharedText;

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[Tooltip("Bark duration depends on text length")]
	public bool BarkDurationByText;

	[Tooltip("Wait until bark disappears before starting next command")]
	public bool AwaitFinish;

	[Tooltip("Extra delay time before starting next command. Can be negative.")]
	[FormerlySerializedAs("DelayTime")]
	public float CommandDurationShift;

	[Tooltip("If true, speaker is considered controlled by the cutscene")]
	public bool ControlsUnit = true;

	[Tooltip("If true, override unit bark and broadcast it to subtitle")]
	public bool IsSubText;

	[Tooltip("Allow set exact playback time")]
	public bool OverrideBarkDuration;

	[Tooltip("Try playing the voiceover if available")]
	public bool TryPlayVoiceOver = true;

	[Tooltip("Exact playback time")]
	[ShowIf("OverrideBarkDuration")]
	public float BarkDuration;

	public bool IsUnitOptional;

	[SerializeField]
	[ShowIf("IsSubText")]
	[CanBeNull]
	private LocalizedString m_SpeakerName;

	protected override bool StopPlaySignalIsReady(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		return SignalService.Instance.CheckReadyOrSend(ref commandData.StopPlaySignal);
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		Data commandData = player.GetCommandData<Data>(this);
		commandData.BarkHandle?.InterruptBark();
		commandData.Delay = 0f;
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
		AbstractUnitEntity value = null;
		if (IsUnitOptional)
		{
			Unit?.TryGetValue(out value);
		}
		else
		{
			value = Unit?.GetValue();
		}
		if (IsSubText)
		{
			commandData.BarkHandle = (SharedText ? BarkPlayer.BarkSubtitle(value, SharedText.String, duration, BarkDurationByText, m_SpeakerName) : BarkPlayer.BarkSubtitle(value, Text, duration, m_SpeakerName));
		}
		else if (value != null && value.LifeState.IsConscious)
		{
			commandData.BarkHandle = (SharedText ? BarkPlayer.Bark(value, SharedText.String, duration, TryPlayVoiceOver) : BarkPlayer.Bark(value, Text, duration));
		}
		commandData.Delay = CommandDurationShift;
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
		else if (time >= (double)commandData.Delay && (!AwaitFinish || !commandData.BarkHandle.IsPlayingBark()))
		{
			commandData.Finished = true;
		}
	}

	public override string GetCaption()
	{
		if ((bool)Unit)
		{
			return Unit.GetCaptionShort() + "<b> bark</b> " + (SharedText ? ((string)SharedText.String) : Text);
		}
		return "No unit<b> bark</b> " + (SharedText ? ((string)SharedText.String) : Text);
	}

	public override string GetWarning()
	{
		if (Unit == null)
		{
			if (!IsSubText)
			{
				return "No unit";
			}
			return null;
		}
		if (!Unit.CanEvaluate())
		{
			return "No unit";
		}
		return null;
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (!ControlsUnit || !Unit || !Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}

	public override IAbstractUnitEntity GetAnchorUnit()
	{
		if (!Unit || !Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}
}
