using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Serializable]
[TypeId("5d582208451380241b50fe71a9d94298")]
public sealed class CommandUnitPlayCutsceneAnimation : CommandBase
{
	public class Data
	{
		internal bool Started;

		internal bool Finished;

		internal float FiniteLoopDuration;

		internal bool SkippedByPlayer;

		internal UnitAnimationAction Action;

		internal AnimationActionHandle Handle;

		internal AnimationClipWrapper CutsceneClipWrapper;

		[CanBeNull]
		internal AbstractUnitEntity Unit;

		internal float RandomDelay;
	}

	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	[Obsolete]
	[HideInInspector]
	private AnimationClipWrapper m_CutsceneClipWrapper;

	[SerializeField]
	[ValidateNotNull]
	private AnimationClipWrapperLink m_CutsceneClipWrapperLink;

	[SerializeField]
	[InfoBox("If true, animation would not start until previous one finishes")]
	private bool m_WaitForCurrentAnimation;

	[SerializeField]
	[FormerlySerializedAs("m_DontReturnToIdle")]
	[HideInInspector]
	[Obsolete("Remove in WH2")]
	private bool m_ExitBeforeDone;

	[SerializeField]
	[Tooltip("Normally, animation command ends at the start of Out transition. Enable this to wait until animation is fully finished before going to the next command.")]
	private bool m_StopAfterTransition;

	[SerializeField]
	private float m_CrossfadeIn = 0.1f;

	[SerializeField]
	private float m_CrossfadeOut = 0.1f;

	[SerializeField]
	private bool RandomDelayBeforeAnimation;

	[SerializeField]
	[ShowIf("RandomDelayBeforeAnimation")]
	private float m_RandomDelayMin;

	[SerializeField]
	[ShowIf("RandomDelayBeforeAnimation")]
	private float m_RandomDelayMax;

	[SerializeField]
	private bool m_LockRotation;

	[SerializeField]
	private bool m_MarkUnit;

	[SerializeField]
	private bool m_OnlyIfIdle;

	[SerializeField]
	private bool m_FiniteLoop;

	[SerializeField]
	[ConditionalShow("m_FiniteLoop")]
	private float m_FiniteLoopDuration;

	[SerializeField]
	[ConditionalShow("m_FiniteLoop")]
	private bool m_FiniteLoopRandomDuration;

	[SerializeField]
	[ConditionalShow("m_FiniteLoopRandomDuration")]
	private float m_FiniteLoorDurationMax;

	[SerializeField]
	[ConditionalHide("IsContinuous")]
	[Tooltip("Timeout in case something breaks, forces this command to stop after this many seconds")]
	private float m_Timeout = 20f;

	[SerializeField]
	private bool m_UseAvatarMask;

	[SerializeField]
	[ConditionalShow("m_UseAvatarMask")]
	private AvatarMask m_AvatarMask;

	[SerializeField]
	private bool m_RerunBeforeReleaseAnimation;

	[SerializeField]
	[Tooltip("Normally, if this animation comes after another animation, it's CrossfadeOut value will be used as Transition In duration. Enable this to ignore previous animation's CrossfadeOut value.")]
	private bool m_DontMatchCrossfade;

	public bool MuteAttacker;

	private bool? m_CutsceneClipIsLooping;

	public override bool IsContinuous
	{
		get
		{
			if (ClipIsLooping)
			{
				return !m_FiniteLoop;
			}
			return false;
		}
	}

	private bool ClipIsLooping
	{
		get
		{
			bool valueOrDefault = m_CutsceneClipIsLooping.GetValueOrDefault();
			if (!m_CutsceneClipIsLooping.HasValue)
			{
				valueOrDefault = GetIsLooping();
				m_CutsceneClipIsLooping = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public AbstractUnitEvaluator UnitEvaluator => m_Unit;

	private bool GetIsLooping()
	{
		AnimationClipWrapper animationClipWrapper = m_CutsceneClipWrapperLink?.Load();
		if (animationClipWrapper != null)
		{
			return animationClipWrapper.IsLooping;
		}
		return false;
	}

	public void Preload()
	{
		m_CutsceneClipWrapperLink.Load();
	}

	public override bool TrySkip(CutscenePlayerData player)
	{
		if (IsContinuous && !IsFinished(player))
		{
			return false;
		}
		player.GetCommandData<Data>(this).SkippedByPlayer = true;
		return true;
	}

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Finished = false;
		commandData.Started = false;
		commandData.Unit = m_Unit.GetValue();
		commandData.FiniteLoopDuration = (m_FiniteLoopRandomDuration ? player.Random.Range(m_FiniteLoopDuration, m_FiniteLoorDurationMax) : m_FiniteLoopDuration);
		if (skipping)
		{
			commandData.Finished = true;
			commandData.Handle?.ActiveAnimation?.ChangeTransitionTime(0f);
			commandData.Handle?.Release();
		}
		else if (RandomDelayBeforeAnimation)
		{
			commandData.RandomDelay = player.Random.Range(m_RandomDelayMin, m_RandomDelayMax);
		}
		else
		{
			PlayAnimation(commandData, player);
		}
	}

	private UnitAnimationActionClip CreateAction(AnimationClipWrapper clipWrapper)
	{
		UnitAnimationActionClip unitAnimationActionClip = UnitAnimationActionClip.Create(clipWrapper, "CreateAction");
		unitAnimationActionClip.DurationType = ClipDurationType.Endless;
		if (m_UseAvatarMask)
		{
			unitAnimationActionClip.UseEmptyAvatarMask = false;
			unitAnimationActionClip.AvatarMasks = new List<AvatarMask> { m_AvatarMask };
		}
		unitAnimationActionClip.TransitionIn = m_CrossfadeIn;
		unitAnimationActionClip.TransitionOut = m_CrossfadeOut;
		unitAnimationActionClip.ExecutionMode = (m_WaitForCurrentAnimation ? ExecutionMode.Sequenced : ExecutionMode.Interrupted);
		return unitAnimationActionClip;
	}

	private void PlayAnimation(Data data, CutscenePlayerData player)
	{
		AbstractUnitEntity unit = data.Unit;
		if (unit == null || !unit.IsInGame)
		{
			data.Finished = true;
			data.Handle?.Release();
			return;
		}
		if (unit.LifeState.IsDead || unit.IsInCombat)
		{
			data.Finished = true;
			data.Handle?.Release();
			return;
		}
		if (data.CutsceneClipWrapper == null)
		{
			data.CutsceneClipWrapper = m_CutsceneClipWrapperLink.Load();
			if (!data.CutsceneClipWrapper)
			{
				PFLog.Default.Error(this, $"No clip found for {this} in {player.Cutscene}");
				data.Finished = true;
				return;
			}
		}
		UnitAnimationManager animationManager = unit.View.AnimationManager;
		if (!animationManager)
		{
			return;
		}
		bool flag = false;
		if (!data.Started && !data.Finished && (!m_OnlyIfIdle || animationManager.CanRunIdleAction()))
		{
			data.Action = CreateAction(data.CutsceneClipWrapper);
			AnimationActionHandle animationActionHandle = (data.Handle = animationManager.CreateHandle(data.Action));
			animationActionHandle.PreventsRotation = m_LockRotation;
			animationActionHandle.HasCrossfadePriority = true;
			animationActionHandle.SkipFirstTick = false;
			animationActionHandle.SkipFirstTickOnHandle = false;
			animationActionHandle.CorrectTransitionOutTime = true;
			if (!m_DontMatchCrossfade && animationActionHandle.Manager.CurrentAction?.Action is UnitAnimationActionClip unitAnimationActionClip)
			{
				animationActionHandle.Action.TransitionIn = unitAnimationActionClip.TransitionOut;
			}
			animationManager.Execute(data.Handle);
			data.Started = true;
			flag = true;
		}
		if (data.Started && IsReadyToFinishCommand(data))
		{
			data.Finished = true;
		}
		if (MuteAttacker && m_Unit.CanEvaluate() && flag && !data.Finished)
		{
			UpdateMute(mute: true);
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return player.GetCommandData<Data>(this).Finished;
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		if (MuteAttacker && m_Unit.CanEvaluate())
		{
			UpdateMute(mute: false);
		}
		base.OnStop(player);
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData?.Unit == null)
		{
			return;
		}
		if (commandData.Unit.LifeState.IsDead)
		{
			commandData.Finished = true;
		}
		else if (commandData.Handle != null)
		{
			if (!commandData.Finished && commandData.Handle.ActiveAnimation != null)
			{
				commandData.Handle.ActiveAnimation.ChangeTransitionTime(0f);
			}
			commandData.Handle.Release();
		}
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (!RandomDelayBeforeAnimation || !(time < (double)commandData.RandomDelay))
		{
			PlayAnimation(commandData, player);
			if ((commandData.SkippedByPlayer || !IsContinuous) && time > (double)m_Timeout)
			{
				commandData.Finished = true;
			}
			bool flag = !commandData.SkippedByPlayer && IsContinuous && m_RerunBeforeReleaseAnimation;
			if (flag)
			{
				AnimationActionHandle handle = commandData.Handle;
				bool flag2 = ((handle != null && handle.IsReleased && !(handle.Manager is UnitAnimationManager { InCover: not false })) ? true : false);
				flag = flag2;
			}
			if (flag)
			{
				OnRun(player, skipping: false);
			}
		}
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		Data commandData = player.GetCommandData<Data>(this);
		if ((commandData.SkippedByPlayer || !IsContinuous) && !commandData.Finished)
		{
			Stop(player);
		}
	}

	protected override void OnRunException()
	{
		if (MuteAttacker && m_Unit.CanEvaluate())
		{
			UpdateMute(mute: false);
		}
		base.OnRunException();
	}

	private void UpdateMute(bool mute)
	{
		try
		{
			AkSoundEngine.SetRTPCValue("MuteEntity", (!mute) ? 1 : 0, m_Unit.GetValue().View.gameObject);
		}
		catch (Exception ex)
		{
			PFLog.Audio.Log(ex);
		}
	}

	public override bool TryPrepareForStop(CutscenePlayerData player)
	{
		if ((!player.GetCommandData<Data>(this).SkippedByPlayer && IsContinuous) || !IsFinished(player))
		{
			return false;
		}
		return StopPlaySignalIsReady(player);
	}

	private bool IsReadyToFinishCommand(Data data)
	{
		if (m_StopAfterTransition ? data.Handle.IsFinished : data.Handle.IsReleased)
		{
			return true;
		}
		if (!IsContinuous || data.SkippedByPlayer)
		{
			float num = data.Handle.GetTime() + data.Handle.Manager.LastDeltaTime;
			float num2 = (m_FiniteLoop ? data.FiniteLoopDuration : data.CutsceneClipWrapper.Length) - m_CrossfadeOut;
			if (m_StopAfterTransition)
			{
				if (!data.Handle.IsReleased && num > num2 - 0.001f)
				{
					data.Handle.Release();
				}
			}
			else if (num > num2 - 0.001f)
			{
				return true;
			}
		}
		return false;
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (!m_MarkUnit || m_Unit == null || !m_Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}

	public override IAbstractUnitEntity GetAnchorUnit()
	{
		if (m_Unit == null || !m_Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}

	public override string GetCaption()
	{
		string text = string.Empty;
		if (m_FiniteLoop)
		{
			text = ((!m_FiniteLoopRandomDuration) ? $" {m_FiniteLoopDuration} secs" : $" {m_FiniteLoopDuration}-{m_FiniteLoorDurationMax} secs");
		}
		bool flag = m_CutsceneClipWrapperLink?.LoadAsset()?.AnimationClip != null;
		string text2 = string.Empty;
		if (flag)
		{
			text2 = m_CutsceneClipWrapperLink?.LoadAsset()?.AnimationClip.name;
		}
		string text3 = (RandomDelayBeforeAnimation ? $" With Delay {m_RandomDelayMin}s-{m_RandomDelayMax}s " : string.Empty);
		return (m_Unit?.GetCaptionShort() ?? "(none)") + " <b>animation</b> " + (flag ? text2 : "(none)") + text3 + text;
	}
}
