using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("9daba7fe0908422baedebe48e2e0d05d")]
public class CommandPlayAnimationSequence : CommandBase
{
	public class Data
	{
		internal bool Started;

		internal bool Finished;

		internal AnimationActionHandle Handle;
	}

	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	[ValidateNotNull]
	private UnitAnimationActionSequence m_Sequence;

	[SerializeField]
	[Tooltip("If true, animation would not start until previous one finishes")]
	private bool m_WaitForCurrentAnimation;

	[SerializeField]
	private bool m_MarkUnit;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Finished = false;
		commandData.Started = false;
		if (skipping)
		{
			commandData.Handle = null;
			commandData.Finished = true;
		}
		else
		{
			PlayAnimation(commandData, 0.0);
		}
	}

	private void PlayAnimation(Data data, double time)
	{
		AbstractUnitEntity value = m_Unit.GetValue();
		if (value.LifeState.IsDead)
		{
			data.Finished = true;
			return;
		}
		UnitAnimationManager animationManager = value.View.AnimationManager;
		if ((bool)animationManager && !data.Started && !data.Finished)
		{
			m_Sequence.ExecutionMode = (m_WaitForCurrentAnimation ? ExecutionMode.Sequenced : ExecutionMode.Interrupted);
			data.Handle = animationManager.CreateHandle(m_Sequence);
			animationManager.Execute(data.Handle);
			data.Started = true;
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (!commandData.Finished)
		{
			return commandData.Handle.IsReleased;
		}
		return true;
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		base.OnStop(player);
		Data commandData = player.GetCommandData<Data>(this);
		AbstractUnitEntity value = m_Unit.GetValue();
		if (value.LifeState.IsDead)
		{
			commandData.Finished = true;
		}
		else if ((bool)value.View.AnimationManager && commandData.Handle != null && !commandData.Handle.IsFinished)
		{
			commandData.Handle.Release();
		}
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		player.GetCommandData<Data>(this).Finished = true;
		Stop(player);
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		PlayAnimation(commandData, time);
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (m_MarkUnit)
		{
			if (!m_Unit.TryGetValue(out var value))
			{
				return null;
			}
			return value;
		}
		return null;
	}

	public override string GetCaption()
	{
		return m_Unit?.GetCaption() + " play sequence";
	}
}
