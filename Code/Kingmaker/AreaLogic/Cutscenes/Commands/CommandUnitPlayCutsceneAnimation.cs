using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
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
	[InfoBox("If true, command would finish a bit before animation, allowing next animation to blend smoothly")]
	private bool m_ExitBeforeDone;

	[SerializeField]
	private float m_CrossfadeIn = 0.1f;

	[SerializeField]
	private float m_CrossfadeOut = 0.1f;

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
			commandData.Handle?.Release();
		}
		else
		{
			PlayAnimation(commandData, player);
		}
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
		if (unit.LifeState.IsDead)
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
		if (!data.Started && !data.Finished && (!m_OnlyIfIdle || animationManager.CanRunIdleAction()))
		{
			data.Action = UnitAnimationActionClip.Create(data.CutsceneClipWrapper, "PlayAnimation");
			if (m_UseAvatarMask)
			{
				data.Action.UseEmptyAvatarMask = false;
				data.Action.AvatarMasks = new List<AvatarMask> { m_AvatarMask };
			}
			data.Action.TransitionIn = m_CrossfadeIn;
			data.Action.TransitionOut = m_CrossfadeOut;
			data.Action.ExecutionMode = (m_WaitForCurrentAnimation ? ExecutionMode.Sequenced : ExecutionMode.Interrupted);
			AnimationActionHandle animationActionHandle = (data.Handle = animationManager.CreateHandle(data.Action));
			animationActionHandle.PreventsRotation = m_LockRotation;
			animationActionHandle.HasCrossfadePriority = true;
			animationManager.Execute(data.Handle);
			data.Started = true;
		}
		if (data.Started)
		{
			if (data.Handle.IsReleased)
			{
				data.Finished = true;
			}
			else if ((!IsContinuous || data.SkippedByPlayer) && m_ExitBeforeDone && data.Handle.GetTime() >= data.CutsceneClipWrapper.Length - m_CrossfadeOut - RealTimeController.SystemStepDurationSeconds * 3f)
			{
				data.Finished = true;
			}
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return player.GetCommandData<Data>(this).Finished;
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		base.OnStop(player);
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData == null || commandData.Unit == null)
		{
			return;
		}
		if (commandData.Unit.LifeState.IsDead)
		{
			commandData.Finished = true;
		}
		else if ((bool)commandData.Unit.View.AnimationManager && (m_FiniteLoop || !commandData.Finished || (!m_ExitBeforeDone && commandData.Handle != null && !commandData.Handle.IsFinished)))
		{
			commandData.Handle?.Release();
			if (commandData.Handle?.Action != null)
			{
				UnityEngine.Object.Destroy(commandData.Handle.Action);
			}
		}
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (m_FiniteLoop)
		{
			float num = commandData.FiniteLoopDuration;
			if (m_ExitBeforeDone)
			{
				num -= m_CrossfadeOut;
			}
			if (time > (double)num)
			{
				commandData.Finished = true;
			}
		}
		PlayAnimation(commandData, player);
		if ((commandData.SkippedByPlayer || !IsContinuous) && time > (double)m_Timeout)
		{
			commandData.Finished = true;
		}
		if (!commandData.SkippedByPlayer && IsContinuous && m_RerunBeforeReleaseAnimation && commandData.Handle.IsReleased)
		{
			OnRun(player, skipping: false);
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

	public override bool TryPrepareForStop(CutscenePlayerData player)
	{
		if ((!player.GetCommandData<Data>(this).SkippedByPlayer && IsContinuous) || !IsFinished(player))
		{
			return false;
		}
		return StopPlaySignalIsReady(player);
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (!m_MarkUnit || m_Unit == null || !m_Unit.TryGetValue(out var value))
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
		return (m_Unit?.GetCaption() ?? "(none)") + " <b>animation</b> " + (m_CutsceneClipWrapperLink?.LoadAsset()?.AnimationClip.name ?? "(none)") + text;
	}
}
